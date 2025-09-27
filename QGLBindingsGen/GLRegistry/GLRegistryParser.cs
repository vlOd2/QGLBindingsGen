using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Xml;
using QGLBindingsGen.CParsing;

namespace QGLBindingsGen.GLRegistry;

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

    private static async Task<ConcurrentBag<CConstant>> GetEnums(XmlDocument root)
    {
        ConcurrentBag<CConstant> allEnums = [];

        await Parallel.ForEachAsync(root.GetElementsByTagName("enums").Cast<XmlElement>(), async (enums, _) =>
        {
            await Parallel.ForEachAsync(enums.GetElementsByTagName("enum").Cast<XmlElement>(), (enm, _) =>
            {
                string name = enm.GetAttribute("name").Trim();
                string rawValue = enm.GetAttribute("value").Trim();
                (CType type, string value) = CTypeConverter.ProcessConstant(rawValue);
                if (type == null)
                    return new();
                allEnums.Add(new CConstant(name, value, type));
                return new();
            });
        });

        return allEnums;
    }

    private static async Task<ConcurrentBag<CFunction>> GetCommands(CParserContext ctx, XmlDocument root)
    {
        ConcurrentBag<CFunction> funcs = [];

        await Parallel.ForEachAsync(root.GetElementsByTagName("commands").Cast<XmlElement>(), async (commands, _) =>
        {
            await Parallel.ForEachAsync(commands.GetElementsByTagName("command").Cast<XmlElement>(), (cmd, _) =>
            {
                XmlNode protoElem = cmd.SelectSingleNode("proto");
                if (protoElem == null)
                    return new();

                XmlNode nameElem = protoElem.SelectSingleNode("name");
                if (nameElem == null)
                    return new();

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
                return new();
            });
        });

        return funcs;
    }

    private static async Task<CParserContext> GetFeature(CParserContext baseCtx, XmlElement feature, 
        ConcurrentBag<CConstant> enums, ConcurrentBag<CFunction> commands)
    {
        CParserContext ctx = new(baseCtx.RemoveWords);
        foreach (KeyValuePair<string, string> kv in baseCtx.TypeMap)
            ctx.TypeMap[kv.Key] = kv.Value;

        await Parallel.ForEachAsync(feature.GetElementsByTagName("require").Cast<XmlElement>(), async (require, _) =>
        {
            await Parallel.ForEachAsync(require.GetElementsByTagName("enum").Cast<XmlElement>(), (enm, _) =>
            {
                string name = enm.GetAttribute("name").Trim();
                foreach (CConstant c in enums)
                {
                    if (c.Name == name)
                    {
                        if (ctx.CheckSymbol(c.Name))
                            ctx.Constants.Add(c);
                        break;
                    }
                }
                return new();
            });

            await Parallel.ForEachAsync(require.GetElementsByTagName("command").Cast<XmlElement>(), (cmd, _) =>
            {
                string name = cmd.GetAttribute("name").Trim();
                foreach (CFunction f in commands)
                {
                    if (f.Name == name)
                    {
                        if (ctx.CheckSymbol(f.Name))
                            ctx.Functions.Add(f);
                        break;
                    }
                }
                return new();
            });
        });

        return ctx;
    }

    public static async Task<List<GLFeature>> Parse(CParserContext baseCtx, string[] lines, List<string> allowedFeatures, List<string> allowedExt)
    {
        XmlDocument root = new();
        root.Load(new StringReader(string.Join('\n', lines)));

        ConcurrentBag<CConstant> constants = await TaskRunner.Run("Parsing enums", GetEnums(root));
        ConcurrentBag<CFunction> functions = await TaskRunner.Run("Parsing commands", GetCommands(baseCtx, root));
        List<GLFeature> features = [];

        await TaskRunner.Run("Parsing features", Parallel.ForEachAsync(
            root.GetElementsByTagName("feature").Cast<XmlElement>(), async (feature, _) =>
        {
            string name = feature.GetAttribute("name").Trim();
            string api = feature.GetAttribute("api").Trim();
            if (allowedFeatures != null && !allowedFeatures.Contains(name))
                return;
            CParserContext ctx = await GetFeature(baseCtx, feature, constants, functions);
            features.Add(new GLFeature(name, false, api.Contains("gles"), ctx));
        }));

        await TaskRunner.Run("Parsing extensions", Parallel.ForEachAsync(
            root.GetElementsByTagName("extension").Cast<XmlElement>(), async (extension, _) =>
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
                return;
            }

        passed:
            string[] supportedAPI = extension.GetAttribute("supported").Trim().Split('|');
            bool isEs = !supportedAPI.Contains("gl") && !supportedAPI.Contains("glcore");
            CParserContext ctx = await GetFeature(baseCtx, extension, constants, functions);
            features.Add(new GLFeature(name, true, isEs, ctx));
        }));

        return features;
    }
}
