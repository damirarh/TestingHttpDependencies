using OrchestrationApi.Contracts;
using OrchestrationApi.Models;

namespace OrchestrationApi.Services;

public class IssueRoutingService : IIssueRoutingService
{
    private static readonly string[] euCountries =
    [
        "at",
        "be",
        "bg",
        "hr",
        "cy",
        "cz",
        "dk",
        "ee",
        "fi",
        "fr",
        "de",
        "gr",
        "hu",
        "ie",
        "it",
        "lv",
        "lt",
        "lu",
        "mt",
        "nl",
        "pl",
        "pt",
        "ro",
        "sk",
        "si",
        "es",
        "se"
    ];

    public Task<IssuesBackend> GetBackendAsync(IssueRequest issue)
    {
        return Task.FromResult(
            euCountries.Contains(issue.Country.ToLowerInvariant())
                ? IssuesBackend.Eu
                : IssuesBackend.Foreign
        );
    }
}
