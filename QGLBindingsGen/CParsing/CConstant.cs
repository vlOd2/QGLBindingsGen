using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal partial class CConstant
{
    #region Patterns
    [GeneratedRegex(@"#define ([a-zA-Z0-9_]+)\s+((?:0x[0-9]*)|(?:[\-0-9.]+)|(?:(?:\()?[a-zA-Z0-9| _]*(?:\))?))")]
    private static partial Regex MacroPattern();
    #endregion

    public string Name;
    public string Value;
    public CType CType;

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
        string value = match.Groups[2].Value.Trim();
        CType type = CTypeConverter.GetMacroLiteralType(value);

        if (type == null)
            return null;

        return new CConstant(name, value, type);
    }
}
