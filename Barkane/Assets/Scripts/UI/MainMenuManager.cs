using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public int gameStartScene;

    public void StartGame() {
        LevelManager.Instance.LoadLevel(0);
    }

    public void LoadLevel(int level)
    {
        LevelManager.Instance.LoadLevel(level);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
