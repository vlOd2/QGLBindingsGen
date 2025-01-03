import re

_PARSE_ARGS = r"([a-zA-Z0-9_ *]+(?= )(?!, ))([a-zA-Z0-9_ *]+(?:\[\d*\]){0,1})" # still not perfect, gotta cope with strip
parse_args_pattern = re.compile(_PARSE_ARGS)