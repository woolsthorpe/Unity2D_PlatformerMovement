using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerMovementData Data;
    private PlayerMovementData beForData;
    public Rigidbody2D Rigid { get; private set; }
    public PlayerContactChecker Contact { get; private set; }
    public PlayerAnimator AnimHandler { get; private set; }

    public PlayerMovement Movement { get; private set; }
    public PlayerJump Jump { get; private set; }
    public PlayerDash Dash { get; private set; }

    public bool isDashing { get; set; }
    public bool isDashAttacking { get; set; }

    public bool iskeyLocked { get;  private set; }

    [field:SerializeField]public GameObject playerSprite { get; set; }
    private void Awake()
    {
        InitializeComponents();

        if (playerSprite == null)
            Debug.LogError("playerSprite is None");
    }
    private void Update()
    {
        if (beForData != Data)
            InitializeComponents();
        //테스트 할때만 남겨두고 작업마무리할시 삭제
    }

    private void InitializeComponents()
    {
        Rigid = GetComponent<Rigidbody2D>();
        AnimHandler = GetComponent<PlayerAnimator>();
        Contact = GetComponent<PlayerContactChecker>();
        Movement = GetComponent<PlayerMovement>();
        Jump = GetComponent<PlayerJump>();

        AnimHandler.Initialize(this);
        Movement.Initialize(this);
        Jump.Initialize(this);

        if (TryGetComponent(out PlayerDash tmp))
        {
            Dash = tmp;
            Dash.Initialize(this);
        }
           
        

        isDashing = false;
        isDashAttacking = false;

        beForData = Data;
    }

   
    public bool IsOnGround() => Contact.IsOnGround();
    public bool IsOnFrontWall() => Contact.IsOnFrontWall();
    public bool IsOnBackWall() => Contact.IsOnBackWall();

    public Vector2 GetInputDirection() => Movement.GetInputDirection();

    public void SetCurrentVelocity(Vector2 newVelocity)
    {
        Rigid.velocity = newVelocity;
    }
    public Vector2 GetCurrentVelocity() => Rigid.velocity;
    public void SetGravityScale(float scale)
    {
        Rigid.gravityScale=scale;   
    }

    public bool FacingRight() => transform.localScale.x > 0;

    public void TurnPlayerSprite(int direction)
    {
        transform.localScale = new Vector3(direction, 1f, 1f);
    }
    public bool IsOnWall() => Jump.isSliding || Jump.isWallJumping;

    public void OnJumpEffect()
    {
        AnimHandler.JumpEffect();
    }
    public void OnWallJumpEffect()
    {
        AnimHandler.WallJumpEffect();
    }
    public void OnLandingEffect()
    {
      //  AnimHandler.LandingEffect();
    }
    public void SlideEffect()
    {

    }
}
