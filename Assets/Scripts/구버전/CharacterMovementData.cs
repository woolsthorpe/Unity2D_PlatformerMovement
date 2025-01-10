using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Character Movement Data")]
public class CharacterMovementData : ScriptableObject
{
    [Header("Run")]
    [Range(0f, 50f)] public float maxSpeed = 9.01f;
    [Range(0f, 100f)] public float maxAcceleration;
    [Range(0f, 100f)] public float maxDecceleration;

    [Range(0f, 100f)] public float maxAirAcceleration;
    [Range(0f, 100f)] public float maxAirDecceleration;

    [Range(0f, 100f)] public float maxTurnSpeed;
    [Range(0f, 100f)] public float maxAirTurnSpeed;

    [Space(5)]
    [Header("Jump")]
    [Range(2f, 5.5f)] public float jumpHeight;
    [Range(0.2f, 2f)] public float timeToJumpApex;

    [Range(0f, 5f)] public float jumpUpGravityScale;
    [Range(1f, 10f)] public float jumpFallGravityScale;
    [Range(0f,1f)] public float jumpHangGravityScale;
    public float jumpHangTimeThreshold;

    [Space(2)]
    public bool variableJumpHeight;
    [Range(1f, 10f)] public float jumpCutGravityScale;

    public int jumpCount = 0;

    private void OnValidate()
    {
    }
 }
