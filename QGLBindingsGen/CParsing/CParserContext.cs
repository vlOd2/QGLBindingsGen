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
    private readonly List<string> declaredSymbols = [];

    public CParserContext(List<string> removeWords = null)
    {
        TypeConv = new(this);
        if (removeWords != null)
            RemoveWords.AddRange(removeWords);
    }

    public bool CheckSymbol(string name)
    {
        lock (declaredSymbols)
        {
            if (declaredSymbols.Contains(name))
            {
                Logger.Warn($"Symbol \"{name}\" is already declared");
                return false;
            }
            declaredSymbols.Add(name);
            return true;
        }
    }

    public void RemoveSymbol(string name) => declaredSymbols.Remove(name);
}
