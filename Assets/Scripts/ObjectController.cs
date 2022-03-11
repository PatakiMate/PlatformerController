using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ObjectController : MonoBehaviour
{
    public bool FacingRight { get; set; }
    public bool IgnoreFriction { get; set; }
    public Vector2 TotalSpeed => Speed + ExternalForce;
    [Header("Collision Parameters")]
    public float RaySpacing = 0.125f;
    public float SkinWidth = 0.015f;
    public float OneWayPlatformDelay = 0.1f;
    public float LadderClimbThreshold = 0.3f;
    public float LadderDelay = 0.3f;
    public float EdgeDelay = 0.3f;
    public float MaxSlopeAngle = 60f;
    public float MinWallAngle = 80f;

    protected RaycastOrigins RayOrigins;
    protected CollisionInfo Collisions;
    protected BoxCollider2D MyCollider;
    protected PhysicsConfig PConfig;
    protected Vector2 Speed = Vector2.zero;
    protected Vector2 ExternalForce = Vector2.zero;
    protected float HorizontalRaySpacing;
    protected float HorizontalRayCount;
    protected float VerticalRaySpacing;
    protected float VerticalRayCount;
    protected float GravityScale = 1;
    protected LayerMask CollisionMask;
    protected float IgnorePlatformsTime = 0;
    protected float MinimumMoveThreshold = 0.01f;
    protected bool UpdateCoyoteTimer = false;

    public virtual void Start()
    {
        MyCollider = GetComponent<BoxCollider2D>();
        PConfig = GameObject.FindObjectOfType<PhysicsConfig>();
        CalculateSpacing();
        CollisionMask = PConfig.CharacterCollisionMask;
        FacingRight = true;
    }

    public virtual void FixedUpdate()
    {
        Collisions.Reset();
        Move((TotalSpeed) * Time.fixedDeltaTime);
        PostMove();
    }

    #region Calculate Raycasts
    void CalculateSpacing()
    {
        Bounds bounds = MyCollider.bounds;
        bounds.Expand(SkinWidth * -2);
        HorizontalRayCount = Mathf.Round(bounds.size.y / RaySpacing);
        VerticalRayCount = Mathf.Round(bounds.size.x / RaySpacing);
        HorizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
        VerticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);
    }

    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = MyCollider.bounds;
        bounds.Expand(SkinWidth * -2);
        RayOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        RayOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        RayOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        RayOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }
    #endregion

    #region Handle Physics Forces
    protected virtual void UpdateGravity()
    {
        float g = PConfig.Gravity * GravityScale * Time.fixedDeltaTime;
        if (Speed.y > 0)
        {
            Speed.y += g;
        }
        else
        {
            ExternalForce.y += g;
        }
    }

    protected virtual void UpdateExternalForce()
    {
        if (IgnoreFriction)
        {
            return;
        }
        float friction = Collisions.onGround ? PConfig.GroundFriction : PConfig.AirFriction;
        ExternalForce = Vector2.MoveTowards(ExternalForce, Vector2.zero,
            ExternalForce.magnitude * friction * Time.fixedDeltaTime);
        if (ExternalForce.magnitude <= MinimumMoveThreshold)
        {
            ExternalForce = Vector2.zero;
        }
    }
    #endregion

    #region Movement
    protected virtual void PreMove(ref Vector2 deltaMove)
    {
        UpdateRaycastOrigins();
        float xDir = Mathf.Sign(deltaMove.x);
        CheckGround(xDir);
        UpdateExternalForce();
        UpdateGravity();
        if (Collisions.onSlope && Collisions.groundAngle > MaxSlopeAngle &&
            (Collisions.groundAngle < MinWallAngle || Speed.x == 0))
        {
            ExternalForce.x += -PConfig.Gravity * PConfig.GroundFriction * Collisions.groundDirection * Time.fixedDeltaTime / 4;
        }
    }

    public virtual Vector2 Move(Vector2 deltaMove)
    {
        int layer = gameObject.layer;
        gameObject.layer = Physics2D.IgnoreRaycastLayer;
        PreMove(ref deltaMove);
        float xDir = Mathf.Sign(deltaMove.x);
        if (deltaMove.x != 0)
        {
            // Slope checks and processing
            if (deltaMove.y <= 0)
            {
                if (Collisions.onSlope)
                {
                    if (Collisions.groundDirection == xDir)
                    {
                        DescendSlope(ref deltaMove);
                    }
                    else
                    {
                        ClimbSlope(ref deltaMove);
                    }
                }
            }
            HorizontalCollisions(ref deltaMove);
        }
        if (Collisions.onSlope && Collisions.groundAngle >= MinWallAngle && Collisions.groundDirection != xDir && Speed.y < 0)
        {
            Speed.x = 0;
            Vector2 origin = Collisions.groundDirection == -1 ? RayOrigins.bottomRight : RayOrigins.bottomRight;
            Collisions.hHit = Physics2D.Raycast(origin, Vector2.left * Collisions.groundDirection, 1f, CollisionMask);
        }
        if (deltaMove.y > 0 || (deltaMove.y < 0 && (!Collisions.onSlope || deltaMove.x == 0)))
        {
            VerticalCollisions(ref deltaMove);
        }
        if (Collisions.onGround && deltaMove.x != 0 && Speed.y <= 0)
        {
            HandleSlopeChange(ref deltaMove);
        }
        Debug.DrawRay(transform.position, deltaMove * 3f, Color.green);
        transform.Translate(deltaMove);
        // Checks for ground and ceiling, resets jumps if grounded
        if (Collisions.vHit)
        {
            if ((Collisions.below && TotalSpeed.y < 0) || (Collisions.above && TotalSpeed.y > 0))
            {
                if (!Collisions.onSlope || Collisions.groundAngle < MinWallAngle)
                {
                    Speed.y = 0;
                    ExternalForce.y = 0;
                }
            }
        }
        gameObject.layer = layer;
        return deltaMove;
    }
    #endregion

    #region Handle Collisions
    protected void CheckGround(float direction)
    {
        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 rayOrigin = direction == 1 ? RayOrigins.bottomLeft : RayOrigins.bottomRight;
            rayOrigin += (direction == 1 ? Vector2.right : Vector2.left) * (VerticalRaySpacing * i);
            rayOrigin.y += SkinWidth * 2;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, SkinWidth * 4f, CollisionMask);
            if (!hit && IgnorePlatformsTime <= 0)
            {
                hit = Physics2D.Raycast(rayOrigin, Vector2.down, SkinWidth * 4f, PConfig.OneWayPlatformMask);
                if (hit.distance <= 0)
                {
                    continue;
                }
            }
            if (hit)
            {
                Collisions.onGround = true;
                Collisions.wasOnGround = true;
                Collisions.groundAngle = Vector2.Angle(hit.normal, Vector2.up);
                Collisions.groundDirection = Mathf.Sign(hit.normal.x);
                Collisions.groundLayer = hit.collider.gameObject.layer;
                Collisions.vHit = hit;
                Collisions.below = true;
                Debug.DrawRay(rayOrigin, Vector2.down * SkinWidth * 2, Color.blue);
                break;
            }
        }
        if (Collisions.onGround == false && Collisions.wasOnGround == true)
        {
            if (Speed.y <= 0)
            {
                UpdateCoyoteTimer = true;
            }
            Collisions.wasOnGround = false;
        }
    }

    protected void HorizontalCollisions(ref Vector2 deltaMove)
    {
        float directionX = Mathf.Sign(deltaMove.x);
        float rayLength = Mathf.Abs(deltaMove.x) + SkinWidth;
        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 rayOrigin = directionX == -1 ? RayOrigins.bottomLeft : RayOrigins.bottomRight;
            rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX,
                rayLength, CollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && !Collisions.onSlope && angle < MinWallAngle)
                {
                    Collisions.onGround = true;
                    Collisions.groundAngle = angle;
                    Collisions.groundDirection = Mathf.Sign(hit.normal.x);
                    deltaMove.x -= (hit.distance - SkinWidth) * directionX;
                    ClimbSlope(ref deltaMove);
                    deltaMove.x += (hit.distance - SkinWidth) * directionX;
                    rayLength = Mathf.Min(Mathf.Abs(deltaMove.x) + SkinWidth, hit.distance);
                }
                if (!(i == 0 && Collisions.onSlope))
                {
                    if (angle > MaxSlopeAngle)
                    {
                        if (angle < MinWallAngle)
                        {
                            continue;
                        }
                        deltaMove.x = Mathf.Min(Mathf.Abs(deltaMove.x), (hit.distance - SkinWidth)) * directionX;
                        rayLength = Mathf.Min(Mathf.Abs(deltaMove.x) + SkinWidth, hit.distance);
                        if (Collisions.onSlope && Collisions.groundAngle < MinWallAngle)
                        {
                            if (deltaMove.y < 0)
                            {
                                deltaMove.y = 0;
                            }
                            else
                            {
                                deltaMove.y = Mathf.Tan(Collisions.groundAngle * Mathf.Deg2Rad) *
                                    Mathf.Abs(deltaMove.x) * Mathf.Sign(deltaMove.y);
                            }
                        }
                        Collisions.left = directionX < 0;
                        Collisions.right = directionX > 0;
                        Collisions.hHit = hit;
                        Speed.x = 0;
                        ExternalForce.x = 0;
                    }
                }
            }
        }
    }

    protected virtual void VerticalCollisions(ref Vector2 deltaMove)
    {
        float directionY = Mathf.Sign(deltaMove.y);
        float rayLength = Mathf.Abs(deltaMove.y) + SkinWidth;
        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 rayOrigin = directionY == -1 ? RayOrigins.bottomLeft : RayOrigins.topLeft;
            rayOrigin += Vector2.right * (VerticalRaySpacing * i + deltaMove.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY,
                rayLength, CollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (directionY < 0 && !hit)
            {
                hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY,
                    rayLength, PConfig.OneWayPlatformMask);
            }
            if (hit)
            {
                deltaMove.y = (hit.distance - SkinWidth) * directionY;
                rayLength = hit.distance;
                if (Collisions.onSlope && directionY == 1)
                {
                    deltaMove.x = deltaMove.y / Mathf.Tan(Collisions.groundAngle * Mathf.Deg2Rad) *
                        Mathf.Sign(deltaMove.x);
                    Speed.x = 0;
                    ExternalForce.x = 0;
                }
                Collisions.above = directionY > 0;
                Collisions.below = directionY < 0;
                Collisions.vHit = hit;

            }
        }
    }
    #endregion

    #region Handle Slopes
    protected void ClimbSlope(ref Vector2 deltaMove)
    {
        if (Collisions.groundAngle < MinWallAngle)
        {
            float distance = Mathf.Abs(deltaMove.x);
            float yMove = Mathf.Sin(Collisions.groundAngle * Mathf.Deg2Rad) * distance;
            if (deltaMove.y <= yMove)
            {
                deltaMove.y = yMove;
                deltaMove.x = Mathf.Cos(Collisions.groundAngle * Mathf.Deg2Rad) * distance * Mathf.Sign(deltaMove.x);
                Collisions.below = true;
                Speed.y = 0;
                ExternalForce.y = 0;
            }
        }
    }

    protected void DescendSlope(ref Vector2 deltaMove)
    {
        float distance = Mathf.Abs(deltaMove.x);
        deltaMove.x = (Mathf.Cos(Collisions.groundAngle * Mathf.Deg2Rad) * distance) * Mathf.Sign(deltaMove.x);
        deltaMove.y = -Mathf.Sin(Collisions.groundAngle * Mathf.Deg2Rad) * distance;
        Collisions.below = true;
        Speed.y = 0;
        ExternalForce.y = 0;
    }

    protected virtual void HandleSlopeChange(ref Vector2 deltaMove)
    {
        float directionX = Mathf.Sign(deltaMove.x);
        if (deltaMove.y > 0)
        {
            // climb steeper slope
            float rayLength = Mathf.Abs(deltaMove.x) + SkinWidth * 2;
            Vector2 rayOrigin = (directionX == -1 ? RayOrigins.bottomLeft : RayOrigins.bottomRight) +
                Vector2.up * deltaMove.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);
            if (hit)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle != Collisions.groundAngle)
                {
                    deltaMove.x = (hit.distance - SkinWidth) * directionX;
                    Collisions.groundAngle = angle;
                    Collisions.groundDirection = Mathf.Sign(hit.normal.x);
                }
            }
            else
            {
                // climb milder slope or flat ground
                rayOrigin = (directionX == -1 ? RayOrigins.bottomLeft : RayOrigins.bottomRight) + deltaMove;
                hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1f, CollisionMask);
                Debug.DrawRay(rayOrigin, Vector2.down, Color.yellow);
                if (hit && hit.collider.gameObject.layer == Collisions.groundLayer)
                {
                    float angle = Vector2.Angle(hit.normal, Vector2.up);
                    float overshoot = 0;
                    if (angle < Collisions.groundAngle)
                    {
                        if (angle > 0)
                        {
                            float tanA = Mathf.Tan(angle * Mathf.Deg2Rad);
                            float tanB = Mathf.Tan(Collisions.groundAngle * Mathf.Deg2Rad);
                            float sin = Mathf.Sin(Collisions.groundAngle * Mathf.Deg2Rad);
                            overshoot = (2 * tanA * hit.distance - tanB * hit.distance) /
                                (tanA * sin - tanB * sin);
                        }
                        else
                        {
                            overshoot = hit.distance / Mathf.Sin(Collisions.groundAngle * Mathf.Deg2Rad);
                        }
                        float removeX = Mathf.Cos(Collisions.groundAngle * Mathf.Deg2Rad) * overshoot * Mathf.Sign(deltaMove.x);
                        float removeY = Mathf.Sin(Collisions.groundAngle * Mathf.Deg2Rad) * overshoot;
                        float addX = Mathf.Cos(angle * Mathf.Deg2Rad) * overshoot * Mathf.Sign(deltaMove.x);
                        float addY = Mathf.Sin(angle * Mathf.Deg2Rad) * overshoot;
                        deltaMove += new Vector2(addX - removeX, addY - removeY + SkinWidth);
                    }
                }
            }
        }
        else
        {
            // descend milder slope or flat ground
            float rayLength = Mathf.Abs(deltaMove.y) + SkinWidth;
            Vector2 rayOrigin = (directionX == -1 ? RayOrigins.bottomRight : RayOrigins.bottomLeft) +
                Vector2.right * deltaMove.x;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, CollisionMask);
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (hit && angle < Collisions.groundAngle)
            {
                deltaMove.y = -(hit.distance - SkinWidth);
                Collisions.groundAngle = angle;
                Collisions.groundDirection = Mathf.Sign(hit.normal.x);
            }
            else
            {
                // descend steeper slope
                rayOrigin = (directionX == 1 ? RayOrigins.bottomLeft : RayOrigins.bottomRight) + deltaMove;
                hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1f, CollisionMask);
                Debug.DrawRay(rayOrigin, Vector2.down, Color.yellow);
                if (hit && Mathf.Sign(hit.normal.x) == directionX &&
                    hit.collider.gameObject.layer == Collisions.groundLayer)
                {
                    angle = Vector2.Angle(hit.normal, Vector2.up);
                    float overshoot = 0;
                    if (angle > Collisions.groundAngle && Mathf.Sign(hit.normal.x) == (FacingRight ? 1 : -1))
                    {
                        if (Collisions.groundAngle > 0)
                        {
                            float sin = Mathf.Sin((Collisions.groundAngle) * Mathf.Deg2Rad);
                            float cos = Mathf.Cos((Collisions.groundAngle) * Mathf.Deg2Rad);
                            float tan = Mathf.Tan(angle * Mathf.Deg2Rad);
                            overshoot = hit.distance * cos / (tan / cos - sin);
                        }
                        else
                        {
                            overshoot = hit.distance / Mathf.Tan(angle * Mathf.Deg2Rad);
                        }
                        float removeX = Mathf.Cos(Collisions.groundAngle * Mathf.Deg2Rad) * overshoot * Mathf.Sign(deltaMove.x);
                        float removeY = -Mathf.Sin(Collisions.groundAngle * Mathf.Deg2Rad) * overshoot;
                        float addX = Mathf.Cos(angle * Mathf.Deg2Rad) * overshoot * Mathf.Sign(deltaMove.x);
                        float addY = -Mathf.Sin(angle * Mathf.Deg2Rad) * overshoot;
                        deltaMove += new Vector2(addX - removeX, addY - removeY - SkinWidth);
                    }
                }
            }
        }
    }
    #endregion

    #region Utility Funcions
    protected void PostMove()
    {
        IgnoreFriction = false;
    }

    public void ApplyForce(Vector2 force)
    {
        ExternalForce += force;
    }

    public virtual void SetForce(Vector2 force)
    {
        ExternalForce = force;
        // resets gravity
        if (Speed.y < 0)
        {
            Speed.y = 0;
        }
    }

    public void SetGravityScale(float gravityScale)
    {
        this.GravityScale = gravityScale;
    }
    #endregion

    #region Structs
    protected struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    protected struct CollisionInfo
    {
        public bool above, below, left, right;
        public RaycastHit2D hHit, vHit;
        public bool onGround;
        public bool wasOnGround;
        public float groundAngle;
        public float groundDirection;
        public int groundLayer;
        public bool onSlope { get { return onGround && groundAngle != 0; } }

        public void Reset()
        {
            above = false;
            below = false;
            left = false;
            right = false;
            hHit = new RaycastHit2D();
            vHit = new RaycastHit2D();
            onGround = false;
            groundAngle = 0;
            groundDirection = 0;
        }
    }
    #endregion
}