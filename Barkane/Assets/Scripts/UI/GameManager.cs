using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int gameStartScene;

    public void StartGame() {
        SceneManager.LoadScene(gameStartScene);
    }

    public void StartLevelSelect() {
        SceneManager.LoadScene(gameStartScene);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
