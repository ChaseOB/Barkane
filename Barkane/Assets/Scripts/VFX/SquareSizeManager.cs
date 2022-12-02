using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneEditor;

[ExecuteInEditMode]
public class SquareSizeManager : MonoBehaviour, IRefreshable
{
    [SerializeField] SquareRenderSettings settings;

#if UNITY_EDITOR
    [SerializeField, HideInInspector] float currsize = 0.5f;

    void Refresh()
    {
        float projected = (1f - settings.margin) * 0.5f;

        if (!Mathf.Approximately(projected, currsize))
        {
            currsize = projected;
            transform.localScale = new Vector3(currsize, 1, currsize);
        }
    }

    void IRefreshable.EditorRefresh()
    {
        Refresh();
    }

    void IRefreshable.RuntimeRefresh()
    {
        Refresh();
    }
#endif
}
