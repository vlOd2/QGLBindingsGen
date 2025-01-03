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
import re
import common

_STRUCT_DEFS = r"typedef struct (GLFW[a-zA-Z0-9_*]+)\n{\s*((?:[a-zA-Z0-9_*\n; \[\]]+(?:\/\/.*)?(?:\/\*(?:.|\n)*?\*\/)?\s*)+)\n} (GLFW[a-zA-Z0-9_*]+);"
_struct_defs_pattern = re.compile(_STRUCT_DEFS)

class GLFWStruct:
    name : str
    typedef : str
    fields : dict[str, str]

    def __init__(self, name : str, typedef : str, fields_raw : str) -> None:
        self.name = name
        self.typedef = typedef
        self.fields = {}
        if not fields_raw:
            return
        self._parse_fields(fields_raw)

    def _parse_fields(self, fields_raw : str):
        for line in fields_raw.splitlines():
            line = line.strip().removesuffix(";")
            # Ignore comments
            if line.startswith("/*") or line.startswith("*") or line.startswith("!"):
                continue

            match = common.parse_args_pattern.match(line)
            if match == None:
                continue

            groups = match.groups()
            type = groups[0].strip()
            name = groups[1].strip()
            self.fields[name] = type

def parse(header_content : str) -> list[GLFWStruct]:
    structs = []

    for match in _struct_defs_pattern.finditer(header_content):
        groups = match.groups()

        name = groups[0].strip()
        typedef = groups[2].strip()
        fields_raw = groups[1].strip()

        structs.append(GLFWStruct(name, typedef, fields_raw))

    return structs