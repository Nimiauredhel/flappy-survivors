namespace Gameplay.Player
{
    public class PlayerModel
    {
        public int TotalXP => totalXp;
        public float Health => health;
        public float CurrentXSpeed => _currentXSpeed;
        public float CurrentYSpeed => _currentYSpeed;
        
        private int totalXp = 0;
        private float health = 5.0f;
        
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
            health += value;
            
            if (health <= 0.0f)
            {
                health = 0.0f;
            }
        }
    }
}
