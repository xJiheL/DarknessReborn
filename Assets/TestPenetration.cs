using System;
using UnityEngine;

public class TestPenetration : MonoBehaviour
{
    private CapsuleCollider capsule;
    
    void OnDrawGizmos()
    {
        capsule = GetComponent<CapsuleCollider>();

        Vector3 point1 = transform.position - transform.up * (capsule.height / 2f - capsule.radius); 
        Vector3 point2 = transform.position + transform.up * (capsule.height / 2f - capsule.radius); 
        
        Vector3 finalPoint1 = point1;
        Vector3 finalPoint2 = point2;
        
        DebugExt.DrawWireCapsule(point1, point2, capsule.radius, Color.blue);

        int iteration = 0;
        
        Collider[] colliders = Physics.OverlapCapsule(
            finalPoint1, 
            finalPoint2, 
            capsule.radius, 
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);

        Vector3 moveVector3 = Vector3.zero;
        
        while (colliders.Length > 0 && iteration < 10)
        {
            if (Physics.ComputePenetration(
                capsule,
                capsule.transform.position + moveVector3,
                capsule.transform.rotation,
                colliders[0],
                colliders[0].transform.position,
                colliders[0].transform.rotation,
                out var direction,
                out var distance))
            {
                distance += Physics.defaultContactOffset;
                moveVector3 += direction * distance;
            }
            else
            {
                // possible with floating value...
                moveVector3 += Vector3.up * Physics.defaultContactOffset;
                Debug.LogWarning("overlap but no penetration");
            }
            
            finalPoint1 = point1 + moveVector3;
            finalPoint2 = point2 + moveVector3;
            
            colliders = Physics.OverlapCapsule(
                finalPoint1, 
                finalPoint2, 
                capsule.radius, 
                PlayerController.GetGroundMask(),
                QueryTriggerInteraction.Ignore);

            iteration++;
        }
        
        DebugExt.DrawWireCapsule(finalPoint1, finalPoint2, capsule.radius, iteration == 10 ? Color.red : Color.green);
        GizmosExt.DrawText("Iteration "+iteration, finalPoint2);
        
        colliders = Physics.OverlapSphere(
            finalPoint1,
            capsule.radius, 
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);
        
        DebugExt.DrawWireSphere(finalPoint1, capsule.radius, colliders.Length == 0 ? Color.yellow : Color.magenta, Quaternion.identity);

        if (colliders.Length == 0)
        {
            if (Physics.SphereCast(
                finalPoint1,
                capsule.radius - Physics.defaultContactOffset,
                -Vector3.up,
                out var hitInfo,
                float.MaxValue,
                PlayerController.GetGroundMask(),
                QueryTriggerInteraction.Ignore))
            {
                DebugExt.DrawMarker(hitInfo.point, 1f, Color.yellow);
                Debug.DrawRay(hitInfo.point, hitInfo.normal * 2f, Color.yellow);
            }
            else
            {
                Debug.LogError("No normal !");
            }
        }
    }
}