namespace OrchestrationApi.Models;

public record EuIssueResponse(
    Guid Id,
    string Subject,
    string Details,
    string CountryCode,
    string Email,
    DateTime CreatedOn,
    EuIssueStatus Status
) : EuIssueRequest(Subject, Details, CountryCode, Email);
