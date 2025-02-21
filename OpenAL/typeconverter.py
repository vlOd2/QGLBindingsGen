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

_unknown : list[str] = []

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

def convert(type : str, name : str | None) -> tuple[str, str | None]:
    type = type.strip()
    type = type.removeprefix("const ")
    ptr_count = 0

    while type.endswith("*"):
        ptr_count += 1
        type = type.removesuffix("*")
    
    if name != None:
        if name.startswith("*"):
            ptr_count += 1
            name = name.removeprefix("*")
        name = _sanitize_name(name)

    t = None
    match type:
        case "ALCcontext":
            t = "nint"
            ptr_count = 0
        case "ALCdevice":
            t = "nint"
            ptr_count = 0
        case "ALboolean":
            t = "bool"
        case "ALchar":
            t = "char"
        case "ALbyte":
            t = "sbyte"
        case "ALubyte":
            t = "byte"
        case "ALshort":
            t = "short"
        case "ALushort":
            t = "ushort"
        case "ALint":
            t = "int"
        case "ALuint":
            t = "uint"
        case "ALsizei":
            t = "int"
        case "ALenum":
            t = "int"
        case "ALfloat":
            t = "float"
        case "ALdouble":
            t = "double"
        case "ALvoid":
            t = "void"
        case "ALCboolean":
            t = "bool"
        case "ALCchar":
            t = "char"
        case "ALCbyte":
            t = "sbyte"
        case "ALCubyte":
            t = "byte"
        case "ALCshort":
            t = "short"
        case "ALCushort":
            t = "ushort"
        case "ALCint":
            t = "int"
        case "ALCuint":
            t = "uint"
        case "ALCsizei":
            t = "int"
        case "ALCenum":
            t = "int"
        case "ALCfloat":
            t = "float"
        case "ALCdouble":
            t = "double"
        case "ALCvoid":
            t = "void"
        case "void":
            t = "void"
        case _:
            t = f"__UNKNOWN_{type}"
            if not type in _unknown:
                _unknown.append(type)
                print(f"Unknown type {type}")    

    return (t + "".ljust(ptr_count, "*"), name)