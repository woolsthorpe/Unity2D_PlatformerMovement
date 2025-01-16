using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    private PlayerController controller;
    private PlayerMovementData data;

    [Header("Options")]
    [SerializeField] private float friction = 0f;

    [Header("Current State")]
    private Vector2 inputDirection;
    private Vector2 currentVelocity;
    private Vector2 desiredVelocity;

    public bool IsPressingKey { get; private set; }
    private bool onGround;
    private float maxSpeedChange;

    public void Initialize(PlayerController controller)
    {
        this.controller = controller;
        this.data = controller.Data;
    }

    private void Update()
    {
        HandlePlayerTurning();
        desiredVelocity = new Vector2(inputDirection.x, 0f) * Mathf.Max(data.MaxSpeed - friction, 0f);
    }

    private void FixedUpdate()
    {
        currentVelocity = controller.GetCurrentVelocity();
        onGround = controller.IsOnGround();

        if (controller.isDashing)
            return;


        if (data.UseAcceleration)
        {
            RunWithAcceleration();
        }
        else
        {
            if (onGround) 
                RunWithoutAcceleration();
            else 
                RunWithAcceleration();
        }
    }

    public void HandleInput()
    {
        inputDirection.x = Input.GetAxisRaw("Horizontal");
        inputDirection.y = Input.GetAxisRaw("Vertical");
        IsPressingKey = inputDirection.x != 0;
    }

    private void HandlePlayerTurning()
    {
        if (!IsPressingKey)
            return;

        int direction = (int)Mathf.Sign(inputDirection.x);

        if (controller.Jump.isSliding && controller.Jump.Apply_TurnSpriteOnSlide)
        {
            direction *= -1;
        }

        if (!controller.Jump.isWallJumping && !controller.isDashing)
        {
            controller.TurnPlayerSprite(direction);
        }
    }
    private void RunWithAcceleration()
    {
        if (IsPressingKey)
        {
            if (Mathf.Sign(inputDirection.x) != Mathf.Sign(currentVelocity.x))
                maxSpeedChange = onGround ? data.MaxTurnSpeed : data.MaxAirTurnSpeed;
            else
                maxSpeedChange = onGround ? data.MaxAcceleration : data.MaxAirAcceleration;
        }
        else
            maxSpeedChange = onGround ? data.MaxDecceleration : data.MaxAirDecceleration;

        if (controller.Jump.CanJumpHang())
            desiredVelocity.x *= data.JumpHangAcceleration;

        if (controller.isDashAttacking)
            maxSpeedChange *= data.DashTurnSpeed;

        currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, desiredVelocity.x, maxSpeedChange * Time.deltaTime);

        if (!controller.isDashing && !controller.Jump.isWallJumping)
            controller.SetCurrentVelocity(currentVelocity);
    }

    private void RunWithoutAcceleration()
    {
        currentVelocity.x = desiredVelocity.x;
        controller.SetCurrentVelocity(currentVelocity);
    }

   
   
    public Vector2 GetInputDirection() => inputDirection;

    #region 외부 상호작용
    public void ChangeFriction(float newFriction)
    {
        Debug.Log($"{friction} => {newFriction}");
        friction = newFriction;
    }
    #endregion
}
