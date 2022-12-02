using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BarkaneJoint
{
    [CreateAssetMenu(menuName = "Barkane/Settings/Glowstick Render Settings")]
    public class GlowstickRenderSettings : ScriptableObject, IDynamicMeshRenderSettings
    {
        [Range(0f, 1f)]
        public float halfLength;
        [Range(0f, 0.1f)]
        public float radius;
        [Range(0f, 0.2f)]
        public float elevation;
        [Range(0, 32)]
        public int resolution;
        [HideInInspector] public int[] indices;
        [HideInInspector] public Vector2[] angles;

        public int VCount => 5 * resolution + 2;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GlowstickRenderSettings))]
    public class GlowstickRenderSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate Indices & Angles"))
            {
                var resolution = (target as GlowstickRenderSettings).resolution;
                if (resolution > 1)
                {
                    var angs = new Vector2[resolution];
                    for (int i = 0; i < resolution; i++)
                    {
                        var ang = 2f * Mathf.PI * i / resolution;
                        angs[i] = new Vector2(Mathf.Sin(ang), Mathf.Cos(ang));
                    }
                    (target as GlowstickRenderSettings).angles = angs;
                    Debug.Log(string.Join(", ", angs));

                    var ids = ColumnMeshIndex.Create(resolution);

                    (target as GlowstickRenderSettings).indices = ids;
                    Debug.Log(string.Join(", ", ids));


                    EditorUtility.SetDirty(target as GlowstickRenderSettings);
                } else
                {
                    throw new UnityException("Resolution must be at least 2");
                }
            }
            
        }
    }
#endif
}
