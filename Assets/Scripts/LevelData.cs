using UnityEngine;

// The settings for one day (level). [System.Serializable] shows it in the Inspector.
[System.Serializable]
public class LevelData
{
    public string Name = "Day 1";

    // Money you need to collect before closing time to pass the day.
    public int GoalMoney = 150;

    // How long the day lasts, in seconds.
    public float SessionLength = 90f;

    // Seconds between new customers (smaller = busier).
    public float SpawnInterval = 7f;

    // Customer patience range, in seconds (smaller = harder).
    public float MinPatience = 20f;
    public float MaxPatience = 28f;

    // Chance that customers come as a pair.
    [Range(0f, 1f)]
    public float PairChance = 0.1f;

    // How many customers fit in the waiting line.
    public int MaxWaiting = 3;

    // Empty constructor: Unity uses this when you add a new day in the Inspector.
    public LevelData()
    {
    }

    // Constructor, so the default days can be written in one line each.
    public LevelData(string name, int goal, float length, float spawn, float minPatience, float maxPatience,
                     float pairChance, int maxWaiting)
    {
        Name = name;
        GoalMoney = goal;
        SessionLength = length;
        SpawnInterval = spawn;
        MinPatience = minPatience;
        MaxPatience = maxPatience;
        PairChance = pairChance;
        MaxWaiting = maxWaiting;
    }
}
