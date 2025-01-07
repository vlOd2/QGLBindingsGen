import re

_comment_block_pattern = re.compile(r"\/\*\!([\s\S]*?)\*\/\s+GLFWAPI [a-zA-Z0-9_ *]+ (glfw[a-zA-Z0-9_]+)")
_brief_section_pattern = re.compile(r"@brief ([\s\S]+?)(?=\n+@)")
_standard_ref_pattern = re.compile(r"@ref\s*([a-zA-Z_0-9]*?)(?=(?: |\n|,|\.))")
_named_ref_pattern = re.compile(r"\[([a-zA-Z_0-9 ]*)\]\(@ref ([a-zA-Z_0-9]*)\)")
_param_section_pattern = re.compile(r"@param\[\w+\] ([a-zA-Z_0-9]*) ([\s\S]+?)(?=\n@)")

func_docs : dict[str, str] = {}

def _clean_comment(comment : str) -> str:
    cleaned = ""
    
    for s in comment.splitlines():
        s = s.strip().removeprefix("*").strip()
        cleaned += f"{s}\n"

    return cleaned.strip()

def parse(header_content : str) -> None:
    for match in _comment_block_pattern.finditer(header_content):
        groups = match.groups()
        comment = _clean_comment(groups[0])
        comment = _named_ref_pattern.sub(lambda m: f"<see cref=\"{m.group(2)}\">{m.group(1)}</see>", comment)
        comment = _standard_ref_pattern.sub(lambda m: f"<see cref=\"{m.group(1)}\"/>", comment)

        brief_match = _brief_section_pattern.match(comment)
        if not brief_match:
            continue

        brief_section = brief_match.group(1)
        doc = "/// <summary>\n"
        doc += "\n".join([f"/// {s.strip()}" for s in brief_section.splitlines()])
        doc += "\n/// </summary>\n"

        for param_match in _param_section_pattern.finditer(comment):
            param_groups = param_match.groups()
            doc += f"/// <param name=\"{param_groups[0].strip()}\">{param_groups[1].strip()}</param>\n"

        func_docs[groups[1].strip()] = doc.strip()
        