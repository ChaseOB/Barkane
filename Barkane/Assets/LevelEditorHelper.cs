using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneEditor;


public class LevelEditorHelper : MonoBehaviour
{
    private void Start() {
        VFXManager.Instance.Refresh();
        //FindObjectOfType<FoldablePaper>().PopulateOcclusionMap();
        FindObjectOfType<TileSelector>().ReloadReferences();
    }
}
