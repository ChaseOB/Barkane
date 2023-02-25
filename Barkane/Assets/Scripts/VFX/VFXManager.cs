using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using BarkaneJoint;

namespace BarkaneEditor
{

#if UNITY_EDITOR
    [InitializeOnLoad, ExecuteInEditMode]
#endif
    public class VFXManager : MonoBehaviour
    {
        [SerializeField] private ThemeChoice themeChoice;
        [SerializeField] private Theme[] themes;
        public static Theme Theme => Instance.themes[(int)Instance.themeChoice];

        private static VFXManager _instance;
        public static VFXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<VFXManager>();
                }

                return _instance;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void DidReloadScripts()
        {
            var singleton = FindObjectOfType<VFXManager>();
            //if (singleton)
            //{
            //    singleton.Refresh();
            //}
        }
#endif

        internal void Refresh()
        {
            _instance = this;
            if (themes == null || themes.Length != System.Enum.GetNames(typeof(ThemeChoice)).Length)
            {
                throw new UnityException("Theme assets are referenced incorrectly in VFXManager.");
            }

            Load();

            Refresh<EdgeParticles>();
            Refresh<MaskFoldParticles>();
            Refresh<SquareSide>();
            Refresh<JointRenderer>();
            Refresh<SquareSizeManager>();
            Refresh<GlowStick>();
        }

        internal void Refresh<T>() where T: MonoBehaviour, IRefreshable
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                foreach (var t in FindObjectsOfType<T>())
                {
                    t.EditorRefresh();
                    EditorUtility.SetDirty(t);
                }
#endif
            }
            else
            {
                foreach (var t in FindObjectsOfType<T>())
                {
                    t.RuntimeRefresh();
                }
            }
        }
        
        internal void UpdateTheme()
        {
            UpdateTheme(Theme);
        }

        internal void UpdateTheme(Theme theme)
        {
            foreach (var t in FindObjectsOfType<MonoBehaviour>().OfType<IThemedItem>())
            {
                t.UpdateTheme(theme);
            }
            Refresh();
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
        void EditorRefresh();
        void RuntimeRefresh();
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
            if (GUILayout.Button("Update Theme"))
            {
                (target as VFXManager).UpdateTheme();
            }
        }
    }

#endif
}