using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<string> OnSceneLoadStarted;
    public static event Action<string> OnSceneLoaded;

    private bool isLoadingScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Multiple volatile GameManagers in one scene. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Loads a scene by its exact name in Build Settings.
    /// This GameManager will be destroyed as the new scene loads.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[GameManager] Scene name is null or empty.");
            return;
        }

        if (isLoadingScene)
        {
            return;
        }

        isLoadingScene = true;
        OnSceneLoadStarted?.Invoke(sceneName);
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Loads a scene by its build index in Build Settings.
    /// </summary>
    public void LoadScene(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[GameManager] Build index {buildIndex} is out of range.");
            return;
        }

        if (isLoadingScene)
        {
            return;
        }

        string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        isLoadingScene = true;
        OnSceneLoadStarted?.Invoke(scenePath);
        SceneManager.LoadScene(buildIndex);
    }

    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadSceneAsync(string sceneName, Action<float> onProgress = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[GameManager] Scene name is null or empty.");
            return;
        }

        if (isLoadingScene)
        {
            return;
        }

        StartCoroutine(LoadSceneAsyncRoutine(sceneName, onProgress));
    }

    private IEnumerator LoadSceneAsyncRoutine(string sceneName, Action<float> onProgress)
    {
        isLoadingScene = true;
        OnSceneLoadStarted?.Invoke(sceneName);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            Debug.LogError($"[GameManager] Failed to start async load for '{sceneName}'. Check Build Settings.");
            isLoadingScene = false;
            yield break;
        }

        while (!operation.isDone)
        {
            float normalizedProgress = Mathf.Clamp01(operation.progress / 0.9f);
            onProgress?.Invoke(normalizedProgress);
            yield return null;
        }

        onProgress?.Invoke(1f);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoadingScene = false;
        OnSceneLoaded?.Invoke(scene.name);
    }

    public string GetCurrentSceneName() => SceneManager.GetActiveScene().name;
    public int GetCurrentSceneIndex() => SceneManager.GetActiveScene().buildIndex;
}