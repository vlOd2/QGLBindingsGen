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
        case "int":
            t = "int"
        case "uint32_t":
            t = "uint"
        case "uint64_t":
            t = "ulong"
        case "void":
            t = "void"
        case "char":
            t = "char"
        case "float":
            t = "float"
        case "double":
            t = "double"
        case "unsigned char":
            t = "byte"
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

    return t + "".ljust(ptr_count, "*")