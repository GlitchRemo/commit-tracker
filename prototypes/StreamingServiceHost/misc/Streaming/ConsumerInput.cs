using Avro.Specific;
using Microsoft.Extensions.Logging;
using NGrid.Customer.Framework.EventStreaming.Consumer;
using NGrid.Customer.Framework.StreamingServiceHost.Streaming;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NGrid.Customer.Framework.FunctionalTests.Base.Streaming;

public class ConsumerInput : IStreamInputProcessor<SpecificMessage>
{
    private readonly ILogger<ConsumerInput> _logger;
    private readonly IEventConsumer<string, ISpecificRecord> _consumer;

    public ConsumerInput(IEventConsumerFactory eventConsumerFactory, ILogger<ConsumerInput> logger)
    {
        _logger = logger;
        _consumer = eventConsumerFactory.CreateEventConsumer<string, ISpecificRecord>();
    }

    private ILogger Logger => _logger;

    public virtual async Task<StreamStatus> ProcessOneItem(Func<SpecificMessage, Task> next, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Enter");
        var message = await GetOneItem(cancellationToken);

        if (message == null)
        {
            Logger.LogInformation("Break");
            return StreamStatus.Finished;
        }

        await next(message);
        Logger.LogInformation("Exit");
        return StreamStatus.Running;
    }

    public async Task<SpecificMessage?> GetOneItem(CancellationToken cancellationToken)
    {
        var consumeResult = await _consumer.ConsumeResultAsync2(cancellationToken);
        return consumeResult?.Message;
    }
}