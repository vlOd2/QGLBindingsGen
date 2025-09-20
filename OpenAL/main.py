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

AL_HEADER_FILE_NAME = "al.h"
ALC_HEADER_FILE_NAME = "alc.h"

def handle_const_parser(input_line : str) -> str | None:
    const = constparser.parse(input_line)
    if const == None:
        return None
    
    const_val : str
    const_type : str
    try:
        value = int(const[1].replace("(", "").replace(")", ""), 0)
        const_type = typeconverter.get_for_const(value)
        const_val = hex(value).upper().replace('X', 'x', 1)
    except:
        const_val = const[1].strip()
        const_type = "uint" # I am hoping this is large enough for most

    return f"public const {const_type} {const[0]} = {const_val};"

def _generate_wrapper_func(func : funcparser.ALFunc, func_ret_type : str, func_args : dict[str, str]) -> str:
    output = ""
    if "bool" in func_ret_type:
        output = "[return: MarshalAs(UnmanagedType.I1)] "
    output += f"public static {func_ret_type} {func.name}("
    call_args = ""
    for i, (name, type) in enumerate(func_args.items()):
        if "bool" in type:
            output += "[MarshalAs(UnmanagedType.I1)] "
        output += f"{type} {name}"
        call_args += name
        if i < len(func_args) - 1:
            output += ", "
            call_args += ", "
    output += f") "
    output += "{ "
    output += f"QGLFeature.VerifyFunc((nint)_{func.name}); "
    if func_ret_type != "void":
        output += f"return _{func.name}({call_args}); "
    else:
        output += f"_{func.name}({call_args}); "
    output += "}"

    return output

def get_func_type(func : funcparser.ALFunc) -> str | None:
    func_args : dict[str, str] = {}
    func_ret_type = typeconverter.convert(func.ret_type, None)[0]

    for name, type in func.args.items():
        converted = typeconverter.convert(type, name)
        if converted[1] == None:
            print(f"Skipping function (invalid arg name): {func.name}")
            return None
        func_args[converted[1]] = converted[0]

    s = f"delegate* unmanaged[Cdecl]<"
    for name, type in func_args.items():
        s += f"{type}, "
    s += f"{func_ret_type}>"

    return s

def handle_func_parser(input_line : str) -> tuple[list[str], funcparser.ALFunc] | None:
    func = funcparser.parse(input_line)

    if func == None:
        return None
    func_args : dict[str, str] = {}
    func_ret_type = typeconverter.convert(func.ret_type, None)[0]

    for name, type in func.args.items():
        converted = typeconverter.convert(type, name)
        if converted[1] == None:
            print(f"Skipping function (invalid arg name): {func.name}")
            return None
        func_args[converted[1]] = converted[0]

    definition = f"{_generate_wrapper_func(func, func_ret_type, func_args)}\n"
    definition += f"internal static {get_func_type(func)} _{func.name} = null;"

    # if "__UNKNOWN_" in definition:
    #     print(f"Skipping function (contains unknown types): {func.name}")
    #     return None

    return definition.splitlines(), func

def get_indent(lvl : int) -> str:
    return "".ljust(lvl * 4, " ")

def write_indent(file : TextIOWrapper, lvl : int, str : str) -> None:
    file.write(f"{get_indent(lvl)}{str}")

# Retarded jank shit, but it's the easiest thing I could do
def undo_last_line(file : TextIOWrapper, indent : int) -> None:
    file.seek(file.tell() - len(get_indent(indent)) - 1, 0)

def generate_loader_functions(functions : list[funcparser.ALFunc], output_file : TextIOWrapper, indent : int) -> None:
    write_indent(output_file, indent, "internal static void Load()\n")
    write_indent(output_file, indent, "{\n")
    indent += 1
    for func in functions:
        write_indent(output_file, indent, f"_{func.name} = ({get_func_type(func)})QuickGL.GetALProcAddress(\"{func.name}\");\n")
    indent -= 1
    write_indent(output_file, indent, "}\n")
    write_indent(output_file, indent, "\n")

    write_indent(output_file, indent, "internal static void Unload()\n")
    write_indent(output_file, indent, "{\n")
    indent += 1
    for func in functions:
        write_indent(output_file, indent, f"_{func.name} = null;\n")
    indent -= 1
    write_indent(output_file, indent, "}\n")
    write_indent(output_file, indent, "\n")

def generate_class(file : TextIOWrapper, output_file : TextIOWrapper, class_name : str, indent : int) -> None:
    functions : list[funcparser.ALFunc] = []
    
    # License header
    with open(LICENSE_FILE_NAME, "r") as license_file:
        for line in iter(license_file.readline, ""):
            output_file.write(f"// {line.strip()}\n")
        output_file.write("\n")

    write_indent(output_file, indent, "using System.Runtime.InteropServices;\n")
    write_indent(output_file, indent, "using QuickGLNS.Internal;\n")
    write_indent(output_file, indent, "\n")
    write_indent(output_file, indent, f"// Bindings generated at {datetime.datetime.now()}\n")
    write_indent(output_file, indent, f"namespace {NAMESPACE};\n")
    write_indent(output_file, indent, "\n")
    write_indent(output_file, indent, f"public static unsafe class {class_name}\n")
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
        result = handle_func_parser(line)
        if result == None:
            continue
        functions.append(result[1])
        if not done_const:
            print("Generating methods")
            write_indent(output_file, indent, "#endregion\n")
            write_indent(output_file, indent, "\n")
            write_indent(output_file, indent, "#region Functions\n")
            done_const = True
        for out_line in result[0]:
            write_indent(output_file, indent, f"{out_line}\n")
        write_indent(output_file, indent, "\n")
    
    undo_last_line(output_file, indent)
    write_indent(output_file, indent, "#endregion\n")
    write_indent(output_file, indent, "\n")
    
    generate_loader_functions(functions, output_file, indent)
    
    indent -= 1
    write_indent(output_file, indent, "}\n")

def main():
    # Ensure output dir exists
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    print("Generating AL")
    with open(f"{OUTPUT_DIR}AL.cs", "w") as output_file:
        with open(AL_HEADER_FILE_NAME, "r") as file:
            generate_class(file, output_file, "AL", 0)

    print("Generating ALC")
    with open(f"{OUTPUT_DIR}ALC.cs", "w") as output_file:
        with open(ALC_HEADER_FILE_NAME, "r") as file:
            generate_class(file, output_file, "ALC", 0)

    print("Done")

if __name__ == "__main__":
    main()
