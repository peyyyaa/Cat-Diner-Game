using UnityEngine;
using UnityEngine.UI;   // needed for Slider and Toggle

// Put this on the Canvas (main menu and/or diner).
// Connect the gear button's On Click to OpenSettings, and the close button to CloseSettings.
public class SettingsMenu : MonoBehaviour
{
    // The whole settings window (hidden until the gear is clicked).
    public GameObject SettingsPanel;

    public Slider MusicSlider;
    public Slider SfxSlider;
    public Toggle FullscreenToggle;

    // Tick this in the Diner scene: the game pauses while Settings is open.
    public bool PauseGameWhileOpen = false;

    void Start()
    {
        SettingsPanel.SetActive(false);

        // Show the saved values, then listen for changes.
        // AddListener = "when this changes, call this method" (same idea as On Click in the Inspector).
        if (MusicSlider != null)
        {
            MusicSlider.value = SoundManager.SavedMusicVolume();
            MusicSlider.onValueChanged.AddListener(SoundManager.SetMusicVolume);
        }

        if (SfxSlider != null)
        {
            SfxSlider.value = SoundManager.SavedSfxVolume();
            SfxSlider.onValueChanged.AddListener(SoundManager.SetSfxVolume);
        }

        if (FullscreenToggle != null)
        {
            FullscreenToggle.isOn = Screen.fullScreen;
            FullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }

    void Update()
    {
        // The Escape key also opens / closes Settings.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SettingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
    }

    // Button: the gear.
    public void OpenSettings()
    {
        SettingsPanel.SetActive(true);
        SoundManager.Play(Sfx.Click);

        if (PauseGameWhileOpen)
        {
            Time.timeScale = 0f;  // freezes patience, cooking, walking, the timer...
        }
    }

    // Button: the X.
    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
        SoundManager.Play(Sfx.Click);
        PlayerPrefs.Save();  // write the volume settings to disk

        // Un-pause, unless the day already ended (it stays frozen on the Game Over screen).
        bool dayOver = GameManager.Instance != null && GameManager.Instance.GameFinished;
        if (PauseGameWhileOpen && !dayOver)
        {
            Time.timeScale = 1f;
        }
    }

    void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;  // only noticeable in the built .exe, not the Editor
    }

    // Button: Reset Progress (back to Day 1).
    public void ResetProgress()
    {
        LevelManager.ResetProgress();
        SoundManager.Play(Sfx.Click);
        Debug.Log("Progress reset: back to Day 1.");
    }
}
