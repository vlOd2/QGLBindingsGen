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

def parse_features(root : xml.Element) -> list[GLFeature]:
    features = []

    for element in root.findall("./feature"):
        feature_name = element.get("name")
        if feature_name == None:
            continue

        enums = []
        for enum_element in element.findall("./require/enum"):
            name = enum_element.get("name")
            if name == None:
                continue
            enums.append(name)

        commands = []
        for cmd_element in element.findall("./require/command"):
            name = cmd_element.get("name")
            if name == None:
                continue
            commands.append(name)

        features.append(GLFeature(feature_name, enums, commands))

    return features

def parse_extensions(root : xml.Element) -> list[GLFeature]:
    extensions = []

    for element in root.findall("./extensions/extension"):
        ext_name = element.get("name")
        if ext_name == None:
            continue

        enums = []
        for enum_element in element.findall("./require/enum"):
            name = enum_element.get("name")
            if name == None:
                continue
            enums.append(name)

        commands = []
        for cmd_element in element.findall("./require/command"):
            name = cmd_element.get("name")
            if name == None:
                continue
            commands.append(name)

        extensions.append(GLFeature(ext_name, enums, commands))

    return extensions