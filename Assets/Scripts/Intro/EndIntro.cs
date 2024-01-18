using UnityEngine;
using UnityEngine.SceneManagement;

namespace Intro
{
    public class EndIntro : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
