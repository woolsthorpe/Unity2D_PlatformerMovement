using System;
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    private PlayerController controller;
    private PlayerMovementData data;

    [Header("Current JumpState")]
    private float jumpForce;

    public bool isJumping { get;  set; }
    public bool isWallJumping { get;  set; }

    public bool isJumpCut { get; set; }
    [field:SerializeField] public bool isSliding { get;  set; }

    [SerializeField] private bool isJumpFalling;

    [field:SerializeField]
    public float lastOnGroundTime { get;  set; } //ground Check + apply coyoteTIme
    public float lastPressedJumpTime { get;  set; } // key input + apply BufferTime

    private Vector2 wallJumpForce;

    private float lastOnWallRightTime;
    private float lastOnWallLeftTime;
    [SerializeField] private float lastOnWallTime;

    [SerializeField] private float wallJumpStartTime;
    [SerializeField] private int lastWallJumpDir;

   
  

    [SerializeField] private float slideSpeed;

     public bool wallSlideTurnSprite;

    [SerializeField] private bool canJumpAgain;
    private void Start()
    {
        controller = GetComponent<PlayerController>();
        this.data = controller.Data;
       
    }
    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C))
        {
            lastPressedJumpTime = data.InputBuffectTime;
        }
        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C))&& CanJumpCut())
        {
            isJumpCut = true;
        }
    }

    private void Update()
    {
        JumpCheck();
        SlideCheck();

        if (lastPressedJumpTime > 0)
            lastPressedJumpTime -= Time.deltaTime;

        if (!controller.isDashing && !isJumping)
        {
            if (controller.Contact.GetOnGround())
            {
                canJumpAgain = data.DoubleJump;
                lastOnGroundTime = data.CoyoteTime;
                isJumpCut = false;
            }
            // �� üũ ����
            if (controller.Contact.GetOnFrontWall())
                lastOnWallRightTime = data.CoyoteTime;

            if (controller.Contact.GetOnBackWall())
                lastOnWallLeftTime = data.CoyoteTime;

            lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);
        }
       

        if (CanWallJump() && lastPressedJumpTime > 0)
        {
            wallJumpStartTime = Time.time;
            lastWallJumpDir = (lastOnWallRightTime > 0) ? -1 : 1;
            WallJump(lastWallJumpDir);
        }
        else if ((CanJump() || (canJumpAgain&&!isJumping && !isWallJumping)) && lastPressedJumpTime > 0)
        {
            Jump();


            //���� ��� �����ϸ� �������� �ƴ� ���������� �ҋ��� ����
        }

        ApplyGravity();

    }
    private void FixedUpdate()
    {
        if (isSliding)
        {
            Slide();
        }
    }
    private void SlideCheck()
    {
        if (CanSlide() && ((lastOnWallLeftTime > 0 && controller.Movement.GetInputDirectionX() < 0)
                     || (lastOnWallRightTime > 0 && controller.Movement.GetInputDirectionX() > 0)))
        //if(CanSlide() && Mathf.Sign(controller.Movement.inputDirection.x) == Mathf.Sign(lastWallJumpDir))
        {
            canJumpAgain = data.DoubleJump;
            isSliding = true;
        }
        else
            isSliding = false;
    }
    private void JumpCheck()
    {
        if (isJumping && controller.Rigid.linearVelocity.y < 0)
        {
            isJumping = false;

            if (!isWallJumping)
                isJumpFalling = true;
        }

        if (isWallJumping && Time.time - wallJumpStartTime > data.WallJumpTime)
            isWallJumping = false;

        if (lastOnGroundTime > 0 && !isJumping && !isWallJumping)
        {
            isJumpCut = false;

            if (!isJumping)
                isJumpFalling = false;
        }

        if (!controller.Contact.GetOnGround())
            lastOnGroundTime -= Time.deltaTime;

        if (lastOnWallRightTime > 0)
            lastOnWallRightTime -= Time.deltaTime;
        if (lastOnWallLeftTime > 0)
            lastOnWallLeftTime -= Time.deltaTime;
        if (lastOnWallTime > 0)
            lastOnWallTime -= Time.deltaTime;
    }
    private void Jump()
    {
        if(lastOnGroundTime<0)
        {
            if (canJumpAgain)
            {
                canJumpAgain = false;
                SetGravityScale(data.GravityScale);
            }
            else
                return;
        }

        lastOnGroundTime = 0;
        lastPressedJumpTime = 0;
        isJumping = true;
        isWallJumping = false;
        isJumpCut = false;
        isJumpFalling = false;

        jumpForce = data.JumpForce;


        if (controller.Rigid.linearVelocity.y < 0f)
            controller.Rigid.linearVelocity = new Vector2(controller.Rigid.linearVelocity.x,0);

        controller.Rigid.linearVelocity = new Vector2(controller.Rigid.linearVelocity.x, jumpForce);
     //   Debug.Log($"{jumpForce}  {controller.Rigid.gravityScale}");
    }

    private void WallJump(int dir)
    {

        canJumpAgain = data.DoubleJump;
        isWallJumping = true;
        isJumping = false;
        isJumpCut = false;
        isJumpFalling = false;

        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;

        lastOnWallLeftTime = 0;
        lastOnWallRightTime = 0;
        lastOnWallTime = 0;
        //canJumpAgain = data.ApplyDoubleJump;

        wallJumpForce = data.WallJumpForce;
        wallJumpForce.x *= dir;

        if (Mathf.Sign(controller.Rigid.linearVelocity.x) != Mathf.Sign(wallJumpForce.x))
            wallJumpForce.x += Mathf.Abs(controller.Rigid.linearVelocity.x);
        if (controller.Rigid.linearVelocity.y < 0f)
            wallJumpForce.y += Mathf.Abs(controller.Rigid.linearVelocity.y);

          controller.Rigid.AddForce(wallJumpForce, ForceMode2D.Impulse);
     //  controller.Rigid.velocity= Vector2.MoveTowards(controller.Rigid.velocity, wallJumpForce, data.WallJumpTurnSpeed * Time.deltaTime);
        // controller.Rigid.velocity = new Vector2(wallJumpForce.x, wallJumpForce.y);
        controller.Movement.TurnPlayerSprite(dir);
        // walljump�� ������ȯ
    }
    private void Slide()
    {
        slideSpeed = Mathf.MoveTowards(controller.Rigid.linearVelocity.y, -data.SlideSpeed, data.SlideSpeedChange*Time.deltaTime);
        controller.Rigid.linearVelocity = new Vector2(controller.Rigid.linearVelocity.x,slideSpeed);

    }
    private void ApplyGravity()
    {
        if (isSliding || controller.isDashing)
            SetGravityScale(0);
        else if (isJumpCut)
        {
            SetGravityScale(data.GravityScale * data.JumpCutGravityMult);
            SetMaxFallSpeed(Mathf.Max(MathF.Abs(controller.Rigid.linearVelocity.y),data.MaxFallSpeed));
        }
        else if (CanJumpHang())
        {
            // hang
            SetGravityScale(data.GravityScale * data.JumpHangGravityMult);
        }
        else if (controller.Rigid.linearVelocity.y < 0)
        {
            // �϶�
        
            if(controller.Movement.GetInputDirectionY()<0)
            {
                SetGravityScale(data.GravityScale * data.FastFallGravityMult);
                SetMaxFallSpeed(MathF.Max(MathF.Abs(controller.Rigid.linearVelocity.y), data.MaxFastFallSpeed));
            }
            else
            {
                SetGravityScale(data.GravityScale * data.FallGravityMult);
                SetMaxFallSpeed(MathF.Max(MathF.Abs(controller.Rigid.linearVelocity.y), data.FallGravityMult));
            }
        }
        else
            SetGravityScale(data.GravityScale);
    }

    public void SetGravityScale(float scaleAmount)
    {
        controller.Rigid.gravityScale = scaleAmount;
    }
    private void SetMaxFallSpeed(float speedAmount)
    {
        controller.Rigid.linearVelocity = new Vector2(controller.Rigid.linearVelocity.x, Mathf.Max(controller.Rigid.linearVelocity.y, -speedAmount));
    }
    private bool CanJumpCut()
    {
        return (isJumping || isWallJumping) && controller.Rigid.linearVelocity.y > 0;
    }
    private bool CanJump()
    {
        return lastOnGroundTime > 0 && !controller.isDashing;
    }
    private bool CanWallJump()
    {
        return lastOnWallTime > 0 && lastOnGroundTime <= 0 && (!isWallJumping ||
            (lastOnWallRightTime >0 && lastWallJumpDir ==1)||(lastOnWallLeftTime>0 && lastWallJumpDir==-1));
    }
    private bool CanSlide()
    {
        return (lastOnWallTime > 0 && !isJumping && !isWallJumping && !controller.isDashing && lastOnGroundTime <= 0);
    }
    public bool CanJumpHang()
    {
        return CheckJumpSatae() && Mathf.Abs(controller.Rigid.linearVelocity.y) < data.JumpHangTimeThreshold;
    }
    private bool CheckJumpSatae()
    {
        return isJumping || isWallJumping || isJumpFalling;
    }
}
