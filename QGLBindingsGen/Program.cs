using System.Text.Json;
using System.Text.RegularExpressions;
using QGLBindingsGen.CParsing;

namespace QGLBindingsGen;

public partial class Program
{
    [GeneratedRegex(@"GLFWAPI ([a-zA-Z0-9_ *]+) (glfw[a-zA-Z0-9_]+)\((.*)\);")]
    private static partial Regex GLFWFuncPattern();

    [GeneratedRegex(@"_UI_EXTERN ([a-zA-Z0-9_ *]+) (ui[a-zA-Z0-9_]+)\((.*)\);")]
    private static partial Regex LibUIFuncPattern();

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

    private static void DumpHeader(string name, Regex funcPattern)
    {
        CParserContext ctx = new(funcPattern);
        CParser.ParseFile(File.ReadAllLines($"{name}.h"), ctx);
        using FileStream dumpStream = File.Open($"{name}-dump.json", FileMode.Create);
        JsonSerializer.Serialize(dumpStream, DumpContext(ctx), new JsonSerializerOptions()
        {
            WriteIndented = true
        });
    }

    static void Main()
    {
        DumpHeader("glfw3", GLFWFuncPattern());
        DumpHeader("libui", LibUIFuncPattern());
    }
}
