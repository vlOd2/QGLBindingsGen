using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal class CParserContext
{
    public Regex FuncPattern { get; private set; }
    public CTypeConverter TypeConv { get; private set; }
    public readonly List<CConstant> Constants = [];
    public readonly List<CDefinition> Definitions = [];
    public readonly List<CStruct> Structs = [];
    public readonly Dictionary<string, string> TypeMap = [];
    public readonly List<string> UnknownTypes = [];
    public readonly List<CFunction> Functions = [];

    public CParserContext(Regex funcPattern)
    {
        FuncPattern = funcPattern;
        TypeConv = new(this);
    }
}
