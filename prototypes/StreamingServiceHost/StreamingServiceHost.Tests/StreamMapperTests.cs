using AutoMapper;
using Moq;
using NGrid.Customer.Framework.StreamingServiceHost.Mapping;

namespace StreamingServiceHost.Tests;

public class Tests
{
    [Test]
    public void InvalidTypes_ThrowsException()
    {
        var mapperMock = new Mock<IMapper>();
        var sut = new StreamMapper(mapperMock.Object);
        Assert.Throws<MappingException>(() => sut.Map<One, Two>(new One()));
    }

    private class One {}
    private class Two {}
}