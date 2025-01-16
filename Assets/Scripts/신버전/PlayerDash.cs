using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    private PlayerController controller;
    private PlayerMovementData data;

    
    private float lastPressedDashTime;
    private Vector2 lastDashDir;
    private int currentDashCount;
    private bool dashRefilling=false;
    [SerializeField] private bool refillDashOnWall;
    [SerializeField] private bool noCoolTimeDashReFill;

    public void Initialize(PlayerController controller)
    {
        this.controller = controller;
        this.data = controller.Data;

        currentDashCount = data.DashCount;
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift))
            lastPressedDashTime = data.DashBufferTime;
    }

    private void Update()
    {
        if (noCoolTimeDashReFill && CheckDashRefill())
            currentDashCount = data.DashCount;


        if (CanDash()&& lastPressedDashTime>0)
        {
            StartCoroutine(PerformSleep(data.DashSleepTime));

            if (controller.GetInputDirection() != Vector2.zero)
                lastDashDir = controller.GetInputDirection();
            else
                lastDashDir = controller.FacingRight() ?Vector2.right:Vector2.left;

            if (lastDashDir.x != 0)
                controller.TurnPlayerSprite((int)Mathf.Sign(lastDashDir.x));
            StartCoroutine(Dash());
        }
        UpdateDashTimers();
        
    }
    private IEnumerator Dash()
    {
        controller.isDashing = true;
        controller.Jump.isJumping = false;
        controller.Jump.isWallJumping = false;
        controller.Jump.isJumpCut = false;

        controller.Jump.lastOnGroundTime = 0;
        lastPressedDashTime = 0;

        currentDashCount--;
        controller.isDashAttacking = true;

        float currentTime = 0;
        while (currentTime <= data.DashAttackTime)
        {
           controller.SetCurrentVelocity(lastDashDir.normalized*data.DashSpeed);
            currentTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }
      
        currentTime = 0;
        controller.isDashAttacking = false;


        controller.Jump.SetGravityScale(data.GravityScale);
      
        while (currentTime <= data.DashEndTime)
        {
            controller.SetCurrentVelocity(lastDashDir.normalized * data.DashEndSpeed);
            currentTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }
        
        controller.isDashing = false;
    }

    private void UpdateDashTimers()
    {
        if (lastPressedDashTime > 0)
            lastPressedDashTime -= Time.deltaTime;
    }
    public void Sleep(float duration)
    {
        StartCoroutine(PerformSleep(duration));
    }
    private IEnumerator PerformSleep(float duration)
    {
        float currentTimeScale = Time.timeScale;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = (currentTimeScale==0)?1:currentTimeScale;
    }
    private bool CanDash()
    {
        if (!controller.isDashing && !dashRefilling && currentDashCount < data.DashCount && CheckDashRefill())//���� ����
            StartCoroutine(RefillDash(data.DashRefillTime));

        return currentDashCount>0 && data.DashDistance>0;
    }

    private bool CheckDashRefill()
    {
        return (controller.Jump.lastOnGroundTime > 0||controller.IsOnGround()) || (refillDashOnWall && controller.IsOnWall());
    }

    private IEnumerator RefillDash(float time)
    {
        dashRefilling = true;
        yield return new WaitForSeconds(time);
        dashRefilling = false;
        currentDashCount = data.DashCount;
    }
}
