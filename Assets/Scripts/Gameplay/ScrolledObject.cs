using UnityEngine;

namespace Gameplay
{
    public class ScrolledObject : MonoBehaviour
    {
        public bool Active => active;
        public float Value => value;
        public ScrolledObjectType ObjectType => objectType;

        private bool active = false;
        [SerializeField] private float value;
        [SerializeField] private ScrolledObjectType objectType;
        [SerializeField] private SpriteRenderer graphic;
        [SerializeField] private GameObject explosion;
    
        public enum ScrolledObjectType
        {
            None,
            Enemy,
            Experience,
        
        }

        public void Activate()
        {
            graphic.gameObject.SetActive(true);
            explosion.SetActive(false);
            active = true;
        }

        public void Deactivate()
        {
            graphic.gameObject.SetActive(false);
            explosion.SetActive(true);
            active = false;
        }
    }
}
