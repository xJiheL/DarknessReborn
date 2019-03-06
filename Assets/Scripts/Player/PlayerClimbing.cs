using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    public Collider Collider;
    
    public void OnDrawGizmos()
    {
        Vector3 closestPoint = Collider.ClosestPoint(transform.position);

        Gizmos.DrawWireSphere(closestPoint, 0.1f);
    }
}
