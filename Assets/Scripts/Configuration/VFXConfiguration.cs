using System;
using TMPro;
using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "VFX Config", menuName = "Config/VFX Config", order = 0)]
    public class VFXConfiguration : ScriptableObject
    {
        private const string EMISSION_STRING = "_Emission";
        private const string CONTRAST_RANGE_STRING = "_ContrastModifier";
        
        public float InitialBaselineEmission => initialBaselineEmission;
        public float InitialBaselineContrastRange => initialBaselineContrastRange;
        public float EmissionChangeDelay => emissionChangeDelay;
        public float ContrastRangeChangeDelay => contrastRangeChangeDelay;
        public float ExplosionDelay => explosionDelay;
        public float DamageTextInDelay => damageTextInDelay;
        public float DamageTextOutDelay => damageTextOutDelay;
        public GameObject ExplosionPrefab => explosionPrefab;
        public TextMeshPro DamageTextPrefab => damageTextPrefab;
        public Material SharedSpriteMaterial => sharedSpriteMaterial;
        public int EmissionHash => emissionHash;
        public int ContrastRangeHash => contrastRangeHash;

        [SerializeField] private float initialBaselineEmission = 1.0f;
        [SerializeField] private float initialBaselineContrastRange = 1.0f;
        [SerializeField] private float emissionChangeDelay = 3.0f;
        [SerializeField] private float contrastRangeChangeDelay = 2.0f;
        [SerializeField] private float explosionDelay = 1.0f;
        [SerializeField] private float damageTextInDelay = 0.3f;
        [SerializeField] private float damageTextOutDelay = 0.7f;
        
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private TextMeshPro damageTextPrefab;
        [SerializeField] private Material sharedSpriteMaterial;
        
        private readonly int emissionHash = Shader.PropertyToID(EMISSION_STRING);
        private readonly int contrastRangeHash = Shader.PropertyToID(CONTRAST_RANGE_STRING);

        public void Cleanup()
        {
            sharedSpriteMaterial.SetFloat(emissionHash, initialBaselineEmission);
            sharedSpriteMaterial.SetFloat(contrastRangeHash, InitialBaselineContrastRange);
        }
    }
}