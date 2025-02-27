namespace NGrid.Customer.GraphQl.FusionGateway;

public class GatewayConfigurationProvider : IObservable<GatewayConfiguration>
{
    private readonly List<IObserver<GatewayConfiguration>> _observers = new();

    public async Task UpdateSubscribers(GatewayConfiguration gatewayConfiguration)
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(gatewayConfiguration);
        }
    }
    public IDisposable Subscribe(IObserver<GatewayConfiguration> observer)
    {
        _observers.Add(observer);
        return new ObserverUnsubscribe(() => _observers.Remove(observer));
    }

    private class ObserverUnsubscribe(Action removeObserver) : IDisposable
    {
        public void Dispose()
        {
            removeObserver();
        }
    }
}