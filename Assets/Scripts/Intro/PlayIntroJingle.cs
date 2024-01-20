using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Intro
{
    public class IntroJingle : MonoBehaviour
    {
        [SerializeField] private EventReference introJingle;
        private EventInstance introJingleInstance;
        
        private void OnEnable()
        {
            introJingleInstance = RuntimeManager.CreateInstance(introJingle);
            introJingleInstance.start();
        }

        private void OnDisable()
        {
            introJingleInstance.stop(STOP_MODE.ALLOWFADEOUT);
            introJingleInstance.release();
        }
    }
}
