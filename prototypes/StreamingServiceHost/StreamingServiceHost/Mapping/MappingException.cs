namespace NGrid.Customer.Framework.StreamingServiceHost.Mapping;

public class MappingException : Exception
{
    private readonly Type _inType;
    private readonly Type _outType;

    public MappingException(Type inType, Type outType)
    {
        _inType = inType;
        _outType = outType;
    }

    public override string ToString()
    {
        return $"Mapping from type '{_inType.Name}' to type '{_outType.Name}' is undefined";
    }
}