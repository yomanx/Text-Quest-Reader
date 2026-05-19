using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PictureNode : MonoBehaviour
{
    [SerializeField] private Image outerPicture;
    [SerializeField] private Image innerPicture;
    [SerializeField] private Sprite defaultSprite;

    private Animator animator;
    private string lastPictureName;
    private Coroutine loadPictureCoroutine;

    private readonly Dictionary<string, Sprite> spriteCache = new();
    private string remoteQuestFolder;

    private static readonly string[] imageExtensions =
    {
        ".png",
        ".jpg",
        ".jpeg"
    };

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ClearPicturesColor();
        ApplyDefaultSpritesIfNeeded();
    }

    public void InitImages(string pictureName, string questName)
    {
        if (loadPictureCoroutine != null)
            StopCoroutine(loadPictureCoroutine);

        loadPictureCoroutine = StartCoroutine(InitImagesRoutine(pictureName, questName));
    }

    private IEnumerator InitImagesRoutine(string pictureName, string questName)
    {
        Sprite sprite = null;

        if (!string.IsNullOrEmpty(pictureName))
            yield return StartCoroutine(LoadSprite(pictureName, questName, loadedSprite => sprite = loadedSprite));

        Sprite finalSprite = GetSafeSprite(sprite);
        innerPicture.sprite = finalSprite;
        outerPicture.sprite = finalSprite;

        lastPictureName = pictureName;
        loadPictureCoroutine = null;
    }

    public void ClearPicturesColor()
    {
        outerPicture.color = Color.white;
        innerPicture.color = Color.white;
    }

    public void SetNewPicture(string pictureName, string questName, bool mayBeSame)
    {
        if (lastPictureName == pictureName && !mayBeSame)
            return;

        if (loadPictureCoroutine != null)
            StopCoroutine(loadPictureCoroutine);

        loadPictureCoroutine = StartCoroutine(SetNewPictureRoutine(pictureName, questName));
    }

    private IEnumerator SetNewPictureRoutine(string pictureName, string questName)
    {
        Sprite sprite = null;

        if (!string.IsNullOrEmpty(pictureName))
            yield return StartCoroutine(LoadSprite(pictureName, questName, loadedSprite => sprite = loadedSprite));

        innerPicture.sprite = GetSafeSprite(sprite);

        if (animator != null)
            animator.Play("FadePictures");

        lastPictureName = pictureName;
        loadPictureCoroutine = null;
    }

    public void SetSpriteRemote(Sprite sprite, string questName, bool mayBeSame = false)
    {
        string pictureName = sprite != null ? sprite.name : "default";
        string pictureKey = $"{questName}/{pictureName}";

        if (lastPictureName == pictureKey && !mayBeSame)
            return;

        if (loadPictureCoroutine != null)
            StopCoroutine(loadPictureCoroutine);

        loadPictureCoroutine = StartCoroutine(SetSpriteRemoteRoutine(sprite, pictureKey));
    }

    private IEnumerator SetSpriteRemoteRoutine(Sprite sprite, string pictureKey)
    {
        yield return null;

        innerPicture.sprite = GetSafeSprite(sprite);

        if (animator != null)
            animator.Play("FadePictures");

        lastPictureName = pictureKey;
        loadPictureCoroutine = null;
    }

    public void Callback()
    {
        outerPicture.sprite = GetSafeSprite(innerPicture.sprite);
    }

    private Sprite GetSafeSprite(Sprite sprite)
    {
        return sprite != null ? sprite : defaultSprite;
    }

    private void ApplyDefaultSpritesIfNeeded()
    {
        if (innerPicture.sprite == null)
            innerPicture.sprite = defaultSprite;

        if (outerPicture.sprite == null)
            outerPicture.sprite = defaultSprite;
    }

    private IEnumerator LoadSprite(string pictureNameWithoutExtensionOrWithIt, string questName, Action<Sprite> onLoaded)
    {
        string resolvedPath = FindImagePath(pictureNameWithoutExtensionOrWithIt, questName);

        if (string.IsNullOrEmpty(resolvedPath))
        {
            Debug.Log($"[PictureNode] Image not on disk, procedural background takes over. Quest: {questName}, Name: {pictureNameWithoutExtensionOrWithIt}");
            onLoaded?.Invoke(defaultSprite);
            yield break;
        }

        string cacheKey = resolvedPath;

        if (spriteCache.TryGetValue(cacheKey, out Sprite cachedSprite) && cachedSprite != null)
        {
            onLoaded?.Invoke(cachedSprite);
            yield break;
        }

        string url = ToFileUrl(resolvedPath);

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Failed to load image: {resolvedPath}\n{request.error}");
            onLoaded?.Invoke(defaultSprite);
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        if (texture == null)
        {
            Debug.LogWarning($"Failed to decode image: {resolvedPath}");
            onLoaded?.Invoke(defaultSprite);
            yield break;
        }

        texture.name = Path.GetFileNameWithoutExtension(resolvedPath);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        sprite.name = texture.name;
        spriteCache[cacheKey] = sprite;

        onLoaded?.Invoke(sprite);
    }

    private string FindImagePath(string pictureNameWithoutExtensionOrWithIt, string questName)
    {
        if (!string.IsNullOrEmpty(remoteQuestFolder))
        {
            string remoteImagesFolder = Path.Combine(remoteQuestFolder, "Images");
            string remotePath = FindImagePathInFolder(remoteImagesFolder, pictureNameWithoutExtensionOrWithIt);

            if (!string.IsNullOrEmpty(remotePath))
                return remotePath;
        }

        string persistentFolder = Path.Combine(Application.persistentDataPath, "Quests", questName, "Images");
        string streamingFolder = Path.Combine(Application.streamingAssetsPath, "Quests", questName, "Images");

        string path = FindImagePathInFolder(persistentFolder, pictureNameWithoutExtensionOrWithIt);

        if (!string.IsNullOrEmpty(path))
            return path;

        return FindImagePathInFolder(streamingFolder, pictureNameWithoutExtensionOrWithIt);
    }

    private string FindImagePathInFolder(string baseFolder, string pictureNameWithoutExtensionOrWithIt)
    {
        if (!Directory.Exists(baseFolder) || string.IsNullOrWhiteSpace(pictureNameWithoutExtensionOrWithIt))
            return null;

        string name = Path.GetFileNameWithoutExtension(pictureNameWithoutExtensionOrWithIt);

        foreach (string ext in imageExtensions)
        {
            string fullPath = Path.Combine(baseFolder, name + ext);

            if (File.Exists(fullPath))
                return fullPath;
        }

        return null;
    }

    private static string ToFileUrl(string path)
    {
        string normalizedPath = path.Replace("\\", "/");

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return "file:///" + normalizedPath;
#else
        return "file://" + normalizedPath;
#endif
    }

    public void ClearLoadedQuestPictures(string questName)
    {
        if (string.IsNullOrEmpty(questName))
            return;

        List<string> keysToRemove = new();

        foreach (var pair in spriteCache)
        {
            string normalized = pair.Key.Replace("\\", "/");
            string questPart = "/Quests/" + questName + "/";

            if (normalized.Contains(questPart, StringComparison.OrdinalIgnoreCase))
            {
                if (pair.Value != null)
                {
                    if (pair.Value.texture != null)
                        Destroy(pair.Value.texture);

                    Destroy(pair.Value);
                }

                keysToRemove.Add(pair.Key);
            }
        }

        foreach (string key in keysToRemove)
            spriteCache.Remove(key);
    }

    public void ClearAllLoadedPictures()
    {
        foreach (var pair in spriteCache)
        {
            if (pair.Value != null)
            {
                if (pair.Value.texture != null)
                    Destroy(pair.Value.texture);

                Destroy(pair.Value);
            }
        }

        spriteCache.Clear();
        ClearPictures();
    }

    public void SetRemoteQuestFolder(string folderPath) => remoteQuestFolder = folderPath;

    public void ClearRemoteQuestFolder()
    {
        if (!string.IsNullOrEmpty(remoteQuestFolder))
            ClearLoadedPicturesFromFolder(remoteQuestFolder);

        remoteQuestFolder = null;
    }

    private void ClearLoadedPicturesFromFolder(string folderPath)
    {
        string normalizedFolder = folderPath.Replace("\\", "/");
        List<string> keysToRemove = new();

        foreach (var pair in spriteCache)
        {
            string normalizedKey = pair.Key.Replace("\\", "/");

            if (!normalizedKey.StartsWith(normalizedFolder, StringComparison.OrdinalIgnoreCase))
                continue;

            if (pair.Value != null)
            {
                if (pair.Value.texture != null)
                    Destroy(pair.Value.texture);

                Destroy(pair.Value);
            }

            keysToRemove.Add(pair.Key);
        }

        foreach (string key in keysToRemove)
            spriteCache.Remove(key);
    }

    public void ClearPictures()
    {
        innerPicture.sprite = defaultSprite;
        outerPicture.sprite = defaultSprite;
        lastPictureName = null;
    }
}