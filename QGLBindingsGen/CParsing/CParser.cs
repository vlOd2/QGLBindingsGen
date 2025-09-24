using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal static partial class CParser
{
    #region Patterns
    [GeneratedRegex(@"([a-zA-Z0-9_ *]+(?= )(?!, ))([a-zA-Z0-9_ *]+(?:\[\d*\]){0,1})")]
    public static partial Regex ArgsPattern();
    #endregion

    public static void ParseFile(string[] lines, CParserContext ctx)
    {
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
