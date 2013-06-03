using UnityEngine;

namespace Assets.Scripts
{
    class CameraController : MonoBehaviour
    {
        public GameObject Target;
        private GameObject _camera;

        public float AcceptableXDifference = 1.0f; // How far is the target allowed to move away from the center of the camera before the camera tries to catch up?
        public float AcceptableYDifference = 5.0f;
        public float DefaultCameraRest; // In comparison to the player object!
        public float CameraSinkY = 3.0f; // How far below the character model is the camera comfy?
        
        public void Start()
        {
            _camera = GameObject.Find("Main Camera");
        }

        public void Update()
        {
            DefaultCameraRest = Target.transform.position.y - CameraSinkY;

            if (transform.position.y - Target.transform.position.y >= AcceptableYDifference) // When the player is too far 
            {
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, DefaultCameraRest, Time.deltaTime *2), transform.position.z);
            }

            if (transform.position.x - Target.transform.position.x >= AcceptableXDifference) // When the player is to the left of the camera, move left.
            {
                transform.position = new Vector3(Mathf.Lerp(transform.position.x, Target.transform.position.x, Time.deltaTime), transform.position.y, transform.position.z);
            }

            if (-(transform.position.x - Target.transform.position.x) >= AcceptableXDifference) // When the player is to the left of the camera, move right.
            {
                Debug.Log(Target.transform.position.x - transform.position.x);
                transform.position = new Vector3(Mathf.Lerp(transform.position.x, Target.transform.position.x, Time.deltaTime), transform.position.y, transform.position.z);
            }
        }
    }
}
