using System;
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
   

    [Header("Jump Option")]
    private PlayerController controller;
    private PlayerMovementData data;
    public bool isJumping { get;  set; }
    public bool isWallJumping { get;  set; }
    public bool isJumpCut { get; set; }
    public bool isSliding { get;  set; }
    public bool isJumpFalling { get; private set; }
    public float lastOnGroundTime { get;  set; } //ground Check + apply coyoteTIme
    public float lastPressedJumpTime { get;  set; } // key input + apply BufferTime

    [Header("Wall Check Settings")]
    private float lastOnWallRightTime;
    private float lastOnWallLeftTime;
    private float lastOnWallTime;

    private float wallJumpStartTime;
    private int lastWallJumpDir;

    [Header("WallJump Option")]
    [SerializeField] private bool Apply_WallJump;
    [SerializeField] private bool Apply_WallSlide;
  

    private bool canJumpAgain;
    public void Initialize(PlayerController controller)
    {
        this.controller = controller;
        this.data = controller.Data;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (controller.iskeyLocked)
            return;

        if(context.started)
            lastPressedJumpTime = data.InputBufferTime;
        if(context.canceled && CanJumpCut())
            isJumpCut = true;
    }


    private void Update()
    {
       
        JumpCheck();
        SlideCheck();
        CollisionCheck();
        Timer();

        if (lastPressedJumpTime > 0)
        {

            if (CanWallJump() && Apply_WallJump)
            {
                wallJumpStartTime = data.WallJumpTime;
                lastWallJumpDir = (lastOnWallRightTime > 0) ? -1 : 1;
                WallJump(lastWallJumpDir);
            }
            else if (CanJump())
            {
                Jump();
            }
            else if (CanDoubleJump())
            {
                canJumpAgain = false;
                Jump();
            }

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
    private void Timer()
    {
        float deltaTime = Time.deltaTime;

        //지면
        if (!controller.IsOnGround())
            lastOnGroundTime -= deltaTime;

        //입력
        if (lastPressedJumpTime > 0)
            lastPressedJumpTime -= deltaTime;

        //벽
        if (lastOnWallTime > 0)
            lastOnWallTime -= deltaTime;
        if (lastOnWallRightTime > 0)
            lastOnWallRightTime -= deltaTime;
        if (lastOnWallLeftTime > 0)
            lastOnWallLeftTime -= deltaTime;

        if (wallJumpStartTime > 0)
        {
            wallJumpStartTime -= deltaTime;
        }
            
    }
    private void SlideCheck()
    {
        if (CanSlide() &&Apply_WallSlide&&((lastOnWallLeftTime > 0 && controller.GetInputDirection().x < 0)
                     || (lastOnWallRightTime > 0 && controller.GetInputDirection().x > 0)))
        {
            canJumpAgain = data.Apply_RefillDoubleJumpOnWall && data.EnableDoubleJump;
            isSliding = true;
        }
        else if(data.Apply_keepWallSlideOnNoInput&&CanSlideOff())
            isSliding = false;
        else if(!data.Apply_keepWallSlideOnNoInput)
            isSliding = false;
    
    }
    private void JumpCheck()
    {
        if (isJumping && controller.GetCurrentVelocity().y < 0)
        {
            isJumping = false;
            isJumpFalling = true;
        }

        if (isWallJumping &&wallJumpStartTime<0)
            isWallJumping = false;

        if (lastOnGroundTime > 0 && !isJumping && !isWallJumping)
        {
            isJumpCut = false;
            isJumpFalling = false;
        }
    }
  
    private void CollisionCheck()
    {
        if (!controller.isDashing)
        {
            if (controller.IsOnGround() && (!isJumping || isWallJumping))
            {
                canJumpAgain = data.EnableDoubleJump;
                lastOnGroundTime = data.CoyoteTime;

                isJumpCut = false;
                isJumping = false;
                isWallJumping = false;
                isJumpFalling = false;

                controller.OnLandingEffect();
            }

            if (controller.IsOnFrontWall())
                lastOnWallRightTime = data.CoyoteTime;

            if (controller.IsOnBackWall())
                lastOnWallLeftTime = data.CoyoteTime;

            lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);
        }
    }
    private void Jump()
    {
        controller.OnJumpEffect();

        lastOnGroundTime = 0;
        lastPressedJumpTime = 0;

        isJumping = true;
        isWallJumping = false;
        isJumpCut = false;
        isJumpFalling = false;

        float jumpForce = data.JumpForce;

        //if (controller.GetCurrentVelocity().y < 0)
        //    jumpForce -= controller.GetCurrentVelocity().y;
        //Debug.Log(jumpForce);
        controller.SetCurrentVelocity(new Vector2(controller.GetCurrentVelocity().x,0));
        controller.Rigid.AddForce(Vector2.up* jumpForce, ForceMode2D.Impulse);
    }
    private void WallJump(int dir)
    {
        controller.OnWallJumpEffect();

        canJumpAgain = data.Apply_RefillDoubleJumpOnWall && data.EnableDoubleJump;
        isWallJumping = true;
        isJumping = false;
        isJumpCut = false;
        isJumpFalling = false;
        isSliding = false;

        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;

        lastOnWallLeftTime = 0;
        lastOnWallRightTime = 0;
        lastOnWallTime = 0;

        Vector2 wallJumpForce = data.WallJumpForce;
        wallJumpForce.x *= dir;

        //if (Mathf.Sign(controller.GetCurrentVelocity().x) != Mathf.Sign(wallJumpForce.x))
        //    wallJumpForce.x -= controller.GetCurrentVelocity().x;
        //if (controller.GetCurrentVelocity().y < 0)
        //    wallJumpForce.y -= controller.GetCurrentVelocity().y;

        controller.SetCurrentVelocity(Vector2.zero);
        controller.Rigid.AddForce(wallJumpForce, ForceMode2D.Impulse);
    }
    private void Slide()
    {
        float speedDif = -data.SlideSpeed - controller.GetCurrentVelocity().y;
        float movement = speedDif * data.SlideAccel;

        movement = Mathf.Clamp(movement, -Math.Abs(speedDif) * (1 / Time.fixedDeltaTime), Math.Abs(speedDif) * (1 / Time.fixedDeltaTime));
        controller.Rigid.AddForce(movement * Vector2.up);
    }
    private void ApplyGravity()
    {

        if (isSliding || controller.isDashAttacking)
            SetGravityScale(0);
        else if(controller.GetCurrentVelocity().y < 0 && controller.GetInputDirection().y<0)
        {
            SetGravityScale(data.GravityScale * data.FastFallGravityMultiplier);
            SetMaxFallSpeed(data.MaxFastFallSpeed);
        }
        else if (isJumpCut)
        {
            SetGravityScale(data.GravityScale * data.JumpCutGravityMultiplier);
            SetMaxFallSpeed(data.MaxFallSpeed);
        }
        else if (CanJumpHang())
        {
            // hang
            SetGravityScale(data.GravityScale * data.JumpHangGravityMultiplier);
        }
        else if (controller.GetCurrentVelocity().y < 0)
        {
            SetGravityScale(data.GravityScale * data.FallGravityMultiplier);
            SetMaxFallSpeed(data.MaxFallSpeed);
        }
        else
            SetGravityScale(data.GravityScale);
    }

    public void SetGravityScale(float scaleAmount)
    {
        controller.SetGravityScale(scaleAmount);
    }
    private void SetMaxFallSpeed(float speedAmount)
    {
        controller.SetCurrentVelocity( new Vector2(controller.GetCurrentVelocity().x, Mathf.Max(controller.GetCurrentVelocity().y, -speedAmount)));
    }
    private bool CanJumpCut()
    {
        return (isJumping || isWallJumping) && controller.GetCurrentVelocity().y > 0;
    }
    private bool CanJump()
    {
        return lastOnGroundTime > 0 && !controller.isDashing&&!isJumping && !isWallJumping;
    }
    private bool CanDoubleJump()
    {
        //추후 더블점프 디자인관련해 수정 필요
        return canJumpAgain && !controller.isDashing;
    }
    private bool CanWallJump()
    {
        return lastOnWallTime > 0 && !controller.IsOnGround() && !controller.isDashing&&(!isWallJumping ||
            (lastOnWallRightTime >0 && lastWallJumpDir ==1)||(lastOnWallLeftTime>0 && lastWallJumpDir==-1));
    }
    private bool CanSlide()
    {
        return (lastOnWallTime > 0 && !isJumping && !isWallJumping && !controller.isDashing && lastOnGroundTime <= 0);
    }
    private bool CanSlideOff()
    {
        return (!controller.IsOnFrontWall()&& !controller.IsOnBackWall())||controller.IsOnGround();
    }
    public bool CanJumpHang()
    {
        return CheckJumpSatae() && Mathf.Abs(controller.GetCurrentVelocity().y) < data.JumpHangTimeThreshold;
    }
   
    private bool CheckJumpSatae()
    {
        return isJumping || isWallJumping || isJumpFalling;
    }

    #region 외부 상호작용
    public void RefillDoubleJump()
    {
        canJumpAgain = true;
    }
    #endregion();
}
