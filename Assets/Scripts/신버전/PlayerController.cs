using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerMovementData Data;
    public Rigidbody2D Rigid { get; private set; }
    public PlayerContactChecker Contact { get; private set; }
    public PlayerAnimator AnimHandler { get; private set; }

    public PlayerMovement Movement { get; private set; }
    public PlayerJump Jump { get; private set; }
    public PlayerDash Dash { get; private set; }

    public bool isDashing { get; set; }
    public bool isDashAttacking { get; set; }

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        HandlePlayerInput();
    }

    private void InitializeComponents()
    {
        Rigid = GetComponent<Rigidbody2D>();
        AnimHandler = GetComponent<PlayerAnimator>();
        Contact = GetComponent<PlayerContactChecker>();
        Movement = GetComponent<PlayerMovement>();
        Jump = GetComponent<PlayerJump>();

        if (TryGetComponent(out PlayerDash tmp))
            Dash = tmp;
        

        isDashing = false;
        isDashAttacking = false;
    }

    private void HandlePlayerInput()
    {
        Movement.HandleInput();
        Jump.HandleInput();
        if(Dash)
            Dash.HandleInput();

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
}
