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
_unknown : list[str] = []

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

def get_for_const(val : int) -> str:
    if val > 0x7FFFFFFF_FFFFFFFF:
        return "ulong"
    elif val > 0xFFFFFFFF:
        return "long"
    elif val > 0x7FFFFFFF:
        return "uint"
    else:
        return "int"

def sanitize_name(name : str) -> str:
    name = name.strip()
    if name in _RESERVED_NAMES:
        return f"@{name}"
    return name

def convert(type : str) -> str:
    type = type.strip()
    type = type.replace("const ", "")
    type = type.replace(" const", "")
    type = type.replace(" const ", "")
    type = type.replace("const *", "*")
    type = type.replace("*const", "*")
    ptr_count = 0

    while type.endswith("*"):
        ptr_count += 1
        type = type.removesuffix("*")
    type = type.strip()

    t = None
    match type:
        case "void":
            t = "void"
        case "GLenum":
            t = "uint"
        case "GLboolean":
            t = "bool"
        case "GLbitfield":
            t = "uint"
        case "GLvoid":
            t = "void"
        case "GLbyte":
            t = "sbyte"
        case "GLubyte":
            t = "byte"
        case "GLshort":
            t = "short"
        case "GLushort":
            t = "ushort"
        case "GLint":
            t = "int"
        case "GLuint":
            t = "uint"
        case "GLclampx":
            t = "int"
        case "GLsizei":
            t = "int"
        case "GLfloat":
            t = "float"
        case "GLclampf":
            t = "float"
        case "GLdouble":
            t = "double"
        case "GLclampd":
            t = "double"
        case "GLchar":
            t = "byte"
        case "GLcharARB":
            t = "byte"
        case "GLhalf":
            t = "ushort"
        case "GLhalfARB":
            t = "ushort"
        case "GLfixed":
            t = "int"
        case "GLintptr":
            t = "nint"
        case "GLintptrARB":
            t = "nint"
        case "GLsizeiptr":
            t = "nint" # Apparently it's signed
        case "GLsizeiptrARB":
            t = "nint"
        case "GLint64":
            t = "long"
        case "GLint64EXT":
            t = "long"
        case "GLuint64":
            t = "ulong"
        case "GLuint64EXT":
            t = "ulong"
        case "GLhalfNV":
            t = "ushort"
        case "GLsync":
            t = "nint"
        case "GLvdpauSurfaceNV":
            t = "nint"
        case _:
            t = f"__UNKNOWN_{type}"
            if not type in _unknown:
                _unknown.append(type)
                print(f"Unknown type {type}")

    return t + "".ljust(ptr_count, "*")