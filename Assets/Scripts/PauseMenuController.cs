using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Cursor Settings (Optional)")]
    [SerializeField] private bool manageCursorLock = false;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        // Ensure gameplay starts unpaused and the menu starts hidden
        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // Safety net in case the object is destroyed while paused
        if (IsPaused)
        {
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Always guarantee normal time flow when entering any scene
        Time.timeScale = 1f;
        IsPaused = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
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

    // Hook this up to your Pause button if you want an explicit Pause-only method
    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f; // Freezes scaled time (Time.deltaTime / FixedUpdate)

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPausedState(true);
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPausedState(false);
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
        // Unpause BEFORE changing scenes so nothing loads into a frozen timeScale
        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (manageCursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}