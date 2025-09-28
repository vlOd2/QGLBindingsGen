using System.Globalization;
using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal partial class CTypeConverter
{
    #region Patterns
    [GeneratedRegex(@"\[\d+\]")]
    private static partial Regex ArraySizeRemovalPattern();

    [GeneratedRegex(@"^.*\[(\d+)\]$")]
    private static partial Regex ArraySizePattern();

    [GeneratedRegex(@"\b(const|volatile|restrict|inline)\b")]
    private static partial Regex QualifierPattern();
    #endregion
    private CParserContext ctx;

    public CTypeConverter(CParserContext ctx)
    {
        this.ctx = ctx;
    }

    private static readonly List<string> RESERVED_NAMES =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class",
        "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
        "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach",
        "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
        "namespace", "new", "null", "object", "operator", "out", "override", "params", "private",
        "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof",
        "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
        "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    ];

    // doesn't handle octal numbers, but those are almost never used anyway
    public static (CType type, string value) ProcessConstant(string s)
    {
        ulong value;
        long sValue = 0;
        bool useSigned = false;

        if (s.StartsWith("0x"))
        {
            s = s[2..];
            if (!ulong.TryParse(s, NumberStyles.HexNumber, null, out value))
                return (null, null);
        }
        else if (!ulong.TryParse(s, null, out value))
        {
            if (!long.TryParse(s, null, out sValue))
            {
                if (float.TryParse(s, out _))
                    return (new("float"), s);
                return (null, null);
            }
            useSigned = true;
        }

        if (useSigned)
        {
            if (sValue < 0)
            {
                if (sValue < int.MinValue)
                    return (new("long"), sValue.ToString());
                return (new("int"), sValue.ToString());
            }
            Logger.Warn($"Parsed as signed value when unsigned was possible: {s}");
            return (null, null);
        }

        if (value > 0x7FFFFFFF_FFFFFFFF)
            return (new("ulong"), $"0x{value:X16}");
        else if (value > 0xFFFFFFFF)
            return (new("long"), $"0x{(long)value:X8}");
        else if (value > 0x7FFFFFFF)
            return (new("uint"), $"0x{(uint)value:X8}");
        return (new("int"), $"0x{(int)value:X8}");
    }

    private static string SanitizeName(string name)
    {
        name = name.Trim();
        if (RESERVED_NAMES.Contains(name))
            return $"@{name}";
        return name;
    }

    public (CType, string) Convert(string cType, string argName, bool convertCallbacks)
    {
        cType = QualifierPattern().Replace(cType, "").Trim();
        int ptrCount = cType.Where(c => c == '*').Count();
        cType = cType.Replace("*", "").Trim();

        if (argName != null)
        {
            while (argName.StartsWith('*'))
            {
                argName = argName[1..].Trim();
                ptrCount++;
            }
            if (argName.EndsWith("[]"))
            {
                ptrCount++;
                argName = argName[..^2].Trim();
            }
            else
            {
                Match match = ArraySizePattern().Match(argName);
                if (match.Success)
                {
                    ptrCount++;
                    argName = ArraySizeRemovalPattern().Replace(argName, "").Trim();
                }
            }
            argName = SanitizeName(argName);
        }

        string convertedType = null;
        foreach (CDefinition def in ctx.Definitions)
        {
            if (cType == def.Name)
            {
                if (def.Callback != null && convertCallbacks)
                    convertedType = "nint";
                else
                    convertedType = cType;
                break;
            }
        }

        foreach (CStruct s in ctx.Structs)
        {
            if (cType == s.Name)
            {
                convertedType = cType;
                break;
            }
        }

        convertedType ??= cType switch
        {
            "void" => "void",
            "char" => "byte",
            "short" => "short",
            "int" => "int",
            "long" => "nint",
            "long long" => "long",
            "unsigned char" => "byte",
            "unsigned short" => "ushort",
            "unsigned int" => "uint",
            "unsigned long" => "nuint",
            "unsigned long long" => "ulong",
            "float" => "float",
            "double" => "double",
            "size_t" => "nuint",
            "int8_t" => "sbyte",
            "int16_t" => "short",
            "int32_t" => "int",
            "int64_t" => "long",
            "uint8_t" => "byte",
            "uint16_t" => "ushort",
            "uint32_t" => "uint",
            "uint64_t" => "ulong",
            "intptr_t" => "nint",
            "uintptr_t" => "nuint",
            _ => null
        };
        convertedType ??= ctx.TypeMap.GetValueOrDefault(cType);

        if (convertedType == null)
        {
            convertedType = "nint";
            ptrCount = 0;
            if (!ctx.UnknownTypes.Contains(cType))
            {
                ctx.UnknownTypes.Add(cType);
                Logger.Warn($"Unknown type: {cType}");
            }
        }

        return (new CType(convertedType, ptrCount), argName?.Trim());
    }
}
