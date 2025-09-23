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
    private readonly List<string> opaqueStructs = [];
    private readonly List<string> dataStructs = [];
    private readonly Dictionary<string, string> typeMap = [];
    private readonly List<string> unknownTypes = [];

    public void AddOpaqueStruct(string name) => opaqueStructs.Add(name);

    public void AddDataStruct(string name) => dataStructs.Add(name);

    public void AddTypeMap(string cType, string nType) => typeMap[cType] = nType;

    public static CType GetMacroLiteralType(string val)
    {
        if (int.TryParse(val, out _))
            return new CType("int");
        if (uint.TryParse(val, out _))
            return new CType("uint");
        if (long.TryParse(val, out _))
            return new CType("long");
        if (ulong.TryParse(val, out _))
            return new CType("ulong");
        if (float.TryParse(val, out _))
            return new CType("float");
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
        if (opaqueStructs.Contains(cType))
        {
            convertedType = "nint";
            ptrCount--;
            if (ptrCount < 0)
                ptrCount = 0;
        }

        if (dataStructs.Contains(cType))
            convertedType = cType;

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
            _ => null
        };
        convertedType ??= typeMap.GetValueOrDefault(cType);

        if (convertedType == null)
        {
            convertedType = "nint";
            ptrCount = 0;
            if (!unknownTypes.Contains(cType))
            {
                unknownTypes.Add(cType);
                Console.WriteLine($"Unknown type: {cType}");
            }
        }

        return (new CType(convertedType, ptrCount), argName);
    }
}
