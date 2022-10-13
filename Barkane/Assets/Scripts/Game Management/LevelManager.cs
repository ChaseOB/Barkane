using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BarkaneEditor;

public class LevelManager : Singleton<LevelManager>
{
    private GameObject level;
    private GameObject instantiatedLevel;
    [SerializeField] private GameObject playerPrefab;
    private GameObject playerInstance = null;
    [SerializeField] private GameObject levelSwitchScreen; //C: Used to hid VFX Loading
    [SerializeField] private List<GameObject> levelList;
    public int currLevelIndex = -1;

    public List<int> levelScenes = new List<int>();

    private void Awake() 
    {
        InitializeSingleton();
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
            LoadLevel(0, true);
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

    }

    public void SwitchLevel(GameObject level) {
        StartCoroutine(OneSecTransition());
        this.level = level;
        if (instantiatedLevel != null) {
            Destroy(instantiatedLevel);
        }
        SpawnLevel(level);
    }

    public void SwitchLevel(string level) {
        StartCoroutine(OneSecTransition());
        this.level = (GameObject) Resources.Load("Prefabs/" + level);
        if (instantiatedLevel != null) {
            Destroy(instantiatedLevel);
        }
        SpawnLevel(this.level);
    }

    public void ResetLevel() {
        StartCoroutine(OneSecTransition());
        if (instantiatedLevel != null) 
        {
            Destroy(instantiatedLevel);
            SpawnLevel(level);        
        }
    }


    public void SpawnLevel(GameObject level)
    {
        instantiatedLevel = Instantiate(level, transform.position, Quaternion.identity);
        FoldablePaper paper = instantiatedLevel.GetComponent<FoldablePaper>();
        Transform playerPos = paper.playerSpawn;
        if(playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }
        playerInstance= Instantiate(playerPrefab, playerPos.position, Quaternion.identity);

        FollowTarget.Instance.SetTargetAndPosition(playerInstance.transform);    
        StartCoroutine(waitfive());
    }

    private IEnumerator waitfive()
    {
        yield return new WaitForSeconds(5);
                FindObjectOfType<VFXManager>().Refresh();
    }

    //C: used when switching from level back to a non-level scene
    public void UnloadLevel()
    {
        StartCoroutine(OneSecTransition());
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
    private IEnumerator OneSecTransition()
    {
        SetTransitionScreen(true);
        yield return new WaitForSeconds(1);
        SetTransitionScreen(false);
    }


    public void ChangeSkybox(Material skybox) {
        RenderSettings.skybox = skybox;
    }

    public void ChangeSkybox(string skybox) {
        RenderSettings.skybox = (Material) Resources.Load(skybox);
    }
}