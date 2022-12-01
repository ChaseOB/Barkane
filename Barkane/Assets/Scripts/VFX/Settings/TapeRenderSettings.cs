using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BarkaneJoint
{
    [CreateAssetMenu(menuName = "Barkane/Settings/Tape Render Settings")]
    public class TapeRenderSettings : ScriptableObject, IDynamicMeshRenderSettings
    {
        [Range(0f, 0.2f)]
        public float elevation;
        [Range(0f, 2f)]
        public float width;
        [Range(0f, 0.1f)]
        public float thickness;
        [Range(0f, 1f)]
        public float halfLength;
        [Range(0f, 0.25f)]
        public float randomizeX, randomizeY;

        // a tape is just a 4 sided column
        [HideInInspector] public int[] ids = ColumnMeshIndex.Create(4);

        public int VCount => 22;
    }

}
