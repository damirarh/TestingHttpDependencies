using OrchestrationApi.Models;

namespace OrchestrationApi.Contracts;

public interface IIssuesService
{
    Task<Issue> SubmitAsync(IssueRequest issue);
}
