using TMPro;
using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "VFX Config", menuName = "Config/VFX Config", order = 0)]
    public class VFXConfiguration : ScriptableObject
    {
        private const string EMISSION_STRING = "_Emission";
        
        public float InitialBaselineEmission => initialBaselineEmission;
        public float EmissionChangeDelay => emissionChangeDelay;
        public float ExplosionDelay => explosionDelay;
        public float DamageTextInDelay => damageTextInDelay;
        public float DamageTextOutDelay => damageTextOutDelay;
        public GameObject ExplosionPrefab => explosionPrefab;
        public TextMeshPro DamageTextPrefab => damageTextPrefab;
        public Material SharedSpriteMaterial => sharedSpriteMaterial;
        public int EmissionHash => emissionHash;

        [SerializeField] private float initialBaselineEmission = 1.0f;
        [SerializeField] private float emissionChangeDelay = 3.0f;
        [SerializeField] private float explosionDelay = 1.0f;
        [SerializeField] private float damageTextInDelay = 0.3f;
        [SerializeField] private float damageTextOutDelay = 0.7f;
        
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private TextMeshPro damageTextPrefab;
        [SerializeField] private Material sharedSpriteMaterial;
        
        private readonly int emissionHash = Shader.PropertyToID(EMISSION_STRING);

        public void Cleanup()
        {
            sharedSpriteMaterial.SetFloat(emissionHash, 1.0f);
        }
    }
}