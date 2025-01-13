using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement Data")]
public class PlayerMovementData : ScriptableObject
{
    [Header("Movement Settings")]
    [Range(0f, 50f)] public float MaxSpeed = 9.01f;

    [Range(0f, 100f)] public float MaxAcceleration=55;
    [Range(0f, 100f)] public float MaxDecceleration=55;
    [Range(0f, 100f)] public float MaxTurnSpeed = 76;

    [Range(0f, 100f)] public float MaxAirAcceleration=60;
    [Range(0f, 100f)] public float MaxAirDecceleration=58;
    [Range(0f, 100f)] public float MaxAirTurnSpeed = 75;

    public bool UseAcceleration=true;

    [Header("Movement Settings")]
    [Range(2f, 10f)] public float JumpHeight=3.5f;
    [Range(0.2f, 2f)] public float TimeToJumpApex=0.3f;
    [HideInInspector] public float JumpForce;

    [Header("Gravity Settings")]
    public float FallGravityMultiplier = 1.5f;
    public float FastFallGravityMultiplier = 2f;
    public float MaxFallSpeed = 25;
    public float MaxFastFallSpeed = 25;

    [HideInInspector] public float GravityStrength;
    [HideInInspector]public float GravityScale;

    [Header("Jump Modifiers")]

    [Range(1f, 10f)] public float JumpCutGravityMultiplier=2;
    [Range(0f, 1f)] public float JumpHangGravityMultiplier=0.5f;
    [Range(0f,2f)]public float JumpHangTimeThreshold=1;
    [Range(1f, 3f)] public float JumpHangAcceleration=1.1f;


    [Header("Wall Jump")]
    public Vector2 WallJumpForce;
    [Range(0, 1.5f)] public float WallJumpTime;

    [Header("Gameplay Assists")]
    [Range(0.1f, 1f)] public float CoyoteTime=0.1f;
    [Range(0.1f, 1f)] public float InputBufferTime=0.1f;
    public bool EnableDoubleJump;

    [Header("Slide Settings")]
    public float SlideSpeed;
    public float SlideSpeedChangeRate;

    [Header("Dash Settings")]
    [Range(1, 10)] public int DashCount;
    public float DashDistance;
    public float DashSpeed;
    public float DashEndSpeed;
    [Range(0.1f, 1f)] public float DashTurnSpeed;

    public float DashSleepTime;
    public float DashAttackTime;
    public float DashEndTime;
    public float DashRefillTime;
    [Range(0.01f, 0.5f)] public float DashBufferTime;


    private void OnValidate()
    {
        // Calculate gravity strength and scale
        GravityStrength = -(2 * JumpHeight) / (TimeToJumpApex * TimeToJumpApex);
        GravityScale = GravityStrength / Physics2D.gravity.y;

        // Calculate jump force
        JumpForce = Mathf.Abs(GravityStrength) * TimeToJumpApex;
    }


}
