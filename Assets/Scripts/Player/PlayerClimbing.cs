using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    public Collider Collider;
    
    public float radius = 0.5f;
    
    public void OnDrawGizmos()
    {
        Vector3 closestPoint = Collider.ClosestPoint(transform.position);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(closestPoint, 0.1f);
        
        Vector3 normalMove = transform.position - closestPoint;
        
        Gizmos.DrawRay(closestPoint, normalMove);
        Gizmos.DrawWireSphere(closestPoint + normalMove.normalized * radius, radius);
        
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward);


        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normalMove);

        Vector3 moveDirectionProject = rotation * transform.forward;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(closestPoint, moveDirectionProject.normalized);
    }
}
