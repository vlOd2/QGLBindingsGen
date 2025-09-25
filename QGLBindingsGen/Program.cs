using System.Text.Json;
using QGLBindingsGen.CParsing;

namespace QGLBindingsGen;

public static class Program
{
    private const string GLFW_HEADER_URL = "https://raw.githubusercontent.com/glfw/glfw/refs/heads/master/include/GLFW/glfw3.h";
    private const string GL_REGISTRY_URL = "https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/gl.xml";
    private const string AL_HEADER_URL = "https://raw.githubusercontent.com/kcat/openal-soft/refs/heads/master/include/AL/al.h";

    private static string[] GetOrCacheFile(string fileName, string url)
    {
        List<string> lines;
        if (!File.Exists(fileName))
        {
            StringReader reader = new(new HttpClient().GetStringAsync(url).GetAwaiter().GetResult());
            lines = [];
            string line;
            while ((line = reader.ReadLine()) != null)
                lines.Add(line);
            File.WriteAllLines(fileName, lines);
        }
        else
            lines = [.. File.ReadAllLines(fileName)];
        return [.. lines];
    }

    private static CParserContext ParseGLFWHeader()
    {
        string[] header = GetOrCacheFile("glfw3.h", GLFW_HEADER_URL);
        CParserContext ctx = new(["GLFWAPI"]);
        CParser.ParseFile(header, ctx);
        return ctx;
    }

    private static List<GLFeature> ParseGLRegistry(List<string> allowedFeatures, List<string> allowedExt)
    {
        string[] registry = GetOrCacheFile("gl.xml", GL_REGISTRY_URL);
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
        return GLRegistryParser.Parse(baseCtx, registry, allowedFeatures, allowedExt);
    }

    private static CParserContext ParseALHeader()
    {
        string[] header = GetOrCacheFile("al.h", AL_HEADER_URL);
        CParserContext ctx = new(["AL_APIENTRY", "AL_API_NOEXCEPT17", "AL_API_NOEXCEPT", "AL_API"]);
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
        CParser.ParseFile(header, ctx);
        return ctx;
    }

    public static void Main()
    {
        JsonSerializerOptions serializerOptions = new() { WriteIndented = true };

        Directory.CreateDirectory(Path.Combine("dump", "gl", "ext"));
        File.WriteAllText(Path.Combine("dump", "glfw.json"), JsonSerializer.Serialize(ParseGLFWHeader().GetDump(), serializerOptions));
        File.WriteAllText(Path.Combine("dump", "al.json"), JsonSerializer.Serialize(ParseALHeader().GetDump(), serializerOptions));

        List<GLFeature> glFeatures = ParseGLRegistry(null, null);
        foreach (GLFeature feature in glFeatures)
        {
            string fileName = $"{feature.Name.Replace(".", "_").Trim()}.json";
            string path = feature.IsExtension ? Path.Combine("dump", "gl", "ext", fileName) : Path.Combine("dump", "gl", fileName);
            File.WriteAllText(path, JsonSerializer.Serialize(feature.ParserContext.GetDump(), serializerOptions));
        }
    }
}
