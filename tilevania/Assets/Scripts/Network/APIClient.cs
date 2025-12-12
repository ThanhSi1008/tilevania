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

        var response = new APIResponse<string>
        {
            success = !request.result.HasFlag(UnityWebRequest.Result.ConnectionError) &&
                      !request.result.HasFlag(UnityWebRequest.Result.ProtocolError),
            data = request.downloadHandler?.text,
            error = request.error,
            statusCode = (HttpStatusCode)request.responseCode
        };

        onComplete?.Invoke(response);
        request.Dispose();
    }
}

