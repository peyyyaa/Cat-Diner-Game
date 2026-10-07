using TMPro;
using UnityEngine;

// The "How to Play" picture guide.
// Put this on the Canvas (main menu and/or diner) and connect the buttons.
public class HowToPlay : MonoBehaviour
{
    // The whole guide window (hidden until opened).
    public GameObject Panel;

    // One object per page (picture + title + text), in order.
    public GameObject[] Pages;

    public GameObject BackButton;
    public GameObject NextButton;
    public GameObject DoneButton;   // "Got it!" on the last page
    public TMP_Text PageNumberText; // e.g. "2 / 6" (optional)

    // Diner scene: tick both, so the guide opens by itself the very first time
    // someone plays, and the game waits while it's open.
    public bool ShowOnFirstPlay = false;
    public bool PauseGameWhileOpen = false;

    int page = 0;
    const string SeenKey = "CatDiner_SeenHowToPlay";

    void Start()
    {
        Panel.SetActive(false);

        if (ShowOnFirstPlay && PlayerPrefs.GetInt(SeenKey, 0) == 0)
        {
            Open();
        }
    }

    // Button: "How to Play".
    public void Open()
    {
        page = 0;
        Panel.SetActive(true);
        ShowPage();
        SoundManager.Play(Sfx.Click);

        if (PauseGameWhileOpen)
        {
            Time.timeScale = 0f;
        }
    }

    // Button: "Got it!" (or an X in the corner).
    public void Close()
    {
        Panel.SetActive(false);
        SoundManager.Play(Sfx.Click);

        // Remember that the guide was seen, so it doesn't pop up by itself again.
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();

        bool dayOver = GameManager.Instance != null && GameManager.Instance.GameFinished;
        if (PauseGameWhileOpen && !dayOver)
        {
            Time.timeScale = 1f;
        }
    }

    public void NextPage()
    {
        if (page < Pages.Length - 1)
        {
            page++;
            ShowPage();
            SoundManager.Play(Sfx.Click);
        }
    }

    public void PreviousPage()
    {
        if (page > 0)
        {
            page--;
            ShowPage();
            SoundManager.Play(Sfx.Click);
        }
    }

    // Shows only the current page, and only the buttons that make sense.
    void ShowPage()
    {
        for (int i = 0; i < Pages.Length; i++)
        {
            Pages[i].SetActive(i == page);
        }

        bool firstPage = page == 0;
        bool lastPage = page == Pages.Length - 1;

        if (BackButton != null) BackButton.SetActive(!firstPage);
        if (NextButton != null) NextButton.SetActive(!lastPage);
        if (DoneButton != null) DoneButton.SetActive(lastPage);

        if (PageNumberText != null)
        {
            PageNumberText.text = (page + 1) + " / " + Pages.Length;
        }
    }

    // Makes the guide open by itself again on the next game.
    // Called by Reset Days; also available by right-clicking the component's title.
    [ContextMenu("Show guide again next time")]
    public void ForgetSeenFromMenu()
    {
        ForgetSeen();
    }

    public static void ForgetSeen()
    {
        PlayerPrefs.DeleteKey(SeenKey);
        Debug.Log("How to Play will open again on the next first play.");
    }
}