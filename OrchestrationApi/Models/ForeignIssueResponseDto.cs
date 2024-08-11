namespace OrchestrationApi.Models;

public record ForeignIssueResponseDto(
    bool Success,
    ForeignIssueResponse? Data,
    ForeignIssueErrorResponse? Error
) { }
