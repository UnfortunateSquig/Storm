using UnityEngine;

namespace Assets.Scripts.States
{
    class MenuScreenState : IGameState
    {
        private readonly GameManager _manager;

        public MenuScreenState(GameManager manager)
        {
            _manager = manager;
        }

        public void Update()
        {
            if (Input.anyKeyDown)
            {
                _manager.SwitchState(new PlayingGameState(_manager));
            }
        }

        public void Render()
        {
        }

        public void OnGUI()
        {
            GUI.DrawTexture(new Rect(0,0, Screen.width, Screen.height), _manager.MenuScreenBackground);
        }
    }
}
