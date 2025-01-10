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
        Dash = GetComponent<PlayerDash>();

        isDashing = false;
        isDashAttacking = false;
    }

    private void HandlePlayerInput()
    {
        Movement.HandleInput();
        Jump.HandleInput();
        Dash.HandleInput();
    }
}
