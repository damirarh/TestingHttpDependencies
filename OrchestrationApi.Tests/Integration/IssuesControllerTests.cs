using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Flurl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using OrchestrationApi.Models;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace OrchestrationApi.Tests.Integration;

public class IssuesControllerTests
{
    private WebApplicationFactory<Program> factory;
    private WireMockServer server;

    private readonly JsonSerializerOptions jsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [SetUp]
    public void Setup()
    {
        server = WireMockServer.Start();

        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(
                (context, config) =>
                {
                    config.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ExternalServices__EuIssuesBackendBaseUrl"] =
                                server.Url.AppendPathSegment("eu/"),
                            ["ExternalServices__ForeignIssuesBackendBaseUrl"] =
                                server.Url.AppendPathSegment("foreign/")
                        }
                    );
                }
            );
        });
    }

    [TearDown]
    public void Teardown()
    {
        factory.Dispose();
        server.Dispose();
    }

    [Test]
    public async Task SubmitIssue_CallsEuBackendForTicketFromSlovenia()
    {
        var request = new IssueRequest("Title", "Description", "SI", "somebody@mail.com");
        var expectedEuIssueRequest = new EuIssueRequest(
            request.Title,
            request.Description,
            request.Country,
            request.ContactEmail
        );
        var euIssueResponse = new EuIssueResponse(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.Country,
            request.ContactEmail,
            DateTime.UtcNow,
            EuIssueStatus.New
        );

        server
            .Given(
                Request
                    .Create()
                    .WithPath("/eu/claims")
                    .WithBody(
                        new JsonMatcher(
                            JsonSerializer.Serialize(expectedEuIssueRequest, jsonOptions)
                        )
                    )
                    .UsingPost()
            )
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(euIssueResponse));

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/issues", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var issue = await response.Content.ReadFromJsonAsync<Issue>();
        issue!.Id.Should().Be(euIssueResponse.Id.ToString());
        issue!.Title.Should().Be(request.Title);
        issue!.Description.Should().Be(request.Description);
        issue!.Country.Should().Be(request.Country);
        issue!.ContactEmail.Should().Be(request.ContactEmail);
        issue!.SubmittedAt.Should().Be(euIssueResponse.CreatedOn);
        issue!.IsClosed.Should().BeFalse();
    }

    [Test]
    public async Task SubmitIssue_CallsForeignBackendForTicketFromGreatBritain()
    {
        var request = new IssueRequest("Title", "Description", "GB", "somebody@mail.com");
        var expectedForeignIssueRequest = new ForeignIssueRequest(
            request.Title,
            request.Description,
            request.ContactEmail
        );
        var foreignIssueResponse = new ForeignIssueResponseDto(
            true,
            new ForeignIssueResponse(
                42,
                request.Title,
                request.Description,
                request.ContactEmail,
                false
            ),
            null
        );

        server
            .Given(
                Request
                    .Create()
                    .WithPath("/foreign/tickets")
                    .WithBody(
                        new JsonMatcher(
                            JsonSerializer.Serialize(expectedForeignIssueRequest, jsonOptions)
                        )
                    )
                    .UsingPost()
            )
            .RespondWith(
                Response.Create().WithStatusCode(200).WithBodyAsJson(foreignIssueResponse)
            );

        var client = factory.CreateClient();

        var startTime = DateTime.UtcNow;
        var response = await client.PostAsJsonAsync("/issues", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var issue = await response.Content.ReadFromJsonAsync<Issue>();
        issue!.Id.Should().Be(foreignIssueResponse.Data!.Id.ToString());
        issue!.Title.Should().Be(request.Title);
        issue!.Description.Should().Be(request.Description);
        issue!.Country.Should().Be(request.Country);
        issue!.ContactEmail.Should().Be(request.ContactEmail);
        issue!.SubmittedAt.Should().BeAfter(startTime).And.BeBefore(DateTime.UtcNow);
        issue!.IsClosed.Should().BeFalse();
    }

    [Test]
    public async Task SubmitIssue_ReturnsBadGatewayForEuBackendErrors()
    {
        var request = new IssueRequest("Title", "Description", "SI", "somebody@mail.com");
        var euIssueErrorResponse = new EuIssueErrorResponse("ErrorCode", "ErrorMessage");

        server
            .Given(Request.Create().WithPath("/eu/claims").UsingPost())
            .RespondWith(
                Response.Create().WithStatusCode(400).WithBodyAsJson(euIssueErrorResponse)
            );

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/issues", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3");
        problem!.Title.Should().Be("Bad Gateway");
        problem!.Status.Should().Be(StatusCodes.Status502BadGateway);
        problem!.Detail.Should().Be(euIssueErrorResponse.ErrorMessage);
        problem!
            .Extensions.Should()
            .ContainKey("ErrorCode")
            .WhoseValue!.ToString()
            .Should()
            .Be(euIssueErrorResponse.ErrorCode);
    }

    [Test]
    public async Task SubmitIssue_ReturnsBadGatewayForExpectedForeignBackendErrors()
    {
        var request = new IssueRequest("Title", "Description", "GB", "somebody@mail.com");
        var foreignIssueErrorResponse = new ForeignIssueResponseDto(
            false,
            null,
            new ForeignIssueErrorResponse(42, "Desc")
        );

        server
            .Given(Request.Create().WithPath("/foreign/tickets").UsingPost())
            .RespondWith(
                Response.Create().WithStatusCode(200).WithBodyAsJson(foreignIssueErrorResponse)
            );

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/issues", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3");
        problem!.Title.Should().Be("Bad Gateway");
        problem!.Status.Should().Be(StatusCodes.Status502BadGateway);
        problem!.Detail.Should().Be(foreignIssueErrorResponse.Error!.Desc);
        problem!
            .Extensions.Should()
            .ContainKey("ErrorCode")
            .WhoseValue!.ToString()
            .Should()
            .Be(foreignIssueErrorResponse.Error!.Error.ToString());
    }

    [Test]
    public async Task SubmitIssue_ReturnsInternalServerErrorForUnexpectedForeignBackendErrors()
    {
        var request = new IssueRequest("Title", "Description", "GB", "somebody@mail.com");

        server
            .Given(Request.Create().WithPath("/foreign/tickets").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(500));

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/issues", request);
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1");
        problem!.Title.Should().Be("Internal Server Error");
        problem!.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problem!.Detail.Should().Be("An unexpected error occurred. Please try again later.");
    }
}
