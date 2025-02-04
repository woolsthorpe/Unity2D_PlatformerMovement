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

        //�����̵� �ɼ��� Ŀ��������� �����̵��� �÷��̾� Sprite�� ������Ų��
        if (SlidingToTurnSprite())
            direction *= -1;

        //�÷��̾ �������� �ϰų� �뽬���϶��� �÷��̾��� �Է¿��ο� ������� Sprite�� ������ �ʴ´�
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

        if (controller.Jump.lastOnGroundTime>0)//����
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.MaxAccelAmount : data.MaxDeccelAmount;
        else//����
        {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.MaxAccelAmount * data.MaxAirAcceleration : data.MaxDeccelAmount * data.MaxAirDecceleration;
            //���߿����� ���ӵ��� AirControlFactor �ٿ��� �ε巴�� ����
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


        if (Mathf.Abs(currentVelocity.x) < 0.01f && inputDirection.x == 0 && controller.Jump.lastOnGroundTime>0)//�÷��̾� ������ lerp�������� ���� ����� �̹��� �������� ���ֱ� ���� ���
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

    #region �ܺ� ��ȣ�ۿ�
    public void ChangeFriction(float newFriction)
    {
        Debug.Log($"{friction} => {newFriction}");
        friction = newFriction;
    }
    #endregion
}
