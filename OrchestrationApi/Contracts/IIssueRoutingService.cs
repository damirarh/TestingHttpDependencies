using OrchestrationApi.Models;

namespace OrchestrationApi.Contracts;

public interface IIssueRoutingService
{
    Task<IssuesBackend> GetBackendAsync(IssueRequest issue);
}
