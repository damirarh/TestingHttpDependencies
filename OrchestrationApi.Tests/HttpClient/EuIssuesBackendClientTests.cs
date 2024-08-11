using FluentAssertions;
using Flurl;
using Moq;
using Moq.AutoMock;
using Moq.Contrib.HttpClient;
using OrchestrationApi.Exceptions;
using OrchestrationApi.Models;
using OrchestrationApi.Services;
using System.Net;
using System.Net.Http.Json;

namespace OrchestrationApi.Tests.HttpClient;

public class EuIssuesBackendClientTests
{
    private static readonly string baseUrl = "https://localhost:5001/";

    [Test]
    public async Task SubmitAsync_ReturnsSuccessfulResponse()
    {
        var request = new EuIssueRequest("Subject", "Details", "CountryCode", "Email");
        var response = new EuIssueResponse(
            Guid.NewGuid(),
            "Subject",
            "Details",
            "CountryCode",
            "Email",
            DateTime.UtcNow,
            EuIssueStatus.New
        );

        var mocker = new AutoMocker();

        var handler = new Mock<HttpMessageHandler>();
        handler
            .SetupRequest(
                HttpMethod.Post,
                baseUrl.AppendPathSegment("claims"),
                async r =>
                {
                    var body =
                        r.Content != null
                            ? await r.Content.ReadFromJsonAsync<EuIssueRequest>()
                            : null;
                    return body?.Subject == request.Subject
                        && body?.Details == request.Details
                        && body?.CountryCode == request.CountryCode
                        && body?.Email == request.Email;
                }
            )
            .ReturnsJsonResponse(response);
        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(baseUrl);
        mocker.Use(httpClient);

        var client = mocker.CreateInstance<EuIssuesBackendClient>();

        var issue = await client.SubmitAsync(request);

        issue.Id.Should().Be(response.Id);
        issue.Subject.Should().Be(response.Subject);
        issue.Details.Should().Be(response.Details);
        issue.CountryCode.Should().Be(response.CountryCode);
        issue.Email.Should().Be(response.Email);
        issue.CreatedOn.Should().Be(response.CreatedOn);
        issue.Status.Should().Be(response.Status);
    }

    [Test]
    public async Task SubmitAsync_ThrowsOnUnsuccessfulResponse()
    {
        var request = new EuIssueRequest("Subject", "Details", "CountryCode", "Email");
        var response = new EuIssueErrorResponse("ErrorCode", "ErrorMessage");

        var mocker = new AutoMocker();

        var handler = new Mock<HttpMessageHandler>();
        handler
            .SetupRequest(
                HttpMethod.Post,
                baseUrl.AppendPathSegment("claims"),
                async r =>
                {
                    var body =
                        r.Content != null
                            ? await r.Content.ReadFromJsonAsync<EuIssueRequest>()
                            : null;
                    return body?.Subject == request.Subject
                        && body?.Details == request.Details
                        && body?.CountryCode == request.CountryCode
                        && body?.Email == request.Email;
                }
            )
            .ReturnsJsonResponse(HttpStatusCode.BadRequest, response);
        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(baseUrl);
        mocker.Use(httpClient);

        var client = mocker.CreateInstance<EuIssuesBackendClient>();

        var submitAction = async () => await client.SubmitAsync(request);

        await submitAction
            .Should()
            .ThrowAsync<BackendServiceException>()
            .WithMessage(response.ErrorMessage)
            .Where(e => e.Code == response.ErrorCode);
    }
}
