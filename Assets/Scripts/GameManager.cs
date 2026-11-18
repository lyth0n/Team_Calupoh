using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración de Estado")]
    [SerializeField] private string currentSceneName;

    // Diccionario para almacenar variables arbitrarias entre escenas y scripts (ej: Llaves, Puntos, Banderas)
    private Dictionary<string, object> gameVariables = new Dictionary<string, object>();

    // Estructura para guardar el progreso del árbol de diálogo (ID del diálogo -> ID del nodo actual / completado)
    private Dictionary<string, DialogueProgress> dialogueProgressData = new Dictionary<string, DialogueProgress>();

    [Serializable]
    public class DialogueProgress
    {
        public string currentNodeId;
        public List<string> visitedNodeIds = new List<string>();
        public bool isDialogueCompleted;
    }

    private void Awake()
    {
        // Implementación del Patrón Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        Debug.Log($"[GameManager] Escena cargada: {currentSceneName}");
    }

    #region 1. Scene Select Function
    /// <summary>
    /// Carga una escena por su nombre de forma segura.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[GameManager] La escena '{sceneName}' no existe en las Build Settings.");
        }
    }

    /// <summary>
    /// Carga una escena por su índice en las Build Settings.
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    #endregion

    #region 2. Variable Persistence System
    /// <summary>
    /// Establece o actualiza una variable global compartida entre scripts y escenas.
    /// </summary>
    public void SetVariable(string key, object value)
    {
        if (gameVariables.ContainsKey(key))
        {
            gameVariables[key] = value;
        }
        else
        {
            gameVariables.Add(key, value);
        }
    }

    /// <summary>
    /// Obtiene una variable global casteada al tipo de dato esperado.
    /// </summary>
    public T GetVariable<T>(string key, T defaultValue = default)
    {
        if (gameVariables.ContainsKey(key) && gameVariables[key] is T val)
        {
            return val;
        }
        return defaultValue;
    }
    #endregion

    #region 3. Dialogue Tree Progress System
    /// <summary>
    /// Guarda o actualiza el progreso actual de un árbol de diálogo específico.
    /// </summary>
    public void SaveDialogueProgress(string dialogueId, string currentNodeId, string visitedNodeId = null, bool completed = false)
    {
        if (!dialogueProgressData.ContainsKey(dialogueId))
        {
            dialogueProgressData[dialogueId] = new DialogueProgress
            {
                currentNodeId = currentNodeId,
                isDialogueCompleted = completed
            };
        }

        var progress = dialogueProgressData[dialogueId];
        progress.currentNodeId = currentNodeId;
        progress.isDialogueCompleted = completed;

        if (!string.IsNullOrEmpty(visitedNodeId) && !progress.visitedNodeIds.Contains(visitedNodeId))
        {
            progress.visitedNodeIds.Add(visitedNodeId);
        }
    }

    /// <summary>
    /// Obtiene el progreso guardado de un árbol de diálogo.
    /// </summary>
    public DialogueProgress GetDialogueProgress(string dialogueId)
    {
        if (dialogueProgressData.TryGetValue(dialogueId, out var progress))
        {
            return progress;
        }
        return null;
    }
    #endregion
}
