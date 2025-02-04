using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    private PlayerController controller;
    private PlayerMovementData data;

    [Header("Options")]
    [SerializeField] private float friction = 0f;
    [SerializeField] private float AirControlFactor = 0.8f;
    [Header("Current State")]
     private Vector2 inputDirection;
    private Vector2 currentVelocity;
    public void Initialize(PlayerController controller)
    {
        this.controller = controller;
        this.data = controller.Data;
    }
    public void OnMovement(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (inputDirection.x != 0)
            HandlePlayerTurning();
    }

    private void FixedUpdate()
    {


        //if (!controller.isDashing)
        //{
        //    if (controller.Jump.isWallJumping)
        //        RunWithAcceleration(data.WallJumpRunLerp);
        //    else
        //        RunWithAcceleration(1);
        //}
        //else if(controller.isDashAttacking)
        //    RunWithAcceleration(data.DashEndRunLerp);


        if (!controller.isDashing)
        {
            if (controller.Jump.isWallJumping)
                RunWithAcceleration(data.WallJumpRunLerp);
            else
                RunWithAcceleration(1);
        }
     

    }
   
    private void HandlePlayerTurning()
    {
        int direction = (int)Mathf.Sign(inputDirection.x);

        //슬라이딩 옵션이 커져있을경우 슬라이딩시 플레이어 Sprite를 반전시킨다
        if (SlidingToTurnSprite())
            direction *= -1;

        //플레이어가 벽점프를 하거나 대쉬중일때는 플레이어의 입력여부와 상관없이 Sprite를 돌리지 않는다
        if ( !controller.isDashAttacking &&  !controller.Jump.isWallJumping)
        {
            controller.TurnPlayerSprite(direction);
        }
    }
    private void RunWithAcceleration(float lerpAmount)
    {
        currentVelocity = controller.GetCurrentVelocity();

        float targetSpeed= inputDirection.x * GetMaxSpeed();
        targetSpeed = Mathf.Lerp(currentVelocity.x, targetSpeed, lerpAmount);
        float accelRate;

        if (controller.Jump.lastOnGroundTime>0)//지면
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.MaxAccelAmount : data.MaxDeccelAmount;
        else//공중
        {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.MaxAccelAmount * data.MaxAirAcceleration : data.MaxDeccelAmount * data.MaxAirDecceleration;
            //공중에서는 가속도를 AirControlFactor 줄여서 부드럽게 변경
            accelRate *= AirControlFactor;
        }
          

        if (controller.Jump.CanJumpHang())
        {
            targetSpeed *= data.JumpHangMaxSpeed;
            accelRate *= data.JumpHangAcceleration;
        }

        if (DoConserveMomentum(targetSpeed))
            accelRate = 0;

        float speedDif = targetSpeed - currentVelocity.x;
        float moveForce = speedDif * accelRate;


        if (Mathf.Abs(currentVelocity.x) < 0.01f && inputDirection.x == 0 && controller.Jump.lastOnGroundTime>0)//플레이어 정지시 lerp보정으로 인해 생기는 미묘한 움직임을 없애기 위해 사용
            controller.SetCurrentVelocity(new Vector2(0,currentVelocity.y));
        else
            controller.Rigid.AddForce(Vector2.right * moveForce, ForceMode2D.Force);
    }

    private void RunWithoutAcceleration()
    {
        float targetSpeed = inputDirection.x * GetMaxSpeed();
        controller.SetCurrentVelocity(Vector2.right*targetSpeed);
    }



    public Vector2 GetInputDirection() => inputDirection;

    private bool SlidingToTurnSprite()
    {
        return controller.Jump.isSliding && data.Apply_TurnSpriteOnSlide;
    }
    private bool DoConserveMomentum(float targetSpeed)
    {
        return data.doConserveMomentum && Mathf.Abs(currentVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(currentVelocity.x) == Mathf.Sign(targetSpeed) &&
           Mathf.Abs(targetSpeed) > 0.01f && controller.Jump.lastOnGroundTime < 0;
    }
    private float GetMaxSpeed()
    {
        return Mathf.Max(data.MaxSpeed-friction, 0f);
    }

    #region 외부 상호작용
    public void ChangeFriction(float newFriction)
    {
        Debug.Log($"{friction} => {newFriction}");
        friction = newFriction;
    }
    #endregion
}
