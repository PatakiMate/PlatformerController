using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterData))]
public class CharacterController : ObjectController
{
    public bool OnLadder { get; set; }
    public bool OnEdge { get; set; }
    public bool Dashing { get; set; }
    public bool Flying { get; set; }
    public bool Gliding { get; set; }
    public SpriteRenderer Visual;

    private CharacterData _cData;
    private float _ignoreLaddersTime = 0;
    private float _ignoreEdgesTime = 0;
    private int _extraJumps = 0;
    private int _airDashes = 0;
    private float _dashCooldown = 0;
    private float _airStaggerTime = 0;
    private float _ladderX = 0;
    private float _coyoteTime = 0;
    private float _jumpBufferTime = 0;

    public override void Start()
    {
        _cData = GetComponent<CharacterData>();
        OnLadder = false;
        OnEdge = false;
        Dashing = false;
        base.Start();
    }

    public override void FixedUpdate()
    {
        UpdateTimers();
        Collisions.Reset();
        Move((TotalSpeed) * Time.fixedDeltaTime);
        PostMove();
    }

    #region Handle Physics Forces
    protected override void UpdateGravity()
    {
        if (!OnLadder && !OnEdge && !Dashing && _airStaggerTime <= 0)
        {
            base.UpdateGravity();
        }
    }

    protected override void UpdateExternalForce()
    {
        if (!Dashing && _airStaggerTime <= 0)
        {
            base.UpdateExternalForce();
        }
    }
    #endregion

