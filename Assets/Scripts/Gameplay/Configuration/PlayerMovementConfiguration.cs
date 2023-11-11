using UnityEngine;

namespace Gameplay.Configuration
{
    [CreateAssetMenu(fileName = "Player Movement Data", menuName = "Data/Player Movement Data", order = 0)]
    public class PlayerMovementConfiguration : ScriptableObject
    {
        public float NeutralDuration => neutralDuration;
        public float ClimbSpeed => climbSpeed;
        public float DiveSpeed => diveSpeed;
        public float ClimbAccelTime => climbAccelTime;
        public float DiveAccelTime => diveAccelTime;
        public float ForwardSpeed => forwardSpeed;
        public float ReverseSpeed => reverseSpeed;
        public float MaxX => maxX;
        public float MinX => minX;
        public float MaxY => maxY;
        public float MinY => minY;

        [SerializeField] private float neutralDuration;
        [SerializeField] private float climbSpeed;
        [SerializeField] private float diveSpeed;
        [SerializeField] private float climbAccelTime;
        [SerializeField] private float diveAccelTime;
        [SerializeField] private float forwardSpeed;
        [SerializeField] private float reverseSpeed;
        [SerializeField] private float maxX;
        [SerializeField] private float minX;
        [SerializeField] private float maxY;
        [SerializeField] private float minY;
    }
}
