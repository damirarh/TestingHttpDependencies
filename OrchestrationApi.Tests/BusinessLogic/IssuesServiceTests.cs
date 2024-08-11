using FluentAssertions;
using Moq;
using Moq.AutoMock;
using OrchestrationApi.Contracts;
using OrchestrationApi.Models;
using OrchestrationApi.Services;

namespace OrchestrationApi.Tests.BusinessLogic;

public class IssuesServiceTests
{
    [Test]
    public async Task SubmitAsync_CallsEuBackend()
    {
        var mocker = new AutoMocker();
        var service = mocker.CreateInstance<IssuesService>();

        mocker
            .GetMock<IIssueRoutingService>()
            .Setup(m => m.GetBackendAsync(It.IsAny<IssueRequest>()))
            .ReturnsAsync(IssuesBackend.Eu);

        var response = new EuIssueResponse(
            Guid.NewGuid(),
            "Subject",
            "Details",
            "CountryCode",
            "Email",
            DateTime.UtcNow,
            EuIssueStatus.New
        );

        mocker
            .GetMock<IEuIssuesBackendClient>()
            .Setup(m => m.SubmitAsync(It.IsAny<EuIssueRequest>()))
            .ReturnsAsync(response);

        var request = new IssueRequest("Title", "Description", "Country", "ContactEmail");
        var issue = await service.SubmitAsync(request);

        mocker.Verify<IEuIssuesBackendClient>(
            x =>
                x.SubmitAsync(
                    It.Is<EuIssueRequest>(r =>
                        r.Subject == "Title"
                        && r.Details == "Description"
                        && r.CountryCode == "Country"
                        && r.Email == "ContactEmail"
                    )
                ),
            Times.Once
        );

        issue.Id.Should().Be(response.Id.ToString());
        issue.Title.Should().Be(response.Subject);
        issue.Description.Should().Be(response.Details);
        issue.Country.Should().Be(response.CountryCode);
        issue.ContactEmail.Should().Be(response.Email);
        issue.SubmittedAt.Should().Be(response.CreatedOn);
        issue.IsClosed.Should().Be(false);
    }

    [Test]
    public async Task SubmitAsync_CallsForeignBackend()
    {
        var mocker = new AutoMocker();
        var service = mocker.CreateInstance<IssuesService>();

        mocker
            .GetMock<IIssueRoutingService>()
            .Setup(m => m.GetBackendAsync(It.IsAny<IssueRequest>()))
            .ReturnsAsync(IssuesBackend.Foreign);

        var response = new ForeignIssueResponse(42, "ShortDesc", "LongDesc", "ReportedBy", false);

        mocker
            .GetMock<IForeignIssuesBackendClient>()
            .Setup(m => m.SubmitAsync(It.IsAny<ForeignIssueRequest>()))
            .ReturnsAsync(response);

        var request = new IssueRequest("Title", "Description", "Country", "ContactEmail");
        var callTime = DateTime.UtcNow;
        var issue = await service.SubmitAsync(request);

        mocker.Verify<IForeignIssuesBackendClient>(
            x =>
                x.SubmitAsync(
                    It.Is<ForeignIssueRequest>(r =>
                        r.ShortDesc == "Title"
                        && r.LongDesc == "Description"
                        && r.ReportedBy == "ContactEmail"
                    )
                ),
            Times.Once
        );

        issue.Id.Should().Be("42");
        issue.Title.Should().Be(response.ShortDesc);
        issue.Description.Should().Be(response.LongDesc);
        issue.Country.Should().Be(request.Country);
        issue.ContactEmail.Should().Be(response.ReportedBy);
        issue.SubmittedAt.Should().BeBefore(DateTime.UtcNow).And.BeAfter(callTime);
        issue.IsClosed.Should().Be(false);
    }

    [Test]
    public async Task SubmitAsync_ThrowsForUnknownBackend()
    {
        var mocker = new AutoMocker();
        var service = mocker.CreateInstance<IssuesService>();

        mocker
            .GetMock<IIssueRoutingService>()
            .Setup(m => m.GetBackendAsync(It.IsAny<IssueRequest>()))
            .ReturnsAsync((IssuesBackend)(-1));

        var request = new IssueRequest("Title", "Description", "Country", "ContactEmail");

        var submitAction = async () => await service.SubmitAsync(request);

        await submitAction
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Unknown backend: -1");
    }
}
