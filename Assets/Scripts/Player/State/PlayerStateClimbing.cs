using System;
using UnityEngine;

[Serializable]
public class PlayerStateClimbing : PlayerState
{
    private Collider[] overlapSphere;
    
   
    
    public PlayerStateClimbing(Transform t) : base(t)
    {
        overlapSphere = new Collider[16];
    }
    
    public Collider _startCollider;


    public override void Enter(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void Update(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        Vector3 closestPoint = _startCollider.ClosestPointExt(t.Position);
        
        DebugExt.DrawWireSphere(closestPoint, 0.1f, Color.red, Quaternion.identity);
        
        Vector3 normalMove = (t.Position - closestPoint).normalized;
        
        Debug.DrawRay(closestPoint, normalMove, Color.blue);
        DebugExt.DrawWireSphere(closestPoint + normalMove * p.Radius, p.Radius, Color.red, Quaternion.identity);
        
        Quaternion rotation = Quaternion.FromToRotation(t.Up, normalMove);

        Vector3 moveDirectionProject = rotation * t.Rotation * new Vector3(t.Direction.x, 0f, t.Direction.y);
        
        Debug.DrawRay(closestPoint, moveDirectionProject, Color.green);

        Vector3 newPos = closestPoint + normalMove * p.Radius + moveDirectionProject * p.MoveSpeed * Time.deltaTime;


        Vector3 movementVector = newPos - t.Position;
        Vector3 middleMovementVector = t.Position + movementVector / 2;
        float radiusOverlap = p.Radius + movementVector.magnitude / 2f;// can be optimised 
        
        int colliderTouched = Physics.OverlapSphereNonAlloc(
            middleMovementVector, 
            radiusOverlap, 
            overlapSphere, 
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);

        DebugExt.DrawWireSphere(middleMovementVector, radiusOverlap, Color.cyan, Quaternion.identity);
        
        float minDistance = float.MaxValue;
        Collider newCollider = null; // TODO : handle when there are no colliders
        
        // TODO faire une méthode utilitaire pour ça
        for (int i = 0; i < colliderTouched; i++)
        {
            Vector3 newClosestPoint = overlapSphere[i].ClosestPointExt(newPos); // TODO check si le collider est bien compatible (donc tout sauf les mesh collider pas convex : https://docs.unity3d.com/ScriptReference/Physics.ClosestPoint.html)

            float distance = Vector3.Distance(newClosestPoint, newPos); // TODO use sqrMagnitude instead
            if (distance < minDistance)
            {
                minDistance = distance;
                newCollider = overlapSphere[i];
            }
        }
        
        if (newCollider != null)
        {
            _startCollider = newCollider;
        }

        Vector3 newClos = _startCollider.ClosestPointExt(newPos); // déjà fait au dessus
        Vector3 newNormal = newPos - newClos;
        
        /*transform.position = newClos + newNormal.normalized * p.Radius;
        transform.forward = -newNormal;*/
        //  transform.position = closestPoint + moveDirectionProject * speed * Time.deltaTime;
    }
}
