using UnityEngine;

namespace Configuration
{
    [CreateAssetMenu(fileName = "Level Registry", menuName = "Config/Level Registry", order = 0)]
    public class LevelRegistry : ScriptableObject
    {
        public LevelConfiguration[] Levels => levels;
        
        [SerializeField] private LevelConfiguration[] levels;
    }
}