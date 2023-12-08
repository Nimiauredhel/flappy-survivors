using System;
using UnityEditor;
using UnityEngine;

namespace Gameplay.ScrolledObjects
{
    [RequireComponent(typeof(Collider2D))]
    public class HitTrigger : MonoBehaviour
    {
        public ScrolledObjectView HitReceiver => hitReceiver;

        [SerializeField] private ScrolledObjectView hitReceiver;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (hitReceiver == null)
            {
                hitReceiver = GetComponentInParent<ScrolledObjectView>();
            }

            Collider2D coll = GetComponent<Collider2D>();
            coll.isTrigger = true;
        }

        private void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy) return;
            if (Camera.current != Camera.main) return;
            
            switch (gameObject.layer)
            {
                case 6:
                    Handles.color = Color.green;
                    break;
                case 7:
                    Handles.color = Color.cyan;
                    break;
                case 8:
                    Handles.color = Color.yellow;
                    break;
                case 9:
                    Handles.color = Color.red;
                    break;
                default:
                    return;
            }

            Collider2D coll = GetComponent<Collider2D>();

            if (coll == null) return;
            
            Vector3[] points = new Vector3[5]
            {
                new Vector3(coll.bounds.min.x, coll.bounds.min.y, 0.0f),
                new Vector3(coll.bounds.min.x, coll.bounds.max.y, 0.0f),
                new Vector3(coll.bounds.max.x, coll.bounds.max.y, 0.0f),
                new Vector3(coll.bounds.max.x, coll.bounds.min.y, 0.0f),
                new Vector3(coll.bounds.min.x, coll.bounds.min.y, 0.0f)
            };
            
            Handles.DrawPolyLine(points);
        }
#endif
    }
}
