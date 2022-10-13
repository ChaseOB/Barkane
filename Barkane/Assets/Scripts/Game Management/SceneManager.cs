using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : Singleton<SceneManager>
{
    private GameObject level;
    private GameObject instantiatedLevel;
    [SerializeField] private GameObject playerPrefab;
    private GameObject playerInstance;
    [SerializeField] private GameObject levelSwitchScreen; //C: Used to hid VFX Loading

    private void Awake() 
    {
        InitializeSingleton();
        DontDestroyOnLoad(gameObject);
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
        Transform playerPos = instantiatedLevel.GetComponent<FoldablePaper>().playerSpawn;
        if(playerInstance = null)
            playerInstance = Instantiate(playerPrefab, playerPos.position, Quaternion.identity);
        else
            playerInstance.transform.SetPositionAndRotation(playerPos.position, playerPos.rotation);
            
    }

    //C: used when switching from level back to a non-level scene
    public void UnloadLevel()
    {
        StartCoroutine(OneSecTransition());
        if (instantiatedLevel != null) 
            Destroy(instantiatedLevel);
        if(playerInstance != null)
            Destroy(playerInstance);
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