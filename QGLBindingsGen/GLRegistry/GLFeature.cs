using QGLBindingsGen.CParsing;

namespace QGLBindingsGen.GLRegistry;

internal class GLFeature
{
    public string Name;
    public bool IsExtension;
    public CParserContext ParserContext;

    public GLFeature(string name, bool isExtension, CParserContext ctx)
    {
        Name = name;
        IsExtension = isExtension;
        ParserContext = ctx;
    }
}
