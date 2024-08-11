namespace OrchestrationApi.Exceptions;

public class BackendServiceException(string code, string Message) : Exception(Message)
{
    public string Code => code;
}
