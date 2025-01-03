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

class GLCommand:
    ret_type : str
    name : str
    params : dict[str, str]
    
    def __init__(self, ret_type : str, name : str, params : dict[str, str]) -> None:
        self.ret_type = ret_type
        self.name = name
        self.params = params

def _get_type(element : xml.Element) -> str:
    type = element.text or ""
    for child in element:
        if child.tag == "name":
            continue
        if child.text:
            type += child.text
        if child.tail:
            type += child.tail
    return type.strip()

def parse(root : xml.Element) -> dict[str, GLCommand]:
    cmds = {}

    for element in root.findall("./commands/command"):
        proto_elem = element.find("./proto")
        if proto_elem == None: 
            continue
        name_elem = proto_elem.find("./name")
        if name_elem == None or name_elem.text == None: 
            continue
        
        params = {}
        for param_elem in element.findall("./param"):
            pname_elem = param_elem.find("./name")
            if pname_elem == None:
                continue
            params[pname_elem.text] = _get_type(param_elem)

        name = name_elem.text.strip()
        cmds[name] = GLCommand(_get_type(proto_elem), name, params)

    return cmds