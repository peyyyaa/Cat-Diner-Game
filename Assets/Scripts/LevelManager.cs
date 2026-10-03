using UnityEngine;
using UnityEngine.SceneManagement;  // needed to reload the scene for the next day

// Put this on the GameManager object in the Diner scene.
// It sets up the current day from the Levels list and remembers which days you've unlocked.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    // static: keeps its value when the scene reloads (Play Again / Next Day).
    // -1 means "not chosen yet": start at the highest day you've unlocked.
    public static int CurrentLevel = -1;

    // Drag the CustomerSpawner here so each day can change how busy it is.
    public CustomerSpawner Spawner;

    // Every day, from easiest to hardest. You can change these in the Inspector.
    public LevelData[] Levels = DefaultLevels();

    // Set at closing time: did the player reach the goal?
    public bool LastDayPassed { get; private set; }

    // The key used to save progress on this computer (PlayerPrefs).
    const string SaveKey = "CatDiner_HighestDay";

    // Property: the settings of the day being played.
    public LevelData Current
    {
        get { return Levels[CurrentLevel]; }
    }

    public bool HasNextLevel
    {
        get { return CurrentLevel + 1 < Levels.Length; }
    }

    void Awake()
    {
        Instance = this;

        if (CurrentLevel < 0)
        {
            CurrentLevel = HighestUnlocked();
        }
        CurrentLevel = Mathf.Clamp(CurrentLevel, 0, Levels.Length - 1);

        ApplyLevel();
    }

    // Copies this day's settings into the GameManager and the spawner.
    // (Runs in Awake, so it happens before their Start.)
    void ApplyLevel()
    {
        LevelData day = Current;

        GameManager gameManager = GetComponent<GameManager>();
        if (gameManager != null)
        {
            gameManager.SessionLength = day.SessionLength;
        }

        if (Spawner != null)
        {
            Spawner.SpawnInterval = day.SpawnInterval;
            Spawner.MinPatience = day.MinPatience;
            Spawner.MaxPatience = day.MaxPatience;
            Spawner.PairChance = day.PairChance;
            Spawner.MaxWaiting = day.MaxWaiting;
        }

        Debug.Log("Starting " + day.Name + ". Goal: ₱" + day.GoalMoney);
    }

    // Called by the GameManager at closing time.
    public bool FinishDay(decimal money)
    {
        LastDayPassed = money >= Current.GoalMoney;

        // Passed: unlock the next day (and save it).
        if (LastDayPassed && HasNextLevel && CurrentLevel + 1 > HighestUnlocked())
        {
            PlayerPrefs.SetInt(SaveKey, CurrentLevel + 1);
            PlayerPrefs.Save();
        }

        return LastDayPassed;
    }

    // Button: go to the next day.
    public void GoToNextLevel()
    {
        if (HasNextLevel)
        {
            CurrentLevel++;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // The highest day unlocked on this computer (0 = Day 1).
    public static int HighestUnlocked()
    {
        return PlayerPrefs.GetInt(SaveKey, 0);
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        CurrentLevel = 0;
    }

    // array: the default five days, from easy to hard.
    static LevelData[] DefaultLevels()
    {
        return new LevelData[]
        {
            //            name     goal  length spawn  patience   pairs  line
            new LevelData("Day 1", 150,  90f,   7f,    20f, 28f,  0.10f, 3),
            new LevelData("Day 2", 250,  90f,   6f,    17f, 24f,  0.25f, 4),
            new LevelData("Day 3", 350,  100f,  5f,    15f, 21f,  0.35f, 4),
            new LevelData("Day 4", 450,  110f,  4.5f,  13f, 19f,  0.45f, 5),
            new LevelData("Day 5", 550,  120f,  4f,    11f, 17f,  0.55f, 5),
        };
    }
}
