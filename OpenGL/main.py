# MIT License
# 
# Copyright (c) 2025 vlOd
# 
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
import os
from io import TextIOWrapper
import xml.etree.ElementTree as xml
import datetime
import enumparser
import commandparser
import featureparser
import typeconverter
from commandparser import GLCommand
from featureparser import GLFeature

# Target features to generate bindings for
# Only features that generate fully are included by default
# Normal OpenGL
TARGET_FEATURES = [
    "GL_VERSION_1_0", "GL_VERSION_1_1", 
    "GL_VERSION_1_2", "GL_VERSION_1_3", 
    "GL_VERSION_1_4", "GL_VERSION_1_5",
    "GL_VERSION_2_0", "GL_VERSION_2_1", 
    "GL_VERSION_3_0", "GL_VERSION_3_1", 
    "GL_VERSION_3_2", "GL_VERSION_3_3",
    "GL_VERSION_4_0", "GL_VERSION_4_1",
    "GL_VERSION_4_2"
]
# OpenGL ES
TARGET_FEATURES.extend([ 
    "GL_VERSION_ES_CM_1_0", "GL_ES_VERSION_2_0", 
    "GL_ES_VERSION_3_0", "GL_ES_VERSION_3_1"
])

# Target extensions to generate bindings for
# No extension is included by default
TARGET_EXTENSIONS = []

NAMESPACE = "QuickGLNS"
REGISTRY_FILE = "gl.xml"
LICENSE_FILE_NAME = "LICENSE"
OUTPUT_DIR = "generated/"

_enums : dict[str, int]
_commands : dict[str, GLCommand]

def get_indent(lvl : int) -> str:
    return "".ljust(lvl * 4, " ")

def write_indent(file : TextIOWrapper, lvl : int, str : str) -> None:
    file.write(f"{get_indent(lvl)}{str}")

def generate_enums(feature : GLFeature, output_file : TextIOWrapper, indent : int) -> None:
    for enum in feature.enums:
        value = _enums[enum]
        type = typeconverter.get_for_const(value)
        hex_value = hex(value).upper().replace('X', 'x', 1)
        write_indent(output_file, indent, f"public const {type} {enum} = {hex_value};\n")

def generate_commands(feature : GLFeature, output_file : TextIOWrapper, indent : int) -> None:
    for _cmd in feature.commands:
        cmd = _commands[_cmd]
        
        line = f"public static delegate* unmanaged<"
        for type in cmd.params.values():
            line += f"{typeconverter.convert(type)}, "
        line += f"{typeconverter.convert(cmd.ret_type)}> {cmd.name};"

        if "__UNKNOWN_" in line:
            print(f"Skipping function (contains unknown types): {cmd.name}")
            continue

        write_indent(output_file, indent, f"{line}\n")

def generate_class(feature : GLFeature, class_name : str, output_file : TextIOWrapper, indent : int) -> None:
    write_indent(output_file, indent, f"public static unsafe class {class_name}\n")
    write_indent(output_file, indent, "{\n")
    indent += 1
    
    write_indent(output_file, indent, "#region Enums\n")
    generate_enums(feature, output_file, indent)
    write_indent(output_file, indent, "#endregion\n")
    write_indent(output_file, indent, "\n")
    write_indent(output_file, indent, "#region Commands\n")
    generate_commands(feature, output_file, indent)
    write_indent(output_file, indent, "#endregion\n")

    indent -= 1
    write_indent(output_file, indent, "}\n")

def generate(feature : GLFeature, class_name : str, output_file : TextIOWrapper):
    with open(LICENSE_FILE_NAME, "r") as file:
        for line in iter(file.readline, ""):
            output_file.write(f"// {line.strip()}\n")
        output_file.write("\n")

    indent = 0
    write_indent(output_file, indent, f"// Bindings generated at {datetime.datetime.now()}\n")
    write_indent(output_file, indent, f"namespace {NAMESPACE}\n")
    write_indent(output_file, indent, "{\n")
    generate_class(feature, class_name, output_file, indent + 1)
    write_indent(output_file, indent, "}\n")

def main():
    global _enums
    global _commands

    root : xml.Element = xml.parse(REGISTRY_FILE).getroot()
    _enums = enumparser.parse(root)
    _commands = commandparser.parse(root)
    features = featureparser.parse_features(root)
    extensions = featureparser.parse_extensions(root)

    os.makedirs(OUTPUT_DIR, exist_ok=True)
    for feature in features:
        if not feature.name in TARGET_FEATURES:
            continue
        class_name = feature.name.replace("GL_", "GL").replace("VERSION_", "").replace("_", "")
        with open(f"{OUTPUT_DIR}{class_name}.cs", "w") as output_file:
            generate(feature, class_name, output_file)
    
    for ext in extensions:
        if not ext.name in TARGET_EXTENSIONS:
            continue
        class_name = ext.name.replace("GL_", "GLEXT###").replace("_", "").replace("###", "_")
        with open(f"{OUTPUT_DIR}{class_name}.cs", "w") as output_file:
            generate(ext, class_name, output_file)


if __name__ == "__main__":
    main()