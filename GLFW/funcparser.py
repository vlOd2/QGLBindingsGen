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

_PARSE_FUNC = r"GLFWAPI ([a-zA-Z0-9_ *]+) (glfw[a-zA-Z0-9_]+)\((.*)\);"
_parse_func_pattern = re.compile(_PARSE_FUNC)

class GLFWFunc:
    ret_val : str
    name : str
    args : dict[str, str]

    def __init__(self, ret_val : str, func_name : str, args_raw : str | None):
        self.ret_val = ret_val
        self.name = func_name
        self.args = {}
        if not args_raw:
            return
        self._parse_args(args_raw)

    def _parse_args(self, args_raw : str):
        for match in common.parse_args_pattern.finditer(args_raw):
            groups = match.groups()
            type = groups[0].strip()
            name = groups[1].strip()
            self.args[name] = type

def parse(line : str) -> GLFWFunc | None:
    match = _parse_func_pattern.match(line)

    if match == None:
        return None
    
    groups = match.groups()
    ret_val = groups[0].strip()
    func_name = groups[1].strip()
    args_raw = groups[2].strip()

    return GLFWFunc(ret_val, func_name, args_raw)