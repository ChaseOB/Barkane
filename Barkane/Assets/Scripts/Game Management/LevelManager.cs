using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BarkaneEditor;


public class LevelManager : Singleton<LevelManager>
{
    private Level level;
    private GameObject instantiatedLevel;

    [SerializeField] private GameObject playerPrefab;
    private GameObject playerInstance = null;

    [SerializeField] private GameObject levelSwitchScreen; //C: Used to hide VFX Loading
    [SerializeField] private List<Level> levelList;
    public int currLevelIndex = 0;

    public List<int> levelScenes = new List<int>();

    public Theme currLevelTheme;

    public ImageAnimator imageAnimator;

    private void Awake() 
    {
        InitializeSingleton(this.gameObject);
        DontDestroyOnLoad(gameObject);
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(level != null & levelScenes.Contains(scene.buildIndex))
            SpawnLevel(level);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //Handles index setting, special case of last level
    public void LoadLevel(int index)
    {
        
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

    //C: allows us to do something special on last level completion. currently returns to main menu.
    public void OnCompleteLastLevel()
    {
        UIManager.Instance.endLevelGroup.SetActive(false);
        LevelManager.Instance.UnloadLevel();
        SceneManager.LoadScene(0);
    }

    //Handles scene and theme switching
    public void SwitchLevel(Level level) {
        this.level = level;
        if(currLevelTheme == null || level.theme != currLevelTheme)
        {
            AudioManager.Instance.PlayList(level.theme.musicStringName);
        }
        StartCoroutine(LoadLevelAsynch(levelScenes[(int)level.theme.themeEnum]));
    }


    public void ResetLevel() {
        SwitchLevel(level);
    }


    //Handles actual spawning of paper object
    public void SpawnLevel(Level level)
    {
        instantiatedLevel = Instantiate(level.levelObject, Vector3.zero, Quaternion.identity);
        FoldablePaper paper = instantiatedLevel.GetComponent<FoldablePaper>();
        Transform playerPos = paper.playerSpawn;
        if(playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }
        playerInstance= Instantiate(playerPrefab, playerPos.position, Quaternion.identity);

        FollowTarget.Instance.SetTargetAndPosition(playerInstance.GetComponent<PlayerMovement>().cameraTrackingTransform);    
        VFXManager.Instance.Refresh();
        FindObjectOfType<TileSelector>().ReloadReferences();
        currLevelTheme = level.theme;
    }

    //C: used when switching from level back to a non-level scene
    public void UnloadLevel()
    {
        StartCoroutine(LoadLevelAsynch(0));
        instantiatedLevel = null;
        playerInstance = null;
        currLevelTheme = null;
        level = null;
    }

    private IEnumerator LoadLevelAsynch(int sceneIndex)
    {
        ActionLockManager.Instance.ForceTakeLock(this);
        levelSwitchScreen.SetActive(true);
        if(UIManager.Instance != null)
            UIManager.Instance.ToggleGroup(false);
        
        imageAnimator.Play();

        if (instantiatedLevel != null) {
            Destroy(instantiatedLevel);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        print("starting asynch load");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        print("end asynch load");

        //bonus placebo time
        float elapsedTime = 0.0f;
        float time = 2.5f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        levelSwitchScreen.SetActive(false);
        imageAnimator.Stop();
        if(UIManager.Instance != null)
            UIManager.Instance.ToggleGroup(true);
        ActionLockManager.Instance.ForceRemoveLock();
    }

    public void ReturnToMenu() {
        UnloadLevel();
    }

    public void EndLevel()
    {
        
        instantiatedLevel.GetComponent<FoldablePaper>().isComplete = true;
        
        //Unlock Cosmetics if there are any
        if(level.cosmeticUnlock != string.Empty){
            PlayerPrefs.SetInt($"CosmeticUnlock{level.cosmeticUnlock}", 1);
        }
    }

}