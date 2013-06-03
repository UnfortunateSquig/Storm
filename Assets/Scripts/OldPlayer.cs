using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class OldPlayer : MonoBehaviour
    {
        public bool IsControllable = true; //You can take away the player's control here. If you're a jerk.

        private Transform _spawnPoint;
        private PlayerMovementVariables _movementVariables;
        private CharacterController _controller;

        // Moving Platform support.
        private Transform _activePlatform;
        private Vector3 _activeLocalPlatformPoint;
        private Vector3 _activeGlobalPlatfromPoint;
        private Vector3 _lastPlatformVelocity;

        // Attack objects
        private Transform _attackParent;
        private Renderer _attackRenderer;
        private OTAnimatingSprite _attackSprite;

        //Movement
        private float _baseMoveSpeed = 5;
        private int _moveDirX;
        private int _moveDirY;
        private Vector3 _movement;
        private Transform _thisTransform;
        private bool _isTouchingCeiling;

        //Raycasts
        private float _rayBlockedDistX = 0.6f;
        private RaycastHit _raycastHit;

        //Layer Masks
        private int _groundMask = 1 << 8; // Layer = Ground

        void Awake()
        {
            _thisTransform = transform;
            _movementVariables = new PlayerMovementVariables();
            _movementVariables.Direction = transform.TransformDirection(Vector3.forward);
            //_controller = GetComponent<CharacterController>();
            Spawn();
        }

        void Spawn()
        {
            _movementVariables.VerticalSpeed = 0.0f;
            _movementVariables.CurrentMovementSpeed = 0.0f;
            //transform.position = _spawnPoint.position;
        }

        void OnDeath()
        {
            Spawn();
        }

        void UpdateSmoothedMovementDirection()
        {
            float h = Input.GetAxisRaw("Horizontal");
            if (!IsControllable)
                h = 0.0f;

            _movementVariables.PlayerInputtingHorizontalMovement = Mathf.Abs(h) > 0.1;
            if (_movementVariables.PlayerInputtingHorizontalMovement)
                _movementVariables.Direction = new Vector3(h, 0, 0);
            
            /* Grounded controls
            if (_controller.isGrounded)
            */
                 
            float curSmooth = _movementVariables.SpeedSmoothingRatio * Time.deltaTime;
            float targetSpeed = Mathf.Min(Mathf.Abs(h), 1.0f);
            
            /* Keyboard speed modifier toggling. Do not use. Currently is too "instantaneous."
            if (Input.GetButton("Fire2") && IsControllable)
                targetSpeed *= _movementVariables.MaxSpeed;
            else
                targetSpeed *= _movementVariables.WalkSpeed;
            */

            targetSpeed *= _movementVariables.MaxSpeed;
            _movementVariables.CurrentMovementSpeed = Mathf.Lerp(_movementVariables.CurrentMovementSpeed, targetSpeed, curSmooth);
            _movementVariables.HangTime = 0.0f;

            //}
            /*else
            {
                _movementVariables.HangTime += Time.deltaTime;

                if (_movementVariables.PlayerInputtingHorizontalMovement)
                    _movementVariables.InAirVelocity += new Vector3(Mathf.Sign(h), 0, 0)*Time.deltaTime * _movementVariables.InAirControlAcceleration;
            }*/
        }

        void Start()
        {
            PlayerBackend.alive = true;
        }

        void Update()
        {
            UpdateRaycasts();

            if (Input.GetButton("Jump") && IsControllable)
                _movementVariables.LastButtonPressedTime = Time.time;

            UpdateSmoothedMovementDirection();

            ApplyGravity();

            Jump();

            if (_activePlatform != null)
            {
                Vector3 newGlobalPlatformPoint = _activePlatform.TransformPoint(_activeLocalPlatformPoint);
                Vector3 moveDistance = (newGlobalPlatformPoint - _activeGlobalPlatfromPoint);
                transform.position = transform.position + moveDistance;
                _lastPlatformVelocity = (newGlobalPlatformPoint - _activeGlobalPlatfromPoint)/Time.deltaTime;
            }

            else
            {
                _lastPlatformVelocity = Vector3.zero;
            }

            _activePlatform = null;

            Vector3 lastPosition = transform.position;
            Vector3 currentMovementOffset = _movementVariables.Direction*_movementVariables.CurrentMovementSpeed +
                                            new Vector3(0, _movementVariables.VerticalSpeed, 0) +
                                            _movementVariables.InAirVelocity;
            currentMovementOffset *= Time.deltaTime; //Makes movement framerate independant.

            //_movementVariables.CollisionFlags = _controller.Move(currentMovementOffset); // Move!
            this.transform.Translate(currentMovementOffset);

            _movementVariables.Velocity = (transform.position - lastPosition)/Time.deltaTime; // Make sure that we calculate our actual velocity, in case we're stopped by a collision.

            if (_activePlatform != null)
            {
                _activeGlobalPlatfromPoint = transform.position;
                _activeLocalPlatformPoint = _activePlatform.InverseTransformPoint(transform.position);
            }

            if (!PlayerBackend.falling)
            {
                _movementVariables.InAirVelocity = Vector3.zero;

                if (_movementVariables.PlayerJumping || _movementVariables.PlayerDoubleJumping)
                {
                    _movementVariables.PlayerJumping = false;
                    _movementVariables.PlayerDoubleJumping = false;
                    _movementVariables.AllowedToDoubleJump = false;

                    SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);

                    Vector3 jumpMoveDirection = _movementVariables.Direction*_movementVariables.CurrentMovementSpeed +
                                                _movementVariables.InAirVelocity;

                    if (jumpMoveDirection.sqrMagnitude > 0.01)
                        _movementVariables.Direction = jumpMoveDirection.normalized;
                }
            }

            /*
            _moveDirX = 0;
            _moveDirY = 0;

            if (PlayerBackend.isLeft && !PlayerBackend.blockedLeft && !PlayerBackend.attacking)
            {
                _moveDirX = -1;
                PlayerBackend.facingDir = 1;
            }

            if (PlayerBackend.isRight && !PlayerBackend.blockedRight && !PlayerBackend.attacking)
            {
                _moveDirX = 1;
                PlayerBackend.facingDir = 2;
            }
            */
           // UpdateMovement();
        }

        void FixedUpdate()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -1.0f); // Absolutely, positively ensure that we're always on the 2d plane.
        }

        /*private void UpdateMovement()
        {
            if (!PlayerBackend.falling)
            {
                _movement = new Vector3(_moveDirX, _moveDirY, 0f);
                _movement *= Time.deltaTime*_baseMoveSpeed*(Math.Abs(Input.GetAxis("Horizontal")));
                _thisTransform.Translate(_movement.x, _movement.y, 0f);
            }
            else
            {
                _movement = new Vector3(_moveDirX, -1f, 0f);
                _movement *= Time.deltaTime*_baseMoveSpeed;
                _thisTransform.Translate(0f, _movement.y, 0f);
            }
        }*/

        void UpdateRaycasts()
        {
            PlayerBackend.blockedRight = false;
            PlayerBackend.blockedLeft = false;

            //Cast two rays down, one from each side of Player, to see if it is standing on the ground
            if (Physics.Raycast(new Vector3(_thisTransform.position.x - 0.3f, _thisTransform.position.y, _thisTransform.position.z + 1f), -Vector3.up, out _raycastHit, 0.7f, _groundMask)
                || Physics.Raycast(new Vector3(_thisTransform.position.x + 0.3f, _thisTransform.position.y, _thisTransform.position.z + 1f), -Vector3.up, out _raycastHit, 0.7f, _groundMask))
                PlayerBackend.falling = false;
            else
                PlayerBackend.falling = true;

            //Cast two rays up, one from each side of Player, to see if it is hitting a ceiling
            if (Physics.Raycast(new Vector3(_thisTransform.position.x - 0.3f, _thisTransform.position.y, _thisTransform.position.z + 1f), Vector3.up, out _raycastHit, 0.7f, _groundMask)
                || Physics.Raycast(new Vector3(_thisTransform.position.x + 0.3f, _thisTransform.position.y, _thisTransform.position.z + 1f), Vector3.up, out _raycastHit, 0.7f, _groundMask))
                _isTouchingCeiling = true;
            else
                _isTouchingCeiling = false;
        }

        void Jump()
        {
            if (_movementVariables.LastJumpTime + _movementVariables.RepeatTime > Time.time)
                return;

            if (!PlayerBackend.falling)
            {
                if (_movementVariables.AllowedToJump && Time.time < _movementVariables.LastJumpTime + _movementVariables.Timeout)
                {
                    _movementVariables.VerticalSpeed = CalculateJumpVerticalSpeed(_movementVariables.BaseJumpHeight);
                    _movementVariables.InAirVelocity = _lastPlatformVelocity;
                    //SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
                    DidJump();
                }
            }
        }

        void ApplyGravity()
        {
            bool jumpButton = Input.GetButton("Jump");

            if (!IsControllable)
                jumpButton = false;

            if (_movementVariables.PlayerJumping && !_movementVariables.ReachedApex && _movementVariables.VerticalSpeed <= 0.0)
            {
                _movementVariables.ReachedApex = true;
                SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
            }

            if ((_movementVariables.PlayerJumping && Input.GetButtonUp("Jump") && //If we are jumping or falling we can double jump.
                 !_movementVariables.PlayerDoubleJumping ||
                 (PlayerBackend.falling && !_movementVariables.PlayerJumping &&
                  !_movementVariables.PlayerDoubleJumping && _movementVariables.VerticalSpeed < -12.0)))
                _movementVariables.AllowedToDoubleJump = true;

            if (_movementVariables.AllowedToDoubleJump && Input.GetButtonDown("Jump") && !IsTouchingCeiling()) // Actually double jumps
            {
                _movementVariables.PlayerDoubleJumping = true;
                _movementVariables.VerticalSpeed = CalculateJumpVerticalSpeed(_movementVariables.DoubleJumpHeight);
                _movementVariables.PlayerDoubleJumping = false;
            }

            bool extraJumpTime = _movementVariables.PlayerJumping && !_movementVariables.PlayerDoubleJumping && // Allows for holding button for longer jumps
                                 _movementVariables.VerticalSpeed > 0.0 && jumpButton &&
                                 transform.position.y <
                                 _movementVariables.LastJumpStartHeight + _movementVariables.ExtraJumpExtension &&
                                 !IsTouchingCeiling();

            if (extraJumpTime)
                return;

            if (!PlayerBackend.falling)
            {
                _movementVariables.VerticalSpeed = -_movementVariables.Gravity*Time.deltaTime;
                _movementVariables.AllowedToDoubleJump = false;
            } 
            else
            {
                _movementVariables.VerticalSpeed -= _movementVariables.Gravity*Time.deltaTime;
            }

            _movementVariables.VerticalSpeed = Mathf.Max(_movementVariables.VerticalSpeed, -_movementVariables.MaxFallSpeed); //Terminal velocity
        }

        float CalculateJumpVerticalSpeed(float targetJumpHeight)
        {
            return Mathf.Sqrt(2*targetJumpHeight*_movementVariables.Gravity); // Take in jump height to deduce upwards speed for character to reach the apex.
        }

        void DidJump()
        {
            Debug.Log("DidJump()");
            _movementVariables.PlayerJumping = true;
            _movementVariables.ReachedApex = false;
            _movementVariables.LastJumpTime = Time.time;
            _movementVariables.LastJumpStartHeight = transform.position.y;
            _movementVariables.LastButtonPressedTime = -10;
        }

        public float GetSpeed()
        {
            return _movementVariables.CurrentMovementSpeed;
        }

        public Vector3 GetVelocity()
        {
            return _movementVariables.Velocity;
        }

        public bool IsMoving()
        {
            return _movementVariables.PlayerInputtingHorizontalMovement;
        }

        public bool IsJumping()
        {
            return _movementVariables.PlayerJumping;
        }

        public bool IsDoubleJumping()
        {
            return _movementVariables.PlayerDoubleJumping;
        }

        public bool CanDoubleJump()
        {
            return _movementVariables.AllowedToDoubleJump;
        }

        public bool IsTouchingCeiling()
        {
            return _isTouchingCeiling;
        }

        public Vector3 GetDirection()
        {
            return _movementVariables.Direction;
        }

        public float GetHangTime()
        {
            return _movementVariables.HangTime;
        }

        public void Reset()
        {
            gameObject.tag = "Player";
        }

        public void SetControllable(bool controllable)
        {
            IsControllable = controllable;
        }
    }
}
