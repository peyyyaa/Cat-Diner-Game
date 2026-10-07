using TMPro;
using UnityEngine;

// Shows the day and goal, and the result at closing time.
// In the Diner scene: put it on the GameManager object (or the Canvas).
// In the MainMenu scene: put it on the MenuController and only fill Menu Level Text.
public class LevelUI : MonoBehaviour
{
    [Header("Diner scene")]
    public TMP_Text LevelText;          // HUD, e.g. "Day 2 / Goal: PHP 250"
    public TMP_Text ResultText;         // on the Game Over card
    public GameObject NextLevelButton;  // only shown when the day was passed

    [Header("Main menu (optional)")]
    public TMP_Text MenuLevelText;      // e.g. "Day 3"

    bool resultShown = false;

    void Start()
    {
        if (MenuLevelText != null)
        {
            // In the menu: start on the highest unlocked day.
            LevelManager.CurrentLevel = LevelManager.HighestUnlocked();
        }

        if (NextLevelButton != null)
        {
            NextLevelButton.SetActive(false);
        }
    }

    void Update()
    {
        if (MenuLevelText != null)
        {
            UpdateMenuLabel();
        }

        LevelManager levels = LevelManager.Instance;
        GameManager game = GameManager.Instance;
        if (levels == null || game == null)
        {
            return;
        }

        if (LevelText != null)
        {
            LevelText.text = levels.Current.Name + "\nGoal: PHP " + levels.Current.GoalMoney;
        }

        if (game.GameFinished && !resultShown)
        {
            resultShown = true;
            bool passed = levels.LastDayPassed;

            if (ResultText != null)
            {
                if (passed && levels.HasNextLevel)
                {
                    ResultText.text = levels.Current.Name + " complete!";
                }
                else if (passed)
                {
                    ResultText.text = "You beat every day!";
                }
                else
                {
                    ResultText.text = "Goal not reached... try again!";
                }
            }

            if (NextLevelButton != null)
            {
                NextLevelButton.SetActive(passed && levels.HasNextLevel);
            }
        }
    }

    // Main menu: keep the label up to date every frame
    // (so it changes straight away after a reset or a day change).
    void UpdateMenuLabel()
    {
        // Never show a day that isn't unlocked yet.
        int highest = LevelManager.HighestUnlocked();
        LevelManager.CurrentLevel = Mathf.Clamp(LevelManager.CurrentLevel, 0, highest);

        int day = LevelManager.CurrentLevel;
        int stars = DaySummary.BestStars(day);

        string label = "Day " + (day + 1);
        if (stars > 0)
        {
            label += "  (" + stars + "/3 stars)";
        }
        MenuLevelText.text = label;
    }

    // Main menu buttons: pick an earlier day, or go back up to the highest unlocked one.
    public void MenuPreviousDay()
    {
        LevelManager.CurrentLevel = Mathf.Max(0, LevelManager.CurrentLevel - 1);
        SoundManager.Play(Sfx.Click);
    }

    public void MenuNextDay()
    {
        LevelManager.CurrentLevel = Mathf.Min(LevelManager.HighestUnlocked(), LevelManager.CurrentLevel + 1);
        SoundManager.Play(Sfx.Click);
    }

    // Button: Next Day.
    public void NextLevel()
    {
        LevelManager.Instance.GoToNextLevel();
    }

    // Button (main menu, optional): start over from Day 1.
    public void ResetProgress()
    {
        LevelManager.ResetProgress();  // the label updates by itself next frame
    }
}