using UnityEngine;

namespace Assets.Scripts
{
    public class Player : MonoBehaviour
    {
        #region Horizontal Movement Variables

        public float WalkSpeed = 3.0f; // Default speed value when using keyboard and WALK modifier is depressed.
        public float MaxRunSpeed = 10.0f;// Default speed for keyboard movement. Maximum cap for analogue stick movement.
        public float InputAxisRunRatio = 1.0f; // Ratio at which analogue stick input is multiplied to result in movement. Cannot exceed MaxRunSpeed.
        public float InAirControlAcceleration = 1.0f;
        public float Gravity = 5.0f;
        public float MaxFallSpeed = -5.0f;

        public float SpeedSmoothingRatio = 20.0f; // Lower number = more smoothing. Setting this too low will cause the player to "slide."

        private Vector3 _direction = Vector3.zero; // The current move direction in x-y. Will always be (1,0,0) or (-1,0,0)

        private float _verticalSpeed = 0.0f;
        private float _currentMovementSpeed = 0.0f;
        private bool _isPlayerInputtingHorizontalMovement = false;
        private CollisionFlags _collisionFlags;
        private Vector3 _velocity; // An approximation of th character's current velocity, used for camera prediction.

        #endregion

        #region Jumping Movement Variables

        public bool CanJump = true;

        public float BasicJumpHeight = 1.0f;
        public float ExtraJumpHeight = 5.0f;
        public float RepeatTime = 0.05f; // Stops the player from accidentally triggering jumps too quickly.
        public  float JumpTimeout = 0.15f;

        private bool _isJumping = false;
        private bool _reachedApex = false;
        private float _lastButtonPressedTime = -10.0f;
        private float _lastJumpTime = -1.0f;
        private float _lastJumpStartHeight = 0.0f;
        private float _inAirTime; // How long have we been jumping for?
        private Vector3 _inAirVelocity; // Same as _velocity, for when not grounded.
        private bool _holdingJumpButton = false; // Used to stop the player from holding the jump button and bouncing continuously.

        #endregion

        #region Miscellaneous Player Variables

        public bool IsControllable = true; //You can take away the player's control here, if you're a jerk.

        public static float PlayerModelXWidth = 0.6f; // How far across is the player model? Used for Raycasts to offset to the left and right of the model.

        private Transform _spawnPoint;
        private GameObject _player;

        // Movement Variables
        private bool _isGrounded = true;
        private bool _isTouchingCeiling = false;
        private bool _collideLeft = false;
        private bool _collideRight = false;
        private Transform _thisTransform;

        //Raycasts
        public static float RayBlockedDistX = 0.6f; // How long does a raycast need to be to decide if we're blocked on the horizontal plane?
        public static float RayBlockedDistY = 0.7f; // And on the vertical.
        private readonly float _playerModelRaycastOffsetX = (PlayerModelXWidth/2);
        private RaycastHit _raycastHit;

        //Layer Masks
        private int _groundMask = 1 << 8; // Layer = ground.

        #endregion

        public void Awake()
        {
            _thisTransform = transform;
            _direction = transform.TransformDirection(Vector3.forward);
            Spawn();
        }

        private void Spawn()
        {
            _verticalSpeed = 0.0f;
            _currentMovementSpeed = 0.0f;
        }

        void UpdateRaycasts()
        {
            //Cast two sets of Raycasts. One down, one up, to determine if we're on the ground or touching a ceiling.
            if (Physics.Raycast(new Vector3(_thisTransform.position.x - _playerModelRaycastOffsetX, _thisTransform.position.y, _thisTransform.position.z + 1.0f), -Vector3.up, out _raycastHit, RayBlockedDistY, _groundMask)
                || Physics.Raycast(new Vector3(_thisTransform.position.x + _playerModelRaycastOffsetX, _thisTransform.position.y, _thisTransform.position.z + 1.0f), -Vector3.up, out _raycastHit, RayBlockedDistY, _groundMask))
                _isGrounded = true;
            else
                _isGrounded = false;

            if (Physics.Raycast(new Vector3(_thisTransform.position.x - _playerModelRaycastOffsetX, _thisTransform.position.y, _thisTransform.position.z + 1.0f), Vector3.up, out _raycastHit, RayBlockedDistY, _groundMask)
                || Physics.Raycast(new Vector3(_thisTransform.position.x + _playerModelRaycastOffsetX, _thisTransform.position.y, _thisTransform.position.z + 1.0f), Vector3.up, out _raycastHit, RayBlockedDistY, _groundMask))
                _isTouchingCeiling = true;
            else
                _isTouchingCeiling = false;

            //Do the same for the left and right.
            if (Physics.Raycast(new Vector3(_thisTransform.position.x - _playerModelRaycastOffsetX, _thisTransform.position.y, _thisTransform.position.z + 1.0f), Vector3.left, out _raycastHit, RayBlockedDistX, _groundMask)
                || Physics.Raycast(new Vector3(_thisTransform.position.x + _playerModelRaycastOffsetX, _thisTransform.position.y, _thisTransform.position.z + 1.0f), Vector3.left, out _raycastHit, RayBlockedDistX, _groundMask))
                _collideLeft = true;
            else
                _collideLeft = false;

            if (Physics.Raycast(new Vector3(_thisTransform.position.x - _playerModelRaycastOffsetX, _thisTransform.position.y, _thisTransform.position.z + 1.0f), Vector3.right, out _raycastHit, RayBlockedDistX, _groundMask)
                || Physics.Raycast(new Vector3(_thisTransform.position.x + _playerModelRaycastOffsetX, _thisTransform.position.y, _thisTransform.position.z + 1.0f), Vector3.right, out _raycastHit, RayBlockedDistX, _groundMask))
                _collideRight = true;
            else
                _collideRight = false;
        }

