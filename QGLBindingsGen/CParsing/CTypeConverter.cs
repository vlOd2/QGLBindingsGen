using System.Globalization;
using System.Text.RegularExpressions;

namespace QGLBindingsGen.CParsing;

internal partial class CTypeConverter
{
    #region Patterns
    [GeneratedRegex(@"\[\d+\]")]
    private partial Regex ArraySizeRemovalPattern();

    [GeneratedRegex(@"^.*\[(\d+)\]$")]
    private partial Regex ArraySizePattern();
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
    public static CType GetMacroLiteralType(string val)
    {
        if (val.StartsWith("0x"))
        {
            val = val[2..];
            if (int.TryParse(val, NumberStyles.HexNumber, null, out _))
                return new("int");
            if (uint.TryParse(val, NumberStyles.HexNumber, null, out _))
                return new("uint");
            if (long.TryParse(val, NumberStyles.HexNumber, null, out _))
                return new("long");
            if (ulong.TryParse(val, NumberStyles.HexNumber, null, out _))
                return new("ulong");
            return null;
        }

        if (int.TryParse(val, out _))
            return new("int");
        if (uint.TryParse(val, out _))
            return new("uint");
        if (long.TryParse(val, out _))
            return new("long");
        if (ulong.TryParse(val, out _))
            return new("ulong");
        if (float.TryParse(val, out _))
            return new("float");

        return null;
    }

    private static string SanitizeName(string name)
    {
        name = name.Trim();
        if (RESERVED_NAMES.Contains(name))
            return $"@{name}";
        return name;
    }

    public (CType, string) Convert(string cType, string argName)
    {
        cType = cType.Trim().Replace("const ", "");
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
        lock (ctx.Definitions)
        {
            foreach (CDefinition def in ctx.Definitions)
            {
                if (cType == def.Name)
                {
                    convertedType = cType;
                    break;
                }
            }
        }

        lock (ctx.Structs)
        {
            foreach (CStruct s in ctx.Structs)
            {
                if (cType == s.Name)
                {
                    convertedType = cType;
                    break;
                }
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
                Console.WriteLine($"Unknown type: {cType}");
            }
        }

        return (new CType(convertedType, ptrCount), argName);
    }
}
