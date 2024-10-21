using RhythmGameStarter;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, Paused, Ended }
    public GameState currentState = GameState.Menu;

    // Cached references to UI components and game elements - initialized in Awake
    private Button pauseButton;
    private Button stopButton;
    private GameObject startMenu;
    private SongManager songManager;

    private void Awake()
    {
        // Cache references to UI components and game elements here
        startMenu = GameObject.FindWithTag("StartMenu");
        if (startMenu == null)
        {
            Debug.LogError("StartMenu GameObject with tag 'StartMenu' not found.");
        }

        GameObject pauseObj = GameObject.FindWithTag("Pause");
        if (pauseObj != null)
        {
            pauseButton = pauseObj.GetComponent<Button>();
            if (pauseButton == null)
            {
                Debug.LogError("Pause GameObject found, but no Button component attached.");
            }
        }
        else
        {
            Debug.LogError("Pause GameObject with tag 'Pause' not found.");
        }

        GameObject stopObj = GameObject.FindWithTag("Stop");
        if (stopObj != null)
        {
            stopButton = stopObj.GetComponent<Button>();
            if (stopButton == null)
            {
                Debug.LogError("Stop GameObject found, but no Button component attached.");
            }
        }
        else
        {
            Debug.LogError("Stop GameObject with tag 'Stop' not found.");
        }

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

    // Called when start game event is raised
    public void OnGameStart()
    {
        if (currentState == GameState.Menu)
        {
            currentState = GameState.Playing;
            Debug.Log("GameManager: Game state changed to Playing");

            // Enable Pause and Stop buttons
            if (pauseButton != null)
            {
                pauseButton.interactable = true;
                Debug.Log("Pause button set to interactable.");
            }
            else
            {
                Debug.LogError("Pause button is null in OnGameStart.");
            }

            if (stopButton != null)
            {
                stopButton.interactable = true;
                Debug.Log("Stop button set to interactable.");
            }
            else
            {
                Debug.LogError("Stop button is null in OnGameStart.");
            }

            // Hide Start Menu
            if (startMenu != null)
            {
                startMenu.SetActive(false);
                Debug.Log("Start menu set to inactive.");
            }
            else
            {
                Debug.LogError("Start menu is null in OnGameStart.");
            }

            // Play the song using SongManager
            if (songManager != null)
            {
                songManager.PlaySong();
                Debug.Log("SongManager: PlaySong() called.");
            }
            else
            {
                Debug.LogError("SongManager is null in OnGameStart.");
            }

            Debug.Log("Play sequence completed.");
        }
    }

    // Called when pause game event is raised
    public void OnGamePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Debug.Log("GameManager: Game state changed to Paused");
            // Additional logic to pause the game, like disabling player movement
        }
    }

    // Called when end game event is raised
    public void OnGameEnd()
    {
        if (currentState == GameState.Playing || currentState == GameState.Paused)
        {
            currentState = GameState.Ended;
            Debug.Log("GameManager: Game state changed to Ended");
            // Additional logic to end the game, like showing game over UI
        }
    }
}
