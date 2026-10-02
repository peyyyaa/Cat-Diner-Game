using UnityEngine;
using UnityEngine.SceneManagement;  // needed to switch scenes

// Put this on an empty object named MenuController in the MainMenu scene,
// then connect the buttons' On Click () to these methods.
public class MainMenu : MonoBehaviour
{
    // Must match the game scene's file name exactly (without .unity).
    public string GameSceneName = "Diner";

    public void StartGame()
    {
        Time.timeScale = 1f;  // in case we came back from a frozen Game Over
        SceneManager.LoadScene(GameSceneName);
    }

    public void QuitGame()
    {
        // Quit only works in the built game, not inside the Unity Editor,
        // so we also log a message to show the button works.
        Debug.Log("Quit button pressed.");
        Application.Quit();
    }
}
