using System;
using UnityEngine;

[Serializable]
public class PlayerStateGrounded : PlayerState
{
    private Collider[] _overlapCapsule;
    private RaycastHit[] _hits;
    
    public PlayerStateGrounded()
    {
        _overlapCapsule = new Collider[16];
        _hits = new RaycastHit[16];
    }

    public override void Enter(
        Parameters p,
        CurrentTransform t,
        ControllerDebug d)
    {
        if (GroundCheck(p, d, t.Position, out RaycastHit hit))
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
        
        CheckPlayerPosition(p, nextPosition);

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

                if (!GroundCheck(p, d, nextPosition, out RaycastHit hit))
                {
                    // TODO set next pos ?!
                    
                    OnRequestState(State.Falling);
                    return;
                }
                
                /* ---------------- Check state ---------------- */

                {
                    float angle = Vector3.Angle(Vector3.up, hit.normal);
                    State state = p.GetStateWithAngle(angle);

                    if (state != State.Grounded)
                    {
                        OnRequestState.Invoke(state);
                        return;
                    }
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
                    _hits,
                    currentMoveStep,
                    PlayerController.GetGroundMask(),
                    QueryTriggerInteraction.Ignore);
                
                if (hitNumber == 0)
                {
                    nextPosition += moveDirectionProject * currentMoveStep;
                }
                else
                {
                    //nextPosition += moveDirectionProject * (hit.distance - Physics.defaultContactOffset);
                    
                    hit = SortHit(_hits, hitNumber);
                    
                    for (int i = 0; i < hitNumber; i++)
                    {
                        DebugExt.DrawWireSphere(hit.point, 0.1f, Color.magenta, Quaternion.identity);
                        Debug.DrawRay(hit.point, _hits[i].normal, Color.magenta);
                        DebugExt.DrawMarker(hit.point, 1f, Color.magenta);
                    }
                    
                    float angle = Vector3.Angle(Vector3.up, hit.normal);
                    State state = p.GetStateWithAngle(angle);

                    if (state == State.Grounded)
                    {
                        nextPosition = hit.point + hit.normal * p.Radius - Vector3.up * (p.Radius - Physics.defaultContactOffset); // TODO generique method?...
                        CheckPlayerPosition(p, nextPosition);
                        
                        // TODO iterate with the left distance
                        
                        
                        Debug.Log("resolve ground detection, left: "+(currentMoveStep - hit.distance));
                    }
                    else
                    {
                        /*if (!GroundCheckWithCastNewSusu(p, d, nextPosition, out RaycastHit hit))
                        {
                            // TODO set next pos ?!
                    
                            OnRequestState(State.Falling);
                            return;
                        }*/
                    }
                }
                
                
                // TODO la question est : on clamp au sol ici, ou au début de la prochaine loop? je dirais ici, on considère que cette itération est "valide" donc pas besoin de clamper au milieu là, enfin si mais on le fait sur une variable temp
                
                /* ---------------- Final clamp to ground ---------------- */
                
                if (!GroundCheck(p, d, nextPosition, out RaycastHit hitFinal)) // TODO relou les vieux hit, faire des braces
                {
                    OnSetPosition.Invoke(nextPosition);
                    OnSetStandingCollider.Invoke(null);
                    
                    // TODO set next pos ?! ben oui non? sinon c'est con !
                    
                    OnRequestState(State.Falling);
                    return;
                }
                
                // TODO check climbing etc?
            
                nextPosition = hitFinal.point + hitFinal.normal * p.Radius - Vector3.up * (p.Radius - Physics.defaultContactOffset);
            
                OnSetPosition.Invoke(nextPosition);
                OnSetStandingCollider.Invoke(hitFinal.collider);
            }
            
            moveDistance -= currentMoveStep;
        }
        
        
        /*if (_direction.sqrMagnitude > 0.2f)
        {
            transform.forward = -_direction; // TODO ultra temp wtf
        }*/
    }

    private void CheckPlayerPosition(Parameters p, Vector3 nextPosition)
    {
        int resultsCount = Physics.OverlapCapsuleNonAlloc(
            p.GetCapsuleBottom(nextPosition, Vector3.up),
            p.GetCapsuleTop(nextPosition, Vector3.up),
            p.Radius,
            _overlapCapsule,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);

        Debug.Assert(resultsCount == 0, "Player is inside a wall!");
    }

    private bool GroundCheck(
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
            _hits,
            distance,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);

        if (hitNumber <= 0)
        {
            return false;
        }

        hit = SortHit(_hits, hitNumber);

        if (d.ShowGroundCheck)
        {
            DebugExt.DrawWireSphere(hit.point, 0.1f, d.GroundCheckColor, Quaternion.identity);
            DebugExt.DrawMarker(hit.point, 1f, d.GroundCheckColor);
        }
        
        return true;
    }

    private RaycastHit SortHit(RaycastHit[] hits, int hitNumber)
    {
        RaycastHit neareastHit = new RaycastHit();
        float nearestHitDistance = float.MaxValue;

        for (int i = 0; i < hitNumber; i++)
        {
            if (hits[i].distance < nearestHitDistance)
            {
                nearestHitDistance = hits[i].distance;
                neareastHit = hits[i];
            }
        }

        return neareastHit;
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
