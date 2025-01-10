using JY.PlatformerBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerController controller;
    private PlayerMovementData data;

   
    [Header("Options")]
    [SerializeField] private float friction = 0;
    private float maxSpeedChange;

    [Header("Current State")]
    private Vector2 inputDirection;
    public Vector2 currentVelocity;
    [SerializeField] private Vector2 desireVelocity;
     public bool isPressingKey { get; private set; }
    [SerializeField] private bool onGround;
    private void Start()
    {
        controller = GetComponent<PlayerController>();
        data = controller.Data;
    }

    public void HandleInput()
    {
        inputDirection.x = Input.GetAxisRaw("Horizontal");
        inputDirection.y = Input.GetAxisRaw("Vertical");
    }
   
    private void Update()
    {
        if (inputDirection.x != 0)
        {
            isPressingKey = true;

            if(controller.Jump.isSliding && controller.Jump.wallSlideTurnSprite)
                TurnPlayerSprite((int)Mathf.Sign(inputDirection.x*-1));
            else if(!controller.Jump.isWallJumping)
                TurnPlayerSprite((int)Mathf.Sign(inputDirection.x));
        }
        else
            isPressingKey = false;

        desireVelocity = new Vector2(inputDirection.x, 0f) * Mathf.Max(data.MaxSpeed - friction, 0f);
    }



    private void FixedUpdate()
    {


        currentVelocity = controller.Rigid.linearVelocity;
        onGround = controller.Contact.GetOnGround();

        //���ӵ��� �̿��ؼ� �����ϰ� ��������
        //�ƴϸ� ���ӵ��� ������� �ʰ� �ﰡ������ ���������� �����Ѵ�.


        if (data.UseAcceleration)
            RunWIthAcceleration();
        else
        {
            if (onGround)
                RunWithoutAcceleration();
            else
                RunWIthAcceleration();
        }

    }


    private void RunWIthAcceleration()
    {

        if (isPressingKey)
        {
            if (Mathf.Sign(inputDirection.x) != Mathf.Sign(currentVelocity.x))
                maxSpeedChange = onGround ? data.MaxTurnSpeed : data.MaxAirTurnSpeed;
            else
                maxSpeedChange = onGround ?data. MaxAcceleration : data.MaxAirAcceleration;
        }
        else
            maxSpeedChange = onGround ? data.MaxDecceleration : data.MaxAirDecceleration;

        if (controller.Jump.CanJumpHang())
            desireVelocity.x *= data.JumpHangAcceleration;
        
        if (controller.Jump.isWallJumping)
            maxSpeedChange = data.WallJumpTurnSpeed * data.MaxAirTurnSpeed;

        if (controller.isDashAttacking)
            maxSpeedChange *= data.DashTurnSpeed;

        currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, desireVelocity.x, maxSpeedChange * Time.deltaTime);
      
        if(!controller.isDashing)
            controller.Rigid.linearVelocity = currentVelocity;

      
    }
    private void RunWithoutAcceleration()
    {
        currentVelocity.x = desireVelocity.x;
        controller.Rigid.linearVelocity = currentVelocity;
    }

    public void ChangeFriction(float friction)
    {
        Debug.Log($"{this.friction} => {friction} ����");
        this.friction = friction;
    }
    public bool FacingRight()
    {
        return (transform.localScale.x > 0);
    }
    public void TurnPlayerSprite(int direciton)
    {
        transform.localScale = new Vector3(direciton, 1, 1);
    }
    public float GetInputDirectionX()
    {
        return inputDirection.x;
    }
    public float GetInputDirectionY()
    {
        return inputDirection.y;
    }
    public Vector2 GetInputDirection()
    {
        return inputDirection;
    }
}
