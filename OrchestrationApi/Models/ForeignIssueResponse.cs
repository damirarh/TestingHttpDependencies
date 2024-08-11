namespace OrchestrationApi.Models;

public record ForeignIssueResponse(
    int Id,
    string ShortDesc,
    string LongDesc,
    string ReportedBy,
    bool Closed
) : ForeignIssueRequest(ShortDesc, LongDesc, ReportedBy) { }
