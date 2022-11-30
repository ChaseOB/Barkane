using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BarkaneEditor;
using UnityEngine.InputSystem;


public class LevelManager : Singleton<LevelManager>
{
    private GameObject level;
    private GameObject instantiatedLevel;
    [SerializeField] private GameObject playerPrefab;
    private GameObject playerInstance = null;
    [SerializeField] private GameObject levelSwitchScreen; //C: Used to hide VFX Loading
    [SerializeField] private List<GameObject> levelList;
    public int currLevelIndex = 0;

    public List<int> levelScenes = new List<int>();

    public Theme currLevelTheme;

    public ImageAnimator imageAnimator;

    private void Awake() 
    {
        InitializeSingleton(this.gameObject);
        DontDestroyOnLoad(gameObject);
    }

    
    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("loaded scene " + scene.name);
        if(levelScenes.Contains(scene.buildIndex))
            LoadLevel(currLevelIndex, true);
    }

    public void LoadLevel(int index)
    {
        currLevelIndex = index;
        SceneManager.LoadScene(1);
    }

    public void LoadLevel(int index, bool set = false)
    {
        if(set)
            currLevelIndex = index;
        if (index >= levelList.Count) 
            OnCompleteLastLevel();
        else
            SwitchLevel(levelList[currLevelIndex]);
    }

    public void LoadNextLevel()
    {
        LoadLevel(++currLevelIndex);
    }

    //C: allows us to do something special on last level completion
    public void OnCompleteLastLevel()
    {
        UIManager.Instance.endLevelGroup.SetActive(false);
        LevelManager.Instance.UnloadLevel();
        SceneManager.LoadScene(0);
    }

    public void SwitchLevel(GameObject level) {
        StartCoroutine(ShowTransition());
        this.level = level;
        if (instantiatedLevel != null) {
            Destroy(instantiatedLevel);
        }
        SpawnLevel(level);
    }

    public void SwitchLevel(string level) {
        StartCoroutine(ShowTransition());
        this.level = (GameObject) Resources.Load("Prefabs/" + level);
        if (instantiatedLevel != null) {
            Destroy(instantiatedLevel);
        }
        SpawnLevel(this.level);
    }

    public void ResetLevel() {
        StartCoroutine(ShowTransition());
        if (instantiatedLevel != null) 
        {
            Destroy(instantiatedLevel);
            SpawnLevel(level); 
        }
    }


    public void SpawnLevel(GameObject level)
    {
        instantiatedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
        FoldablePaper paper = instantiatedLevel.GetComponent<FoldablePaper>();
        Transform playerPos = paper.playerSpawn;
        if(playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }
        playerInstance= Instantiate(playerPrefab, playerPos.position, Quaternion.identity);

        FollowTarget.Instance.SetTargetAndPosition(playerInstance.GetComponent<PlayerMovement>().raycastStart);    
        VFXManager.Instance.Refresh();
        FindObjectOfType<TileSelector>().ReloadReferences();
    }

    //C: used when switching from level back to a non-level scene
    public void UnloadLevel()
    {
        StartCoroutine(ShowTransition());
        if (instantiatedLevel != null) 
            Destroy(instantiatedLevel);
        instantiatedLevel = null;
        if(playerInstance != null)
            Destroy(playerInstance);
        playerInstance = null;
    }

    public void SetTransitionScreen(bool val)
    {
        levelSwitchScreen.SetActive(val);
    }

    //C: this is what we're doing for now lol
    private IEnumerator ShowTransition()
    {
        ActionLockManager.Instance.ForceTakeLock(this);
        SetTransitionScreen(true);
        if(UIManager.Instance != null)
            UIManager.Instance.ToggleGroup(false);
        float elapsedTime = 0.0f;
        float time = 2.5f;
        imageAnimator.Play();
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SetTransitionScreen(false);
        imageAnimator.Stop();
        if(UIManager.Instance != null)
            UIManager.Instance.ToggleGroup(true);
        ActionLockManager.Instance.ForceRemoveLock();
    }

    public void ReturnToMenu() {
        LevelManager.Instance.UnloadLevel();
        SceneManager.LoadScene(0);
    }

    public void EndLevel()
    {
        instantiatedLevel.GetComponent<FoldablePaper>().isComplete = true;
    }

}