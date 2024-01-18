using FMODUnity;
using UnityEngine;

public class FocusHelper : MonoBehaviour
{
    private float timeScale = 1.0f;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Time.timeScale = timeScale;
        }
        else
        {
            timeScale = Time.timeScale;
            Time.timeScale = 0.0f;
        }

        if (RuntimeManager.StudioSystem.isValid())
        {
            RuntimeManager.PauseAllEvents(!focus);

            if (!focus)
            {
                RuntimeManager.CoreSystem.mixerSuspend();
            }
            else
            {
                RuntimeManager.CoreSystem.mixerResume();
            }
        }
    }
}
