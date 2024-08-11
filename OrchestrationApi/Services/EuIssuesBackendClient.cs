using OrchestrationApi.Contracts;
using OrchestrationApi.Mapping;
using OrchestrationApi.Models;

namespace OrchestrationApi.Services;

public class EuIssuesBackendClient(HttpClient httpClient) : IEuIssuesBackendClient
{
    public async Task<EuIssueResponse> SubmitAsync(EuIssueRequest issue)
    {
        var response = await httpClient.PostAsJsonAsync("claims", issue);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<EuIssueResponse>()
                ?? throw new InvalidOperationException("Successful response with null body.");
        }
        else
        {
            var error =
                await response.Content.ReadFromJsonAsync<EuIssueErrorResponse>()
                ?? throw new InvalidOperationException("Unsuccessful response with null body.");
            throw error.ToException();
        }
    }
}
