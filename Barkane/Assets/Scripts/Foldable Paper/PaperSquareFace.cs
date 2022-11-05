using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

public enum FaceType
{
    WALKABLE,
    UNWALKABLE,
}

public enum FaceObjectType
{
    SHARD,
    GOAL,
}

[ExecuteInEditMode]
public class PaperSquareFace : MonoBehaviour
{
    [Header("Surface")]
    [SerializeField] private FaceType faceType;
    [SerializeField] private Material walkMat;
    [SerializeField] private Material unWalkMat; //C: Would be better to store these somewhere else in the future, but for now is fine

    [Header("Objects")]
    [SerializeField] private bool shard;
    [SerializeField] private bool goal;

    [Header("References")]
    [SerializeField] private BoxCollider playerWalk;
    [SerializeField] private SquareSide squareSide;
    [SerializeField] private GameObject shardPrefab;
    [SerializeField] private GameObject goalPrefab;

    //L: This dictionary ensures only one of an object exists on a face at a time.
    private Dictionary<FaceObjectType, GameObject> _faceObjectPrefabs;
    public Dictionary<FaceObjectType, GameObject> FaceObjectPrefabs
    {
        get
        {
            if (_faceObjectPrefabs == null)
            {
                _faceObjectPrefabs = GetFaceObjectPrefabs();
            }

            return _faceObjectPrefabs;
        }
    }

    //private Dictionary<FaceObjectType, bool> _faceObjectStatuses;

    private Dictionary<FaceObjectType, GameObject> _faceObjects = new Dictionary<FaceObjectType, GameObject>();

    private void OnValidate()
    {
        _faceObjectPrefabs = GetFaceObjectPrefabs();

        //RefreshFaceObjects(); //Dis sucks
    }

    private Dictionary<FaceObjectType, GameObject> GetFaceObjectPrefabs()
    {
        return new Dictionary<FaceObjectType, GameObject>()
        {
            {FaceObjectType.SHARD, shardPrefab },
            {FaceObjectType.GOAL, goalPrefab },
        };
    }

    private Dictionary<FaceObjectType, bool> GetFaceObjectStatuses()
    {
        return new Dictionary<FaceObjectType, bool>()
        {
            {FaceObjectType.SHARD, shard },
            {FaceObjectType.GOAL, goal },
        };
    }

    private void RefreshFaceObjects()
    {
        var faceObjectStatuses = GetFaceObjectStatuses();

        foreach (FaceObjectType type in faceObjectStatuses.Keys)
        {
            SetFaceObject(type, faceObjectStatuses[type]);
        }
    }


    public void ChangeFaceType()
    {
        if (faceType == FaceType.WALKABLE)
        {
            playerWalk.enabled = true;
            squareSide.materialPrototype = walkMat;
            squareSide.UpdateMesh();
        }
        else if(faceType == FaceType.UNWALKABLE)
        {
            playerWalk.enabled = false;
            squareSide.materialPrototype = unWalkMat;
            squareSide.UpdateMesh();        
        }
        Debug.Log($"Type of {gameObject.name} changed to {faceType}");
    }

    public void SetFaceObject(FaceObjectType type, bool status)
    {
        if (status)
        {
            CreateFaceObjectIfNotExists(type);
        } else
        {
            DeleteFaceObjectIfExists(type);
        }
    }

    private void CreateFaceObjectIfNotExists(FaceObjectType type)
    {
        if (!_faceObjects.ContainsKey(type) || _faceObjects[type] == null)
        {
            _faceObjects.Add(type, InstantiationExtension.InstantiateKeepPrefab(_faceObjectPrefabs[type]));

            GameObject newObject = _faceObjects[type];
            newObject.transform.SetParent(transform);
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localRotation = Quaternion.identity;
        }
    }

    private void DeleteFaceObjectIfExists(FaceObjectType type)
    {
        if (_faceObjects.ContainsKey(type))
        {
            DestroyImmediate(_faceObjects[type]);
            _faceObjects.Remove(type);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PaperSquareFace))]
public class PaperSquareFaceEditor : Editor
{
    Dictionary<string, Action<SerializedProperty, PaperSquareFace>> propertyActions;

    private void OnEnable()
    {
        this.propertyActions = new Dictionary<string, Action<SerializedProperty, PaperSquareFace>>
        {
            { "faceType", (prop, face) => face.ChangeFaceType() },
            { "shard", (prop, face) => face.SetFaceObject(FaceObjectType.SHARD, prop.boolValue) },
            { "goal", (prop, face) => face.SetFaceObject(FaceObjectType.GOAL, prop.boolValue) },
        };
    }

    public override void OnInspectorGUI()
    {
        //L: Copying code from Unity's codebase :)
        serializedObject.UpdateIfRequiredOrScript();
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
            {
                if (propertyActions.ContainsKey(iterator.name))
                {
                    CreatePropertyAction(iterator, propertyActions[iterator.name]);
                } else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }

            enterChildren = false;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void CreatePropertyAction(SerializedProperty property, Action<SerializedProperty, PaperSquareFace> action)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(property, true);
        base.serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            action(property, target as PaperSquareFace);
        }
    }
}
#endif