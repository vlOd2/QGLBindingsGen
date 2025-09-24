using System.Text.RegularExpressions;
using QGLBindingsGen.CParsing;

namespace QGLBindingsGen;

public partial class Program
{
    [GeneratedRegex(@"GLFWAPI ([a-zA-Z0-9_ *]+) (glfw[a-zA-Z0-9_]+)\((.*)\);")]
    private static partial Regex GLFWFuncPattern();

    static void Main(string[] args)
    {
        CParserContext ctx = new(GLFWFuncPattern());
        CParser.ParseFile(File.ReadAllLines("glfw3.h"), ctx);


    }
}
