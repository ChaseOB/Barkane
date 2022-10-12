using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public int gameStartScene;

    public void StartGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameStartScene);
    }

    public void StartLevelSelect() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameStartScene);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
