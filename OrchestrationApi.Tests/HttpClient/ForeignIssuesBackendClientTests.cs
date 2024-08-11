using System.Net.Http.Json;
using FluentAssertions;
using Flurl;
using Moq;
using Moq.AutoMock;
using Moq.Contrib.HttpClient;
using OrchestrationApi.Exceptions;
using OrchestrationApi.Models;
using OrchestrationApi.Services;

namespace OrchestrationApi.Tests.HttpClient;

public class ForeignIssuesBackendClientTests
{
    private static readonly string baseUrl = "https://localhost:5001/";

    [Test]
    public async Task SubmitAsync_ReturnsSuccessfulResponse()
    {
        var request = new ForeignIssueRequest("ShortDesc", "LongDesc", "ReportedBy");
        var response = new ForeignIssueResponseDto(
            true,
            new ForeignIssueResponse(42, "ShortDesc", "LongDesc", "ReportedBy", false),
            null
        );

        var mocker = new AutoMocker();

        var handler = new Mock<HttpMessageHandler>();
        handler
            .SetupRequest(
                HttpMethod.Post,
                baseUrl.AppendPathSegment("tickets"),
                async r =>
                {
                    var body =
                        r.Content != null
                            ? await r.Content.ReadFromJsonAsync<ForeignIssueRequest>()
                            : null;
                    return body?.ShortDesc == request.ShortDesc
                        && body?.LongDesc == request.LongDesc
                        && body?.ReportedBy == request.ReportedBy;
                }
            )
            .ReturnsJsonResponse(response);
        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(baseUrl);
        mocker.Use(httpClient);

        var client = mocker.CreateInstance<ForeignIssuesBackendClient>();

        var issue = await client.SubmitAsync(request);

        issue.Id.Should().Be(response.Data?.Id);
        issue.ShortDesc.Should().Be(response.Data?.ShortDesc);
        issue.LongDesc.Should().Be(response.Data?.LongDesc);
        issue.ReportedBy.Should().Be(response.Data?.ReportedBy);
        issue.Closed.Should().Be(response.Data!.Closed);
    }

    [Test]
    public async Task SubmitAsync_ThrowsOnUnsuccessfulResponse()
    {
        var request = new ForeignIssueRequest("ShortDesc", "LongDesc", "ReportedBy");
        var response = new ForeignIssueResponseDto(
            false,
            null,
            new ForeignIssueErrorResponse(42, "Desc")
        );

        var mocker = new AutoMocker();
        var handler = new Mock<HttpMessageHandler>();
        handler
            .SetupRequest(
                HttpMethod.Post,
                baseUrl.AppendPathSegment("tickets"),
                async r =>
                {
                    var body =
                        r.Content != null
                            ? await r.Content.ReadFromJsonAsync<ForeignIssueRequest>()
                            : null;
                    return body?.ShortDesc == request.ShortDesc
                        && body?.LongDesc == request.LongDesc
                        && body?.ReportedBy == request.ReportedBy;
                }
            )
            .ReturnsJsonResponse(response);
        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(baseUrl);
        mocker.Use(httpClient);

        var client = mocker.CreateInstance<ForeignIssuesBackendClient>();

        var submitAction = async () => await client.SubmitAsync(request);

        await submitAction
            .Should()
            .ThrowAsync<BackendServiceException>()
            .WithMessage(response.Error?.Desc)
            .Where(e => e.Code == response.Error!.Error.ToString());
    }
}
