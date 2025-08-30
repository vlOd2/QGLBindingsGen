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
import sys
from os import path
sys.path.append(path.dirname(path.dirname(path.abspath(__file__))))
from common_const import *

import os
from io import TextIOWrapper
import xml.etree.ElementTree as xml
import datetime
import enumparser
import commandparser
import featureparser
import typeconverter
import re
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
TARGET_ES_FEATURES = [
    "GL_VERSION_ES_CM_1_0", "GL_ES_VERSION_2_0", 
    "GL_ES_VERSION_3_0", "GL_ES_VERSION_3_1"
]
TARGET_FEATURES.extend(TARGET_ES_FEATURES)

# Target extensions to generate bindings for (or none for all)
# You can also add regex patterns to match by prefexing with @/
#TARGET_EXTENSIONS = None
TARGET_EXTENSIONS = [ "@/GL_ARB+." ]

REGISTRY_FILE = "gl.xml"

_enums : dict[str, int]
_commands : dict[str, GLCommand]

def get_indent(lvl : int) -> str:
    return "".ljust(lvl * 4, " ")

def write_indent(file : TextIOWrapper, lvl : int, str : str) -> None:
    file.write(f"{get_indent(lvl)}{str}")

# Retarded jank shit, but it's the easiest thing I could do
def undo_last_line(file : TextIOWrapper, indent : int) -> None:
    file.seek(file.tell() - len(get_indent(indent)) - 1, 0)

def generate_enums(feature : GLFeature, output_file : TextIOWrapper, indent : int) -> None:
    for enum in feature.enums:
        value = _enums[enum]
        type = typeconverter.get_for_const(value)
        hex_value = hex(value).upper().replace('X', 'x', 1)
        write_indent(output_file, indent, f"public const {type} {enum} = {hex_value};\n")

def _generate_cmd_wrapper(command : GLCommand, cmd_ret_type : str, cmd_params : dict[str, str]) -> str:
    output = ""
    call_args = ""
 
    output += f"public static {cmd_ret_type} {command.name}("
    for i, (name, type) in enumerate(cmd_params.items()):
        output += f"{type} {name}"
        call_args += name
        if i < len(cmd_params) - 1:
            output += ", "
            call_args += ", "
    output += f") "
    output += "{ "
    output += f"QGLNativeAPI.Verify((nint)_{command.name}); "
    if cmd_ret_type != "void":
        output += f"return _{command.name}({call_args}); "
    else:
        output += f"_{command.name}({call_args}); "
    output += "}"

    return output

def generate_commands(feature : GLFeature, output_file : TextIOWrapper, indent : int) -> int:
    generated = 0

    for _cmd in feature.commands:
        cmd = _commands[_cmd]

        cmd_params : dict[str, str] = {}
        cmd_ret_type = typeconverter.convert(cmd.ret_type)
        for name, type in cmd.params.items():
            cmd_params[typeconverter.sanitize_name(name)] = typeconverter.convert(type)

        lines = f"{_generate_cmd_wrapper(cmd, cmd_ret_type, cmd_params)}\n"
        lines += f"[QGLNativeAPI(\"{cmd.name}\")] internal static delegate* unmanaged<"
        for type in cmd_params.values():
            lines += f"{type}, "
        lines += f"{cmd_ret_type}> _{cmd.name} = null;"

        # if "__UNKNOWN_" in lines:
        #     print(f"Skipping command (contains unknown types): {cmd.name}")
        #     continue

        for line in lines.splitlines():
            write_indent(output_file, indent, f"{line}\n")
        write_indent(output_file, indent, "\n")
        generated += 1

    return generated

def generate_class(feature : GLFeature, class_name : str, output_file : TextIOWrapper, indent : int, isGLES : bool, isEXT : bool) -> None:
    write_indent(output_file, indent, f"[QGLFeature(\"{feature.name}\", {'true' if isEXT else 'false'}, {'true' if isGLES else 'false'})]\n")
    write_indent(output_file, indent, f"public static unsafe class {class_name}\n")
    write_indent(output_file, indent, "{\n")
    indent += 1
    
    write_indent(output_file, indent, "#region Enums\n")
    generate_enums(feature, output_file, indent)
    write_indent(output_file, indent, "#endregion\n")
    write_indent(output_file, indent, "\n")
    write_indent(output_file, indent, "#region Commands\n")
    if generate_commands(feature, output_file, indent) > 0:
        undo_last_line(output_file, indent)
    write_indent(output_file, indent, "#endregion\n")

    indent -= 1
    write_indent(output_file, indent, "}\n")

def generate(feature : GLFeature, class_name : str, output_file : TextIOWrapper, isGLES : bool, isEXT : bool):
    with open(LICENSE_FILE_NAME, "r") as file:
        for line in iter(file.readline, ""):
            output_file.write(f"// {line.strip()}\n")
        output_file.write("\n")

    indent = 0
    write_indent(output_file, indent, "using QuickGLNS.Internal;\n")
    write_indent(output_file, indent, "\n")
    write_indent(output_file, indent, f"// Bindings generated at {datetime.datetime.now()}\n")
    if isEXT:
        write_indent(output_file, indent, f"namespace {NAMESPACE}.Extensions;\n")
    else:
        write_indent(output_file, indent, f"namespace {NAMESPACE};\n")
    write_indent(output_file, indent, "\n")
    generate_class(feature, class_name, output_file, indent, isGLES, isEXT)

def main():
    global _enums
    global _commands

    root : xml.Element = xml.parse(REGISTRY_FILE).getroot()
    print("Parsing enums")
    _enums = enumparser.parse(root)
    print("Parsing commands")
    _commands = commandparser.parse(root)
    print("Parsing features")
    features = featureparser.parse(root, "./feature")
    print("Parsing extensions")
    extensions = featureparser.parse(root, "./extensions/extension")

    os.makedirs(OUTPUT_DIR, exist_ok=True)
    for feature in features:
        if TARGET_FEATURES != None and not feature.name in TARGET_FEATURES:
            continue
        print(f"Generating bindings for feature: {feature.name}")
        class_name = feature.name.replace("GL_", "GL").replace("VERSION_", "").replace("_", "")
        with open(f"{OUTPUT_DIR}{class_name}.cs", "w") as output_file:
            generate(feature, class_name, output_file, feature.name in TARGET_ES_FEATURES, False)
    
    os.makedirs(f"{OUTPUT_DIR}/Extensions", exist_ok=True)
    for ext in extensions:
        if TARGET_EXTENSIONS != None and not ext.name in TARGET_EXTENSIONS:
            passed: bool = False
            for pattern in TARGET_EXTENSIONS:
                if not pattern.startswith("@/"):
                    continue
                pattern = pattern.removeprefix("@/")
                if re.match(pattern, ext.name) != None:
                    passed = True
                    break
            if not passed:
                continue

        print(f"Generating bindings for extension: {ext.name}")
        class_name = ext.name.replace("GL_", "GLEXT###").replace("_", "").replace("###", "_")
        with open(f"{OUTPUT_DIR}/Extensions/{class_name}.cs", "w") as output_file:
            generate(ext, class_name, output_file, False, True)

    print("Done")

if __name__ == "__main__":
    main()
