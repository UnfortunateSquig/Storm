using UnityEngine;

namespace Assets.Scripts
{
    class PlayerBackend : MonoBehaviour
    {
        public static float orthSize;
        public static float orthSizeX;
        public static float orthSizeY;
        public static float camRatio;

        public static bool blockedRight = false;
        public static bool blockedLeft = false;
        public static bool blockedUp = false;
        public static bool blockedDown = false;


        public static float playerHitboxX = 0.225f; // player x = 0.45
        public static float playerHitboxY = 0.5f;

        public static bool isLeft;
        public static bool isRight;
        public static bool isUp;
        public static bool isDown;
        public static bool isAttack;

        public static bool alive;
        public static bool falling;
        public static bool attacking;

        public static int facingDir = 1; // 1 = left, 2 = right, 3 = up, 4 = down
        public enum anim { None, WalkLeft, WalkRight, StandLeft, StandRight, FallLeft, FallRight, AttackLeft, AttackRight }

        public static Vector3 glx;

        public void Start()
        {
            camRatio = 1.333f; // 800x600
            orthSize = Camera.mainCamera.camera.orthographicSize;
            orthSizeX = orthSize*camRatio;
        }

        public void Update()
        {
            isLeft = false;
            isRight = false;
            isUp = false;
            isDown = false;
            isAttack = false;

            if (Input.GetAxis("Horizontal") < 0)
            {
                isLeft = true;
            }
            if (Input.GetAxis("Horizontal") > 0)
            {
                isRight = true;
            }
        }
    }
}
