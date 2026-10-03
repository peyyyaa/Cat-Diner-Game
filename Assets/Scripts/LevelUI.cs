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
            // In the menu: Start continues from the highest unlocked day.
            LevelManager.CurrentLevel = LevelManager.HighestUnlocked();
            MenuLevelText.text = "Day " + (LevelManager.CurrentLevel + 1);
        }

        if (NextLevelButton != null)
        {
            NextLevelButton.SetActive(false);
        }
    }

    void Update()
    {
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

    // Button: Next Day.
    public void NextLevel()
    {
        LevelManager.Instance.GoToNextLevel();
    }

    // Button (main menu, optional): start over from Day 1.
    public void ResetProgress()
    {
        LevelManager.ResetProgress();
        if (MenuLevelText != null)
        {
            MenuLevelText.text = "Day 1";
        }
    }
}
