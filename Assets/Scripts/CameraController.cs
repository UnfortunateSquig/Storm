using UnityEngine;

namespace Assets.Scripts
{
    class CameraController : MonoBehaviour
    {
        private GameObject _target;
        private GameObject _camera;

        public float AcceptableXDifference = 1.0f; // How far is the target allowed to move away from the center of the camera before the camera tries to catch up?
        public float AcceptableYDifference = 5.0f;
        
        public void Start()
        {
            _camera = GameObject.Find("Main Camera");
        }

        public void Update()
        {
            if (transform.position.x - _target.transform.position.x > AcceptableXDifference)
            {
                return;
            }
        }
    }
}
