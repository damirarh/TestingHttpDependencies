using OrchestrationApi.Exceptions;
using OrchestrationApi.Models;

namespace OrchestrationApi.Mapping;

public static class EuIssueMapper
{
    public static EuIssueRequest ToEuIssueRequest(this IssueRequest issue)
    {
        return new EuIssueRequest(
            issue.Title,
            issue.Description,
            issue.Country,
            issue.ContactEmail
        );
    }

    public static Issue ToIssue(this EuIssueResponse issue)
    {
        return new Issue(
            issue.Id.ToString(),
            issue.Subject,
            issue.Details,
            issue.CountryCode,
            issue.Email,
            issue.CreatedOn,
            issue.Status == EuIssueStatus.Closed
        );
    }

    public static BackendServiceException ToException(this EuIssueErrorResponse error)
    {
        return new BackendServiceException(error.ErrorCode, error.ErrorMessage);
    }
}
