using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal class CParserContext
{
    public Regex FuncPattern { get; private set; }
    public CTypeConverter TypeConv { get; private set; }

    public CParserContext(Regex funcPattern)
    {
        FuncPattern = funcPattern;
        TypeConv = new();
    }
}
