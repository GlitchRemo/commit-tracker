using Avro.Specific;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using NGrid.Customer.Framework.EventStreaming.Producer;
using NGrid.Customer.Framework.StreamingServiceHost.Streaming;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NGrid.Customer.Framework.FunctionalTests.Base.Streaming;

public class ProducerOutput : IStreamOutput<Message<string,ISpecificRecord>>
{
    private readonly ILogger<ProducerOutput> _logger;
    private readonly IEventProducer<string, ISpecificRecord> _producer;

    public ProducerOutput(IEventProducerFactory eventProducerFactory, ILogger<ProducerOutput> logger)
    {
        _logger = logger;
        _producer = eventProducerFactory.CreateEventProducer<string, ISpecificRecord>();
    }

    private ILogger Logger => _logger;

    public virtual async Task Handle(Message<string,ISpecificRecord> itemToHandle)
    {
        Logger.LogInformation("Enter");
        await _producer.ProduceAsync("topic",
            new SpecificMessage
            {
                Key = itemToHandle.Key,
                Timestamp = Timestamp.Default,
                Value = itemToHandle.Value
            });
        Logger.LogInformation("Exit");
    }
}