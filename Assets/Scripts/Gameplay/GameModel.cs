namespace Gameplay
{
    public class GameModel
    {
        public static GamePhase CurrentGamePhase => instance.currentGamePhase;
        
        private GamePhase currentGamePhase = GamePhase.IntroPhase;

        private static GameModel instance;

        public void Initialize()
        {
            instance = this;
        }

        public void SetGamePhase(GamePhase newPhase)
        {
            currentGamePhase = newPhase;
        }
    }
}