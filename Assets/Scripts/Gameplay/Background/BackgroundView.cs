using System;
using UnityEngine;

namespace Gameplay.Background
{
    public class BackgroundView : MonoBehaviour
    {
        [SerializeField] private float heightMin = 0.0f;
        [SerializeField] private float heightMax = 2.0f;
        [SerializeField] private BackgroundLayer[] layers;
        
        private float referenceHeight = 0.0f;

        public void SetHeightPercent(float percent)
        {
            referenceHeight = Mathf.Lerp(heightMin, heightMax, percent);
        }

        private void Start()
        {
            foreach (BackgroundLayer layer in layers)
            {
                layer.LayerInitialize();
            }
        }

        private void Update()
        {
            foreach (BackgroundLayer layer in layers)
            {
                layer.LayerUpdate(referenceHeight);
            }
        }
        
        #if UNITY_EDITOR

        private void OnValidate()
        {
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].name = "Layer " + (i + 1);
                layers[i].LayerValidate();
            }
        }

        #endif
    }
}
