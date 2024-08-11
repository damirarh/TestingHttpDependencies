using Microsoft.AspNetCore.Mvc;
using OrchestrationApi.Contracts;
using OrchestrationApi.Exceptions;
using OrchestrationApi.Models;

namespace OrchestrationApi.Controllers;

[ApiController]
[Route("[controller]")]
public class IssuesController(IIssuesService issuesService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitAsync([FromBody] IssueRequest issueRequest)
    {
        try
        {
            var issue = await issuesService.SubmitAsync(issueRequest);
            return Ok(issue);
        }
        catch (BackendServiceException e)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ProblemDetails
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3",
                    Title = "Bad Gateway",
                    Status = StatusCodes.Status502BadGateway,
                    Detail = e.Message,
                    Extensions = { ["ErrorCode"] = e.Code }
                }
            );
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred. Please try again later."
                }
            );
        }
    }
}
