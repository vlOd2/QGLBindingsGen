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