namespace QGLBindingsGen.CParsing;

internal class CType
{
    public string Name;
    public int PointerCount;

    public CType(string name, int pointerCount = 0)
    {
        Name = name;
        PointerCount = pointerCount;
    }

    public override string ToString() => Name + "".PadRight(PointerCount, '*');
}
