using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public GameObject firstLevel;
    private GameObject marmalade;
    private GameObject instantiatedMarmalade;
    private GameObject prefab;
    private GameObject instantiatedPrefab;

    void Awake() {
        instantiatedPrefab = Instantiate(firstLevel, transform.position, Quaternion.identity);
        marmalade = (GameObject) Resources.Load("Prefabs/Player");
        instantiatedMarmalade = Instantiate(marmalade, transform.position, Quaternion.identity);
    }

    IEnumerator Start()
    {
        Debug.Log("Start1");
        yield return new WaitForSeconds(5);
        Debug.Log("Start2");
        Destroy(instantiatedMarmalade);
        yield return new WaitForSeconds(10);
        Debug.Log("Start3");
        instantiatedMarmalade = Instantiate(marmalade, transform.position, Quaternion.identity);
    }

    public void SwitchLevel(GameObject prefab) {
        this.prefab = prefab;
        Destroy(instantiatedPrefab);
        instantiatedPrefab = Instantiate(prefab, transform.position, Quaternion.identity);
        Destroy(instantiatedMarmalade);
        instantiatedMarmalade = Instantiate(marmalade, transform.position, Quaternion.identity);
    }

    public void SwitchLevel(string prefabString) {
        prefab = (GameObject) Resources.Load("Prefabs/" + prefabString);
        Destroy(instantiatedPrefab);
        instantiatedPrefab = Instantiate(prefab, transform.position, Quaternion.identity);
        Destroy(instantiatedMarmalade);
        instantiatedMarmalade = Instantiate(marmalade, transform.position, Quaternion.identity);
    }

    public void ResetLevel() {
        Destroy(instantiatedPrefab);
        Destroy(instantiatedMarmalade);
        instantiatedPrefab = Instantiate(prefab, transform.position, Quaternion.identity);
        instantiatedMarmalade = Instantiate(marmalade, transform.position, Quaternion.identity);
    }
}