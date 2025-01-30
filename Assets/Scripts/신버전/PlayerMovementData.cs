using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement Data")]
public class PlayerMovementData : ScriptableObject
{
    [Header("Movement Settings")]
    [Range(0f, 50f)] public float MaxSpeed = 9.01f;

    [SerializeField,Range(0.01f, 20f)] private float RunAcceleration;
    [SerializeField,Range(0.01f, 20f)] private float RunDecceleration;
   
    [HideInInspector] public float MaxAccelAmount;
    [HideInInspector] public float MaxDeccelAmount;
   

    [Range(0f, 3f)] public float MaxAirAcceleration;
    [Range(0f, 3f)] public float MaxAirDecceleration;
  

  

    [Header("Movement Settings")]
    [Range(2f, 10f)] public float JumpHeight;
    [Range(0.2f, 2f)] public float TimeToJumpApex;
    [HideInInspector] public float JumpForce;

    [Header("Gravity Settings")]
    public float FallGravityMultiplier;
    public float FastFallGravityMultiplier;
    public float MaxFallSpeed;
    public float MaxFastFallSpeed;

    [HideInInspector] public float GravityStrength;
    [HideInInspector]public float GravityScale;

    [Header("Jump Modifiers")]

    [Range(1f, 10f)] public float JumpCutGravityMultiplier;
    [Range(0f, 1f)] public float JumpHangGravityMultiplier;
    [Range(0f,2f)]public float JumpHangTimeThreshold;
    [Range(0f, 2f)] public float JumpHangMaxSpeed;
    [Range(0f, 3f)] public float JumpHangAcceleration;


    [Header("Wall Jump")]
    public Vector2 WallJumpForce;
    [Range(0, 1.5f)] public float WallJumpTime;
    [Range(0f, 1f)] public float WallJumpRunLerp;

    [Header("Gameplay Assists")]
    [Range(0.1f, 1f)] public float CoyoteTime;
    [Range(0.1f, 1f)] public float InputBufferTime;
    public bool EnableDoubleJump;

    [Header("Slide Settings")]
    public float SlideSpeed;
    public float SlideAccel;

    [Header("Dash Settings")]
    [Range(1, 10)] public int DashCount;
    //public float DashDistance;
    public float DashSpeed;
    public float DashEndSpeed;
    //[Range(0f, 1f)] public float DashEndRunLerp;
    //[Range(0.1f, 1f)] public float DashTurnSpeed;

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

       // RunAcceleration = Mathf.Clamp(RunAcceleration,0.01f,MaxSpeed);
       // RunDecceleration = Mathf.Clamp(RunDecceleration, 0.01f, MaxSpeed);
      

        //a = ¥Äv / ¥Ät
        MaxAccelAmount = (RunAcceleration / MaxSpeed) / Time.fixedDeltaTime;
        MaxDeccelAmount = (RunDecceleration / MaxSpeed) / Time.fixedDeltaTime;
      
    }


}
