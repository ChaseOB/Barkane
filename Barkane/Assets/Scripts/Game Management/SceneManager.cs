using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private GameObject level;
    private GameObject instantiatedLevel;

    public void SwitchLevel(GameObject level) {
        this.level = level;
        if (instantiatedLevel != null) {
            Destroy(instantiatedLevel);
        }
        instantiatedLevel = Instantiate(level, transform.position, Quaternion.identity);
    }

    public void SwitchLevel(string level) {
        this.level = (GameObject) Resources.Load("Prefabs/" + level);
        if (instantiatedLevel != null) {
            Destroy(instantiatedLevel);
        }
        instantiatedLevel = Instantiate(this.level, transform.position, Quaternion.identity);
    }

    public void ResetLevel() {
        if (instantiatedLevel != null) {
            Destroy(instantiatedLevel);
            instantiatedLevel = Instantiate(level, transform.position, Quaternion.identity);
        }
    }

    public void ChangeSkybox(Material skybox) {
        RenderSettings.skybox = skybox;
    }

    public void ChangeSkybox(string skybox) {
        RenderSettings.skybox = (Material) Resources.Load(skybox);
    }
}