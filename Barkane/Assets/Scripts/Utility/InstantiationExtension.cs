using UnityEditor;
using UnityEngine;

public class InstantiationExtension
{
#if UNITY_EDITOR
    //Source: https://answers.unity.com/questions/667865/prefabutilityinstantiateprefab-returns-null-in-edi.html
    public static GameObject InstantiateKeepPrefab(Object prefabObject)
    {
        bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(prefabObject);
        bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(prefabObject);
        GameObject instance = null;
        if (isPrefabInstance)
        {
            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(prefabObject) as GameObject;
            instance = (GameObject) PrefabUtility.InstantiatePrefab(prefabSource);
            PrefabUtility.SetPropertyModifications(instance, PrefabUtility.GetPropertyModifications(prefabObject));
            return instance;
        } else if (isPrefab)
        {
            return (GameObject) PrefabUtility.InstantiatePrefab(prefabObject);
        }
        else
        {
            return GameObject.Instantiate(prefabObject) as GameObject;
        }
    }
#endif
}
