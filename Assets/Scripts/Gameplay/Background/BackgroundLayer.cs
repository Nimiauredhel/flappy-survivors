using System;
using UnityEngine;

namespace Gameplay.Background
{
    [Serializable]
    public class BackgroundLayer
    {
        private static readonly int XOFFSET_ID = Shader.PropertyToID("_XOffset");
        private static readonly int CONTRAST_ID = Shader.PropertyToID("_Contrast");
        private static readonly int EMISSION_MODIFIER_ID = Shader.PropertyToID("_EmissionModifier");
        
        public string name;
        
        [SerializeField] private int layerDepth;
        [SerializeField] private int sortingLayer;
        [SerializeField][Range(-5.0f, 5.0f)] private float scrollSpeed;
        [SerializeField][Range(0-5.0f, 5.0f)] private float contrast = 1.0f;
        [SerializeField] private float emissionModifier = 0.0f;
        [SerializeField] private SpriteRenderer[] layerElements;

        private MaterialPropertyBlock materialPropertyBlock = null;
        
        private float xOffset = 0.0f;

        public void LayerInitialize()
        {
            xOffset = 0.0f;
            LayerValidate();
        }

        public void LayerUpdate()
        {
            xOffset += scrollSpeed * Time.deltaTime;
            materialPropertyBlock.SetFloat(XOFFSET_ID, xOffset);
            
            foreach (SpriteRenderer renderer in layerElements)
            {
                renderer.SetPropertyBlock(materialPropertyBlock);
            }
        }

        public void LayerValidate()
        {
            ValidateMaterialPropertyBlock();
            materialPropertyBlock.SetFloat(CONTRAST_ID, contrast);
            materialPropertyBlock.SetFloat(EMISSION_MODIFIER_ID, emissionModifier);
            
            foreach (SpriteRenderer renderer in layerElements)
            {
                renderer.sortingOrder = layerDepth;
                renderer.sortingLayerID = SortingLayer.layers[sortingLayer].id;
                renderer.SetPropertyBlock(materialPropertyBlock);
            }
        }

        private void ValidateMaterialPropertyBlock()
        {
            if (materialPropertyBlock == null && layerElements.Length > 0)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
                layerElements[0].GetPropertyBlock(materialPropertyBlock);
            }
        }

    }
}