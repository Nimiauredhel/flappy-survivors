using System;
using UnityEngine;

namespace Intro
{
    public class Init : MonoBehaviour
    {
        private void Awake()
        {
            Preferences.Initialize();
        }
    }
}