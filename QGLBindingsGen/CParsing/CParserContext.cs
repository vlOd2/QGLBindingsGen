using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal class CParserContext
{
    public CTypeConverter TypeConv { get; private set; }
    public readonly Dictionary<string, string> TypeMap = [];
    public readonly List<string> RemoveWords = [];
    public readonly List<CConstant> Constants = [];
    public readonly List<CDefinition> Definitions = [];
    public readonly List<CStruct> Structs = [];
    public readonly List<string> UnknownTypes = [];
    public readonly List<CFunction> Functions = [];

    public CParserContext(List<string> removeWords = null)
    {
        TypeConv = new(this);
        if (removeWords != null)
            RemoveWords.AddRange(removeWords);
    }

    public Dictionary<string, object> GetDump()
    {
        Dictionary<string, object> dump = [];
        Dictionary<string, object> consts = [];
        Dictionary<string, object> defs = [];
        Dictionary<string, object> structs = [];
        Dictionary<string, object> funcs = [];

        foreach (CConstant cconst in Constants)
        {
            Dictionary<string, string> c = [];
            c["value"] = cconst.Value;
            c["type"] = cconst.CType.ToString();
            consts[cconst.Name] = c;
        }

        foreach (CDefinition def in Definitions)
        {
            Dictionary<string, object> d = [];
            d["type"] = def.Callback == null ? "opaque_struct" : "callback";
            if (def.Callback != null)
            {
                d["return_type"] = def.Callback.ReturnType.ToString();
                Dictionary<string, string> args = [];
                foreach (KeyValuePair<string, CType> arg in def.Callback.Args)
                    args[arg.Key] = arg.Value.ToString();
                d["args"] = args;
            }
            defs[def.Name] = d;
        }

        foreach (CStruct s in Structs)
        {
            Dictionary<string, string> fields = [];
            foreach (KeyValuePair<string, CType> field in s.Fields)
                fields[field.Key] = field.Value.ToString();
            structs[s.Name] = fields;
        }

        foreach (CFunction func in Functions)
        {
            Dictionary<string, object> f = [];
            f["return_type"] = func.ReturnType.ToString();
            Dictionary<string, string> args = [];
            foreach (KeyValuePair<string, CType> arg in func.Args)
                args[arg.Key] = arg.Value.ToString();
            f["args"] = args;
            funcs[func.Name] = f;
        }

        dump["constants"] = consts;
        dump["definitions"] = defs;
        dump["structs"] = structs;
        dump["functions"] = funcs;
        dump["unknown_types"] = UnknownTypes;

        return dump;
    }
}
