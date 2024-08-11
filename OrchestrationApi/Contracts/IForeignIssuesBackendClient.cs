using OrchestrationApi.Models;

namespace OrchestrationApi.Contracts;

public interface IForeignIssuesBackendClient
{
    Task<ForeignIssueResponse> SubmitAsync(ForeignIssueRequest issue);
}
