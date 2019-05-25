using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class PlayerStateGrounded : PlayerState
{
    private readonly Collider[] _colliderBuffer;
    private readonly RaycastHit[] _hitBuffer;
    
    public PlayerStateGrounded()
    {
        _colliderBuffer = new Collider[16];
        _hitBuffer = new RaycastHit[16];
    }

    public override void Enter(
        Parameters p,
        CurrentTransform t,
        ControllerDebug d)
    {
        Vector3 position = ComputePenetration(p, t.Collider, d, t.Position);
        OnSetPosition(position);
        
        if (!GroundCheck(p, d, position, out _))
        {
            OnRequestState(State.Falling);
        }
        else
        {
            // TODO coller au sol
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
        if (tempStepOver != null)
        {
            return;
        }
        
        Vector3 nextPosition = t.Position;
        
        // TODO later: mettre à jour la position si le collider où on est a bougé
        // et gérer la pénétration dès maitenant?
        
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
                    State state = p.GetStateWithAngle(hit.normal);

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

                Vector3 newPosition = ComputePenetration(p, t.Collider, d, nextPosition + moveDirectionProject * currentMoveStep);
                float movementCompleted = Vector3.Distance(newPosition, nextPosition) / currentMoveStep;
                nextPosition = newPosition;
                CheckPlayerPosition(p, nextPosition);
                
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
                
                /* ---------------- Check step over ---------------- */
                
                {
                    float distance = 2f;
                    
                    Vector3 point1 = p.GetCapsuleTop(nextPosition, Vector3.up);
                    Vector3 point2 = point1 + p.Radius * distance * moveDirectionProject;

                    if (d.ShowStepOver)
                    {
                        DebugExt.DrawWireCapsule(
                            point1,
                            point2,
                            p.Radius,
                            d.StepOverColor);
                    }

                    int resultNumber = Physics.SphereCastNonAlloc( // TODO handle the empty hit ! (see ground check)
                        point1,
                        p.Radius,
                        moveDirectionProject,
                        _hitBuffer,
                        distance,
                        PlayerController.GetGroundMask(),
                        QueryTriggerInteraction.Ignore);

                    if (resultNumber == 0)
                    {
                        distance = p.Height - p.Radius * 2f;
                        Vector3 point3 = point2 - Vector3.up * distance;

                        if (d.ShowStepOver)
                        {
                            DebugExt.DrawWireCapsule(
                                point2,
                                point3,
                                p.Radius,
                                d.StepOverColor);
                        }

                        resultNumber = Physics.SphereCastNonAlloc( // TODO handle the empty hit ! (see ground check)
                            point2,
                            p.Radius,
                            -Vector3.up,
                            _hitBuffer,
                            distance,
                            PlayerController.GetGroundMask(),
                            QueryTriggerInteraction.Ignore);

                        if (resultNumber != 0)
                        {
                            RaycastHit stepOverHit = SortHit(_hitBuffer, resultNumber);

                            if (d.ShowStepOver)
                            {
                                DebugExt.DrawMarker(stepOverHit.point, 0.05f, d.StepOverColor);
                                Debug.DrawRay(stepOverHit.point, stepOverHit.normal * 2f, d.StepOverColor);
                            }
                            
                            State state = p.GetStateWithAngle(hit.normal);

                            if (state == State.Grounded && movementCompleted < 0.3f)
                            {
                                // TODO et si on peut se positionner là sans overlap un collider!
                                tempStepOver = Command.Instance.StartCoroutine(TempStepOver(
                                    nextPosition,
                                    stepOverHit.point + stepOverHit.normal * p.Radius - Vector3.up * (p.Radius - Physics.defaultContactOffset))); // TODO TEMP
                                //OnSetPosition(nextPosition);
                                //OnRequestState(State.StepOver);
                                return;
                            }
                        }
                    }
                }
            }
            
            moveDistance -= currentMoveStep;
        }
        
        
        /*if (_direction.sqrMagnitude > 0.2f)
        {
            transform.forward = -_direction; // TODO ultra temp wtf
        }*/
    }

    private Coroutine tempStepOver;
    IEnumerator TempStepOver(Vector3 startPos, Vector3 posEnd)
    {
        yield return DOTween.To(() => startPos, x => OnSetPosition(x), posEnd, 0.2f).WaitForCompletion();
        tempStepOver = null;
    }

    private Vector3 ComputePenetration(
        Parameters p,
        CapsuleCollider collider,
        ControllerDebug d,
        Vector3 position)
    {
        const int iterationMax = 10;
        
        Debug.Assert(collider.center.Equals(new Vector3(0f, p.Height / 2f, 0f)), "Incorrect capsule center");
        Debug.Assert(collider.radius.Equals(p.Radius), "Incorrect capsule radius");
        Debug.Assert(collider.height.Equals(p.Height), "Incorrect capsule height");
        Debug.Assert(collider.direction == 1, "Incorrect capsule direction"); // Y-Axis
        
        Vector3 bottom = p.GetCapsuleBottom(position, Vector3.up);
        Vector3 top = p.GetCapsuleTop(position, Vector3.up);

        Vector3 finalBottom = bottom;
        Vector3 finalTop = top;

        if (d.ShowComputePenetration)
        {
            DebugExt.DrawWireCapsule(
                finalBottom, 
                finalTop, 
                p.Radius, 
                d.ComputePenetrationColor);
        }

        int resultNumber = Physics.OverlapCapsuleNonAlloc(
            finalBottom, 
            finalTop, 
            p.Radius, 
            _colliderBuffer, 
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);
        
        Vector3 translation = Vector3.zero;
        int iteration = 0;
        
        while (resultNumber > 0 && iteration < iterationMax)
        {
            if (Physics.ComputePenetration(
                collider,
                position + translation,
                Quaternion.identity, 
                _colliderBuffer[0],
                _colliderBuffer[0].transform.position,
                _colliderBuffer[0].transform.rotation,
                out var direction,
                out var distance))
            {
                distance += Physics.defaultContactOffset;
                translation += direction * distance;
            }
            else
            {
                translation += Vector3.up * Physics.defaultContactOffset; // Attempt to move outside of that collider
                Debug.LogWarning($"Player overlaps {_colliderBuffer[0].name} with an unknown penetration!");
            }

            if (d.ShowComputePenetration)
            {
                DebugExt.DrawWireSphere(finalBottom, 
                    0.05f, d.ComputePenetrationColor, Quaternion.identity);
                DebugExt.DrawWireSphere(finalBottom + translation, 
                    0.05f, d.ComputePenetrationColor, Quaternion.identity);
                Debug.DrawLine(finalBottom, finalBottom + translation, d.ComputePenetrationColor);
            }

            finalBottom = bottom + translation;
            finalTop = top + translation;
            
            resultNumber = Physics.OverlapCapsuleNonAlloc(
                finalBottom, 
                finalTop, 
                p.Radius, 
                _colliderBuffer, 
                PlayerController.GetGroundMask(),
                QueryTriggerInteraction.Ignore);

            iteration++;
        }
        
        Debug.Assert(iteration < iterationMax, $"Cannot resolve penetration with less than {iteration} iterations!");
        /* TODO faire un debugueur du nombre d'itération en fonction de l'endroit pour améliorer le LD */

        if (d.ShowComputePenetration)
        {
            DebugExt.DrawWireCapsule(
                finalBottom,
                finalTop,
                p.Radius,
                d.ComputePenetrationColor);
        }

        return position + translation;
    }
    
    private bool GroundCheck(
        Parameters p,
        ControllerDebug d,
        Vector3 position,
        out RaycastHit hit)
    {
        hit = new RaycastHit();
        
        Vector3 origin = p.GetCapsuleBottom(position, Vector3.up);
        Vector3 direction = -Vector3.up;
        float distance = p.GroundCheckDistance;

        if (d.ShowGroundCheck)
        {
            DebugExt.DrawWireCapsule(
                origin,
                origin + direction * distance,
                p.Radius,
                d.GroundCheckColor);
        }
        
        /*
         * SphereCastNonAlloc = SphereCastAll
         * En soit je pourrais utiliser SphereCast tout court, j'ai pas besoin de SphereCastAll !
         * Sauf que SphereCast retourne faux si il y a quelque chose dans la sphère d'origine (et OverlapSphere
         * ne me dira pas forcément si c'est le cas ou pas, va comprendre !)
         * Du coup l'avantage de SphereCastAll, c'est que lui à le mérite de retourner un hit "vide" si c'est le cas.
         */
        
        int hitNumber = Physics.SphereCastNonAlloc(
            origin,
            p.Radius /* - Physics.defaultContactOffset TODO ????? */,
            direction,
            _hitBuffer,
            distance,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);

        if (hitNumber <= 0)
        {
            return false;
        }

        hit = SortHit(_hitBuffer, hitNumber);

        if (hit.point == Vector3.zero && hit.distance.Equals(0f))
        {
            Debug.LogError($"Ground check error: Player is inside {hit.collider.name}! Cannot get normal!");
            return false;
        }
        
        if (d.ShowGroundCheck)
        {
            DebugExt.DrawWireSphere(hit.point, 0.05f, d.GroundCheckColor, Quaternion.identity);
            Debug.DrawRay(hit.point, hit.normal, d.GroundCheckColor);
        }
        
        return true;
    }

    private RaycastHit SortHit(RaycastHit[] hits, int hitNumber)
    {
        RaycastHit nearestHit = new RaycastHit();
        float nearestHitDistance = float.MaxValue;

        for (int i = 0; i < hitNumber; i++)
        {
            if (hits[i].distance < nearestHitDistance)
            {
                nearestHitDistance = hits[i].distance;
                nearestHit = hits[i];
            }
        }

        return nearestHit;
    }

    
    
    
    /* TODO to reformat below here */
    
    private void CheckPlayerPosition(Parameters p, Vector3 nextPosition)
    {
        int resultsCount = Physics.OverlapCapsuleNonAlloc(
            p.GetCapsuleBottom(nextPosition, Vector3.up),
            p.GetCapsuleTop(nextPosition, Vector3.up),
            p.Radius,
            _colliderBuffer,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);

        Debug.Assert(resultsCount == 0, "Player is inside a wall!");
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
