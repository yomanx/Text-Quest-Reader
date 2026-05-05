using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.IO;

public class ApiManager : MonoBehaviour
{
    // private const string BaseUri = "http://localhost:5205";
    // private const string BaseUri = "http://localhost:8080";
    private const string BaseUri = "https://questapi-a9pu.onrender.com";
    private const string GetAllQuestsUri = BaseUri + "/quests";
    private const string GetQuestUri = BaseUri + "/quest";

    public static ApiManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    #region GET

    public void GetAllQuests(Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(GetRequest(GetAllQuestsUri, onSuccess, onError));
    }

    public void GetQuest(int id, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(GetRequest($"{GetQuestUri}/{id}", onSuccess, onError));
    }

    public void DownloadQuestPackage(int id, Action<byte[]> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(GetBytesRequest($"{GetQuestUri}/{id}/package", onSuccess, onError));
    }

    private IEnumerator GetRequest(string requestUri, Action<string> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(requestUri))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMessage = $"HTTP error. Code: {webRequest.responseCode}, Error: {webRequest.error}, Uri: {requestUri}";
                onError?.Invoke(errorMessage);
            }
            else
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }

    private IEnumerator GetBytesRequest(string requestUri, Action<byte[]> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(requestUri))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMessage = $"HTTP error. Code: {webRequest.responseCode}, Error: {webRequest.error}, Uri: {requestUri}";
                Debug.LogError(errorMessage);
                onError?.Invoke(errorMessage);
            }
            else
            {
                onSuccess?.Invoke(webRequest.downloadHandler.data);
            }
        }
    }

    public void GetQuestPreviewImage(int questId, Action<Sprite> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(GetSpriteRequest($"{GetQuestUri}/{questId}/preview-image", onSuccess, onError));
    }

    private IEnumerator GetSpriteRequest(string requestUri, Action<Sprite> onSuccess, Action<string> onError)
    {
        using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(requestUri);

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            onError?.Invoke(GetErrorMessage(webRequest));
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

        if (texture == null)
        {
            onError?.Invoke("Texture decode failed.");
            yield break;
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        onSuccess?.Invoke(sprite);
    }

    #endregion

    private string GetErrorMessage(UnityWebRequest webRequest)
    {
        string serverMessage = webRequest.downloadHandler?.text ?? "";
        serverMessage = serverMessage.Trim('"');

        if (string.IsNullOrWhiteSpace(serverMessage))
            serverMessage = $"HTTP error. Code: {webRequest.responseCode}, Error: {webRequest.error}";

        return serverMessage;
    }

    private AudioType GetAudioTypeFromExtension(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();

        return ext switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            ".ogg" => AudioType.OGGVORBIS,
            ".aif" => AudioType.AIFF,
            ".aiff" => AudioType.AIFF,
            _ => AudioType.MPEG
        };
    }
}