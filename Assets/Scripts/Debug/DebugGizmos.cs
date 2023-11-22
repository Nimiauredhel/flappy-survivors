using System;
using UnityEditor;
using UnityEngine;

public class DebugGizmos : MonoBehaviour
{
    #if UNITY_EDITOR
    
    public GUIStyle labelStyle;
    
    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main) return;
        
        string update = Mathf.FloorToInt(1.0f / Time.deltaTime).ToString();
        string fixedUpdate = Mathf.FloorToInt(1.0f / Time.fixedDeltaTime).ToString();
        Handles.Label(transform.position, update + "\n" + fixedUpdate, labelStyle);
    }
    
    #endif
}
