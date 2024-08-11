namespace OrchestrationApi.Models;

public record Issue(
    string Id,
    string Title,
    string Description,
    string Country,
    string ContactEmail,
    DateTime SubmittedAt,
    bool IsClosed
) : IssueRequest(Title, Description, Country, ContactEmail);
