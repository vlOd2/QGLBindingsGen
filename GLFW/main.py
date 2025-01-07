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
import datetime
import typeconverter
import funcparser
import constparser
import datastructparser
import docsparser

HEADER_FILE_NAME = "glfw3.h"
STRUCT_FIXED_ARRAYS = True

def handle_const_parser(input_line : str) -> str | None:
    const = constparser.parse(input_line)
    if const == None:
        return None
    
    const_val : str
    const_type : str
    try:
        value = int(const[1], 0)
        const_type = typeconverter.get_for_const(value)
        const_val = hex(value).upper().replace('X', 'x', 1)
    except:
        const_val = const[1].strip()
        const_type = "uint" # I am hoping this is large enough for most

    return f"public const {const_type} {const[0]} = {const_val};"

def handle_func_parser(input_line : str) -> str | None:
    func = funcparser.parse(input_line)
    if func == None:
        return None
    
    line = f"[QGLNativeAPI(\"{func.name}\")] public static delegate* unmanaged<"
    for name, type in func.args.items():
        line += f"{typeconverter.convert(type, name)[0]}, "
    line += f"{typeconverter.convert(func.ret_type, None, convert_callbacks=True)[0]}> {func.name};"

    if "__UNKNOWN_" in line:
        print(f"Skipping function (contains unknown types): {func.name}")
        return None

    return line

def get_indent(lvl : int) -> str:
    return "".ljust(lvl * 4, " ")

def write_indent(file : TextIOWrapper, lvl : int, str : str) -> None:
    file.write(f"{get_indent(lvl)}{str}")

# Retarded jank shit, but it's the easiest thing I could do
def undo_last_line(output_file : TextIOWrapper, indent : int) -> None:
    output_file.seek(output_file.tell() - len(get_indent(indent)) - 1, 0)

def generate_callback(callback : funcparser.GLFWFunc, output_file : TextIOWrapper, indent : int) -> None:
    line = f"public unsafe delegate {callback.ret_type} {callback.name}("

    end_index = len(callback.args) - 1
    for i, (name, type) in enumerate(callback.args.items()):
        converted = typeconverter.convert(type, name)
        line += f"{converted[0]} {converted[1]}"
        if i < end_index:
            line += ", "

    line += ");";
    write_indent(output_file, indent, "[UnmanagedFunctionPointer(CallingConvention.Winapi)]\n")
    write_indent(output_file, indent, f"{line}\n")

def generate_struct(struct : datastructparser.GLFWStruct, output_file : TextIOWrapper, indent : int) -> None:
    write_indent(output_file, indent, f"public unsafe struct {struct.typedef}\n")
    write_indent(output_file, indent, "{\n")

    indent += 1
    for name, type in struct.fields.items():
        converted = typeconverter.convert(type, name, not STRUCT_FIXED_ARRAYS, True)
        t = converted[0]
        n = converted[1] or "__INVALID"
        mod = "public"
        if STRUCT_FIXED_ARRAYS and converted[2] > -1:
            n += f"[{converted[2]}]"
            mod += " fixed"
        write_indent(output_file, indent, f"{mod} {t} {n};\n");

    indent -= 1
    write_indent(output_file, indent, "}\n")
    pass

def generate_class(file : TextIOWrapper, output_file : TextIOWrapper, indent : int) -> None:
    write_indent(output_file, indent, "public static unsafe class GLFW\n")
    write_indent(output_file, indent, "{\n")

    indent += 1
    first_const = False
    done_const = False
    for line in iter(file.readline, ""):
        line = line.strip()

        # Constants
        out_line = handle_const_parser(line)
        if out_line != None:
            if not first_const:
                print("Generating constants")
                write_indent(output_file, indent, "#region Constants\n")
                first_const = True
            write_indent(output_file, indent, f"{out_line}\n")
            continue
        
        # Functions
        out_line = handle_func_parser(line)
        if out_line == None:
            continue
        if not done_const:
            print("Generating methods")
            write_indent(output_file, indent, "#endregion\n")
            write_indent(output_file, indent, "\n")
            write_indent(output_file, indent, "#region Functions\n")
            done_const = True
        write_indent(output_file, indent, f"{out_line}\n")
        write_indent(output_file, indent, "\n")
    
    undo_last_line(output_file, indent)
    write_indent(output_file, indent, "#endregion\n")
    indent -= 1
    write_indent(output_file, indent, "}\n")

def main():
    # Ensure output dir exists
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    with open(f"{OUTPUT_DIR}GLFW.cs", "w") as output_file:
        # License header
        with open(LICENSE_FILE_NAME, "r") as file:
            for line in iter(file.readline, ""):
                output_file.write(f"// {line.strip()}\n")
            output_file.write("\n")

        with open(HEADER_FILE_NAME, "r") as file:
            # Parse type defs
            print("Parsing type definitions")
            for line in iter(file.readline, ""):
                typeconverter.parse_def(line.strip())
            file.seek(0, 0)

            # Parse data structs
            print("Parsing data structs")
            _lines = file.readlines()
            _lines = "\n".join([s.strip() for s in _lines])
            data_structs = datastructparser.parse(_lines)
            typeconverter.register_data_structs(data_structs)

            # Parse documentation
            print("Parsing documentation")
            docsparser.parse(_lines)
            _lines = None
            file.seek(0, 0)

            # Usings
            indent = 0
            write_indent(output_file, indent, "using System.Runtime.InteropServices;\n")
            write_indent(output_file, indent, "using QuickGLNS.Internal;\n")
            write_indent(output_file, indent, "\n")

            # Begin namespace
            print("Generating namespace")
            write_indent(output_file, indent, f"// Bindings generated at {datetime.datetime.now()}\n")
            write_indent(output_file, indent, f"namespace {NAMESPACE}\n")
            write_indent(output_file, indent, "{\n")
            
            # Callbacks
            indent += 1
            print("Generating callbacks")
            write_indent(output_file, indent, "#region Callbacks\n")

            for callback in typeconverter.callbacks:
                generate_callback(callback, output_file, indent)
                write_indent(output_file, indent, "\n")

            undo_last_line(output_file, indent)
            write_indent(output_file, indent, "#endregion\n")
            write_indent(output_file, indent, "\n")

            # Data structs
            print("Generating data structs")
            for struct in data_structs:
                generate_struct(struct, output_file, indent)
                write_indent(output_file, indent, "\n")

            # Class, constants and functions
            print("Generating class")
            generate_class(file, output_file, indent)
            indent -= 1

            # End namespace
            write_indent(output_file, indent, "}\n")

        print("Done")

if __name__ == "__main__":
    main()
