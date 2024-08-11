using OrchestrationApi.Contracts;
using OrchestrationApi.Mapping;
using OrchestrationApi.Models;

namespace OrchestrationApi.Services;

public class IssuesService(
    IIssueRoutingService issueRoutingService,
    IEuIssuesBackendClient euIssuesBackendClient,
    IForeignIssuesBackendClient nonEuIssuesBackendClient
) : IIssuesService
{
    public async Task<Issue> SubmitAsync(IssueRequest issue)
    {
        var backend = await issueRoutingService.GetBackendAsync(issue);
        switch (backend)
        {
            case IssuesBackend.Eu:
                var euIssue = issue.ToEuIssueRequest();
                var euIssueResponse = await euIssuesBackendClient.SubmitAsync(euIssue);
                return euIssueResponse.ToIssue();

            case IssuesBackend.Foreign:
                var foreignIssue = issue.ToForeignIssueRequest();
                var foreignIssueResponse = await nonEuIssuesBackendClient.SubmitAsync(foreignIssue);
                return foreignIssueResponse.ToIssue(issue);

            default:
                throw new InvalidOperationException($"Unknown backend: {backend}");
        }
    }
}
