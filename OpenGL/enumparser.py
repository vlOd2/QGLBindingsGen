import xml.etree.ElementTree as xml

def parse(root : xml.Element) -> dict[str, int]:
    enums = {}

    for element in root.findall("./enums/enum"):
        name = element.get("name")
        value = int(str(element.get("value")), 0)
        enums[name] = value

    return enums