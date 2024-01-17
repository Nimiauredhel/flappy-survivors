using FMODUnity;
using UnityEngine;

namespace Intro
{
    public class IntroJingle : MonoBehaviour
    {
        [SerializeField] private EventReference introJingle;
        
        public void Start()
        {
            RuntimeManager.PlayOneShot(introJingle);
        }
    }
}
