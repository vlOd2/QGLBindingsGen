using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal static partial class CParser
{
    #region Patterns
    [GeneratedRegex(@"([a-zA-Z0-9_ *]+(?= )(?!, ))([a-zA-Z0-9_ *]+(?:\[\d*\]){0,1})")]
    public static partial Regex ArgsPattern();
    #endregion

    public static async Task ParseFile(string[] rawLines, CParserContext ctx)
    {
        List<string> lines = [];

        await TaskRunner.Run("Preparing file", Parallel.ForEachAsync(rawLines, (rawLine, _) =>
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                return new();
            if (line.StartsWith("//") || (line.StartsWith("/*") && line.EndsWith("*/")))
                return new();

            foreach (string word in ctx.RemoveWords)
            {
                line = line.Replace($" {word} ", " ").Trim();
                line = line.Replace($" {word}", " ").Trim();
                line = line.Replace($"{word} ", " ").Trim();
            }

            lines.Add(line);
            return new();
        }));

        await TaskRunner.Run("Parsing constants and opaque structs", Parallel.ForEachAsync(lines, (line, _) =>
        {
            CConstant cconst = CConstant.Parse(line);
            if (cconst != null)
            {
                ctx.Constants.Add(cconst);
                return new();
            }
            CDefinition def = CDefinition.ParseOpaqueStruct(line);
            if (def != null)
                ctx.Definitions.Add(def);
            return new();
        }));

        string[] structNames = await TaskRunner.Run("Parsing structs (lazy)", Task.Run(() =>
        {
            string[] structNames = CStruct.ParseAllNames(lines);
            foreach (CDefinition def in structNames.Select(name => new CDefinition(name, null)))
                ctx.Definitions.Add(def);
            return structNames;
        }));

        await TaskRunner.Run("Parsing callbacks", Parallel.ForEachAsync(lines, (line, _) =>
        {
            CDefinition def = CDefinition.ParseCallback(ctx, line);
            if (def == null)
                return new();
            ctx.Definitions.Add(def);
            return new();
        }));

        await TaskRunner.Run("Parsing structs", Task.Run(() =>
        {
            foreach (CStruct s in CStruct.ParseAll(ctx, lines))
                ctx.Structs.Add(s);
            List<CDefinition> defs = [.. ctx.Definitions.Where(def => !structNames.Contains(def.Name))];
            ctx.Definitions.Clear();
            foreach (CDefinition def in defs)
                ctx.Definitions.Add(def);
        }));

        await TaskRunner.Run("Parsing functions", Parallel.ForEachAsync(lines, (line, _) =>
        {
            CFunction func = CFunction.Parse(ctx, line);
            if (func == null)
                return new();
            ctx.Functions.Add(func);
            return new();
        }));
    }
}
