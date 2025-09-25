using System.Text.Json;
using System.Text.Json.Serialization;
using QGLBindingsGen.CParsing;

namespace QGLBindingsGen;

public static class Program
{
    private const string GLFW_HEADER_URL = "https://raw.githubusercontent.com/glfw/glfw/refs/heads/master/include/GLFW/glfw3.h";
    private const string GL_REGISTRY_URL = "https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/gl.xml";
    private const string AL_HEADER_URL = "https://raw.githubusercontent.com/kcat/openal-soft/refs/heads/master/include/AL/al.h";

    private static Dictionary<string, object> DumpContext(CParserContext ctx)
    {
        Dictionary<string, object> dump = [];
        Dictionary<string, object> consts = [];
        Dictionary<string, object> defs = [];
        Dictionary<string, object> structs = [];
        Dictionary<string, object> funcs = [];

        foreach (CConstant cconst in ctx.Constants)
        {
            Dictionary<string, string> c = [];
            c["value"] = cconst.Value;
            c["type"] = cconst.CType.ToString();
            consts[cconst.Name] = c;
        }

        foreach (CDefinition def in ctx.Definitions)
        {
            Dictionary<string, object> d = [];
            d["type"] = def.Callback == null ? "opaque_struct" : "callback";
            if (def.Callback != null)
            {
                d["rettype"] = def.Callback.ReturnType.ToString();
                Dictionary<string, string> args = [];
                foreach (KeyValuePair<string, CType> arg in def.Callback.Args)
                    args[arg.Key] = arg.Value.ToString();
                d["args"] = args;
            }
            defs[def.Name] = d;
        }

        foreach (CStruct s in ctx.Structs)
        {
            Dictionary<string, string> fields = [];
            foreach (KeyValuePair<string, CType> field in s.Fields)
                fields[field.Key] = field.Value.ToString();
            structs[s.Name] = fields;
        }

        foreach (CFunction func in ctx.Functions)
        {
            Dictionary<string, object> f = [];
            f["rettype"] = func.ReturnType.ToString();
            Dictionary<string, string> args = [];
            foreach (KeyValuePair<string, CType> arg in func.Args)
                args[arg.Key] = arg.Value.ToString();
            f["args"] = args;
            funcs[func.Name] = f;
        }

        dump["consts"] = consts;
        dump["defs"] = defs;
        dump["structs"] = structs;
        dump["funcs"] = funcs;
        dump["unknownTypes"] = ctx.UnknownTypes;

        return dump;
    }

    private static void __OpenAL()
    {
        // CParserContext alCtx = new(["AL_APIENTRY", "AL_API_NOEXCEPT17", "AL_API_NOEXCEPT", "AL_API"]);
        // alCtx.TypeMap.Add("ALboolean", "byte");
        // alCtx.TypeMap.Add("ALchar", "byte");
        // alCtx.TypeMap.Add("ALbyte", "sbyte");
        // alCtx.TypeMap.Add("ALubyte", "byte");
        // alCtx.TypeMap.Add("ALshort", "short");
        // alCtx.TypeMap.Add("ALushort", "ushort");
        // alCtx.TypeMap.Add("ALint", "int");
        // alCtx.TypeMap.Add("ALuint", "uint");
        // alCtx.TypeMap.Add("ALsizei", "int");
        // alCtx.TypeMap.Add("ALenum", "int");
        // alCtx.TypeMap.Add("ALfloat", "float");
        // alCtx.TypeMap.Add("ALdouble", "double");
        // alCtx.TypeMap.Add("ALvoid", "void");
    }

    private static string[] GetOrCacheHeader(string fileName, string url)
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

    public static void Main()
    {
        string[] glfw = GetOrCacheHeader("glfw3.h", GLFW_HEADER_URL);
        CParserContext ctx = new(["GLFWAPI"]);
        CParser.ParseFile(glfw, ctx);
        File.WriteAllText("glfw3.json", JsonSerializer.Serialize(
            DumpContext(ctx), new JsonSerializerOptions() { WriteIndented = true }));
    }
}
