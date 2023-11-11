using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerModel
    {
        public int TotalXP => totalXp;
        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float CurrentXSpeed => _currentXSpeed;
        public float CurrentYSpeed => _currentYSpeed;
        
        private int totalXp = 0;
        private float _maxHealth = 5.0f;
        private float _currentHealth = 5.0f;
        
        private float _currentXSpeed;
        private float _currentYSpeed;

        public void SetXSpeed(float value)
        {
            _currentXSpeed = value;
        }
        
        public void SetYSpeed(float value)
        {
            _currentYSpeed = value;
        }

        public void ChangeHealth(float value)
        {
            _currentHealth = Mathf.Clamp(_currentHealth + value, 0.0f, _maxHealth);
        }

        public void ChangeXP(int value)
        {
            totalXp += value;
        }
    }
}
