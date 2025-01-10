using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    private PlayerController controller;
    private PlayerMovementData data;

    
    [SerializeField] private float lastPressedDashTime;
    [SerializeField] private Vector2 lastDashDir;
    [SerializeField] private int currentDashCount;
    private bool dashRefilling;

    private void Start()
    {
        this.controller = GetComponent<PlayerController>();
        this.data = controller.Data;
        dashRefilling = false;

        currentDashCount = data.DashCount;
    }
    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift))
            lastPressedDashTime = data.DashBufferTime;
    }

    private void Update()
    {
    

        if(CanDash()&& lastPressedDashTime>0)
        {
           

            StartCoroutine(PerformSleep(data.DashSleepTime));

            if (controller.Movement.GetInputDirection() != Vector2.zero)
                lastDashDir = controller.Movement.GetInputDirection();
            else
                lastDashDir = controller.Movement.FacingRight() ?Vector2.right:Vector2.left;

            StartCoroutine(Dash());
        }

        if (lastPressedDashTime > 0)
            lastPressedDashTime -= Time.deltaTime;
    }
    //dash���̰� �������� ����
    //���� ��¦ ����
    //���� �Ƚ�
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
        // Vector2 dashVelocity = lastDashDir.normalized * (data.DashDistance / data.DashAttackTime);
        //Debug.Log(lastDashDir.normalized * data.DashSpeed);
        //�뽬 ���ǵ�� xy����
        while (currentTime <= data.DashAttackTime)
        {
           controller.Rigid.linearVelocity = lastDashDir.normalized*data.DashSpeed;
            currentTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }
      
        currentTime = 0;
        controller.isDashAttacking = false;


        controller.Jump.SetGravityScale(data.GravityScale);
        //Debug.Log(lastDashDir.normalized * data.DashSpeed);
        while (currentTime <= data.DashEndTime)
        {
            controller.Rigid.linearVelocity = lastDashDir.normalized * data.DashEndSpeed;
            //x�� ���� �̿��ϰ� ���ӵȴ� x 13.84 y 15
            currentTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        Debug.Log($" {lastDashDir.normalized}    {data.DashSpeed}  {controller.Rigid.linearVelocity}");
        
        controller.isDashing = false;
    }


    public void Sleep(float duration)
    {
        StartCoroutine(PerformSleep(duration));
    }
    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
    private bool CanDash()
    {
        if (!controller.isDashing && currentDashCount < data.DashCount && controller.Jump.lastOnGroundTime > 0 && !dashRefilling)//���� ����
            StartCoroutine(RefillDash(data.DashRefillTime));

        return currentDashCount>0;
    }

    private IEnumerator RefillDash(float time)
    {
        dashRefilling = true;
        yield return new WaitForSeconds(time);
        dashRefilling = false;
        currentDashCount = (int)Mathf.Min(data.DashCount, currentDashCount + 1);
    }
}
