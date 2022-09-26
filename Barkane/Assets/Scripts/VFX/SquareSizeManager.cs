using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SquareSizeManager : MonoBehaviour
{
    [SerializeField] SquareRenderSettings settings;

#if UNITY_EDITOR
    [SerializeField, HideInInspector] float currsize = 0.5f;

    // Update is called once per frame
    void Update()
    {
        float projected = (1f - settings.margin) * 0.5f;

        if (!Mathf.Approximately(projected, currsize))
        {
            currsize = projected;
            transform.localScale = new Vector3(currsize, 1, currsize);
        }
    }
#endif
}
