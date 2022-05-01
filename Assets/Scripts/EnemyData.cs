using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(EnemyCharacterController))]
public class EnemyData : MonoBehaviour
{
    [Header("Movement")]
    public float PatrolSpeed;
    public float FollowSpeed;
    public float AccelerationTime;
    public float DecelerationTime;
    public bool CanUseSlopes;
    public float DirectionChangeLerpTime;
    [Header("Attack")]
    public float MeleeCooldownTime;
    public float ChargeCooldownTime;
}