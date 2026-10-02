using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using Microsoft.Extensions.Logging;
using Moq;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersApiClientsEdit.Tests;

//ApiKey is a secret: it must show neither in the record menu caption nor in the field status
[Collection(ConsoleCaptureCollection.Name)]
public sealed class ApiClientCruderTests : IDisposable
{
    private const string ApiClientName = "TestClient";
    private const string ApiKey = "fake-api-key-123";
    private const string Server = "https://test.server/api/v1";
    private const string VersionPath = "/api/v1/test/getversion";

    private readonly Dictionary<string, ApiClientSettings> _apiClients;
    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<ILogger> _logger = new();
    private readonly TextWriter _originalConsoleOutput;
    private readonly Mock<IParametersManager> _parametersManager = new();
    private readonly StubHttpMessageHandler _server = new();
    private readonly ApiClientCruder _sut;

    public ApiClientCruderTests()
    {
        _apiClients = new Dictionary<string, ApiClientSettings>
        {
            [ApiClientName] = new() { Server = Server, ApiKey = ApiKey }
        };
        _httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(_server, false));
        _sut = new ApiClientCruder(_logger.Object, _httpClientFactory.Object, _parametersManager.Object, _apiClients);

        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
        _server.Dispose();
    }

    //the record menu caption is the record's GetItemKey
    [Fact]
    public void GetItemMenu_WhenCalled_ShowsOnlyTheServerInTheCaption()
    {
        // Act
        CliMenuSet itemMenu = _sut.GetItemMenu(ApiClientName);

        // Assert
        Assert.Equal(Server, itemMenu.Caption);
    }

    [Fact]
    public void GetDetailsSubMenu_WhenCalled_MasksTheApiKeyLikeAPassword()
    {
        // Arrange
        CliMenuCommand apiKeyCommand = _sut.GetDetailsSubMenu(ApiClientName).Single(x => x.Name == "Api Key");

        // Act
        apiKeyCommand.CountStatus();

        // Assert
        Assert.Equal(new string('*', ApiKey.Length), apiKeyCommand.StatusString);
    }

    [Fact]
    public void GetDetailsSubMenu_WhenCalled_ShowsTheServer()
    {
        // Arrange
        CliMenuCommand serverCommand = _sut.GetDetailsSubMenu(ApiClientName).Single(x => x.Name == "Server");

        // Act
        serverCommand.CountStatus();

        // Assert
        Assert.Equal(Server, serverCommand.StatusString);
    }

    [Theory]
    [InlineData(Server, Server)]
    [InlineData(null, "")]
    public void ApiClientSettingsGetItemKey_WhenCalled_ReturnsOnlyTheServer(string? server, string expected)
    {
        // Arrange
        var apiClientSettings = new ApiClientSettings { Server = server, ApiKey = ApiKey };

        // Act
        string itemKey = apiClientSettings.GetItemKey();

        // Assert
        Assert.Equal(expected, itemKey);
    }

    [Fact]
    public void Constructor_WhenCalled_NamesTheRecordsApiClients()
    {
        // Assert
        Assert.Equal("Api Client", _sut.CrudName);
        Assert.Equal("Api Clients", _sut.CrudNamePlural);
    }

    [Fact]
    public void Create_WhenCalled_EditsTheApiClientsOfTheParameters()
    {
        // Arrange
        var parameters = new Mock<IParametersWithApiClients>();
        parameters.SetupGet(x => x.ApiClients).Returns(_apiClients);
        _parametersManager.SetupGet(x => x.Parameters).Returns(parameters.Object);

        // Act
        var sut = ApiClientCruder.Create(_logger.Object, _httpClientFactory.Object, _parametersManager.Object);

        // Assert
        Assert.Equal([ApiClientName], sut.GetKeys());
    }

    //the status of an api client is its server, never its key
    [Theory]
    [InlineData(ApiClientName, Server)]
    [InlineData("Missing", null)]
    [InlineData("", null)]
    [InlineData(" ", null)]
    [InlineData(null, null)]
    public void GetStatusFor_WhenCalled_ReturnsTheServer(string? name, string? expected)
    {
        // Act
        string? status = _sut.GetStatusFor(name);

        // Assert
        Assert.Equal(expected, status);
    }

    //the server is checked by asking the version of its test api
    [Fact]
    public void CheckValidation_WhenServerAnswersWithAVersion_ReturnsTrue()
    {
        // Arrange
        _server.Respond(HttpStatusCode.OK, "1.2.3");

        // Act
        bool result = _sut.CheckValidation(new ApiClientSettings { Server = Server, ApiKey = ApiKey });

        // Assert
        Assert.True(result);
        Assert.Equal([$"GET {VersionPath}"], _server.Requests);
        string console = _consoleOutput.ToString();
        Assert.Contains("Try connect to Test Api Client...", console, StringComparison.Ordinal);
        Assert.Contains("Connected successfully, Test Api Client version is 1.2.3", console, StringComparison.Ordinal);
    }

    [Fact]
    public void CheckValidation_WhenServerAnswersWithAnEmptyVersion_ReturnsFalse()
    {
        // Arrange
        _server.Respond(HttpStatusCode.OK, string.Empty);

        // Act
        bool result = _sut.CheckValidation(new ApiClientSettings { Server = Server });

        // Assert
        Assert.False(result);
        Assert.DoesNotContain("Connected successfully", _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void CheckValidation_WhenServerAnswersWithAnError_ReturnsFalse()
    {
        // Arrange
        _server.Respond(HttpStatusCode.InternalServerError, "Server is broken");

        // Act
        bool result = _sut.CheckValidation(new ApiClientSettings { Server = Server });

        // Assert
        Assert.False(result);
        Assert.Equal([$"GET {VersionPath}"], _server.Requests);
        string console = _consoleOutput.ToString();
        Assert.DoesNotContain("Connected successfully", console, StringComparison.Ordinal);
        //the client shows the answer, the cruder shows the error the client made of it
        Assert.Contains("Error from server: 500", console, StringComparison.Ordinal);
        Assert.Contains("Server is broken", console, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CheckValidation_WhenServerIsNotGiven_ReturnsFalseWithoutCallingIt(string? server)
    {
        // Act
        bool result = _sut.CheckValidation(new ApiClientSettings { Server = server });

        // Assert
        Assert.False(result);
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public void CheckValidation_WhenItemIsNotAnApiClient_ReturnsFalse()
    {
        // Act
        bool result = _sut.CheckValidation(new ItemData());

        // Assert
        Assert.False(result);
        Assert.Empty(_server.Requests);
    }

    //an address that is not an uri makes the client throw: the check fails and the error is logged
    [Fact]
    public void CheckValidation_WhenServerAddressIsInvalid_LogsTheErrorAndReturnsFalse()
    {
        // Act
        bool result = _sut.CheckValidation(new ApiClientSettings { Server = "not a server address" });

        // Assert
        Assert.False(result);
        Assert.Empty(_server.Requests);
        _logger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() == "ErrorOmd in method CheckValidation"),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    //answers every request with the configured answer and remembers "METHOD path" of every request
    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private string _body = string.Empty;
        private HttpStatusCode _statusCode = HttpStatusCode.OK;

        public List<string> Requests { get; } = [];

        public void Respond(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add($"{request.Method} {request.RequestUri!.AbsolutePath}");
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                RequestMessage = request, Content = new StringContent(_body, Encoding.UTF8, "text/plain")
            });
        }
    }
}