    #region Movement
    public override Vector2 Move(Vector2 deltaMove)
    {
        if (Gliding && TotalSpeed.y < 0)
        {
            GravityScale = 0.05f;
        }
        else
        {
            GravityScale = 1f;
        }
        int layer = gameObject.layer;
        gameObject.layer = Physics2D.IgnoreRaycastLayer;
        PreMove(ref deltaMove);
        float xDir = Mathf.Sign(deltaMove.x);
        if (deltaMove.x != 0)
        {
            // Slope checks and processing
            if (deltaMove.y <= 0 && _cData.CanUseSlopes)
            {
                if (Collisions.onSlope)
                {
                    if (Collisions.groundDirection == xDir)
                    {
                        if ((!Dashing && _airStaggerTime <= 0) || _cData.DashDownSlopes)
                        {
                            DescendSlope(ref deltaMove);
                        }
                    }
                    else
                    {
                        ClimbSlope(ref deltaMove);
                    }
                }
            }
            HorizontalCollisions(ref deltaMove);
        }
        if (Collisions.hHit && _cData.CanWallSlide && TotalSpeed.y <= 0)
        {
            ExternalForce.y = 0;
            Speed.y = -_cData.WallSlideSpeed;
        }
        if (Collisions.onSlope && Collisions.groundAngle >= MinWallAngle &&
            Collisions.groundDirection != xDir && Speed.y < 0)
        {
            float sin = Mathf.Sin(Collisions.groundAngle * Mathf.Deg2Rad);
            float cos = Mathf.Cos(Collisions.groundAngle * Mathf.Deg2Rad);
            deltaMove.x = cos * _cData.WallSlideSpeed * Time.fixedDeltaTime * Collisions.groundDirection;
            deltaMove.y = sin * -_cData.WallSlideSpeed * Time.fixedDeltaTime;
            Speed.y = -_cData.WallSlideSpeed;
            Speed.x = 0;
            Vector2 origin = Collisions.groundDirection == -1 ? RayOrigins.bottomRight : RayOrigins.bottomRight;
            Collisions.hHit = Physics2D.Raycast(origin, Vector2.left * Collisions.groundDirection,
                1f, CollisionMask);
        }
        if (Collisions.onGround && deltaMove.x != 0 && Speed.y <= 0)
        {
            HandleSlopeChange(ref deltaMove);
        }
        if (deltaMove.y > 0 || (deltaMove.y < 0 && (!Collisions.onSlope || deltaMove.x == 0)))
        {
            VerticalCollisions(ref deltaMove);
        }
        Debug.DrawRay(transform.position, deltaMove * 3f, Color.green);
        transform.Translate(deltaMove);
        // Checks for ground and ceiling, resets jumps if grounded
        if (Collisions.vHit)
        {
            if ((Collisions.below && TotalSpeed.y < 0) || (Collisions.above && TotalSpeed.y > 0))
            {
                if (Collisions.below)
                {
                    ResetJumpsAndDashes();
                }
                if (!Collisions.onSlope || Collisions.groundAngle < MinWallAngle)
                {
                    Speed.y = 0;
                    ExternalForce.y = 0;
                }
            }
        }
        //Flying
        if (Flying)
        {
            Fly();
        }
        gameObject.layer = layer;
        return deltaMove;
    }
    public void Walk(float direction)
    {
        if (Collisions.onSlope && Collisions.groundAngle > MaxSlopeAngle && Collisions.groundAngle < MinWallAngle)
        {
            direction = 0;
        }
        if (CanMove() && !Dashing && _airStaggerTime <= 0)
        {
            if (OnLadder || OnEdge)
            {
                if (direction != 0)
                {
                    FacingRight = direction > 0;
                    if (Visual) Visual.flipX = !FacingRight;
                }
                return;
            }
            float acc = 0f;
            float dec = 0f;
            if (_cData.AdvancedAirControl && !Collisions.below)
            {
                acc = _cData.AirAccelerationTime;
                dec = _cData.AirDecelerationTime;
            }
            else
            {
                acc = _cData.AccelerationTime;
                dec = _cData.DecelerationTime;
            }
            if (acc > 0)
            {
                if (ExternalForce.x != 0 && Mathf.Sign(ExternalForce.x) != Mathf.Sign(direction))
                {
                    ExternalForce.x += direction * (1 / acc) * _cData.MaxSpeed * Time.fixedDeltaTime;
                }
                else
                {
                    if (Mathf.Abs(Speed.x) < _cData.MaxSpeed)
                    {
                        Speed.x += direction * (1 / acc) * _cData.MaxSpeed * Time.fixedDeltaTime;
                        Speed.x = Mathf.Min(Mathf.Abs(Speed.x), _cData.MaxSpeed * Mathf.Abs(direction)) *
                            Mathf.Sign(Speed.x);
                    }
                }

            }
            else
            {
                Speed.x = _cData.MaxSpeed * direction;
            }
            if (direction == 0 || Mathf.Sign(direction) != Mathf.Sign(Speed.x))
            {
                if (dec > 0)
                {
                    Speed.x = Mathf.MoveTowards(Speed.x, 0, (1 / dec) * _cData.MaxSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    Speed.x = 0;
                }
            }
        }
    }
    #endregion

    #region Handle Collisions

    protected override void VerticalCollisions(ref Vector2 deltaMove)
    {
        if (OnLadder)
        {
            Collisions.Reset();
            Vector2 origin = MyCollider.bounds.center + Vector3.up * (MyCollider.bounds.extents.y * Mathf.Sign(deltaMove.y) + deltaMove.y);
            Collider2D hit = Physics2D.OverlapCircle(origin, 0, CollisionMask);
            if (!hit)
            {
                return;
            }
            hit = Physics2D.OverlapCircle(origin, 0, PConfig.LadderMask);
            if (hit)
            {
                return;
            }
        }
        float directionY = Mathf.Sign(deltaMove.y);
        float rayLength = Mathf.Abs(deltaMove.y) + SkinWidth;
        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 rayOrigin = directionY == -1 ? RayOrigins.bottomLeft : RayOrigins.topLeft;
            rayOrigin += Vector2.right * (VerticalRaySpacing * i + deltaMove.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            // for one way platforms
            if (IgnorePlatformsTime <= 0 && directionY < 0 && !hit)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down,
                    rayLength, PConfig.OneWayPlatformMask);
                foreach (RaycastHit2D h in hits)
                {
                    if (h.distance > 0)
                    {
                        hit = h;
                        continue;
                    }
                }
            }
            if (hit)
            {
                deltaMove.y = (hit.distance - SkinWidth) * directionY;
                rayLength = hit.distance;
                if (OnLadder && directionY < 0)
                {
                    OnLadder = false;
                    IgnoreLadders();
                }
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
    protected override void HandleSlopeChange(ref Vector2 deltaMove)
    {
        if (deltaMove.y <= 0 && (Dashing || _airStaggerTime > 0) && !_cData.DashDownSlopes)
        {
            return;
        }
        else
        {
            base.HandleSlopeChange(ref deltaMove);
        }
    }
    #endregion

    #region Utility Functions
    private void IgnorePlatforms()
    {
        IgnorePlatformsTime = OneWayPlatformDelay;
    }

    private void IgnoreLadders()
    {
        _ignoreLaddersTime = LadderDelay;
    }
    private void IgnoreEdges()
    {
        _ignoreEdgesTime = EdgeDelay;
    }

    public void ResetJumpsAndDashes()
    {
        _extraJumps = _cData.MaxExtraJumps;
        _airDashes = _cData.MaxAirDashes;
    }

    public override void SetForce(Vector2 force)
    {
        base.SetForce(force);
        // cancels dash
        Dashing = false;
        _airStaggerTime = 0;
    }

    public void SetAirStagger(float duration)
    {
        _airStaggerTime = duration;
    }

    public bool CanMove()
    {
        return true;
    }
    #endregion


    #region Jump
    public void Jump()
    {
        StartCoroutine(JumpMethod());
    }
    public IEnumerator JumpMethod()
    {
        if (CanMove() && (!Dashing || _cData.CanJumpDuringDash))
        {
            _jumpBufferTime = _cData.JumpBufferTime;
            if (Collisions.onGround == false && _extraJumps == 0)
            {
                while (_jumpBufferTime > 0)
                {
                    _jumpBufferTime -= Time.fixedDeltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }
            if (Collisions.onGround || _coyoteTime > 0 || _extraJumps > 0 || (_cData.CanWallJump && Collisions.hHit))
            {
                UpdateCoyoteTimer = false;
                // air jump
                if (!Collisions.onGround && !OnLadder)
                {
                    _extraJumps--;
                    ExternalForce = Vector2.zero;
                }
                float height = _cData.MaxJumpHeight;
                if (OnLadder)
                {
                    Vector2 origin = MyCollider.bounds.center + Vector3.up * MyCollider.bounds.extents.y;
                    Collider2D hit = Physics2D.OverlapCircle(origin, 0, CollisionMask);
                    if (hit)
                    {
                        yield return null;
                    }
                    origin = MyCollider.bounds.center + Vector3.down * MyCollider.bounds.extents.y;
                    hit = Physics2D.OverlapCircle(origin, 0, CollisionMask);
                    if (hit)
                    {
                        yield return null;
                    }
                    height = _cData.LadderJumpHeight;
                    ExternalForce.x += _cData.LadderJumpSpeed * (FacingRight ? 1 : -1);
                    OnLadder = false;
                    IgnoreLadders();
                    ResetJumpsAndDashes();
                }
                if (OnEdge)
                {
                    Vector2 origin = MyCollider.bounds.center + Vector3.up * MyCollider.bounds.extents.y;
                    Collider2D hit = Physics2D.OverlapCircle(origin, 0, CollisionMask);
                    if (hit)
                    {
                        yield return null;
                    }
                    origin = MyCollider.bounds.center + Vector3.down * MyCollider.bounds.extents.y;
                    hit = Physics2D.OverlapCircle(origin, 0, CollisionMask);
                    if (hit)
                    {
                        yield return null;
                    }
                    height = _cData.LadderJumpHeight;
                    ExternalForce.x += _cData.LadderJumpSpeed * (FacingRight ? 1 : -1);
                    OnEdge = false;
                    IgnoreEdges();
                    ResetJumpsAndDashes();
                }
                Speed.y = Mathf.Sqrt(-2 * PConfig.Gravity * height);
                ExternalForce.y = 0;
                if (_cData.JumpCancelStagger)
                {
                    _airStaggerTime = 0;
                }
                // wall jump
                if (_cData.CanWallJump && Collisions.hHit && !Collisions.below)
                {
                    ExternalForce.x += Collisions.left ? _cData.WallJumpSpeed : -_cData.WallJumpSpeed;
                    ResetJumpsAndDashes();
                }
                //slope sliding jump
                if (Collisions.onSlope && Collisions.groundAngle > MaxSlopeAngle &&
                    Collisions.groundAngle < MinWallAngle)
                {
                    Speed.x = _cData.MaxSpeed * Collisions.groundDirection;
                }
                IgnorePlatformsTime = 0;
            }
        }
    }

    public void EndJump()
    {
        float yMove = Mathf.Sqrt(-2 * PConfig.Gravity * _cData.MinJumpHeight);
        if (Speed.y > yMove)
        {
            Speed.y = yMove;
        }
    }

    public void JumpDown()
    {
        if (CanMove())
        {
            if (Collisions.vHit && PConfig.OneWayPlatformMask ==
                (PConfig.OneWayPlatformMask | (1 << Collisions.vHit.collider.gameObject.layer)))
            {
                IgnorePlatforms();
            }
            else
            {
                Jump();
            }
        }
    }
    #endregion

    #region Glide
    public void Glide()
    {
        if (TotalSpeed.y < 0 || Flying)
        {
            Gliding = true;
        }
    }
    public void EndGlide()
    {
        Gliding = false;
    }
    #endregion

    #region Dash
    public void Dash(Vector2 direction)
    {
        if (CanMove() && _cData.CanDash && _dashCooldown <= 0)
        {
            if (OnLadder)
            {
                Vector2 origin = MyCollider.bounds.center + Vector3.up * MyCollider.bounds.extents.y;
                Collider2D hit = Physics2D.OverlapCircle(origin, 0, CollisionMask);
                if (hit)
                {
                    return;
                }
                origin = MyCollider.bounds.center + Vector3.down * MyCollider.bounds.extents.y;
                hit = Physics2D.OverlapCircle(origin, 0, CollisionMask);
                if (hit)
                {
                    return;
                }
                OnLadder = false;
            }
            if (!Collisions.onGround)
            {
                if (_airDashes > 0)
                {
                    _airDashes--;
                }
                else
                {
                    return;
                }
            }
            Dashing = true;
            if (direction.magnitude == 0 || (Collisions.onGround && direction.y < 0))
            {
                direction = FacingRight ? Vector2.right : Vector2.left;
            }
            // wall dash
            if (Collisions.hHit)
            {
                direction = FacingRight ? Vector2.left : Vector2.right;
                ResetJumpsAndDashes();
            }
            if (!_cData.OmnidirectionalDash)
            {
                direction = Vector2.right * Mathf.Sign(direction.x);
            }
            direction = direction.normalized * _cData.DashSpeed;
            Speed.x = 0;
            Speed.y = 0;
            ExternalForce = direction;
            _dashCooldown = _cData.MaxDashCooldown;
            _airStaggerTime = _cData.DashStagger;
            Invoke("StopDash", _cData.DashDistance / _cData.DashSpeed);
        }
    }

    private void StopDash()
    {
        Dashing = false;
    }
    #endregion

    #region Fly
    public void Fly()
    {
        Speed.y = 10;
    }
    public void EndFly()
    {
        Speed.y = 0;
    }
    #endregion


    #region Ladder
    public void ClimbLadder(float direction)
    {
        if (_ignoreLaddersTime > 0 || Dashing)
        {
            return;
        }
        float radius = MyCollider.bounds.extents.x;
        Vector2 topOrigin = ((Vector2)MyCollider.bounds.center) + Vector2.up * (MyCollider.bounds.extents.y - radius);
        Vector2 bottomOrigin = ((Vector2)MyCollider.bounds.center) + Vector2.down *
            (MyCollider.bounds.extents.y + radius + SkinWidth);
        if (!OnLadder && direction != 0 && Mathf.Abs(direction) > LadderClimbThreshold)
        {
            Collider2D hit = Physics2D.OverlapCircle(direction == -1 ? bottomOrigin : topOrigin,
                radius, PConfig.LadderMask);
            if (hit)
            {
                OnLadder = true;
                Speed.x = 0;
                ExternalForce = Vector2.zero;
                _ladderX = hit.transform.position.x;
            }
        }
        if (OnLadder)
        {
            float newX = Mathf.MoveTowards(transform.position.x, _ladderX, 5f * Time.fixedDeltaTime);
            transform.Translate(newX - transform.position.x, 0, 0);
            ResetJumpsAndDashes();
            if (_cData.LadderAccelerationTime > 0)
            {
                if (Mathf.Abs(Speed.y) < _cData.LadderSpeed)
                {
                    Speed.y += direction * (1 / _cData.LadderAccelerationTime) * _cData.LadderSpeed * Time.fixedDeltaTime;
                }
            }
            else
            {
                Speed.y = _cData.LadderSpeed * direction;
            }
            if (direction == 0 || Mathf.Sign(direction) != Mathf.Sign(Speed.y))
            {
                if (_cData.LadderDecelerationTime > 0)
                {
                    Speed.y = Mathf.MoveTowards(Speed.x, 0, (1 / _cData.LadderDecelerationTime) *
                        _cData.LadderSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    Speed.y = 0;
                }
            }
            if (Mathf.Abs(Speed.y) > _cData.LadderSpeed)
            {
                Speed.y = Mathf.Min(Speed.y, _cData.LadderSpeed);
            }
            // checks ladder end
            Collider2D hit = Physics2D.OverlapCircle(topOrigin + Vector2.up * (Speed.y * Time.fixedDeltaTime + radius),
                0, PConfig.LadderMask);
            if (!hit)
            {
                hit = Physics2D.OverlapCircle(bottomOrigin + Vector2.up *
                    (Speed.y * Time.fixedDeltaTime + radius - SkinWidth * 3), SkinWidth, PConfig.LadderMask);
                if (!hit)
                {
                    OnLadder = false;
                    if (Speed.y > 0)
                    {
                        Speed.y = 0;
                    }
                }
            }
        }
    }
    #endregion

    #region Edge Hold
    public void StickToEdge(float direction)
    {
        if (_ignoreEdgesTime > 0 || Dashing)
        {
            return;
        }
        float radius = MyCollider.bounds.extents.x;
        Vector2 topOrigin = ((Vector2)MyCollider.bounds.center) + Vector2.up * (MyCollider.bounds.extents.y - radius);
        Vector2 bottomOrigin = ((Vector2)MyCollider.bounds.center) + Vector2.down * (MyCollider.bounds.extents.y + radius + SkinWidth);
        Collider2D hit = Physics2D.OverlapCircle(direction == -1 ? bottomOrigin : topOrigin, radius, PConfig.EdgeMask);
        if (hit)
        {
            if (OnEdge == false)
            {
                OnEdge = true;
                Speed.x = 0;
                Vector3 height = new Vector3(0, MyCollider.bounds.extents.y * 2 - hit.bounds.extents.y, 0);
                transform.DOMove(hit.transform.position - height, 0.15f);
                ExternalForce = Vector2.zero;
            }
        }
        if (OnEdge)
        {
            ResetJumpsAndDashes();
            if (Mathf.Abs(Speed.y) > 0)
            {
                Speed.y = 0;
            }
        }
    }
    #endregion


    #region Timers
    private void UpdateTimers()
    {
        if (_dashCooldown > 0)
        {
            _dashCooldown -= Time.fixedDeltaTime;
        }
        if (IgnorePlatformsTime > 0)
        {
            IgnorePlatformsTime -= Time.fixedDeltaTime;
        }
        if (_ignoreLaddersTime > 0)
        {
            _ignoreLaddersTime -= Time.fixedDeltaTime;
        }
        if (_ignoreEdgesTime > 0)
        {
            _ignoreEdgesTime -= Time.fixedDeltaTime;
        }
        if (UpdateCoyoteTimer)
        {
            _coyoteTime += Time.fixedDeltaTime;
        }
        if (_coyoteTime > _cData.CoyoteTime || UpdateCoyoteTimer == false)
        {
            _coyoteTime = 0;
            UpdateCoyoteTimer = false;
        }
        if (_airStaggerTime > 0 && !Dashing)
        {
            _airStaggerTime -= Time.fixedDeltaTime;
            //ExternalForce = Vector2.MoveTowards(ExternalForce, Vector2.zero, PConfig.StaggerSpeedFalloff * Time.fixedDeltaTime);
            //Speed = Vector2.MoveTowards(Speed, Vector2.zero, PConfig.StaggerSpeedFalloff * Time.fixedDeltaTime);
        }
    }
    #endregion
}