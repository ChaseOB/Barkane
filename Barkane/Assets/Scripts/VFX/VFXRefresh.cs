using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BarkaneEditor
{
    [InitializeOnLoad, ExecuteInEditMode]
    public class VFXRefresh : MonoBehaviour
    {
        private void Awake()
        {
            // EditorApplication.playModeStateChanged += Load;
            Refresh();
        }

        internal void Refresh()
        {
            Load();
            foreach (var s in FindObjectsOfType<MonoBehaviour>())
            {
                if (s is IRefreshable) (s as IRefreshable).Refresh();
            }
        }

        //internal void Load(PlayModeStateChange change)
        //{
        //    switch (change)
        //    {
        //        case PlayModeStateChange.EnteredEditMode:
        //            Debug.Log("Entering Edit Mode");
        //            Load();
        //            break;
        //        default:
        //            Debug.Log("Other editor mode change, not handled");
        //            break;
        //    }
        //}

        /// <summary>
        /// forced load
        /// </summary>
        private void Load()
        {
            Debug.Log("Re-Loading all VFX assets in Barkane... bork!");
            foreach (var l in GameObject.FindObjectsOfType<MonoBehaviour>())
            {
                if (l is ILoadable) (l as ILoadable).Load();
            }
        }
    }

    public interface IRefreshable
    {
        void Refresh();
    }

    public interface ILoadable
    {
        void Load();
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(VFXRefresh))]
    public class VFXRefreshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Refresh"))
            {
                (target as VFXRefresh).Refresh();
            }
        }
    }

#endif
}