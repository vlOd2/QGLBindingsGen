using QGLBindingsGen.CParsing;

namespace QGLBindingsGen;

public class Program
{
    static void Main(string[] args)
    {
        string[] headerLines = File.ReadAllLines("glfw3.h");
        string headerData = string.Join("\n", headerLines);
        CTypeConverter converter = new();

        foreach (string line in headerLines)
        {
            CFunction func = CFunction.Parse(converter, line);
            if (func != null)
            {
                Console.WriteLine($"{func.Name} -> {func.ReturnType}");
                foreach ((string aName, CType aType) in func.Args)
                    Console.WriteLine($"    {aName}: {aType}");
            }
        }
    }
}
