using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Configuration
{
    [CreateAssetMenu(fileName = "VFX Config", menuName = "Config/VFX Config", order = 0)]
    public class VFXConfiguration : ScriptableObject
    {
        private const string TINT_STRING = "_Color";
        private const string EMISSION_STRING = "_Emission";
        private const string CONTRAST_RANGE_STRING = "_ContrastModifier";
        private const string OUTLINE_THICKNESS_STRING = "_OutlineThickness";
        
        private static readonly int TINT_HASH = Shader.PropertyToID(TINT_STRING);
        private static readonly int EMISSION_HASH = Shader.PropertyToID(EMISSION_STRING);
        private static readonly int CONTRAST_RANGE_HASH = Shader.PropertyToID(CONTRAST_RANGE_STRING);
        private static readonly int OUTLINE_THICKNESS_HASH = Shader.PropertyToID(OUTLINE_THICKNESS_STRING);
        
        public int TintHash => TINT_HASH;
        public int EmissionHash => EMISSION_HASH;
        public int ContrastRangeHash => CONTRAST_RANGE_HASH;
        public int OutlineThicknessHash => OUTLINE_THICKNESS_HASH;

        public Color InitialBaselineTint => initialBaselineTint;
        public float InitialBaselineEmission => initialBaselineEmission;
        public float InitialBaselineContrastRange => initialBaselineContrastRange;
        public float TintChangeDelay => tintChangeDelay;
        public float EmissionChangeDelay => emissionChangeDelay;
        public float ContrastRangeChangeDelay => contrastRangeChangeDelay;
        public float ExplosionDelay => explosionDelay;
        public float DamageTextInDelay => damageTextInDelay;
        public float DamageTextOutDelay => damageTextOutDelay;
        public float CameraMaxHeight => cameraMaxHeight;
        public float CameraMinHeight => cameraMinHeight;
        public float CameraMoveRatio => cameraMoveRatio;
        public GameObject ExplosionPrefab => explosionPrefab;
        public TextMeshPro DamageTextPrefab => damageTextPrefab;
        public Material SharedSpriteMaterial => sharedSpriteMaterial;

        [SerializeField] private Color initialBaselineTint = Color.white;
        [SerializeField] private float initialBaselineEmission = 1.0f;
        [SerializeField] private float initialBaselineContrastRange = 1.0f;
        [SerializeField] private float tintChangeDelay = 1.0f;
        [SerializeField] private float emissionChangeDelay = 3.0f;
        [SerializeField] private float contrastRangeChangeDelay = 2.0f;
        [SerializeField] private float explosionDelay = 1.0f;
        [SerializeField] private float damageTextInDelay = 0.3f;
        [SerializeField] private float damageTextOutDelay = 0.7f;
        [SerializeField] private float cameraMaxHeight = 1.0f;
        [SerializeField] private float cameraMinHeight = -1.0f;
        [SerializeField][Range(0.0f, 1.0f)] private float cameraMoveRatio = 0.1f;
        
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private TextMeshPro damageTextPrefab;
        [SerializeField] private Material sharedSpriteMaterial;

        public void Cleanup()
        {
            sharedSpriteMaterial.SetColor(TINT_HASH, initialBaselineTint);
            sharedSpriteMaterial.SetFloat(EMISSION_HASH, initialBaselineEmission);
            sharedSpriteMaterial.SetFloat(CONTRAST_RANGE_HASH, InitialBaselineContrastRange);
            sharedSpriteMaterial.SetFloat(OUTLINE_THICKNESS_HASH, 0.0f);
        }
    }
}