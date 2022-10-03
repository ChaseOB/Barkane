using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BarkaneEditor
{
    public enum ThemeChoice
    {
        CherryBlossom,
        SnowySnow,
        GlowstickCave,
        CardboardCastle
    }

    [InitializeOnLoad, ExecuteInEditMode]
    public class VFXManager : MonoBehaviour
    {
        [SerializeField] private ThemeChoice themeChoice;
        [SerializeField] private Theme[] themes;
        public static Theme Theme => Instance.themes[(int)Instance.themeChoice];

        public static VFXManager Instance { get; private set; }

        internal void Refresh()
        {
            Instance = this;

            if (themes == null || themes.Length != System.Enum.GetNames(typeof(ThemeChoice)).Length)
            {
                throw new UnityException("Theme assets are referenced incorrectly in VFXManager.");
            }

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

    [CustomEditor(typeof(VFXManager))]
    public class VFXRefreshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Refresh"))
            {
                (target as VFXManager).Refresh();
            }
        }
    }

#endif
}