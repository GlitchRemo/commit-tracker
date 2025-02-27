using AutoMapper;

namespace NGrid.Customer.Framework.StreamingServiceHost.Mapping;

public class StreamMapper : IStreamMapper
{
    private readonly IMapper? _mapper;

    public StreamMapper(IMapper? mapper)
    {
        _mapper = mapper;
    }
    public TOut Map<TIn, TOut>(TIn item) where TOut : class
    {
        if (typeof(TOut).IsAssignableTo(typeof(TIn)))
        {
            return (item as TOut)!;
        }

        var mappedItem = _mapper?.Map<TOut>(item);
        if (mappedItem != null) return mappedItem;

        throw new MappingException(typeof(TIn), typeof(TOut));
    }
}