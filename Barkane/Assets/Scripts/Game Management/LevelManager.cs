using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BarkaneEditor;


public class LevelManager : Singleton<LevelManager>
{
    //If true, completeing a level automatically unlocks the next level in the sequence
    public bool sequentialUnlock = true;

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

    private int currLevelFoldCount = 0;

    private void Awake() 
    {
        InitializeSingleton(this.gameObject);
        DontDestroyOnLoad(gameObject);
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Goal.OnReachGoal += OnEndLevel;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(level != null & levelScenes.Contains(scene.buildIndex))
            SpawnLevel(level);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Goal.OnReachGoal -= OnEndLevel;
    }

    public Level GetCurrentLevel()
    {
        return level;
    }

    //Handles index setting, special case of last level
    public void LoadLevel(int index)
    {
        if(index == 0)
            SaveSystem.Current.SetLevelUnlock(levelList[0].levelName, true);
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
        {
            UIManager.Instance.ToggleLevelGroup(false);
            UIManager.Instance.ToggleEndLevelGroup(false);
        }
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
            UIManager.Instance.ToggleLevelGroup(true);
        ActionLockManager.Instance.ForceRemoveLock();
    }

    public void ReturnToMenu() {
        UnloadLevel();
    }

    public void SetFoldCount(int val)
    {
        currLevelFoldCount = val;
    }

    private void OnEndLevel(object sender, System.EventArgs e) {
        EndLevel();
    }

    public void EndLevel()
    { 
        instantiatedLevel.GetComponent<FoldablePaper>().isComplete = true;

        //set folds for current level
        SaveSystem.Current.SetNumFoldsIfLower(level.levelName, currLevelFoldCount);
        
        //Unlock Cosmetics if there are any
        if(level.cosmeticUnlock != string.Empty){
            SaveSystem.Current.SetCosmeticUnlock(level.cosmeticUnlock, true);
        }

        //unlock next level
        if(sequentialUnlock && currLevelIndex + 1 != levelList.Count)
        {
            string nextLevel = levelList[currLevelIndex + 1].levelName;
            SaveSystem.Current.SetLevelUnlock(nextLevel, true);
        }

        SaveSystem.SaveGame();
    }

}