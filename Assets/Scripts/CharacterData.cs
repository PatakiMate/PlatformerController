using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterData : MonoBehaviour
{
    [Header("Movement")]
    public float MaxSpeed;
    public float AccelerationTime;
    public float DecelerationTime;
    public bool CanUseSlopes;
    [Header("Jumping")]
    public int MaxExtraJumps;
    public float MaxJumpHeight;
    public float MinJumpHeight;
    public bool AdvancedAirControl;
    public float AirAccelerationTime;
    public float AirDecelerationTime;
    public float CoyoteTime;
    public float JumpBufferTime;
    [Header("Wall Sliding/Jumping")]
    public bool CanWallSlide;
    public float WallSlideSpeed;
    public bool CanWallJump;
    public float WallJumpSpeed;
    [Header("Dashing")]
    public bool CanDash;
    public bool OmnidirectionalDash;
    public bool DashDownSlopes;
    public bool CanJumpDuringDash;
    public bool JumpCancelStagger;
    public float DashDistance;
    public float DashSpeed;
    public float DashStagger;
    public float MaxDashCooldown;
    public int MaxAirDashes;
    [Header("Ladders")]
    public float LadderSpeed;
    public float LadderAccelerationTime;
    public float LadderDecelerationTime;
    public float LadderJumpHeight;
    public float LadderJumpSpeed;
}