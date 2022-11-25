using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BarkaneJoint
{
    [CreateAssetMenu(menuName = "Barkane/Settings/Glowstick Render Settings")]
    public class GlowstickRenderSettings : ScriptableObject
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

                    // main body: 5 segments, each is a ring of resolution quads, each quad 2 triangles, each triangle 3 vertices
                    // 2 end caps: resolution triangles, each triangle 3 vertices
                    var ids = new int[(5 - 1) * resolution * 2 * 3 + 2 * resolution * 3];
                    var t = 0;

                    // beginning cap
                    for (int i = 0; i < resolution - 1; i++)
                    {
                        ids[t++] = 0;
                        ids[t++] = 1 + i + 1;
                        ids[t++] = 1 + i;
                    }
                    ids[t++] = 0;
                    ids[t++] = 1;
                    ids[t++] = resolution;

                    // main cylinder
                    for (int i = 0; i < (5 - 1); i++)
                    {
                        // first resolution - 1 quads
                        var j = 0;
                        for (; j < resolution - 1; j++)
                        {
                            ids[t++] = 1 + j + i * resolution;
                            ids[t++] = 1 + j + i * resolution + 1;
                            ids[t++] = 1 + j + i * resolution + resolution;
                            ids[t++] = 1 + j + i * resolution + resolution;
                            ids[t++] = 1 + j + i * resolution + 1;
                            ids[t++] = 1 + j + i * resolution + resolution + 1;
                        }
                        // last quad loops back to j=0
                        ids[t++] = 1 + j + i * resolution;
                        ids[t++] = 1 + 0 + i * resolution;
                        ids[t++] = 1 + j + i * resolution + 1;
                        ids[t++] = 1 + j + i * resolution + 1;
                        ids[t++] = 1 + j + i * resolution + resolution;
                        ids[t++] = 1 + j + i * resolution;
                    }

                    // ending cap, order is reversed
                    for (int i = 0; i < resolution - 1; i++)
                    {
                        ids[t++] = 5 * resolution + 1;
                        ids[t++] = 1 + i + 4 * resolution;
                        ids[t++] = 1 + i + 4 * resolution + 1;
                    }
                    ids[t++] = 5 * resolution + 1;
                    ids[t++] = resolution + 4 * resolution;
                    ids[t++] = 4 * resolution + 1;

                    (target as GlowstickRenderSettings).indices = ids;
                    Debug.Log(string.Join(", ", ids));
                } else
                {
                    throw new UnityException("Resolution must be at least 2");
                }
            }
            
        }
    }
#endif
}
