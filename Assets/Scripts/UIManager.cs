using TMPro;        // needed for TextMeshPro text
using UnityEngine;

// Put this on the GameManager object (or the Canvas) and drag
// each text object into its matching slot in the Inspector.
public class UIManager : MonoBehaviour
{
    public TMP_Text MoneyText;
    public TMP_Text ServedText;
    public TMP_Text LostText;
    public TMP_Text TimerText;
    public TMP_Text GameOverText;

    void Start()
    {
        // Hide the Game Over message until the game actually ends.
        GameOverText.gameObject.SetActive(false);
    }

    // Refresh the text every frame so it always matches the GameManager.
    void Update()
    {
        GameManager gm = GameManager.Instance;

        // "PHP" instead of the peso sign: the default font has no ₱ symbol.
        MoneyText.text = "Money: PHP " + gm.Money;
        ServedText.text = "Served: " + gm.CustomersServed;
        LostText.text = "Lost: " + gm.LostCustomers;

        // Mathf.CeilToInt rounds up, so the timer shows 1 until it truly hits 0.
        TimerText.text = "Time: " + Mathf.CeilToInt(gm.TimeLeft);

        if (gm.GameFinished)
        {
            GameOverText.gameObject.SetActive(true);
            GameOverText.text = "GAME OVER\nEarned PHP " + gm.Money +
                                "\nServed " + gm.CustomersServed + " | Lost " + gm.LostCustomers;
        }
    }
}
