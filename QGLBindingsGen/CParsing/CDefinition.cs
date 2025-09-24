using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal partial class CDefinition
{
    #region Pattern
    [GeneratedRegex(@"typedef struct ([a-zA-Z0-9_*]+) ([a-zA-Z0-9_*]+);")]
    private static partial Regex OpaqueStructPattern();

    [GeneratedRegex(@"typedef ([a-zA-Z0-9_ *]+) \(\*\s*([a-zA-Z0-9_]+)\)\((.*)\);")]
    private static partial Regex CallbackPattern();
    #endregion
    public string Name;
    public CFunction Callback;

    public CDefinition(string name, CFunction callback)
    {
        Name = name;
        Callback = callback;
    }

    public static CDefinition Parse(CParserContext ctx, string line)
    {
        Match match;
        if ((match = OpaqueStructPattern().Match(line)).Success)
            return new CDefinition(match.Groups[2].Value.Trim(), null);

        match = CallbackPattern().Match(line);
        if (!match.Success)
            return null;

        string name = match.Groups[2].Value.Trim();
        string returnType = match.Groups[1].Value.Trim();
        string args = match.Groups[3].Value.Trim();

        return new CDefinition(name, new CFunction(ctx, name, returnType, args));
    }
}
