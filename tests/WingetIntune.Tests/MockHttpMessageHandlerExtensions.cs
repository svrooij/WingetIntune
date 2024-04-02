

using NSubstitute;
using NSubstitute.Core;
using System.Reflection;

namespace WingetIntune.Tests;

public class HttpMessageHandlerWrapper : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

internal static class MockHttpMessageHandlerExtensions
{
    public static ConfiguredCall AddFakeResponse(this HttpMessageHandlerWrapper handler, string uri, HttpMethod httpMethod, HttpResponseMessage responseMessage)
    {
        return handler
            .GetType()
            .GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(handler, new object?[]
            {
                Arg.Is<HttpRequestMessage>(m => m.Matches(uri, httpMethod)),
                Arg.Any<CancellationToken>()
            })
            .Returns(Task.FromResult(responseMessage));
    }

    public static ConfiguredCall AddFakeResponse(this HttpMessageHandlerWrapper handler, string uri, HttpMethod httpMethod, string body, HttpResponseMessage responseMessage)
    {
        return handler
            .GetType()
            .GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(handler, new object?[]
            {
                Arg.Is<HttpRequestMessage>(m => m.Matches(uri, httpMethod, body)),
                Arg.Any<CancellationToken>()
            })
            .Returns(Task.FromResult(responseMessage));
    }

    private static bool Matches(this HttpRequestMessage requestMessage, string uri, HttpMethod httpMethod)
    {
        return requestMessage.RequestUri!.ToString() == uri && requestMessage.Method == httpMethod;
    }

    private static bool Matches(this HttpRequestMessage requestMessage, string uri, HttpMethod httpMethod, string body)
    {
        var result = requestMessage.RequestUri!.ToString() == uri && requestMessage.Method == httpMethod;

        if (result && requestMessage.Content != null)
        {
            var bodyString = requestMessage.Content.ReadAsStringAsync().Result;
            Assert.Equal(body, bodyString, ignoreAllWhiteSpace: true);
            result = bodyString == body;
        }

        return result;
    }
}
