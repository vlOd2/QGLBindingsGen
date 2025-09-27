using System.Collections.Concurrent;

namespace QGLBindingsGen.CParsing;

internal class CParserContext
{
    public CTypeConverter TypeConv { get; private set; }
    public readonly Dictionary<string, string> TypeMap = [];
    public readonly List<string> RemoveWords = [];
    public readonly ConcurrentBag<CConstant> Constants = [];
    public readonly ConcurrentBag<CDefinition> Definitions = [];
    public readonly ConcurrentBag<CStruct> Structs = [];
    public readonly ConcurrentBag<string> UnknownTypes = [];
    public readonly ConcurrentBag<CFunction> Functions = [];

    public CParserContext(List<string> removeWords = null)
    {
        TypeConv = new(this);
        if (removeWords != null)
            RemoveWords.AddRange(removeWords);
    }
}
