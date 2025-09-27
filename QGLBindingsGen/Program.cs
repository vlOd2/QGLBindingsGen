using QGLBindingsGen.CParsing;
using QGLBindingsGen.GLRegistry;

namespace QGLBindingsGen;

public static class Program
{
    private const string OUT_DIR = "BindingsOutput";
    private const string BINDINGS_NAMESPACE = "QuickGLNS.Bindings";
    private const string GLFW_HEADER_URL = "https://raw.githubusercontent.com/glfw/glfw/refs/heads/master/include/GLFW/glfw3.h";
    private const string GL_REGISTRY_URL = "https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/gl.xml";
    private const string AL_HEADER_URL = "https://raw.githubusercontent.com/kcat/openal-soft/refs/heads/master/include/AL/al.h";

    private static async Task<string[]> GetOrCacheFile(string fileName, string url)
    {
        List<string> lines;
        if (!File.Exists(fileName))
        {
            StringReader reader = new(await new HttpClient().GetStringAsync(url));
            lines = [];
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
                lines.Add(line);
            await File.WriteAllLinesAsync(fileName, lines);
        }
        else
            lines = [.. await File.ReadAllLinesAsync(fileName)];
        return [.. lines];
    }

    private static async Task<CParserContext> ParseGLFWHeader()
    {
        string[] header = await ConsoleUtils.RunTask("Downloading GLFW header", GetOrCacheFile("glfw3.h", GLFW_HEADER_URL));
        CParserContext ctx = new(["GLFWAPI", "APIENTRY", "WINGDIAPI", "CALLBACK"]);
        await CParser.ParseFile(header, ctx);
        return ctx;
    }

    private static async Task<List<GLFeature>> ParseGLRegistry(List<string> allowedFeatures, List<string> allowedExt)
    {
        string[] registry = await GetOrCacheFile("gl.xml", GL_REGISTRY_URL);
        CParserContext baseCtx = new();
        baseCtx.TypeMap.Add("GLenum", "uint");
        baseCtx.TypeMap.Add("GLboolean", "bool");
        baseCtx.TypeMap.Add("GLbitfield", "uint");
        baseCtx.TypeMap.Add("GLvoid", "void");
        baseCtx.TypeMap.Add("GLbyte", "sbyte");
        baseCtx.TypeMap.Add("GLubyte", "byte");
        baseCtx.TypeMap.Add("GLshort", "short");
        baseCtx.TypeMap.Add("GLushort", "ushort");
        baseCtx.TypeMap.Add("GLint", "int");
        baseCtx.TypeMap.Add("GLuint", "uint");
        baseCtx.TypeMap.Add("GLclampx", "int");
        baseCtx.TypeMap.Add("GLsizei", "int");
        baseCtx.TypeMap.Add("GLfloat", "float");
        baseCtx.TypeMap.Add("GLclampf", "float");
        baseCtx.TypeMap.Add("GLdouble", "double");
        baseCtx.TypeMap.Add("GLclampd", "double");
        baseCtx.TypeMap.Add("GLchar", "byte");
        baseCtx.TypeMap.Add("GLcharARB", "byte");
        baseCtx.TypeMap.Add("GLhalf", "ushort");
        baseCtx.TypeMap.Add("GLhalfARB", "ushort");
        baseCtx.TypeMap.Add("GLfixed", "int");
        baseCtx.TypeMap.Add("GLintptr", "nint");
        baseCtx.TypeMap.Add("GLintptrARB", "nint");
        baseCtx.TypeMap.Add("GLsizeiptr", "nint");
        baseCtx.TypeMap.Add("GLsizeiptrARB", "nint");
        baseCtx.TypeMap.Add("GLint64", "long");
        baseCtx.TypeMap.Add("GLint64EXT", "long");
        baseCtx.TypeMap.Add("GLuint64", "ulong");
        baseCtx.TypeMap.Add("GLuint64EXT", "ulong");
        baseCtx.TypeMap.Add("GLhalfNV", "ushort");
        baseCtx.TypeMap.Add("GLsync", "nint");
        baseCtx.TypeMap.Add("GLvdpauSurfaceNV", "nint");
        return await GLRegistryParser.Parse(baseCtx, registry, allowedFeatures, allowedExt);
    }

    private static async Task<CParserContext> ParseALHeader()
    {
        string[] header = await GetOrCacheFile("al.h", AL_HEADER_URL);
        CParserContext ctx = new(["AL_APIENTRY", "AL_API_NOEXCEPT17", "AL_API_NOEXCEPT", "AL_API", "AL_CPLUSPLUS"]);
        ctx.TypeMap.Add("ALboolean", "byte");
        ctx.TypeMap.Add("ALchar", "byte");
        ctx.TypeMap.Add("ALbyte", "sbyte");
        ctx.TypeMap.Add("ALubyte", "byte");
        ctx.TypeMap.Add("ALshort", "short");
        ctx.TypeMap.Add("ALushort", "ushort");
        ctx.TypeMap.Add("ALint", "int");
        ctx.TypeMap.Add("ALuint", "uint");
        ctx.TypeMap.Add("ALsizei", "int");
        ctx.TypeMap.Add("ALenum", "int");
        ctx.TypeMap.Add("ALfloat", "float");
        ctx.TypeMap.Add("ALdouble", "double");
        ctx.TypeMap.Add("ALvoid", "void");
        await CParser.ParseFile(header, ctx);
        return ctx;
    }

    private static async Task MainAsync()
    {
        Directory.CreateDirectory(OUT_DIR);

        Console.WriteLine("- Generating bindings for GLFW");
        await File.WriteAllTextAsync(Path.Combine(OUT_DIR, "GLFW.cs"), 
            Generator.Generate(await ParseGLFWHeader(), "GLFW", BINDINGS_NAMESPACE, "QuickGL.GetGLFWProcAddress"));

        Console.WriteLine("- Generating bindings for OpenGL");
        List<GLFeature> features = await ParseGLRegistry(null, [@"@/GL_ARB_.*"]);
        if (features.Any(feature => feature.IsExtension))
            Directory.CreateDirectory(Path.Combine(OUT_DIR, "Extensions"));

        await Parallel.ForEachAsync(features, async (feature, token) =>
        {
            string classData = Generator.GenerateGLFeature(feature, BINDINGS_NAMESPACE);
            string outDir = OUT_DIR;
            if (feature.IsExtension)
                outDir = Path.Combine(OUT_DIR, "Extensions");
            await File.WriteAllTextAsync(Path.Combine(outDir, $"{Generator.GetGLFeatureClassName(feature)}.cs"), classData, token);
        });
    }

    public static void Main()
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            MainAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            ConsoleUtils.WriteError(ex);
        }
    }
}
