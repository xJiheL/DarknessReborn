using System;
using UnityEngine;

[Serializable]
public class PlayerStateGrounded : PlayerState
{
    private Collider[] _overlapCapsule;
    
    public PlayerStateGrounded()
    {
        _overlapCapsule = new Collider[16];
    }

    public override void Enter(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        // snap to the ground
    }

    public override void Exit()
    {
        
    }

    public override void Update(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        Vector3 nextPosition = t.Position;

        float moveDistance = p.MoveSpeed * t.Direction.magnitude * Time.deltaTime;

        while (moveDistance > 0)
        {
            float currentMoveStep = p.MoveStep;

            if (moveDistance < p.MoveStep)
            {
                currentMoveStep = moveDistance;
            }

            {
                /* ---------------- Get ground normal ---------------- */

                if(!GetGroundNormal(p.GetCapsuleBottom(nextPosition, Vector3.up),
                    p.Radius,
                    p.GroundCheckDistance,
                    out Vector3 groundPoint,
                    out Vector3 groundNormal,
                    out Collider standingCollider))
                {
                    OnRequestState.Invoke(PlayerController.State.Falling);
                    UnityEditor.EditorApplication.isPaused = true;
                    return;
                }

                /* ---------------- Check state ---------------- */
                
                float angle = Vector3.Angle(Vector3.up, groundNormal);
                PlayerController.State state = p.GetStateWithAngle(angle);
                
                if (state != PlayerController.State.Grounded)
                {
                    OnRequestState.Invoke(state);
                    return;
                }
                
                DebugExt.DrawMarker(groundPoint, 1f, Color.red);
                Debug.DrawRay(groundPoint, groundNormal * 2f, Color.red);
                
                /* ---------------- Clamp to ground ---------------- */

                nextPosition = groundPoint + groundNormal * p.Radius - Vector3.up * p.Radius; // TODO ATTENTION A PAS LE POUSSER DANS UN AUTRE COLL 
                    
                /* ---------------- Move on the ground ---------------- */
        
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
                Vector3 moveDirectionProject = rotation * t.Direction.normalized;
        
                Debug.DrawRay(nextPosition, moveDirectionProject, Color.yellow);
        
                // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! je dois gérer la pénétration!
        
                nextPosition += moveDirectionProject * currentMoveStep;
            }
            
            moveDistance -= currentMoveStep;
        }
        
        /* ---------------- Final clamp to ground ---------------- */

        {
            if (!GetGroundNormal(p.GetCapsuleBottom(nextPosition, Vector3.up),
                p.Radius,
                p.GroundCheckDistance,
                out Vector3 groundPoint,
                out Vector3 groundNormal,
                out Collider standingCollider))
            {
                OnSetPosition.Invoke(nextPosition);
                OnSetStandingCollider.Invoke(standingCollider);
                
                OnRequestState.Invoke(PlayerController.State.Falling);
                return;
            }
            
            // TODO check climbing etc?
            
            nextPosition = groundPoint + groundNormal * p.Radius - Vector3.up * p.Radius;
            
            OnSetPosition.Invoke(nextPosition);
            OnSetStandingCollider.Invoke(standingCollider);
        }

        
        
        /*if (_direction.sqrMagnitude > 0.2f)
        {
            transform.forward = -_direction; // TODO ultra temp wtf
        }*/
    }

    private bool GetGroundNormal(Vector3 bottom, float radius, float groundCheckDistance, out Vector3 groundPoint, out Vector3 groundNormal, out Collider standingCollider)
    {
        groundNormal = -Vector3.up;
        Vector3 groundCheckVector = -Vector3.up * groundCheckDistance;

        DebugExt.DrawWireCapsule(
            bottom + groundCheckVector,
            bottom,
            radius, Color.red, Quaternion.identity);

        if (ClosestPointFromCapsule(
            bottom,
            bottom + groundCheckVector,
            radius,
            _overlapCapsule,
            out groundPoint,
            out standingCollider))
        {
            groundNormal = (bottom - groundPoint).normalized;
            return true;
        }

        return false;
    }

    public static bool ClosestPointFromCapsule(
        Vector3 fromPoint, 
        Vector3 otherPoint, 
        float radius, 
        Collider[] results, 
        out Vector3 closestPoint, 
        out Collider closestCollider)
    {
        int resultsCount = Physics.OverlapCapsuleNonAlloc(
            fromPoint,
            otherPoint,
            radius,
            results,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);
        
        NewMethod(fromPoint, results, out closestPoint, out closestCollider, resultsCount);

        return closestCollider != null;
    }
    
    public static bool ClosestPointFromSphere(
        Vector3 fromPoint, 
        float radius, 
        Collider[] results, 
        out Vector3 closestPoint, 
        out Collider closestCollider)
    {
        int resultsCount = Physics.OverlapSphereNonAlloc(
            fromPoint,
            radius,
            results,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);
        
        NewMethod(fromPoint, results, out closestPoint, out closestCollider, resultsCount);

        return closestCollider != null;
    }

    private static void NewMethod(
        Vector3 fromPoint, 
        Collider[] results, 
        out Vector3 closestPoint,
        out Collider closestCollider, 
        int resultsCount)
    {
        float minDistance = float.MaxValue;
        closestPoint = Vector3.zero;
        closestCollider = null;

        for (int i = 0; i < resultsCount; i++)
        {
            Vector3 newClosestPoint = results[i].ClosestPointExt(fromPoint);

            float distance = (newClosestPoint - fromPoint).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = newClosestPoint;
                closestCollider = results[i];
            }
        }
    }
}
