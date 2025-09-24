using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal class CFunction
{
    public string Name;
    public CType ReturnType;
    public Dictionary<string, CType> Args;

    public CFunction(CParserContext ctx, string name, string returnType, string args)
    {
        Name = name;

        (ReturnType, _) = ctx.TypeConv.Convert(returnType, null);
        Args = [];

        foreach (Match arg in CParser.ArgsPattern().Matches(args))
        {
            string rawType = arg.Groups[1].Value.Trim();
            string rawName = arg.Groups[2].Value.Trim();
            (CType type, string aName) = ctx.TypeConv.Convert(rawType, rawName);
            Args[aName] = type;
        }
    }

    public static CFunction Parse(CParserContext ctx, string line)
    {
        Match match = ctx.FuncPattern.Match(line);

        if (!match.Success)
            return null;

        string name = match.Groups[2].Value.Trim();
        string returnType = match.Groups[1].Value.Trim();
        string args = match.Groups[3].Value.Trim();

        return new CFunction(ctx, name, returnType, args);
    }
}
