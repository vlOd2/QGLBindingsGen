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

def convert(type : str) -> str:
    type = type.strip()
    type = type.removeprefix("const ")
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
            t = "char"
        case "GLcharARB":
            t = "char"
        case _:
            t = f"__UNKNOWN_{type}"
            if not type in _unknown:
                _unknown.append(type)
                print(f"Unknown type {type}")

    return t + "".ljust(ptr_count, "*")