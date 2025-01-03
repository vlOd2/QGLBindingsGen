import xml.etree.ElementTree as xml

def parse(root : xml.Element) -> dict[str, str]:
    enums = {}

    for element in root.findall("./enums/enum"):
        name = element.get("name")
        value = int(str(element.get("value")), 0)
        hex_value = hex(value).upper().replace('X', 'x', 1)
        enums[name] = hex_value

    return enums