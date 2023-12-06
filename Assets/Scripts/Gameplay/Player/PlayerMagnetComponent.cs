using System.Collections.Generic;
using Gameplay.ScrolledObjects;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerMagnetComponent
    {
        private float timeSinceLastCast = 0.0f;
        private readonly float castGap = 0.5f;
        private Transform playerTransform;
        private Collider2D[] resultsArray;
        private ContactFilter2D contactFilter;

        private HashSet<ScrolledObjectView> currentlyMagnetizedObjects;

        public void Initialize(Transform playerTransform)
        {
            this.playerTransform = playerTransform;
            resultsArray = new Collider2D[20];
            contactFilter = new ContactFilter2D().NoFilter();
            contactFilter.SetLayerMask(LayerMask.GetMask("Pickup"));
            currentlyMagnetizedObjects = new HashSet<ScrolledObjectView>(16);
        }

        public void DoFixedUpdate(float magnetStrength)
        {
            if (timeSinceLastCast <= 0.0f)
            {
                CastMagnetism(magnetStrength);
                timeSinceLastCast = castGap;
            }
            else
            {
                timeSinceLastCast -= Time.fixedDeltaTime;
            }
            
            DrawMagnetizedObjects(magnetStrength);
        }

        private void CastMagnetism(float magnetStrength)
        {
            int numOfResults = Physics2D.OverlapCircle
            (
                playerTransform.position,
                magnetStrength,
                contactFilter,
                resultsArray
            );
            
            HitTrigger result;
            
            for (int i = 0; i < numOfResults; i++)
            {
                result = resultsArray[i].GetComponent<HitTrigger>();
                
                if (result != null)
                {
                    currentlyMagnetizedObjects.Add(result.HitReceiver);
                }
            }
        }

        private void DrawMagnetizedObjects(float magnetStrength)
        {
            if (currentlyMagnetizedObjects.Count == 0) return;

            currentlyMagnetizedObjects.RemoveWhere(s => 
                !s.Active
                || Vector2.Distance(s.transform.position, playerTransform.position) > magnetStrength);

            foreach (ScrolledObjectView currentObject in currentlyMagnetizedObjects)
            {
                Vector3 newPosition = Vector3.Lerp
                (
                    currentObject.transform.position,
                    playerTransform.position, 2.0f * magnetStrength * Time.fixedDeltaTime
                );
                
                currentObject.transform.position = newPosition;
            }
        }
    }
}