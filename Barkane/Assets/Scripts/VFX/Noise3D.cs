using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BarkaneEditor
{
    [ExecuteAlways]
    public class Noise3D : MonoBehaviour
    {
        [SerializeField] private ComputeShader shader;
        private string ShaderName => shader.name;

        [SerializeField] private int Width; // cells per texture
        [SerializeField] private int Density; // samples per cell
        private int Resolution => Width * Density;

#if UNITY_EDITOR
        internal void Generate()
        {
            var t3D = new Texture3D(Resolution, Resolution, Resolution, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat
            };

            var (pts1, pts2, pts3) = (PtsBuffer, PtsBuffer, PtsBuffer);

            shader.SetInt("_Width", Width);
            shader.SetInt("_Density", Density);
            shader.SetBuffer(0, "_Pts1", pts1);
            shader.SetBuffer(0, "_Pts2", pts2);
            shader.SetBuffer(0, "_Pts3", pts3);

            var rt2D = new RenderTexture(Resolution, Resolution, 0)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point
            };
            rt2D.Create();
            BakeTexture(t3D, rt2D);

            AssetDatabase.CreateAsset(t3D, $"Assets/Materials/Noise/{ShaderName}.asset");
            AssetDatabase.SaveAssets();

            #region release
            DestroyImmediate(rt2D);
            pts1.Release();
            pts2.Release();
            pts3.Release();
            #endregion

        }

        // reference Jushii's https://github.com/jushii/WorleyNoise/blob/main/Assets/Scripts/WorleyNoiseGenerator.cs
        private ComputeBuffer PtsBuffer
        {
            get
            {
                var ct = Width * Width * Width;
                var buf = new ComputeBuffer(ct, sizeof(float) * 3, ComputeBufferType.Structured);
                var pts = new Vector3[ct];
                for (int i = 0; i < ct; i++) pts[i] = new Vector3(Random.value, Random.value, Random.value);
                // Debug.Log(string.Join(", ", pts));
                buf.SetData(pts);
                return buf;
            }
        }

        #region https://www.ronja-tutorials.com/post/030-baking-shaders/#3d-texture-baking
        void BakeTexture(Texture3D t3D, RenderTexture rt2D)
        {
            //get rendertexture to render layers to and texture3d to save values to as well as 2d texture for transferring data
            var temp = new Texture2D(Resolution, Resolution)
            {
                filterMode = FilterMode.Point
            };

            //prepare for loop
            var volume = Resolution * Resolution * Resolution;
            var area = Resolution * Resolution;
            
            var colors = new Color[volume];

            shader.SetTexture(0, "_Result", rt2D);

            //loop through slices
            for (var slice = 0; slice < Resolution; slice++)
            {
                //get shader result
                // the original implementation uses Blit (Material-based), this is switched to the newer Compute Shader version of Dispatch
                shader.SetFloat("_z", slice);
                shader.Dispatch(0, Resolution / 8, Resolution / 8, 1);

                RenderTexture.active = rt2D;
                temp.ReadPixels(new Rect(0, 0, Resolution, Resolution), 0, 0);
                temp.Apply();
                RenderTexture.active = null;

                var sliceColors = temp.GetPixels32();

                //copy slice to data for 3d texture
                var sliceBase = slice * area;
                for (var pixel = 0; pixel < area; pixel++)
                {
                    colors[sliceBase + pixel] = sliceColors[pixel];
                }
                // Debug.Log(string.Join(", ", sliceColors));
            }

            t3D.SetPixels(colors);
            // Debug.Log(string.Join(", ", colors));
            t3D.Apply();

            // clean up memory allocated during this function
            // do not include anything allocated by the caller
            DestroyImmediate(temp);
        }
        #endregion
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Noise3D))]
    public class Noise3DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate"))
            {
                (target as Noise3D).Generate();
            }
        }
    } 
#endif

}