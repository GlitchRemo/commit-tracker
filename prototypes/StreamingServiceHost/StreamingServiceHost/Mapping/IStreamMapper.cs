namespace NGrid.Customer.Framework.StreamingServiceHost.Mapping;

public interface IStreamMapper
{
    TOut Map<TIn, TOut>(TIn item) where TOut : class;
}