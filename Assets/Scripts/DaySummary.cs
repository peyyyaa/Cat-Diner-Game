using TMPro;
using UnityEngine;
using UnityEngine.UI;  // needed for Image

// Put this on the GameManager object (or the Canvas) in the Diner scene.
// At closing time it fills in a summary of the day and gives 0-3 stars.
public class DaySummary : MonoBehaviour
{
    // Text on the Game Over card for the day's numbers.
    public TMP_Text SummaryText;

    // Three star images on the Game Over card, left to right.
    public Image[] Stars;
    public Sprite StarFull;
    public Sprite StarEmpty;

    // Stars: 1 = reached the goal, 2 = goal x 1.25, 3 = goal x 1.5.
    public float TwoStarMultiplier = 1.25f;
    public float ThreeStarMultiplier = 1.5f;

    bool shown = false;

    void Update()
    {
        GameManager game = GameManager.Instance;
        if (game == null || !game.GameFinished || shown)
        {
            return;
        }
        shown = true;

        int stars = CountStars(game.Money);
        ShowStars(stars);
        SaveBestStars(stars);

        if (SummaryText != null)
        {
            SummaryText.text =
                "Served: " + game.CustomersServed + "    Lost: " + game.LostCustomers + "\n" +
                "Tips: PHP " + game.TotalTips + "    Best tip: PHP " + game.BestTip + "\n" +
                "VIPs served: " + game.VipsServed;
        }
    }

    // if-else chain: how many stars this money is worth.
    int CountStars(decimal money)
    {
        if (LevelManager.Instance == null)
        {
            return 0;
        }

        decimal goal = LevelManager.Instance.Current.GoalMoney;

        if (money >= goal * (decimal)ThreeStarMultiplier)
        {
            return 3;
        }
        else if (money >= goal * (decimal)TwoStarMultiplier)
        {
            return 2;
        }
        else if (money >= goal)
        {
            return 1;
        }
        return 0;
    }

    void ShowStars(int count)
    {
        if (Stars == null)
        {
            return;
        }

        for (int i = 0; i < Stars.Length; i++)
        {
            if (Stars[i] == null)
            {
                continue;
            }

            bool earned = i < count;
            Stars[i].sprite = earned ? StarFull : StarEmpty;

            if (earned)
            {
                Effects.Pop(Stars[i].gameObject);  // earned stars bounce in
            }
        }
    }

    // Remember the best star rating for each day (saved on this computer).
    void SaveBestStars(int stars)
    {
        if (LevelManager.Instance == null)
        {
            return;
        }

        int day = LevelManager.CurrentLevel;
        if (stars > BestStars(day))
        {
            PlayerPrefs.SetInt("CatDiner_Stars_Day" + day, stars);
            PlayerPrefs.Save();
        }
    }

    // static: the menu (or anything else) can ask for a day's best stars.
    public static int BestStars(int dayIndex)
    {
        return PlayerPrefs.GetInt("CatDiner_Stars_Day" + dayIndex, 0);
    }
}
