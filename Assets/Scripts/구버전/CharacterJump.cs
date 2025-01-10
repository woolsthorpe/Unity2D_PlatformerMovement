using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JY.PlatformerBase
{
    public class CharacterJump : MonoBehaviour
    {
       

        private Rigidbody2D rigid;
        private CharacterGround ground;
        private Vector2 currentVelocity;

       
        [Header("Jump Settings")]
        [Range(2f, 5.5f)] public float jumpHeight;
        [Range(0.2f, 2f)] public float timeToJumpApex;

        [Range(0f, 5f)] public float jumpUpGravityScale;
        [Range(1f, 10f)] public float jumpFallGravityScale;
        [Range(0f, 1f)] public float jumpHangGravityScale;
        public float jumpHangTimeThreshold=0;

        [Space(10)]
        [Header("Option")]
        public bool variableJumpHeight;
        [Range(1f, 10f)] public float jumpCutOff;
        [SerializeField] private float fallSpeedLimit=1000;

        [Range(0f,1f)]public float jumpBufferTime=0.1f;
        [Range(0f, 1f)] public float coyoteTime=0.1f;

        public int maxJumpCount=1;
        [SerializeField]private int currentJumpCount;
        //���� ��������


        [Header("Calculations")]
        [SerializeField] private float jumpForce;
        [SerializeField] private float gravityMultiplier;
        private float gravityStrength;
        private float defaultGravityScale=1;

        [Header("Current State")]
        [SerializeField] private bool onGround;
        [SerializeField] private bool startJump;
        [SerializeField] private bool pressingJump;
        [SerializeField] private bool currentlyJumping;

        [SerializeField] private float currentBufferTime;
        [SerializeField] private float currentCoyoteTime;
        private void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
            ground = GetComponent<CharacterGround>();
        }
      

        private void Update()
        {
            InputJump();
            SetPhysics();
            onGround = ground.GetOnGround();

            //���� ����
            if (jumpBufferTime>0 && startJump)
            {
                currentBufferTime += Time.deltaTime;

                if (currentBufferTime > jumpBufferTime)
                {
                    startJump = false;
                    currentBufferTime = 0;
                }
            }

            //�ڿ��� ����
            if (!currentlyJumping && !onGround)
                currentCoyoteTime += Time.deltaTime;
            else
                currentCoyoteTime = 0;

           
        }
        private void SetPhysics()
        {
            gravityStrength = (-2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
            rigid.gravityScale = (gravityStrength / Physics2D.gravity.y) * gravityMultiplier;
        }
        private void InputJump()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                startJump = true;
                pressingJump = true;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                pressingJump = false;
            }

        }
    
      
        private void FixedUpdate()
        {
          
            currentVelocity = rigid.linearVelocity;

            if (startJump&&CheckJumpAbility())
            {
                DoAJump();
                rigid.linearVelocity = currentVelocity;

                return;
            }
            
            CalculateGravity();

        }
        private bool CheckJumpAbility()
        {
            return (onGround ||currentCoyoteTime > 0) || currentJumpCount>0;
        }

        private void DoAJump()
        {
           
                startJump = false;
                currentBufferTime = 0;
                currentCoyoteTime = 0;
                currentJumpCount = (currentJumpCount > 0) ? currentJumpCount - 1 : currentJumpCount;

                jumpForce= Mathf.Sqrt(-2f * Physics2D.gravity.y * rigid.gravityScale * jumpHeight);
                //jumpForce = Mathf.Abs(gravityStrength) * timeToJumpApex;

               
                if (currentVelocity.y < 0f)
                {
                    jumpForce -= currentVelocity.y;
                }

                currentVelocity.y += jumpForce;
              //  rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                currentlyJumping = true;
                Debug.Log("����"+jumpForce+" , "+ rigid.gravityScale+" , "+gravityMultiplier);
            
        }

        private void CalculateGravity()
        {

              
            if (rigid.linearVelocity.y > 0.01f)
            {
                if (onGround)
                    gravityMultiplier = defaultGravityScale;
                else
                {
                    if (variableJumpHeight)
                    {
                        if (pressingJump && currentlyJumping)
                        {
                            gravityMultiplier = jumpUpGravityScale;
                        }
                        else
                        {
                            gravityMultiplier = jumpCutOff;
                        }
                    }
                    else
                    {
                        gravityMultiplier = jumpUpGravityScale;
                    }
                }
            }
            //�߰�
            else if (currentlyJumping &&Mathf.Abs(rigid.linearVelocity.y) < jumpHangTimeThreshold)
            {
                gravityMultiplier = defaultGravityScale * jumpHangGravityScale;
                Debug.Log("jump hang"+ defaultGravityScale * jumpHangGravityScale);
            }
            else if (rigid.linearVelocity.y < -0.01f)
            {
                if (onGround)
                    gravityMultiplier = defaultGravityScale;
                else
                    gravityMultiplier = jumpFallGravityScale;
               
            }
            else
            {
                if (onGround)
                {
                    currentJumpCount = maxJumpCount;
                    currentlyJumping = false;
                }
                  

                gravityMultiplier = defaultGravityScale;
            }

            
            rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, Mathf.Clamp(rigid.linearVelocity.y, -fallSpeedLimit, 10000));
            }
      
    }
   
}
