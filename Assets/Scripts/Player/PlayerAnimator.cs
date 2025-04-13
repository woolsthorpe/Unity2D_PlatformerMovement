using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerAnimator : MonoBehaviour
{
    private PlayerController controller;
    private PlayerMovementData data;
    private Animator animator;
    [Header("Settings")]
    [SerializeField] private bool Apply_Tilt;
    private Vector3 orginPos;
    private Vector3 orginSize;

    [Header("Tilt Settings")]
    [SerializeField] private float maxTilt;
    [SerializeField] private float tiltSpeed;

    [Header("Squash and Stretch")]
    [SerializeField] private Vector2 jumpSquashSize;
    [SerializeField] private Vector2 landSquashSize;
    [SerializeField] private float jumpSquashTime;
    [SerializeField] private float landSquashTime;
    [SerializeField] private float jumpSquashMultiplier;
    [SerializeField] private float landSquashMultiplier;
    [SerializeField] private float landDrop;

    public bool squeezing { get; private set; }
    private bool jumpSqueezing;
    private bool landSqueezing;
    private bool isGrounded;

    public bool cameraFalling { get; private set; }

    [Header(" Particles Settings")]
    [SerializeField] private ParticleSystem moveParticle;
    [SerializeField] private ParticleSystem jumpParticle;
    [SerializeField] private ParticleSystem landParticle;
    [SerializeField] private ParticleSystem slidingParticle;
    public void Initialize(PlayerController controller)
    {
        this.controller = controller;
        this.data = controller.Data;

        orginSize = controller.playerSprite.transform.localScale;
        orginPos = controller.playerSprite.transform.localPosition;

        animator = GetComponentInChildren<Animator>();
    }
    void Update()
    {

        animator.SetFloat("velocity_X", Mathf.Clamp(Mathf.Abs(controller.GetCurrentVelocity().x), 0, data.MaxSpeed));
        animator.SetFloat("velocity_Y", controller.GetCurrentVelocity().y);
        animator.SetBool("OnGround", controller.IsOnGround());
        if (Apply_Tilt)
            TiltCharcter();

        CheckForLanding();

        if(slidingParticle != null)
            CheckForSliding();
    }
    private void TiltCharcter()
    {
        float directionToTilt = 0;
        if (controller.GetInputDirection().x != 0)
            directionToTilt = Mathf.Sign(controller.GetInputDirection().x);

        Vector3 targetRot = new Vector3(0, 0, Mathf.Lerp(-maxTilt, maxTilt, Mathf.InverseLerp(-1, 1, directionToTilt)));
        animator.transform.rotation = Quaternion.RotateTowards(animator.transform.rotation, Quaternion.Euler(-targetRot), tiltSpeed * Time.deltaTime);

    }


    private void CheckForLanding()
    {
        if (!isGrounded && controller.IsOnGround()&&!controller.Jump.isJumping)
        {
            cameraFalling = false;
            isGrounded = true;
            animator.SetBool("OnGround", true);
            landParticle.Play();

            moveParticle.Play();

            if (!landSqueezing && landSquashMultiplier >= 1)
            {

                StartCoroutine(JumpSqueeze(landSquashSize.x * landSquashMultiplier, landSquashSize.y / landSquashMultiplier, landSquashTime, landDrop, false));
            }
        }
        else if (isGrounded && !controller.IsOnGround())
        {
            isGrounded = false;
            moveParticle.Stop();
        }
    }

    private void CheckForSliding()
    {
        bool isSliding = controller.Jump.isSliding;

        if (isSliding && slidingParticle.isStopped)
            slidingParticle.Play();
        else if (!isSliding)
            slidingParticle.Stop();

        animator.SetBool("Sliding", isSliding);
    }
    public void JumpEffect()
    {
        jumpParticle.Play();
        PlayJumpSequence();
    }
    public void WallJumpEffect()
    {
        //벽점프 이펙트 할당
        PlayJumpSequence();
    }
    private void PlayJumpSequence()
    {
        animator.SetBool("OnGround", false);
        animator.SetTrigger("Jump");
        isGrounded = false;

        if (!jumpSqueezing && jumpSquashMultiplier >= 1)
            StartCoroutine(JumpSqueeze(jumpSquashSize.x * jumpSquashMultiplier, jumpSquashSize.y / jumpSquashMultiplier, jumpSquashTime, 0, true));

        moveParticle.Stop();
    }


    IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds, float dropAmount, bool jumpSqueeze)
    {
        if (jumpSqueeze)
            jumpSqueezing = true;
        else
            landSqueezing = true;

        squeezing = true;

        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, orginSize.z);
        Vector3 newPos = orginPos + new Vector3(0, -dropAmount, 0);

        float currentTime = 0f;
        while (currentTime <= 1.0f)
        {
            currentTime += Time.deltaTime / 0.01f;
            controller.playerSprite.transform.localScale = Vector3.Lerp(orginSize, newSize, currentTime);
            controller.playerSprite.transform.localPosition = Vector3.Lerp(orginPos, newPos, currentTime);
            yield return null;
        }
        controller.playerSprite.transform.localScale = newSize;
        controller.playerSprite.transform.localPosition = newPos;

        currentTime = 0f;
        while (currentTime <= seconds)
        {
            currentTime += Time.deltaTime / seconds;
            controller.playerSprite.transform.localScale = Vector3.Lerp(newSize, orginSize, currentTime);
            controller.playerSprite.transform.localPosition = Vector3.Lerp(newPos, orginPos, currentTime);
            yield return null;
        }
        controller.playerSprite.transform.localScale = orginSize;
        controller.playerSprite.transform.localPosition = orginPos;


        if (jumpSqueeze)
            jumpSqueezing = false;
        else
            landSqueezing = false;

        squeezing = false;
    }
}
