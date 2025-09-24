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
        string allLines = string.Join("\n", lines);

        foreach (string line in lines)
        {
            CDefinition def = CDefinition.ParseOpaqueStruct(line);
            if (def == null)
                continue;
            ctx.Definitions.Add(def);
        }

        foreach (string line in lines)
        {
            CDefinition def = CDefinition.ParseCallback(ctx, line);
            if (def == null)
                continue;
            ctx.Definitions.Add(def);
        }

        ctx.Structs.AddRange(CStruct.ParseAll(ctx, allLines));

        foreach (string line in lines)
        {
            CFunction func = CFunction.Parse(ctx, line);
            if (func == null)
                continue;
            ctx.Functions.Add(func);
        }
    }
}
