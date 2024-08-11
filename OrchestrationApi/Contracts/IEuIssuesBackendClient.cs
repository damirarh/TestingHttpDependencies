using OrchestrationApi.Models;

namespace OrchestrationApi.Contracts;

public interface IEuIssuesBackendClient
{
    Task<EuIssueResponse> SubmitAsync(EuIssueRequest issue);
}
