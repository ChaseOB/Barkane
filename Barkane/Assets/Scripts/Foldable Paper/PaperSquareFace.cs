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
    FENCE,
    CONETREE,
    SNOWBALL,
    GLOWSTICKBOX,
}

[ExecuteInEditMode]
public class PaperSquareFace : MonoBehaviour, IThemedItem
{
    [Header("Surface")]
    [SerializeField] private FaceType faceType;

    [Header("Objects")]
    [SerializeField] private bool shard;
    [SerializeField] private bool goal;
    [SerializeField] private bool fence;
    [SerializeField] private bool coneTree;
    [SerializeField] private bool snowball;
    [SerializeField] private bool glowstickBox;

    [Header("References")]
    [SerializeField] private BoxCollider playerWalk;
    [SerializeField] private SquareSide squareSide;
    [SerializeField] private GameObject shardPrefab;
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject fencePrefab;
    [SerializeField] private GameObject coneTreePrefab;
    [SerializeField] private GameObject snowballPrefab;
    [SerializeField] private GameObject glowstickBoxPrefab;

    [SerializeField, HideInInspector] Theme theme;

    public void UpdateTheme(Theme t)
    {
        theme = t;
        ChangeFaceType();
    }

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

    private Dictionary<FaceObjectType, GameObject> _faceObjects = new Dictionary<FaceObjectType, GameObject>();

    private void OnValidate()
    {
        _faceObjectPrefabs = GetFaceObjectPrefabs();
    }

    private Dictionary<FaceObjectType, GameObject> GetFaceObjectPrefabs()
    {
        return new Dictionary<FaceObjectType, GameObject>()
        {
            {FaceObjectType.SHARD, shardPrefab },
            {FaceObjectType.GOAL, goalPrefab },
            {FaceObjectType.FENCE, fencePrefab},
            {FaceObjectType.CONETREE, coneTreePrefab},
            {FaceObjectType.SNOWBALL, snowballPrefab},
            {FaceObjectType.GLOWSTICKBOX, glowstickBoxPrefab}
        };
    }

    public void ChangeFaceType()
    {
        if (faceType == FaceType.WALKABLE)
        {
            playerWalk.enabled = true;
            squareSide.materialPrototype = theme.WalkMat;
            squareSide.BaseColor = theme.WalkColor;
            squareSide.TintColor = theme.WalkTint;
        }
        else if(faceType == FaceType.UNWALKABLE)
        {
            playerWalk.enabled = false;
            squareSide.materialPrototype = theme.UnWalkMat;
            squareSide.BaseColor = theme.UnwalkColor;
            squareSide.TintColor = theme.UnwalkTint;
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(squareSide);
#endif
        squareSide.UpdateMesh();
    }
#if UNITY_EDITOR
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
            newObject.transform.localRotation = transform.rotation;
            newObject.transform.SetParent(transform);
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localRotation = Quaternion.identity;
        }
    }
#endif

    private void DeleteFaceObjectIfExists(FaceObjectType type)
    {
        if (_faceObjects.ContainsKey(type))
        {
            if (_faceObjects[type] != null)
            {
                DestroyImmediate(_faceObjects[type]);
            }
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
            { "fence", (prop, face) => face.SetFaceObject(FaceObjectType.FENCE, prop.boolValue) },
            { "coneTree", (prop, face) => face.SetFaceObject(FaceObjectType.CONETREE, prop.boolValue) },
            { "snowball", (prop, face) => face.SetFaceObject(FaceObjectType.SNOWBALL, prop.boolValue) },
            { "glowstickBox", (prop, face) => face.SetFaceObject(FaceObjectType.GLOWSTICKBOX, prop.boolValue) },
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