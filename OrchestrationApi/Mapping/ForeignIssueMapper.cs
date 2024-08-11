using OrchestrationApi.Exceptions;
using OrchestrationApi.Models;

namespace OrchestrationApi.Mapping;

public static class ForeignIssueMapper
{
    public static ForeignIssueRequest ToForeignIssueRequest(this IssueRequest issue)
    {
        return new ForeignIssueRequest(issue.Title, issue.Description, issue.ContactEmail);
    }

    public static Issue ToIssue(this ForeignIssueResponse issue, IssueRequest request)
    {
        return new Issue(
            issue.Id.ToString(),
            issue.ShortDesc,
            issue.LongDesc,
            request.Country,
            issue.ReportedBy,
            DateTime.UtcNow,
            issue.Closed
        );
    }

    public static BackendServiceException ToException(this ForeignIssueErrorResponse error)
    {
        return new BackendServiceException(error.Error.ToString(), error.Desc);
    }
}
