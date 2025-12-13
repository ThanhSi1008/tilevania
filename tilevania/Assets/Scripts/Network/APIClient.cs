using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class APIClient
{
    public static IEnumerator Get(string path, Action<APIResponse<string>> onComplete, Dictionary<string, string> headers = null)
    {
        yield return SendRequest(UnityWebRequest.kHttpVerbGET, path, null, onComplete, headers);
    }

    public static IEnumerator Post(string path, string jsonPayload, Action<APIResponse<string>> onComplete, Dictionary<string, string> headers = null)
    {
        yield return SendRequest(UnityWebRequest.kHttpVerbPOST, path, jsonPayload, onComplete, headers);
    }

    public static IEnumerator Put(string path, string jsonPayload, Action<APIResponse<string>> onComplete, Dictionary<string, string> headers = null)
    {
        yield return SendRequest(UnityWebRequest.kHttpVerbPUT, path, jsonPayload, onComplete, headers);
    }

    public static IEnumerator Delete(string path, Action<APIResponse<string>> onComplete, Dictionary<string, string> headers = null)
    {
        yield return SendRequest(UnityWebRequest.kHttpVerbDELETE, path, null, onComplete, headers);
    }

    private static IEnumerator SendRequest(string method, string path, string jsonPayload, Action<APIResponse<string>> onComplete, Dictionary<string, string> headers)
    {
        var url = $"{APIConfig.API_BASE_URL}{path}";
        var request = new UnityWebRequest(url, method)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        if (!string.IsNullOrEmpty(jsonPayload))
        {
            var bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
        }

        request.timeout = APIConfig.REQUEST_TIMEOUT_SECONDS;
        request.SetRequestHeader("x-client-version", APIConfig.CLIENT_VERSION);

        if (headers != null)
        {
            foreach (var kv in headers)
            {
                request.SetRequestHeader(kv.Key, kv.Value);
            }
        }

        yield return request.SendWebRequest();

        // Determine success based on result
        bool isSuccess = request.result == UnityWebRequest.Result.Success;
        HttpStatusCode statusCode = (HttpStatusCode)request.responseCode;
        
        // If responseCode is 0, it means no response (connection error, timeout, etc.)
        if (request.responseCode == 0)
        {
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                // Check if it's a timeout by examining the error message
                if (!string.IsNullOrEmpty(request.error) && 
                    (request.error.Contains("timeout") || request.error.Contains("Timeout")))
                {
                    statusCode = HttpStatusCode.RequestTimeout; // 408
                }
                else
                {
                    statusCode = HttpStatusCode.ServiceUnavailable; // 503
                }
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                statusCode = HttpStatusCode.BadGateway; // 502
            }
            else
            {
                statusCode = HttpStatusCode.BadGateway; // 502
            }
        }

        var response = new APIResponse<string>
        {
            success = isSuccess,
            data = request.downloadHandler?.text ?? string.Empty,
            error = request.error ?? (isSuccess ? string.Empty : $"Request failed: {request.result}"),
            statusCode = statusCode
        };

        // Log detailed error information for debugging
        if (!isSuccess)
        {
            Debug.LogError($"[APIClient] Request failed - URL: {url}, Method: {method}, " +
                          $"Result: {request.result}, Error: {request.error}, " +
                          $"ResponseCode: {request.responseCode}, StatusCode: {statusCode}, " +
                          $"Body: {response.data}");
        }
        else
        {
            Debug.Log($"[APIClient] Request success - URL: {url}, StatusCode: {statusCode}");
        }

        onComplete?.Invoke(response);
        request.Dispose();
    }
}

