using UnityEngine;

namespace Assets.Scripts.States
{
    public class StartScreenState : IGameState
    {
        private readonly GameManager _manager;

        public StartScreenState(GameManager manager)
        {
            _manager = manager;
        }

        public void Update()
        {
            if (Input.anyKeyDown)
                _manager.SwitchState(new MenuScreenState(_manager));
        }

        public void Render()
        {
        }

        public void OnGUI()
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _manager.StartScreenBackground);
        }
    }
}
