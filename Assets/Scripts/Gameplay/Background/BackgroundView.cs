using System;
using UnityEngine;

namespace Gameplay.Background
{
    public class BackgroundView : MonoBehaviour
    {
        [SerializeField] private BackgroundLayer[] layers;

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
                layer.LayerUpdate();
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
