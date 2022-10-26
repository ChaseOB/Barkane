using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BarkaneEditor
{
    [ExecuteAlways]
    public class TestTex3DSizer : MonoBehaviour
    {

        [SerializeField, Range(0, 20)] private float scl;

        // Update is called once per frame
        void Update()
        {
            transform.localScale = scl * Vector3.one;
            if (Application.isPlaying)
            {
                transform.position += Vector3.right * Time.deltaTime;
            }
        }
    }

}
