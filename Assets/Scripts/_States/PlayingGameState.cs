

namespace Assets.Scripts.States
{
    class PlayingGameState : IGameState
    {
        private readonly GameManager _manager;

        public PlayingGameState(GameManager manager)
        {
            _manager = manager;
        }

        public void Update()
        {
        }

        public void Render()
        {
        }

        public void OnGUI()
        {
        }
    }
}
