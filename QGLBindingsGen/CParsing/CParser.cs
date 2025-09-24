using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal static partial class CParser
{
    #region Patterns
    [GeneratedRegex(@"([a-zA-Z0-9_ *]+(?= )(?!, ))([a-zA-Z0-9_ *]+(?:\[\d*\]){0,1})")]
    public static partial Regex ArgsPattern();
    #endregion

    public static void ParseFile(string[] rawLines, CParserContext ctx)
    {
        List<string> lines = [];

        foreach (string rawLine in rawLines)
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (line.StartsWith("//") || (line.StartsWith("/*") && line.EndsWith("*/")))
                continue;

            foreach (string word in ctx.RemoveWords)
            {
                line = line.Replace($" {word} ", " ").Trim();
                line = line.Replace($" {word}", " ").Trim();
                line = line.Replace($"{word} ", " ").Trim();
            }

            lines.Add(line);
        }

        Console.WriteLine("Parsing constants and opaque structs");
        foreach (string line in lines)
        {
            CConstant cconst = CConstant.Parse(line);
            if (cconst != null)
            {
                ctx.Constants.Add(cconst);
                continue;
            }
            CDefinition def = CDefinition.ParseOpaqueStruct(line);
            if (def != null)
                ctx.Definitions.Add(def);
        }

        Console.WriteLine("Lazy parsing structs");
        string[] structNames = CStruct.ParseAllNames(lines);
        ctx.Definitions.AddRange(structNames.Select(name => new CDefinition(name, null)));

        Console.WriteLine("Parsing callbacks");
        foreach (string line in lines)
        {
            CDefinition def = CDefinition.ParseCallback(ctx, line);
            if (def == null)
                continue;
            ctx.Definitions.Add(def);
        }

        Console.WriteLine("Parsing structs");
        ctx.Structs.AddRange(CStruct.ParseAll(ctx, lines));
        for (int i = ctx.Definitions.Count - 1; i >= 0; i--)
        {
            if (structNames.Contains(ctx.Definitions[i].Name))
            {
                ctx.Definitions.RemoveAt(i);
                i--;
            }
        }

        Console.WriteLine("Parsing functions");
        foreach (string line in lines)
        {
            CFunction func = CFunction.Parse(ctx, line);
            if (func == null)
                continue;
            ctx.Functions.Add(func);
        }
    }
}
