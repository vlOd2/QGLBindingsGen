namespace QGLBindingsGen.CParsing;

internal class CParserContext
{
    public CTypeConverter TypeConv { get; private set; }
    public readonly Dictionary<string, string> TypeMap = [];
    public readonly List<string> RemoveWords = [];
    public readonly SynchronizedCollection<CConstant> Constants = [];
    public readonly SynchronizedCollection<CDefinition> Definitions = [];
    public readonly SynchronizedCollection<CStruct> Structs = [];
    public readonly SynchronizedCollection<string> UnknownTypes = [];
    public readonly SynchronizedCollection<CFunction> Functions = [];

    public CParserContext(List<string> removeWords = null)
    {
        TypeConv = new(this);
        if (removeWords != null)
            RemoveWords.AddRange(removeWords);
    }
}
