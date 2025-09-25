using System.Text.RegularExpressions;
using System.Xml;
using QGLBindingsGen.CParsing;

namespace QGLBindingsGen;

internal static class GLRegistryParser
{
    private static string GetArgType(XmlNode node)
    {
        string type = node.Value ?? "";
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.Name == "name")
                continue;
            type += child.InnerText;
        }
        return type.Trim();
    }

    private static List<CConstant> GetEnums(XmlDocument root)
    {
        List<CConstant> allEnums = [];

        foreach (XmlElement enums in root.GetElementsByTagName("enums"))
        {
            foreach (XmlElement enm in enums.GetElementsByTagName("enum"))
            {
                string name = enm.GetAttribute("name").Trim();
                string value = enm.GetAttribute("value").Trim();
                CType type = CTypeConverter.GetMacroLiteralType(value);
                if (type == null)
                    continue;
                allEnums.Add(new CConstant(name, value, type));
            }
        }

        return allEnums;
    }

    private static List<CFunction> GetCommands(CParserContext ctx, XmlDocument root)
    {
        List<CFunction> funcs = [];

        foreach (XmlElement commands in root.GetElementsByTagName("commands"))
        {
            foreach (XmlElement cmd in commands.GetElementsByTagName("command"))
            {
                XmlNode protoElem = cmd.SelectSingleNode("proto");
                if (protoElem == null)
                    continue;

                XmlNode nameElem = protoElem.SelectSingleNode("name");
                if (nameElem == null)
                    continue;

                string args = "";
                foreach (XmlElement arg in cmd.GetElementsByTagName("param"))
                {
                    XmlNode aNameElem = arg.SelectSingleNode("name");
                    if (aNameElem == null)
                        continue;
                    args += $"{GetArgType(arg)} {aNameElem.InnerText.Trim()}, ";
                }
                if (args.Length != 0)
                    args = args[..^2];

                string name = nameElem.InnerText.Trim();
                string retType = GetArgType(protoElem);
                funcs.Add(new CFunction(ctx, name, retType, args));
            }
        }

        return funcs;
    }

    private static CParserContext GetFeature(CParserContext baseCtx, XmlElement feature, List<CConstant> enums, List<CFunction> commands)
    {
        CParserContext ctx = new(baseCtx.RemoveWords);
        foreach (KeyValuePair<string, string> kv in baseCtx.TypeMap)
            ctx.TypeMap[kv.Key] = kv.Value;

        foreach (XmlElement require in feature.GetElementsByTagName("require"))
        {
            foreach (XmlElement enm in require.GetElementsByTagName("enum"))
            {
                string name = enm.GetAttribute("name").Trim();
                foreach (CConstant c in enums)
                {
                    if (c.Name == name)
                    {
                        ctx.Constants.Add(c);
                        break;
                    }
                }
            }

            foreach (XmlElement cmd in require.GetElementsByTagName("command"))
            {
                string name = cmd.GetAttribute("name").Trim();
                foreach (CFunction f in commands)
                {
                    if (f.Name == name)
                    {
                        ctx.Functions.Add(f);
                        break;
                    }
                }
            }
        }

        return ctx;
    }

    public static List<GLFeature> Parse(CParserContext baseCtx, string[] lines, List<string> allowedFeatures, List<string> allowedExt)
    {
        XmlDocument root = new();
        root.Load(new StringReader(string.Join('\n', lines)));

        List<CConstant> constants = GetEnums(root);
        List<CFunction> functions = GetCommands(baseCtx, root);
        List<GLFeature> features = [];

        foreach (XmlElement feature in root.GetElementsByTagName("feature"))
        {
            string name = feature.GetAttribute("name").Trim();
            if (allowedFeatures != null && !allowedFeatures.Contains(name))
                continue;
            CParserContext ctx = GetFeature(baseCtx, feature, constants, functions);
            features.Add(new GLFeature(name, false, ctx));
        }

        foreach (XmlElement extension in root.GetElementsByTagName("extension"))
        {
            string name = extension.GetAttribute("name").Trim();
            if (allowedExt != null && !allowedExt.Contains(name))
            {
                foreach (string pattern in allowedExt)
                {
                    if (!pattern.StartsWith("@/"))
                        continue;
                    if (Regex.IsMatch(name, pattern[2..]))
                        goto passed;
                }
                continue;
            }
        passed: CParserContext ctx = GetFeature(baseCtx, extension, constants, functions);
            features.Add(new GLFeature(name, true, ctx));
        }

        return features;
    }
}
