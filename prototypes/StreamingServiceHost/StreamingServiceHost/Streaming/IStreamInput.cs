namespace NGrid.Customer.Framework.StreamingServiceHost.Streaming;

public interface IStreamInput<T>
{
    Task<T?> GetOneItem(CancellationToken cancellationToken);
}

public interface IStreamInputProcessor<T> : IStreamInput<T>
{
    Task<StreamStatus> ProcessOneItem(Func<T, Task> next, CancellationToken cancellationToken);
}