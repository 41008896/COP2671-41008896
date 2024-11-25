using RhythmGameStarter;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, Paused, Ended }
    public GameState currentState = GameState.Menu;

    private Dictionary<string, GameObject> uiElements = new Dictionary<string, GameObject>();

    private GameObject ui;
    private GameObject startMenu;
    private SongManager songManager;

    // Cached references to frequently used UI elements
    private Button pauseButton;
    private Button stopButton;
    private Button resumeButton;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI songPercentageText;
    private Animator songPercentageAnimator;

    private void Awake()
    {
        // Cache the UI GameObject
        ui = GameObject.FindWithTag("UI");
        if (ui == null)
        {
            Debug.LogError("UI GameObject with tag 'UI' not found.");
            return;
        }

        // Find and cache all components within the UI object by name
        CacheUIComponents();
        RegisterUIComponents();

        GameObject songManagerObj = GameObject.FindWithTag("Game");
        if (songManagerObj != null)
        {
            songManager = songManagerObj.GetComponent<SongManager>();
            if (songManager == null)
            {
                Debug.LogError("No SongManager component found on GameObject with tag 'Game'.");
            }
        }
        else
        {
            Debug.LogError("GameObject with tag 'Game' not found.");
        }
    }

    private void CacheUIComponents()
    {
        Transform[] allUIComponents = ui.GetComponentsInChildren<Transform>(true); // Include inactive objects
        foreach (Transform component in allUIComponents)
        {
            uiElements[component.name] = component.gameObject; // Cache every GameObject by its name
            Debug.Log("Cached UI Element: " + component.name);
        }
    }

    private void RegisterUIComponents()
    {
        // Assign frequently used UI elements to class-level variables
        if (!uiElements.TryGetValue("StartMenu", out startMenu))
            Debug.LogError("StartMenu GameObject not found in cached UI elements.");

        if (uiElements.TryGetValue("Pause", out GameObject pauseObj))
            pauseButton = pauseObj.GetComponent<Button>();

        if (uiElements.TryGetValue("Stop", out GameObject stopObj))
            stopButton = stopObj.GetComponent<Button>();

        if (uiElements.TryGetValue("Resume", out GameObject resumeObj))
            resumeButton = resumeObj.GetComponent<Button>();

        if (uiElements.TryGetValue("Score", out GameObject scoreObj))
            scoreText = scoreObj.GetComponent<TMPro.TextMeshProUGUI>();

        if (uiElements.TryGetValue("Song Precentage", out GameObject songPercentageObj)) // Keeping the typo as requested
        {
            songPercentageText = songPercentageObj.GetComponent<TMPro.TextMeshProUGUI>();
            songPercentageAnimator = songPercentageObj.GetComponent<Animator>();
        }

        // Log errors if any critical UI element is missing
        if (pauseButton == null || stopButton == null || resumeButton == null ||
            scoreText == null || songPercentageText == null)
        {
            Debug.LogError("One or more required UI elements are missing or incorrectly set in the hierarchy.");
        }
    }

    public void OnGameStart()
    {
        if (currentState == GameState.Menu)
        {
            currentState = GameState.Playing;
            Debug.Log("GameManager: Game state changed to Playing");

            // Enable Pause and Stop buttons
            pauseButton.interactable = true;
            stopButton.interactable = true;
            Debug.Log("Pause and Stop buttons set to interactable.");

            // Hide Start Menu
            startMenu?.SetActive(false);
            Debug.Log("Start menu set to inactive.");

            // Play the song using SongManager
            songManager?.PlaySong();
            Debug.Log("SongManager: PlaySong() called.");
        }
    }

    public void OnGamePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Debug.Log("GameManager: Game state changed to Paused");

            // Toggle Resume and Pause button visibility
            resumeButton.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(false);

            // Pause the song
            songManager?.PauseSong();
            Debug.Log("SongManager: PauseSong() called.");
        }
    }

    public void OnGameResume()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Debug.Log("GameManager: Game state changed to Playing");

            // Toggle Pause and Resume button visibility
            pauseButton.gameObject.SetActive(true);
            resumeButton.gameObject.SetActive(false);

            // Resume the song
            songManager?.ResumeSong();
            Debug.Log("SongManager: ResumeSong() called.");
        }
    }

    public void OnGameStop()
    {
        // Toggle button visibility
        pauseButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(true);

        // Disable interactivity on Stop and Pause buttons
        stopButton.interactable = false;
        pauseButton.interactable = false;

        // Reset Score and Song Percentage text
        scoreText.text = "0000";
        songPercentageText.text = "0%";

        // Reset Animator Trigger on Song Percentage if needed
        songPercentageAnimator?.SetTrigger("Reset");

        // Show Start Menu
        startMenu?.SetActive(true);

        // Stop the song
        songManager?.StopSong();
        Debug.Log("SongManager: StopSong() called.");

        // Set game state to Ended
        currentState = GameState.Ended;
        Debug.Log("GameManager: Game state changed to Ended");
    }
}
