using TMPro;                         // needed for TextMeshPro text
using UnityEngine;
using UnityEngine.SceneManagement;  // needed to switch scenes

// Put this on the GameManager object and drag each UI object
// into its matching slot in the Inspector.
public class UIManager : MonoBehaviour
{
    public TMP_Text MoneyText;
    public TMP_Text ServedText;
    public TMP_Text LostText;
    public TMP_Text TimerText;

    // The whole Game Over screen (panel + text + buttons), and the text inside it.
    public GameObject GameOverPanel;
    public TMP_Text GameOverText;

    // Must match the menu scene's file name exactly (without .unity).
    public string MenuSceneName = "MainMenu";

    void Start()
    {
        // Hide the Game Over screen until the game actually ends.
        GameOverPanel.SetActive(false);
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

        if (gm.GameFinished && !GameOverPanel.activeSelf)
        {
            GameOverPanel.SetActive(true);
            GameOverText.text = "GAME OVER\nEarned PHP " + gm.Money +
                                "\nServed " + gm.CustomersServed + " | Lost " + gm.LostCustomers;
        }
    }

    // Button: reload the current scene to start a fresh session.
    public void PlayAgain()
    {
        Time.timeScale = 1f;  // un-freeze time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Button: go back to the main menu.
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MenuSceneName);
    }
}