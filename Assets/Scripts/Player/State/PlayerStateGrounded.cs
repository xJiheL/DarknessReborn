using System;
using UnityEngine;

[Serializable]
public class PlayerStateGrounded : PlayerState
{
    private Collider[] overlapCapsule;
    public Collider closestCollider;
    
    public PlayerStateGrounded(Transform t) : base(t)
    {
        overlapCapsule = new Collider[16];
    }

    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void Update(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {

        Vector3 newPosition = t.Position;

        float distanceToTravel = p.MoveSpeed * Time.deltaTime;

        while (distanceToTravel > 0)
        {
            float currentMoveStep = p.MoveStep;

            if (distanceToTravel < p.MoveStep)
            {
                currentMoveStep = distanceToTravel;
            }

            newPosition += UpdateController(p, t, newPosition, currentMoveStep);
            distanceToTravel -= currentMoveStep;
        }

       /* transform.position = newPosition;

        if (_direction.sqrMagnitude > 0.2f)
        {
            transform.forward = -_direction; // TODO ultra temp wtf
        }*/
    }
    
    private Vector3 UpdateController(PlayerController.Parameters p, PlayerController.CurrentTransform t, Vector3 startPosition, float moveDistance)
    {
        /* ---------------- */

        Vector3 groundNormal = Vector3.up;

        /* ---------------- Ground Test ---------------- */

        {
            Vector3 capsuleBottom = startPosition + Vector3.up * p.Radius;
            Vector3 vector3 = capsuleBottom - Vector3.up * p.GroundCheckDistance;
            
            DebugExt.DrawWireSphere(capsuleBottom, p.Radius, Color.white, t.Rotation);
            DebugExt.DrawWireSphere(vector3, p.Radius, Color.blue, t.Rotation);

            int colliderTouched = Physics.OverlapCapsuleNonAlloc(
                capsuleBottom,
                vector3,
                p.Radius,
                overlapCapsule,
                1 << LayerMask.NameToLayer("Ground"),
                QueryTriggerInteraction.Ignore);

            float minDistance = float.MaxValue;
            closestCollider = null; // TODO : handle when there are no colliders
            Vector3 closestPoint = Vector3.zero; // same

            for (int i = 0; i < colliderTouched; i++)
            {
                Vector3
                    newClosestPoint =
                        overlapCapsule[i]
                            .ClosestPointExt(
                                capsuleBottom); // TODO check si le collider est bien compatible (donc tout sauf les mesh collider pas convex : https://docs.unity3d.com/ScriptReference/Physics.ClosestPoint.html)

                float distance = Vector3.Distance(newClosestPoint, capsuleBottom); // TODO use sqrMagnitude instead
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = overlapCapsule[i];
                    closestPoint = newClosestPoint;
                }
            }

            if (closestCollider == null)
            {
               /* GoToState(State.Falling);*/
            }

            groundNormal = (capsuleBottom - closestPoint).normalized;
            // TODO marche pas du coup !!!!!
          /*  transform.position = closestPoint + groundNormal * radius - Vector3.up * radius; // ATTENTION A PAS LE POUSSER DANS UN AUTRE COLL */

            DebugExt.DrawMarker(closestPoint, 1f, Color.red);

            Debug.DrawRay(closestPoint, groundNormal * 2f, Color.magenta);
        }

        /* ---------------- */

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
        Vector3 moveDirectionProject = rotation * t.Direction;

        Debug.DrawRay(t.Position, moveDirectionProject * 2f, Color.yellow);

        // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! je dois gérer la pénétration!

        return moveDirectionProject * moveDistance;


    }
}
