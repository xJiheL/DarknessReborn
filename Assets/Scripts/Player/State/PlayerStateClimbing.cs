using System;
using UnityEngine;

[Serializable]
public class PlayerStateClimbing : PlayerState
{
    private Collider[] _overlapSphere;
    private Collider _climbingCollider;
    
    public PlayerStateClimbing()
    {
        _overlapSphere = new Collider[16];
    }
    
    public override void Enter(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        Debug.Assert(t.StandingCollider != null);
        _climbingCollider = t.StandingCollider;
        
        // TODO coller au cliff et orienter vers le cliff
    }

    public override void Exit()
    {
        
    }

    public override void Update(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        Vector3 bottom = p.GetCapsuleBottom(t.Position, t.Up);
        
        /* ---------------- Get cliff point and normal ---------------- */
        
        Vector3 cliffClosestPoint = _climbingCollider.ClosestPointExt(bottom);
        DebugExt.DrawWireSphere(cliffClosestPoint, 0.1f, Color.red, Quaternion.identity);
        
        Vector3 cliffNormal = (bottom - cliffClosestPoint).normalized;
        Debug.DrawRay(cliffClosestPoint, cliffNormal, Color.red);
        
        DebugExt.DrawWireSphere(cliffClosestPoint + cliffNormal * p.Radius, p.Radius, Color.red, Quaternion.identity);

        /* ---------------- Get direction on cliff ---------------- */
        
        Quaternion cliffRotation = Quaternion.FromToRotation(t.Up, cliffNormal);
        Vector3 directionOnCliff = cliffRotation * t.Rotation * new Vector3(t.InputMove.x, 0, t.InputMove.y);
        Debug.DrawRay(cliffClosestPoint, directionOnCliff, Color.green);

        /* ---------------- Get new closest collider with movement ---------------- */
        
        Vector3 nextPosition = cliffClosestPoint + 
                         cliffNormal * p.Radius + 
                         directionOnCliff * p.MoveSpeed * Time.deltaTime; // TODO climb speed

        Vector3 movementVector = nextPosition - bottom;
        Vector3 middleMovementVector = bottom + movementVector / 2;
        float radiusOverlap = p.Radius + movementVector.magnitude / 2f; // TODO can be optimised?
        
        DebugExt.DrawWireSphere(middleMovementVector, radiusOverlap, Color.cyan, Quaternion.identity);

        if (PlayerStateGrounded.ClosestPointFromSphere(
            middleMovementVector,
            radiusOverlap,
            _overlapSphere,
            out Vector3 closestPoint,
            out Collider closestCollider))
        {
            if (closestCollider != null)
            {
                _climbingCollider = closestCollider;
            }
        }
        
        /* ---------------- Clamp to that collider ---------------- */

        Vector3 newCliffClosestPoint = _climbingCollider.ClosestPointExt(nextPosition);
        Vector3 newCliffNormal = (nextPosition - newCliffClosestPoint).normalized;

        Vector3 forward = -newCliffNormal;
        Quaternion rotation = Quaternion.LookRotation(forward);
        Vector3 up = rotation * Vector3.up;
        
        OnSetPosition.Invoke(newCliffClosestPoint + newCliffNormal * p.Radius - up * p.Radius);
        OnSetRotation.Invoke(rotation);
        
        /* ---------------- Change state ---------------- */ 
        
        float angle = Vector3.Angle(Vector3.up, newCliffNormal);
        PlayerController.State state = p.GetStateWithAngle(angle);

        if (state != PlayerController.State.Climbing)
        {
            OnRequestState.Invoke(state);
        }
    }
}
