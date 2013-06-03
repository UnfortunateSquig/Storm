using UnityEngine;

public class PlayerMovementVariables
{
    /// <summary>
    /// The following is used in conjunction with Player.cs and contains tons of public variables. It's ugly, but that means that values within can be modified from the Unity inspector.
    /// 
    /// PLEASE DON'T ACCESS ANY DATA WITHIN THIS CLASS EXCEPT FROM PLAYER.CS
    /// 
    /// Consider refactoring once player movement is more readily defined and values aren't subject to quite as many changes.
    /// </summary>
    
    #region Horizontal Movement Variables 
    public float WalkSpeed = 3.0f; // Default speed value when using keyboard and WALK modifier is depressed.
    public float MaxSpeed = 10.0f; // Default speed for keyboard movement. Maximum speed for analogue stick movement.
    public float InputAxisRunRatio = 1.0f; // Ratio at which analogue stick input is multiplied to result in movement. Movement speed cannot exceed MaxSpeed.

    public float InAirControlAcceleration = 1.0f;

    public float Gravity = 5.0f;
    public float MaxFallSpeed = 5.0f;

    public float SpeedSmoothingRatio = 20.0f; // Higher number = smoother transitions.

    [System.NonSerialized] public Vector3 Direction = Vector3.zero; // The current move direction in x-y. This will always be (1,0,0) or (-1,0,0)
    [System.NonSerialized] public float VerticalSpeed = 0.0f;
    [System.NonSerialized] public float CurrentMovementSpeed = 0.0f;
    [System.NonSerialized] public bool PlayerInputtingHorizontalMovement = false;
    [System.NonSerialized] public CollisionFlags CollisionFlags;
    [System.NonSerialized] public Vector3 Velocity; //An approximation of the character's current velocity, used for camera prediction.
    [System.NonSerialized] public Vector3 InAirVelocity; //Same as above, for when not grounded.
    [System.NonSerialized] public float HangTime = 0.0f; //How long have we been in the air for?
    #endregion

    #region Jumping Variables
    public bool AllowedToJump = true;
    public bool AllowedToDoubleJump = true;

    public float BaseJumpHeight = 1.0f;
    public float ExtraJumpExtension = 4.1f; // The amount of extra jump for holding down the button after initial press.
    public float DoubleJumpHeight = 2.1f;

    [System.NonSerialized] public float RepeatTime = 0.05f; // Stops the player from accidentally triggering jumps too quickly.
    [System.NonSerialized] public float Timeout = 0.15f;
    [System.NonSerialized] public bool PlayerJumping = false;
    [System.NonSerialized] public bool PlayerDoubleJumping = false;
    [System.NonSerialized] public bool ReachedApex = false;
    [System.NonSerialized] public float LastButtonPressedTime = -10.0f;
    [System.NonSerialized] public float LastJumpTime = -1.0f;
    [System.NonSerialized] public float LastJumpStartHeight = 0.0f;

    #endregion
}