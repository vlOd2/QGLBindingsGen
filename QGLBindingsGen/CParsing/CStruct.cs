using System;
using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal partial class CStruct
{
    #region Patterns
    [GeneratedRegex(@"typedef struct ([a-zA-Z0-9_*]+)\n{\s*((?:[a-zA-Z0-9_*\n; \[\]]+(?:\/\/.*)?(?:\/\*(?:.|\n)*?\*\/)?\s*)+)\n} (?:[a-zA-Z0-9_*]+);")]
    private static partial Regex StructPattern();
    #endregion
    public string Name;
    public Dictionary<string, CType> Fields;

    public CStruct(CParserContext ctx, string name, string fields)
    {
        Name = name;
        ParseFields(ctx, fields);
    }

    private void ParseFields(CParserContext ctx, string fields)
    {
        Fields = [];
        foreach (string line in fields.Split('\n'))
        {
            string l = line.Trim().Replace(";", "");
            if (l.StartsWith("//") || l.StartsWith("/*") || l.StartsWith('!') || l.StartsWith('*'))
                continue;

            Match match = CParser.ArgsPattern().Match(l);
            if (!match.Success)
                continue;

            string rawType = match.Groups[1].Value.Trim();
            string rawName = match.Groups[2].Value.Trim();
            (CType type, string name) = ctx.TypeConv.Convert(rawType, rawName);
            Fields[name] = type;
        }
    }

    public static CStruct Parse(CParserContext ctx, string line) 
    {
        Match match = StructPattern().Match(line);

        if (!match.Success)
            return null;

        string name = match.Groups[1].Value.Trim();
        string fields = match.Groups[2].Value.Trim();

        return new CStruct(ctx, name, fields);
    }
}
