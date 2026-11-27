using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Meta de Interacciones")]
    [Tooltip("GameObject que se mostrará cuando el contador de interacciones llegue a la meta.")]
    [SerializeField] private GameObject fiveInteractionsPanel;
    [SerializeField] private int targetInteractions = 5;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Cursor Settings (Optional)")]
    [SerializeField] private bool manageCursorLock = false;

    public bool IsPaused { get; private set; }
    private bool targetReachedTriggered = false;

    private void Awake()
    {
        // Ensure gameplay starts unpaused and the menus start hidden
        IsPaused = false;
        targetReachedTriggered = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (fiveInteractionsPanel != null)
        {
            fiveInteractionsPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Safety net if this object is destroyed or disabled while paused
        if (IsPaused)
        {
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        CheckInteractionGoal();

        if (!targetReachedTriggered && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void CheckInteractionGoal()
    {
        if (targetReachedTriggered) return;

        if (InteractableObject.TotalInteractions >= targetInteractions)
        {
            TriggerFiveInteractionsPause();
        }
    }

    public void TriggerFiveInteractionsPause()
    {
        targetReachedTriggered = true;
        IsPaused = true;
        Time.timeScale = 0f;

        PlayVictoryFanfareSound();

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (fiveInteractionsPanel != null)
        {
            fiveInteractionsPanel.SetActive(true);
        }

        if (manageCursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Guarantee normal time flow whenever a new scene loads
        Time.timeScale = 1f;
        IsPaused = false;
        targetReachedTriggered = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (fiveInteractionsPanel != null)
        {
            fiveInteractionsPanel.SetActive(false);
        }
    }

    // Hook this up to a UI Button ("Pause" or "Toggle Pause")
    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // Hook this up to a dedicated "Pause" UI Button if preferred
    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        if (manageCursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Hook this up to your "Resume" UI Button
    public void ResumeGame()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (fiveInteractionsPanel != null)
        {
            fiveInteractionsPanel.SetActive(false);
        }

        if (manageCursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Hook this up to your "Main Menu" UI Button
    public void ReturnToMainMenu()
    {
        // Reset timeScale and pause state BEFORE loading the menu scene
        IsPaused = false;
        Time.timeScale = 1f;
        InteractableObject.ResetInteractionCount();

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (fiveInteractionsPanel != null)
        {
            fiveInteractionsPanel.SetActive(false);
        }

        if (manageCursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void PlayVictoryFanfareSound()
    {
        if (SFXManager.Instance == null) return;
        SFXManager.Instance.PlayFanfareSound();
    }
}