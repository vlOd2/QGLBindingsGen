using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal partial class CFunction
{
    #region Patterns
    [GeneratedRegex(@"([a-zA-Z0-9_ *]+(?= )(?!, ))([a-zA-Z0-9_ *]+(?:\[\d*\]){0,1})")]
    private static partial Regex ArgsPattern();

    [GeneratedRegex(@"([a-zA-Z0-9_* ]+) ([a-zA-Z0-9_]+)\((.*)\).*;")]
    private static partial Regex FuncPattern();
    #endregion

    public string Name;
    public CType ReturnType;
    public Dictionary<string, CType> Args;

    public CFunction(string name, CType returnType, Dictionary<string, CType> args)
    {
        Name = name;
        ReturnType = returnType;
        Args = args;
    }

    public static CFunction Parse(CTypeConverter typeConverter, string line)
    {
        Match match = FuncPattern().Match(line);

        if (!match.Success)
            return null;

        (CType retType, string _) = typeConverter.Convert(match.Groups[1].Value.Trim(), null);
        string name = match.Groups[2].Value.Trim();
        Dictionary<string, CType> args = [];

        foreach (Match arg in ArgsPattern().Matches(match.Groups[3].Value.Trim()))
        {
            (CType aType, string aName) = typeConverter.Convert(arg.Groups[1].Value.Trim(), arg.Groups[2].Value.Trim());
            args[aName] = aType;
        }

        return new CFunction(name, retType, args);
    }
}
