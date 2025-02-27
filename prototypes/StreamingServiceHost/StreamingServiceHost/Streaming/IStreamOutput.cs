namespace NGrid.Customer.Framework.StreamingServiceHost.Streaming;

public interface IStreamOutput<in T>
{
    Task Handle(T itemToHandle);
}