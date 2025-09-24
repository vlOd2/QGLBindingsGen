using System;
using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal partial class CStruct
{
    #region Patterns
    [GeneratedRegex(@"struct ([a-zA-Z0-9_*]+)\s*?{((?:.|\s)+?)\n}(?:[a-zA-Z0-9_* ]+)?;")]
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

    private static void SearchStructs(string[] lines, Action<Match> handleStructMatch)
    {
        string currentStruct = null;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (currentStruct == null)
            {
                if (!line.StartsWith("struct ") && !line.StartsWith("typedef struct "))
                    continue;

                if (!CDefinition.OpaqueStructPattern().Match(line).Success)
                    currentStruct = $"{line}\n";
            }
            else
            {
                currentStruct += $"{line}\n";

                if (!line.Contains('}'))
                    continue;

                Match match = StructPattern().Match(currentStruct);
                currentStruct = null;

                if (match.Success)
                    handleStructMatch(match);
            }
        }
    }

    public static string[] ParseAllNames(string[] lines) 
    {
        List<string> names = [];

        SearchStructs(lines, match => names.Add(match.Groups[1].Value.Trim()));

        return [..names];
    }

    public static CStruct[] ParseAll(CParserContext ctx, string[] lines)
    {
        List<CStruct> structs = [];

        SearchStructs(lines, match => 
        {
            string name = match.Groups[1].Value.Trim();
            string fields = match.Groups[2].Value.Trim();
            structs.Add(new CStruct(ctx, name, fields));
        });

        return [.. structs];
    }
}
