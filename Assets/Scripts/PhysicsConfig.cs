using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsConfig : MonoBehaviour
{
    [Header("Masks")]
    public LayerMask GroundMask;
    public LayerMask OneWayPlatformMask;
    public LayerMask LadderMask;
    public LayerMask CharacterMask;
    public LayerMask CharacterCollisionMask;
    public LayerMask StandOnCollisionMask;
    public LayerMask EdgeMask;
    [Header("Physics Values")]
    public float Gravity;
    public float AirFriction;
    public float GroundFriction;
    public float StaggerSpeedFalloff;

    void Start()
    {
        if (GroundMask == 0)
        {
            GroundMask = LayerMask.GetMask("Ground");
        }
        if (OneWayPlatformMask == 0)
        {
            OneWayPlatformMask = LayerMask.GetMask("OneWayPlatform");
        }
        if (CharacterCollisionMask == 0)
        {
            CharacterCollisionMask = LayerMask.GetMask("Ground");
        }
        if (LadderMask == 0)
        {
            LadderMask = LayerMask.GetMask("Ladder");
        }
        if (CharacterMask == 0)
        {
            CharacterMask = LayerMask.GetMask("Character");
        }
        if (StandOnCollisionMask == 0)
        {
            StandOnCollisionMask = LayerMask.GetMask("Character");
        }
        if (EdgeMask == 0)
        {
            EdgeMask = LayerMask.GetMask("Edge");
        }
    }
}