using UnityEngine;
using System;

public class FocusListener : MonoBehaviour
{
    public static FocusListener Instance;

    public event EventHandler<bool> FocusChanged;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void OnApplicationFocus(bool focus)
    {
        Debug.Log("Focus changed to " + focus);
        FocusChanged?.Invoke(this, focus);
    }
}