using System.Net;

public class APIResponse<T>
{
    public bool success;
    public T data;
    public string error;
    public HttpStatusCode statusCode;
}

