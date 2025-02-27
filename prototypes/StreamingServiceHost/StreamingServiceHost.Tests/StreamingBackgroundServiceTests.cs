using FluentAssertions;
using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NGrid.Customer.Framework.StreamingServiceHost;
using NGrid.Customer.Framework.StreamingServiceHost.Mapping;
using NGrid.Customer.Framework.StreamingServiceHost.Streaming;

namespace StreamingServiceHost.Tests;

public class StreamingBackgroundServiceTests
{
    private readonly HeaderPropagationValues _headerPropagationValues = new();
    private readonly Mock<ILogger<StreamingBackgroundService<string, string>>> _logger = new();
    private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetime = new();
    private readonly Mock<IStreamInputProcessor<string>> _input = new();
    private readonly Mock<IStreamOutput<string>> _output = new();
    private readonly Mock<IStreamMapper> _mapper = new();

    [Test]
    public void NewService_ResolvesDependencies()
    {
        var serviceProvider = CreateServiceProvider();
        var host = new TestHost(serviceProvider);
        host.AssertBase();
    }

    [Test]
    public void LoopFailure_LogsFatal()
    {
        var serviceProvider = CreateServiceProvider();
        var sut = new TestHost(serviceProvider, new());
        sut.StartAsync(CancellationToken.None);
        Task.Delay(500).Wait();
        
        _logger.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    private IServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection()
            .AddSingleton(_hostApplicationLifetime.Object)
            .AddSingleton(_headerPropagationValues)
            .AddSingleton(_input.Object)
            .AddSingleton(_output.Object)
            .AddSingleton(_mapper.Object)
            .AddSingleton(_logger.Object);
        return serviceCollection.BuildServiceProvider();
    }

    public class TestHost : StreamingBackgroundService<string, string>
    {
        private readonly Exception? _exception;

        public TestHost(IServiceProvider serviceProvider, Exception? exception = null) : base(serviceProvider)
        {
            _exception = exception;
        }

        protected override Task<StreamStatus> WorkIteration(CancellationToken cancellationToken)
        {
            if (_exception != null) throw _exception;
            return base.WorkIteration(cancellationToken);
        }

        public void AssertBase()
        {
            Input.Should().NotBeNull();
            Output.Should().NotBeNull();
            Logger.Should().NotBeNull();
            Mapper.Should().NotBeNull();
        }
    }
}