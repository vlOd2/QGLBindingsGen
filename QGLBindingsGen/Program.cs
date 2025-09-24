using System.Text.RegularExpressions;
using QGLBindingsGen.CParsing;

namespace QGLBindingsGen;

public partial class Program
{
    [GeneratedRegex(@"GLFWAPI ([a-zA-Z0-9_ *]+) (glfw[a-zA-Z0-9_]+)\((.*)\);")]
    private static partial Regex GLFWFuncPattern();

    static void Main(string[] args)
    {
        string[] headerLines = File.ReadAllLines("glfw3.h");
        string headerData = string.Join("\n", headerLines);
        CParserContext ctx = new(GLFWFuncPattern());

        foreach (string line in headerLines)
        {
            CFunction func = CFunction.Parse(ctx, line);
            if (func != null)
            {
                Console.WriteLine($"Func {func.Name} -> {func.ReturnType}");
                foreach ((string aName, CType aType) in func.Args)
                    Console.WriteLine($"    {aName}: {aType}");
                continue;
            }

            CDefinition def = CDefinition.Parse(ctx, line);
            if (def != null)
            {
                if (def.Callback != null)
                {
                    Console.WriteLine($"Callback {def.Name} -> {def.Callback.ReturnType}");
                    foreach ((string aName, CType aType) in def.Callback.Args)
                        Console.WriteLine($"    {aName}: {aType}");
                }
                else
                {
                    Console.WriteLine($"Opaque struct {def.Name}");
                }
            }
        }

        CStruct[] structs = CStruct.ParseAll(ctx, headerData);
        foreach (CStruct s in structs)
        {
            Console.WriteLine($"Struct {s.Name}");
            foreach ((string fName, CType fType) in s.Fields)
                Console.WriteLine($"    {fName}: {fType}");
        }
    }
}