        void UpdateSmoothedMovementDirection()
        {
            float h = Input.GetAxisRaw("Horizontal");
            if (!IsControllable)
                h = 0.0f;

            _isPlayerInputtingHorizontalMovement = Mathf.Abs(h) > 0.1;
            if (_isPlayerInputtingHorizontalMovement)
                _direction = new Vector3(h, 0, 0);

            
            float curSmooth = SpeedSmoothingRatio*Time.deltaTime;
            float targetSpeed = Mathf.Min(Mathf.Abs(h), 1.0f);

            targetSpeed *= MaxRunSpeed;
            _currentMovementSpeed = Mathf.Lerp(_currentMovementSpeed, targetSpeed, curSmooth);
            _inAirTime = 0.0f;
        }

        public void Start()
        {
            _player = GameObject.Find("Player");   
        }

        public void Update()
        {
            UpdateRaycasts();

            if (Input.GetButton("Jump") && IsControllable)
            {
                if (Time.time < _lastButtonPressedTime + JumpTimeout)
                {
                    _holdingJumpButton = true;
                }
                _lastButtonPressedTime = Time.time;
            }

            if (Input.GetButtonUp("Jump"))
                _holdingJumpButton = false;

            UpdateSmoothedMovementDirection();
            ApplyGravity();
            Jump();

            Vector3 lastPosition = transform.position;
            Vector3 currentMovementOffset = _direction*_currentMovementSpeed + new Vector3(0, _verticalSpeed, 0) + _inAirVelocity;
            currentMovementOffset *= Time.deltaTime; // Makes movement independant of framerate.

            if (_collideLeft && currentMovementOffset.x < 0)
                currentMovementOffset.x = 0;
            
            if (_collideRight && currentMovementOffset.x > 0)
                currentMovementOffset.x = 0;
            
            transform.Translate(currentMovementOffset); //Actually moves!
            

            _velocity = (transform.position - lastPosition)/Time.deltaTime; // Sets our velocity, but makes sure it's not being stopped by a collision.

            if (_isGrounded && Time.time >= _lastJumpTime + JumpTimeout)
            {
                _inAirVelocity = Vector3.zero;

                if (_isJumping)
                {
                    _isJumping = false;
                    Debug.Log("Landed");
                }

                Vector3 jumpMoveDirection = _direction*_currentMovementSpeed + _inAirVelocity;

                if (jumpMoveDirection.sqrMagnitude > 0.01)
                    _direction = jumpMoveDirection.normalized;
            }
        }

        public void FixedUpdate()
        {
           // transform.position = new Vector3(transform.position.x, transform.position.y, -1.0f); //Absolutely, positively ensure our collider is on the 2D plane.
        }

        void Jump()
        {
            if (_lastJumpTime + RepeatTime > Time.time)
                return;

            if (_isGrounded)
            {
                if (CanJump && Time.time > _lastJumpTime + JumpTimeout && Input.GetButton("Jump") && !_holdingJumpButton)
                {
                    _verticalSpeed = CalculateJumpVerticalSpeed(BasicJumpHeight);
                    DidJump();
                }
            }
        }

        void ApplyGravity()
        {
            bool jumpButtonPressed = Input.GetButton("Jump"); // We need to know if Jump is pressed so that gravity is off when we're trying to jump.
            bool extraJump = _isJumping && _verticalSpeed > 0.0 && jumpButtonPressed && transform.position.y < _lastJumpStartHeight + ExtraJumpHeight && !_isTouchingCeiling;

            if (!IsControllable)
                jumpButtonPressed = false;

            if (_isJumping && !_reachedApex && _verticalSpeed <= 0.0)
            {
                _reachedApex = true;
                Debug.Log("Reached Apex");
            }

            if (_isGrounded && !jumpButtonPressed)
            {
                _verticalSpeed = 0;
            }

            else if (_isGrounded && jumpButtonPressed && _holdingJumpButton)
            {
                _verticalSpeed = 0;
            }

            else if (extraJump)
                return;

            else
            {
                _verticalSpeed -= Gravity * Time.deltaTime;
            }

            _verticalSpeed = Mathf.Max(_verticalSpeed, MaxFallSpeed); // Enforce terminal velocity.
        }

        float CalculateJumpVerticalSpeed(float targetJumpHeight)
        {
            return Mathf.Sqrt(2*targetJumpHeight*Gravity); // From the jump height and gravity, we deduce the upwards speed for the character to reach the apex.
        }

        void DidJump()
        {
            Debug.Log("Did jump!");
            _isJumping = true;
            _reachedApex = false;
            _lastJumpTime = Time.time;
            _lastJumpStartHeight = transform.position.y;
            _lastButtonPressedTime = -10;
        }
    }
}
