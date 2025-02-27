using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.AspNetCore.Http;
using NGrid.Customer.Framework.StreamingServiceHost.Mapping;
using NGrid.Customer.Framework.StreamingServiceHost.Streaming;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace NGrid.Customer.Framework.StreamingServiceHost;

public class StreamingBackgroundService<TIn, TOut> : BackgroundService where TOut : class
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<StreamingBackgroundService<TIn, TOut>> _logger;
    private readonly HeaderPropagationValues _headerPropagationValues;
    private readonly IStreamInputProcessor<TIn> _input;
    private readonly IStreamOutput<TOut> _output;
    private readonly IStreamMapper _mapper;

    public StreamingBackgroundService(IHostApplicationLifetime hostApplicationLifetime, ILogger<StreamingBackgroundService<TIn, TOut>> logger,
        HeaderPropagationValues headerPropagationValues, IStreamInputProcessor<TIn> input, IStreamOutput<TOut> output, IStreamMapper mapper)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _headerPropagationValues = headerPropagationValues;
        _input = input;
        _output = output;
        _mapper = mapper;
    }

    protected StreamingBackgroundService(IServiceProvider serviceProvider)
    {
        _hostApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
        _headerPropagationValues = serviceProvider.GetRequiredService<HeaderPropagationValues>();
        _logger = serviceProvider.GetRequiredService<ILogger<StreamingBackgroundService<TIn, TOut>>>();
        _input = serviceProvider.GetRequiredService<IStreamInputProcessor<TIn>>();
        _output = serviceProvider.GetRequiredService<IStreamOutput<TOut>>();
        _mapper = serviceProvider.GetRequiredService<IStreamMapper>();
    }

    protected ILogger Logger => _logger;
    protected IStreamInputProcessor<TIn> Input => _input;
    protected IStreamOutput<TOut> Output => _output;
    protected IStreamMapper Mapper => _mapper;

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
    {
        Logger.LogInformation("Host starting");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _headerPropagationValues.Headers = new HeaderDictionary();
                var status = await WorkIteration(stoppingToken);
                if (status == StreamStatus.Finished)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Fatal error: {ExceptionMessage}", e.Message);
        }
        finally
        {
            Logger.LogInformation("Host exiting");
            _hostApplicationLifetime.StopApplication();
        }
    }, stoppingToken);

    protected virtual async Task<StreamStatus> WorkIteration(CancellationToken cancellationToken)
    {
        return await _input.ProcessOneItem(async item =>
        {
            Logger.LogDebug("new item");
            var outputItem = _mapper.Map<TIn, TOut>(item);
            await _output.Handle(outputItem);
        }, cancellationToken);
    }
}