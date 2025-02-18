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
import re
from funcparser import GLFWFunc
from datastructparser import GLFWStruct

# I know a lot of these can't be in the header file anyway
# But I am fucking too lazy to filter them out, womp, womp
_RESERVED_NAMES = [
    "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", 
    "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", 
    "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", 
    "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", 
    "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", 
    "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", 
    "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", 
    "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
]
_STRUCT_DEFS = r"typedef struct (GLFW[a-zA-Z0-9_*]+) (GLFW[a-zA-Z0-9_*]+);"
_CALLBACK_DEFS = r"typedef ([a-zA-Z0-9_ *]+) \(\*\s*(GLFW[a-zA-Z0-9_]+)\)\((.*)\);"
_struct_defs_pattern = re.compile(_STRUCT_DEFS)
_callback_defs_pattern = re.compile(_CALLBACK_DEFS)

_unknown : list[str] = []
_structs : list[str] = []
_data_structs : list[str] = []
callbacks : list[GLFWFunc] = []

def get_for_const(val : int) -> str:
    if val > 0x7FFFFFFF_FFFFFFFF:
        return "ulong"
    elif val > 0xFFFFFFFF:
        return "long"
    elif val > 0x7FFFFFFF:
        return "uint"
    else:
        return "int"

def _sanitize_name(name : str) -> str:
    name = name.strip()
    if name in _RESERVED_NAMES:
        return f"@{name}"
    return name

def convert(type : str, name : str | None, convert_fixed_array : bool = True, convert_callbacks : bool = False) -> tuple[str, str | None, int]:
    type = type.strip()
    type = type.removeprefix("const ")
    ptr_count = 0
    array_size = -1

    while type.endswith("*"):
        ptr_count += 1
        type = type.removesuffix("*")
    
    if name != None:
        if name.endswith("[]"):
            ptr_count += 1
            name = name.removesuffix("[]")
        else:
            match = re.match(rf"^.*\[(\d+)\]$", name)
            if match != None:
                if convert_fixed_array:
                    ptr_count += 1
                array_size = int(match.group(1))
                name = re.sub(r"\[\d+\]", "", name)
        name = _sanitize_name(name)

    t = None
    if type in _structs:
        t = "nint"
        ptr_count = 0

    if type in _data_structs:
        t = type

    for callback in callbacks:
        if type == callback.name:
            t = type
            if convert_callbacks:
                t = "nint"
            break

    if t == None: 
        match type:
            case "int":
                t = "int"
            case "uint32_t":
                t = "uint"
            case "uint64_t":
                t = "ulong"
            case "void":
                t = "void"
            case "char":
                t = "byte"
            case "unsigned char":
                t = "byte"
            case "float":
                t = "float"
            case "double":
                t = "double"
            case "unsigned short":
                t = "ushort"
            case "unsigned int":
                t = "uint"
            case "size_t":
                t = "nuint"
            case _:
                t = f"__UNKNOWN_{type}"
                if not type in _unknown:
                    _unknown.append(type)
                    print(f"Unknown type {type}")

    return (t + "".ljust(ptr_count, "*"), name, array_size)

def _parse_struct_def(line : str) -> bool:
    match = _struct_defs_pattern.match(line)

    if match == None:
        return False
    
    t = match.group(2).strip()
    _structs.append(t)

    return True

def _parse_callback_def(line : str) -> None:
    match = _callback_defs_pattern.match(line)
    
    if match == None:
        return
    
    groups = match.groups()
    ret_val = groups[0].strip()
    callback_name = groups[1].strip()
    args_raw = groups[2].strip()

    callback = GLFWFunc(ret_val, callback_name, args_raw)
    callbacks.append(callback)

def parse_def(line : str) -> None:
    if _parse_struct_def(line):
        return
    _parse_callback_def(line)

def register_data_structs(structs : list[GLFWStruct]) -> None:
    for struct in structs:
        _data_structs.append(struct.typedef)