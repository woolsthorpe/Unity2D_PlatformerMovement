using UnityEngine;

public class PlayerContactChecker : MonoBehaviour
{
    private PlayerController controller;
    private bool onGround;
    private bool onFrontWall;
    private bool onBackWall;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private Vector3 groundSizeOffset;

    [Header("Wall Check Settings")]
    [SerializeField] private Transform frontCheckPos;
    [SerializeField] private Transform backCheckPos;
    [SerializeField] private Vector3 wallSizeOffset;

    [Header("Layer Mask")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Default Option")]
    [SerializeField] private bool unifyCheckBoxOffset;

    private Vector2 wallOffset;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        CheckGroundContact();
        CheckWallContact();
    }

    private void CheckGroundContact()
    {
        onGround = Physics2D.OverlapBox(groundCheckPos.position, groundSizeOffset, 0, groundLayer);
    }

    private void CheckWallContact()
    {
        if (!AreWallCheckPositionsAssigned()) return;

        wallOffset = unifyCheckBoxOffset ? new Vector3(groundSizeOffset.y, groundSizeOffset.x, groundSizeOffset.z) : wallSizeOffset;

     
        onFrontWall = ((Physics2D.OverlapBox(frontCheckPos.position, wallOffset, 0, wallLayer) && controller.FacingRight()) ||
             (Physics2D.OverlapBox(backCheckPos.position, wallOffset, 0, wallLayer) && !controller.FacingRight())) && !controller.Jump.isWallJumping;
        onBackWall = ((Physics2D.OverlapBox(frontCheckPos.position, wallOffset, 0, wallLayer) && !controller.FacingRight()) ||
             (Physics2D.OverlapBox(backCheckPos.position, wallOffset, 0, wallLayer) && controller.FacingRight())) && !controller.Jump.isWallJumping;
    }
    private bool AreWallCheckPositionsAssigned()
    {
        return frontCheckPos != null && backCheckPos != null;
    }

    private void OnDrawGizmos()
    {
        DrawContactGizmo(groundCheckPos.position, groundSizeOffset, onGround);

        if (!AreWallCheckPositionsAssigned()) return;

        DrawContactGizmo(frontCheckPos.position, wallOffset, onFrontWall);
        DrawContactGizmo(backCheckPos.position, wallOffset, onBackWall);
    }

    private void DrawContactGizmo(Vector3 position, Vector3 size, bool isContact)
    {
        Gizmos.color = isContact ? Color.green : Color.red;
        Gizmos.DrawWireCube(position, size);
    }

    public bool IsOnGround() => onGround;
    public bool IsOnFrontWall() => onFrontWall;
    public bool IsOnBackWall() => onBackWall;
}
