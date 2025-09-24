using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal class CParserContext
{
    public CTypeConverter TypeConv { get; private set; }
    public readonly Dictionary<string, string> TypeMap = [];
    public readonly List<string> RemoveWords = [];
    public readonly List<CConstant> Constants = [];
    public readonly List<CDefinition> Definitions = [];
    public readonly List<CStruct> Structs = [];
    public readonly List<string> UnknownTypes = [];
    public readonly List<CFunction> Functions = [];

    public CParserContext(List<string> removeWords = null)
    {
        TypeConv = new(this);
        if (removeWords != null)
            RemoveWords.AddRange(removeWords);
    }
}
