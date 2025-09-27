using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal partial class CConstant
{
    #region Patterns
    [GeneratedRegex(@"#define ([a-zA-Z0-9_]+)\s+((?:0x[0-9a-zA-Z]*)|(?:[\-0-9.]+)|(?:(?:\()?[\-a-zA-Z0-9| _]*(?:\))?))")]
    private static partial Regex MacroPattern();
    #endregion

    public string Name;
    public string Value;
    public CType CType;
    public bool IsExpression => CType == null;

    public CConstant(string name, string value, CType type)
    {
        Name = name;
        Value = value;
        CType = type;
    }

    public static CConstant Parse(string line)
    {
        Match match = MacroPattern().Match(line);

        if (!match.Success)
            return null;

        string name = match.Groups[1].Value.Trim();
        string rawValue = match.Groups[2].Value.Trim();
        (CType type, string value) = CTypeConverter.ProcessConstant(rawValue);

        if (type == null)
            return new CConstant(name, rawValue, new CType("int")); // I am hoping this is large enough for most

        return new CConstant(name, value, type);
    }
}
