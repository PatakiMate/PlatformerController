using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacterController : MonoBehaviour
{
    public float MoveSpeed;
    void Start()
    {
        
    }
    void FixedUpdate()
    {
        Vector3 newPos = Vector2.MoveTowards(transform.position, PlayerObject.Instance.transform.position, MoveSpeed * Time.fixedDeltaTime);
        transform.position = newPos;
    }
}
