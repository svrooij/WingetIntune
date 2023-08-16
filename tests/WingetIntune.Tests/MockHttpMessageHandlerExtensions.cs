using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune.Tests;
internal static class MockHttpMessageHandlerExtensions
{
    public static void AddMockResponse(this Mock<HttpMessageHandler> handlerMock, string uri, HttpMethod httpMethod, HttpResponseMessage responseMessage)
    {
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                                              ItExpr.Is<HttpRequestMessage>(m => m.Matches(uri, httpMethod)),
                                              ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
    }

    public static void AddMockResponse(this Mock<HttpMessageHandler> handlerMock, string uri, HttpMethod httpMethod, string body, HttpResponseMessage responseMessage)
    {
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                                              ItExpr.Is<HttpRequestMessage>(m => m.Matches(uri, httpMethod, body)),
                                              ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
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
            Assert.Equal(body, bodyString);
            result = bodyString == body;
        }


        return result;
    }
}
