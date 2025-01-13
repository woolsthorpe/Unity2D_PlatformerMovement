using System;
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;

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

    private float currentJumpTime;
    private int lastWallJumpDir;
    private float slideSpeed;

    [Header("WallJump Option")]
    [SerializeField] private bool Apply_WallJump;
    [SerializeField] private bool Apply_WallSlide;
    [field: SerializeField] public bool Apply_TurnSpriteOnSlide { get; private set; }
    [SerializeField] private bool Apply_RefillDoubleJumpOnWall;
    [SerializeField] private bool Apply_keepWallSlideOnNoInput;

    private bool canJumpAgain;
    private void Start()
    {
        controller = GetComponent<PlayerController>();
        this.data = controller.Data;
       
    }

    
    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C))
        {
            lastPressedJumpTime = data.InputBufferTime;
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

        if (!controller.isDashing)
        {
            if (controller.IsOnGround() && !isJumping)
            {
                canJumpAgain = data.EnableDoubleJump;
                lastOnGroundTime = data.CoyoteTime;
                isJumpCut = false;
            }

            if (controller.IsOnFrontWall())
                lastOnWallRightTime = data.CoyoteTime;

            if (controller.IsOnBackWall())
                lastOnWallLeftTime = data.CoyoteTime;

            lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);
        }
       

        if (CanWallJump() && lastPressedJumpTime > 0 &&Apply_WallJump)
        {
            currentJumpTime = data.WallJumpTime;
            lastWallJumpDir = (lastOnWallRightTime > 0) ? -1 : 1;
            WallJump(lastWallJumpDir);
        }
        else if (CanJump() && lastPressedJumpTime > 0)
        {
            Jump();
        }
        else if(CanDoubleJump()&& lastPressedJumpTime > 0)
        {
            canJumpAgain = false;
            Jump();
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
        if (CanSlide() &&Apply_WallSlide&&((lastOnWallLeftTime > 0 && controller.GetInputDirection().x < 0)
                     || (lastOnWallRightTime > 0 && controller.GetInputDirection().x > 0)))
        //if(CanSlide() && Mathf.Sign(controller.Movement.inputDirection.x) == Mathf.Sign(lastWallJumpDir))
        {
            canJumpAgain = Apply_RefillDoubleJumpOnWall && data.EnableDoubleJump;
            isSliding = true;
        }
        else if(Apply_keepWallSlideOnNoInput&&((lastOnWallLeftTime > 0 && controller.GetInputDirection().x== 1)
                     || (lastOnWallRightTime > 0 && controller.GetInputDirection().x == -1)))
            isSliding = false;
        else if(!Apply_keepWallSlideOnNoInput)
            isSliding = false;
        Debug.Log($"(({lastOnWallLeftTime > 0} && {controller.GetInputDirection().x > 0}) " +
            $"|| ({lastOnWallRightTime > 0} && {controller.GetInputDirection().x < 0})))");
    }
    private void JumpCheck()
    {
        if (isJumping && controller.GetCurrentVelocity().y < 0)
        {
            isJumping = false;

            if (!isWallJumping)
                isJumpFalling = true;
        }

        if (isWallJumping && currentJumpTime<0)
            isWallJumping = false;

        if (lastOnGroundTime > 0 && !isJumping && !isWallJumping)
        {
            isJumpCut = false;

            if (!isJumping)
                isJumpFalling = false;
        }

        if (!controller.IsOnGround())
            lastOnGroundTime -= Time.deltaTime;

        if (lastOnWallRightTime > 0)
            lastOnWallRightTime -= Time.deltaTime;
        if (lastOnWallLeftTime > 0)
            lastOnWallLeftTime -= Time.deltaTime;
        if (lastOnWallTime > 0)
            lastOnWallTime -= Time.deltaTime;

        if (currentJumpTime > 0)
            currentJumpTime -= Time.deltaTime;
    }
    private void Jump()
    {
      

        lastOnGroundTime = 0;
        lastPressedJumpTime = 0;
        isJumping = true;
        isWallJumping = false;
        isJumpCut = false;
        isJumpFalling = false;

        float jumpForce = data.JumpForce;

        //if (controller.GetCurrentVelocity().y < 0)
        //    jumpForce -= controller.GetCurrentVelocity().y;

        controller.SetCurrentVelocity(new Vector2(controller.GetCurrentVelocity().x,0));
        controller.Rigid.AddForce(Vector2.up* jumpForce, ForceMode2D.Impulse);
    }

    private void WallJump(int dir)
    {
        canJumpAgain = Apply_RefillDoubleJumpOnWall && data.EnableDoubleJump;
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
        controller.TurnPlayerSprite(dir);
    }
    private void Slide()
    {
            slideSpeed = Mathf.MoveTowards(controller.GetCurrentVelocity().y, -data.SlideSpeed, data.SlideSpeedChangeRate * Time.fixedDeltaTime);
            controller.SetCurrentVelocity(new Vector2(controller.GetCurrentVelocity().x,slideSpeed));
    }
    private void ApplyGravity()
    {
     
        if (isSliding || controller.isDashing)
            SetGravityScale(0);
        else if (isJumpCut)
        {
            SetGravityScale(data.GravityScale * data.JumpCutGravityMultiplier);
            SetMaxFallSpeed(Mathf.Max(MathF.Abs(controller.GetCurrentVelocity().y),data.MaxFallSpeed));
        }
        else if (CanJumpHang())
        {
            // hang
            SetGravityScale(data.GravityScale * data.JumpHangGravityMultiplier);
        }
        else if (controller.GetCurrentVelocity().y < 0)
        {
            if(controller.GetInputDirection().y< 0)
            {
                SetGravityScale(data.GravityScale * data.FastFallGravityMultiplier);
                SetMaxFallSpeed(MathF.Max(MathF.Abs(controller.GetCurrentVelocity().y), data.MaxFastFallSpeed));
            }
            else
            {
                SetGravityScale(data.GravityScale * data.FallGravityMultiplier);
                SetMaxFallSpeed(MathF.Max(MathF.Abs(controller.GetCurrentVelocity().y), data.FallGravityMultiplier));
            }
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
        return canJumpAgain && !controller.isDashing /*&& !isJumping && !isWallJumping*/;
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
