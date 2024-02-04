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
        private float referenceHeight = -50.0f;
        private Vector3[] initialPositions;

        public void LayerInitialize()
        {
            xOffset = 0.0f;
            initialPositions = new Vector3[layerElements.Length];
            
            for (int i = 0; i < layerElements.Length; i++)
            {
                initialPositions[i] = layerElements[i].transform.position;
            }
            
            LayerValidate();
        }

        public void LayerUpdate(float newReferenceHeight)
        {
            bool heightChanged = false;
            
            if (referenceHeight != newReferenceHeight)
            {
                heightChanged = true;
                referenceHeight = newReferenceHeight;
            }

            float delta = scrollSpeed * Time.deltaTime * (layerElements[0].flipX ? -1 : 1);
            xOffset += delta;
            materialPropertyBlock.SetFloat(XOFFSET_ID, xOffset);

            for (int i = 0; i < layerElements.Length; i++)
            {
                layerElements[i].SetPropertyBlock(materialPropertyBlock);

                if (heightChanged)
                {
                    layerElements[i].transform.position = initialPositions[i] + (Vector3.up * (-referenceHeight * scrollSpeed));
                }
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