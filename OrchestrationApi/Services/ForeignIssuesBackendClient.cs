using OrchestrationApi.Contracts;
using OrchestrationApi.Mapping;
using OrchestrationApi.Models;

namespace OrchestrationApi.Services;

public class ForeignIssuesBackendClient(HttpClient httpClient) : IForeignIssuesBackendClient
{
    public async Task<ForeignIssueResponse> SubmitAsync(ForeignIssueRequest issue)
    {
        var response = await httpClient.PostAsJsonAsync("tickets", issue);
        response.EnsureSuccessStatusCode();
        var deserializedResponse =
            await response.Content.ReadFromJsonAsync<ForeignIssueResponseDto>()
            ?? throw new ArgumentNullException(nameof(response.Content), "Null response.");
        if (deserializedResponse.Success)
        {
            return deserializedResponse.Data
                ?? throw new InvalidOperationException("Successful response with missing data.");
        }
        else
        {
            throw deserializedResponse.Error?.ToException()
                ?? throw new InvalidOperationException("Unsuccessful response with missing error.");
        }
    }
}
