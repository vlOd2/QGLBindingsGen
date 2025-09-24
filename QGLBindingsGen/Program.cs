using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using QGLBindingsGen.CParsing;

namespace QGLBindingsGen;

public partial class Program
{
    [GeneratedRegex(@"GLFWAPI ([a-zA-Z0-9_ *]+) (glfw[a-zA-Z0-9_]+)\((.*)\);")]
    private static partial Regex GLFWFuncPattern();

    static void Main()
    {
        CParserContext ctx = new(GLFWFuncPattern());
        CParser.ParseFile(File.ReadAllLines("glfw3.h"), ctx);

        Dictionary<string, object> dump = [];
        Dictionary<string, object> consts = [];
        Dictionary<string, object> defs = [];
        Dictionary<string, object> structs = [];
        Dictionary<string, object> funcs = [];

        foreach (CConstant cconst in ctx.Constants)
            consts[cconst.Name] = cconst.Value;

        foreach (CDefinition def in ctx.Definitions)
        {
            Dictionary<string, object> d = [];
            d["type"] = def.Callback == null ? "opaque_struct" : "callback";
            if (def.Callback != null)
            {
                d["rettype"] = def.Callback.ReturnType;
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
            f["rettype"] = func.ReturnType;
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
        
        using FileStream dumpStream = File.Open("dump.json", FileMode.Create);
        JsonSerializer.Serialize(dumpStream, dump, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
    }
}
