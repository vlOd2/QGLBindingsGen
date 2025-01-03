import xml.etree.ElementTree as xml
from commandparser import GLCommand

class GLFeature:
    name : str
    enums : list[str]
    commands : list[str]

    def __init__(self, name : str, enums : list[str], commands : list[str]) -> None:
        self.name = name
        self.enums = enums
        self.commands = commands

def parse(root : xml.Element) -> list[GLFeature]:
    features = []

    for element in root.findall("./feature"):
        if element.get("api") != "gl":
            continue
        feature_name = element.get("name")
        if feature_name == None:
            continue

        enums = []
        for enum_element in element.findall("./enum"):
            name = enum_element.get("name")
            if name == None:
                continue
            enums.append(name)

        commands = []
        for cmd_element in element.findall("./command"):
            name = cmd_element.get("name")
            if name == None:
                continue
            commands.append(name)

        features.append(GLFeature(feature_name, enums, commands))

    return features