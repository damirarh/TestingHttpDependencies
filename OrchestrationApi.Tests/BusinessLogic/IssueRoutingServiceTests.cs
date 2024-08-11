using FluentAssertions;
using Moq.AutoMock;
using OrchestrationApi.Models;
using OrchestrationApi.Services;

namespace OrchestrationApi.Tests.BusinessLogic;

public class IssueRoutingServiceTests
{
    [TestCase("si", IssuesBackend.Eu)]
    [TestCase("gb", IssuesBackend.Foreign)]
    public void GetBackendAsync_ShouldReturnCorrectIssuesBackendForCountry(
        string country,
        IssuesBackend expectedBackend
    )
    {
        var mocker = new AutoMocker();
        var service = mocker.CreateInstance<IssueRoutingService>();

        var issue = new IssueRequest("Title", "Description", country, "somebody@mail.com");

        var backend = service.GetBackendAsync(issue).Result;

        backend.Should().Be(expectedBackend);
    }
}
