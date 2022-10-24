using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public static class ApplicationInitializer {
 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeAfterSceneLoad() {
        DebugManager.instance.enableRuntimeUI = false;
    }
 
}