using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonStateHelper : MonoBehaviour
{
    [SerializeField] private Button[] buttons = Array.Empty<Button>();

    public void SetAll(bool value)
    {
        if (buttons == null || buttons.Length <= 0) return;
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null) buttons[i].interactable = value;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(OnEnableRoutine());
    }

    private void OnDisable()
    {
        SetAll(false);
    }

    private void OnValidate()
    {
        SetAll(false);
    }

    private IEnumerator OnEnableRoutine()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        
        SetAll(true);
    }
}
