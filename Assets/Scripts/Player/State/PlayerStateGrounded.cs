using System;
using UnityEngine;

[Serializable]
public class PlayerStateGrounded : PlayerState
{
    private Collider[] _overlapCapsule;
    private RaycastHit[] hits;
    
    public PlayerStateGrounded()
    {
        _overlapCapsule = new Collider[16];
        hits = new RaycastHit[16];
    }

    public override void Enter(
        Parameters p,
        CurrentTransform t,
        ControllerDebug d)
    {
        if (GroundCheckWithCastNewSusu(p, d, t.Position, out RaycastHit hit))
        {
            OnSetPosition(hit.point + hit.normal * p.Radius - Vector3.up * (p.Radius - Physics.defaultContactOffset));
        }
        else
        {
            OnRequestState(State.Falling);
        }
    }

    public override void Exit(
        Parameters p,
        CurrentTransform t,
        ControllerDebug d)
    {
        
    }

    public override void Update(
        Parameters p,
        CurrentTransform t,
        ControllerDebug d)
    {   
        Vector3 nextPosition = t.Position;

        /* Checks if player is inside a wall */
        
        int resultsCount = Physics.OverlapCapsuleNonAlloc(
            p.GetCapsuleBottom(nextPosition, Vector3.up),
            p.GetCapsuleTop(nextPosition, Vector3.up),
            p.Radius,
            _overlapCapsule,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);
        
        Debug.Assert(resultsCount == 0, "Player is inside a wall!");
        
        /* TODO */
        
        // TODO later: mettre à jour la position si le collider où on est a bougé
        
        float moveDistance = p.MoveSpeed * t.Direction.magnitude * t.DeltaTime;

        while (moveDistance > 0)
        {
            float currentMoveStep = p.MoveStep;

            if (moveDistance < p.MoveStep)
            {
                currentMoveStep = moveDistance;
            }

            {
                /* ---------------- Get ground normal ---------------- */

                if (!GroundCheckWithCastNewSusu(p, d, nextPosition, out RaycastHit hit))
                {
                    // TODO set next pos ?!
                    
                    OnRequestState(State.Falling);
                    return;
                }
                
                /* ---------------- Check state ---------------- */
                
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                State state = p.GetStateWithAngle(angle);
                
                if (state != State.Grounded)
                {
                    OnRequestState.Invoke(state);
                    return;
                }

                if (d.ShowGroundNormal)
                {
                    DebugExt.DrawMarker(hit.point, 1f, d.GroundNormalColor);
                    Debug.DrawRay(hit.point, hit.normal * 2f, d.GroundNormalColor);
                }
                
                /* ---------------- Clamp to ground ---------------- */
                
                /*
                 * Important ! pour éviter entre chaque passage dans la loop de ne plus coller au sol
                 */
                
                nextPosition = hit.point + hit.normal * p.Radius - Vector3.up * (p.Radius - Physics.defaultContactOffset); // TODO ATTENTION A PAS LE POUSSER DANS UN AUTRE COLL... faire un check?
                
                /* ---------------- Move on the ground ---------------- */
        
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Vector3 moveDirectionProject = rotation * t.Direction.normalized;

                if (d.ShowMoveDirection)
                {
                    Debug.DrawRay(hit.point, moveDirectionProject, d.MoveDirectionColor);
                }
        
                break;
                
                // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! je dois gérer la pénétration!



                Vector3 point1 = p.GetCapsuleBottom(nextPosition, Vector3.up);
                Vector3 point2 = p.GetCapsuleTop(nextPosition, Vector3.up);
                
                
                DebugExt.DrawWireCapsule(
                    point1,
                    point2,
                    p.Radius,
                    Color.magenta);
                
                DebugExt.DrawWireCapsule(
                    point1 + moveDirectionProject * currentMoveStep,
                    point2 + moveDirectionProject * currentMoveStep,
                    p.Radius,
                    Color.magenta);
                
                int hitNumber = Physics.CapsuleCastNonAlloc(
                    point1,
                    point2,
                    p.Radius,
                    moveDirectionProject,
                    hits,
                    currentMoveStep,
                    PlayerController.GetGroundMask(),
                    QueryTriggerInteraction.Ignore);

                if (hitNumber == 0)
                {
                    nextPosition += moveDirectionProject * currentMoveStep;
                    Debug.Log("énorme");
                }
                else
                {
                    
                    for (int i = 0; i < hitNumber; i++)
                    {
                        DebugExt.DrawWireSphere(hits[i].point, 0.1f, Color.magenta, Quaternion.identity);
                        DebugExt.DrawMarker(hits[i].point, 1f, Color.magenta);
                        Debug.Log(hits[i].collider+" | "+hits[i].point);
                    }

                    
                    Debug.LogError(hitNumber);
                }
            }
            
            moveDistance -= currentMoveStep;
        }
        
        return;
        
        /* ---------------- Final clamp to ground ---------------- */
        
        {
            if (!GetGroundNormal(
                d,
                p.GetCapsuleBottom(nextPosition, Vector3.up),
                p.Radius,
                p.GroundCheckDistance,
                out Vector3 groundPoint,
                out Vector3 groundNormal,
                out Collider standingCollider))
            {
                OnSetPosition.Invoke(nextPosition);
                OnSetStandingCollider.Invoke(standingCollider);
                
                OnRequestState.Invoke(State.Falling);
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

    private bool GroundCheckWithCastNewSusu(
        Parameters p,
        ControllerDebug d,
        Vector3 position,
        out RaycastHit hit)
    {
        hit = new RaycastHit();
        
        Vector3 origin = p.GetCapsuleTop(position, Vector3.up);
        Vector3 direction = -Vector3.up;
        float distance = p.Height - p.Radius * 2f + p.GroundCheckDistance;

        if (d.ShowGroundCheck)
        {
            DebugExt.DrawWireCapsule(
                origin,
                origin + direction * distance,
                p.Radius,
                d.GroundCheckColor);
        }

        int resultNumber = Physics.OverlapSphereNonAlloc(
            origin,
            p.Radius,
            _overlapCapsule,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);

        Debug.Assert(resultNumber == 0, "Player is inside a wall!");

        int hitNumber = Physics.SphereCastNonAlloc(
            origin,
            p.Radius,
            direction,
            hits,
            distance,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);

        if (hitNumber <= 0)
        {
            return false;
        }

        float nearestHitDistance = float.MaxValue;

        for (int i = 0; i < hitNumber; i++)
        {
            if (d.ShowGroundCheck)
            {
                DebugExt.DrawWireSphere(hits[i].point, 0.1f, d.GroundCheckColor, Quaternion.identity);
                DebugExt.DrawMarker(hits[i].point, 1f, d.GroundCheckColor);
            }

            if (hits[i].distance < nearestHitDistance)
            {
                nearestHitDistance = hits[i].distance;
                hit = hits[i];
            }
        }

        return true;
    }

    private bool GetGroundNormal(ControllerDebug d, Vector3 bottom, float radius, float groundCheckDistance, out Vector3 groundPoint, out Vector3 groundNormal, out Collider standingCollider)
    {
        groundNormal = -Vector3.up;
        Vector3 groundCheckVector = -Vector3.up * groundCheckDistance;

        if (d.ShowGroundNormal)
        {
            DebugExt.DrawWireCapsule(
                bottom,
                bottom + groundCheckVector,
                radius,
                Color.red);
        }

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
