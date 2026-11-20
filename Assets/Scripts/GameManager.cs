using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGamePaused { get; private set; }
    public bool IsLoadingScene { get; private set; }

    [Header("Configuración de Estado")]
    [SerializeField] private string currentSceneName;

    // Diccionario para almacenar variables persistentes entre escenas
    private readonly Dictionary<string, object> gameVariables = new Dictionary<string, object>();

    // Progreso persistente de árboles de diálogo (dialogueId -> DialogueProgress)
    private readonly Dictionary<string, DialogueProgress> dialogueProgressData = new Dictionary<string, DialogueProgress>();

    [Serializable]
    public class DialogueProgress
    {
        public string currentNodeId;
        public List<string> visitedNodeIds = new List<string>();
        public bool isDialogueCompleted;

        public DialogueProgress Clone()
        {
            return new DialogueProgress
            {
                currentNodeId = currentNodeId,
                visitedNodeIds = visitedNodeIds != null ? new List<string>(visitedNodeIds) : new List<string>(),
                isDialogueCompleted = isDialogueCompleted
            };
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    public void SetPausedState(bool paused)
    {
        IsGamePaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        // Silenciar/atenuar SFX de gameplay, desactivar Input Map, etc.
    }

    public void ReturnToMainMenu(string mainMenuSceneName, bool resetRunData = false)
    {
        IsGamePaused = false;
        Time.timeScale = 1f;

        if (resetRunData)
        {
            ResetRunState();
        }

        LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Limpia solo datos de partida/run si se solicita explícitamente, preservando datos globales si no se indica.
    /// </summary>
    public void ResetRunState(IEnumerable<string> keysToClear = null, bool clearDialogueProgress = false)
    {
        if (keysToClear == null)
        {
            gameVariables.Clear();
        }
        else
        {
            foreach (var key in keysToClear)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    gameVariables.Remove(key);
                }
            }
        }

        if (clearDialogueProgress)
        {
            dialogueProgressData.Clear();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        IsLoadingScene = false;

        // Evita que una escena cargada quede congelada si se cargó estando en pausa
        if (!IsGamePaused && Math.Abs(Time.timeScale) < 0.0001f)
        {
            Time.timeScale = 1f;
        }

        Debug.Log($"[GameManager] Escena cargada: {currentSceneName} (Modo: {mode})");
    }

    #region 1. Scene Select Function

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[GameManager] El nombre de la escena está vacío.");
            return;
        }

        if (IsLoadingScene)
        {
            Debug.LogWarning($"[GameManager] Ya hay una carga de escena en curso. Se ignoró '{sceneName}'.");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            IsLoadingScene = true;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[GameManager] La escena '{sceneName}' no existe en las Build Settings.");
        }
    }

    public void LoadScene(int sceneIndex)
    {
        if (IsLoadingScene)
        {
            Debug.LogWarning($"[GameManager] Ya hay una carga de escena en curso. Se ignoró el índice {sceneIndex}.");
            return;
        }

        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            IsLoadingScene = true;
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"[GameManager] El índice de escena '{sceneIndex}' está fuera de rango en Build Settings.");
        }
    }

    #endregion

    #region 2. Variable Persistence System

    public void SetVariable(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[GameManager] No se puede guardar una variable con clave vacía.");
            return;
        }

        gameVariables[key] = value;
    }

    public T GetVariable<T>(string key, T defaultValue = default)
    {
        if (string.IsNullOrEmpty(key) || !gameVariables.TryGetValue(key, out object rawValue))
        {
            return defaultValue;
        }

        if (rawValue is T typedValue)
        {
            return typedValue;
        }

        if (rawValue == null)
        {
            return defaultValue;
        }

        // Intento de conversión segura para tipos numéricos/primitivos (ej. int -> float)
        try
        {
            if (typeof(T) == typeof(bool) && rawValue is string sBool && bool.TryParse(sBool, out bool parsedBool))
            {
                return (T)(object)parsedBool;
            }

            return (T)Convert.ChangeType(rawValue, typeof(T));
        }
        catch
        {
            Debug.LogWarning($"[GameManager] No se pudo convertir '{key}' ({rawValue.GetType().Name}) a {typeof(T).Name}. Se devuelve el valor por defecto.");
            return defaultValue;
        }
    }

    public bool HasVariable(string key)
    {
        return !string.IsNullOrEmpty(key) && gameVariables.ContainsKey(key);
    }

    public void RemoveVariable(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            gameVariables.Remove(key);
        }
    }

    #endregion

    #region 3. Dialogue Tree Progress System

    public void SaveDialogueProgress(string dialogueId, string currentNodeId, string visitedNodeId = null, bool completed = false)
    {
        if (string.IsNullOrEmpty(dialogueId))
        {
            Debug.LogWarning("[GameManager] dialogueId no puede estar vacío al guardar progreso de diálogo.");
            return;
        }

        if (!dialogueProgressData.TryGetValue(dialogueId, out DialogueProgress progress))
        {
            progress = new DialogueProgress();
            dialogueProgressData[dialogueId] = progress;
        }

        if (!string.IsNullOrEmpty(currentNodeId))
        {
            progress.currentNodeId = currentNodeId;
        }

        // Mantiene true si ya estaba completado, salvo que se pase explícitamente false en un reinicio
        progress.isDialogueCompleted = completed;

        if (!string.IsNullOrEmpty(visitedNodeId) && !progress.visitedNodeIds.Contains(visitedNodeId))
        {
            progress.visitedNodeIds.Add(visitedNodeId);
        }
    }

    public DialogueProgress GetDialogueProgress(string dialogueId, bool returnClone = true)
    {
        if (string.IsNullOrEmpty(dialogueId))
        {
            return null;
        }

        if (dialogueProgressData.TryGetValue(dialogueId, out DialogueProgress progress))
        {
            return returnClone ? progress.Clone() : progress;
        }

        return null;
    }

    #endregion
}
