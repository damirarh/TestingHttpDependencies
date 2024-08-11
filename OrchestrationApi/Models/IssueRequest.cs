using System.ComponentModel.DataAnnotations;
using OrchestrationApi.DataAnnotations;

namespace OrchestrationApi.Models;

public record IssueRequest(
    string Title,
    string Description,
    [CountryCode] string Country,
    [EmailAddress] string ContactEmail
);
