using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    public Collider Collider;
    
    public float radius = 0.5f;

    

    private void OnEnable()
    {
        Command.Instance.Add(EnumCommand.MoveCharacter, MovePlayer);
    }

    public float iterationMove = 0.1f; // must be smaller than radius

    public float speed = 5f;
    
    private void MovePlayer(Vector2 inputMove)
    {
        Vector3 closestPoint = Collider.ClosestPoint(transform.position);
        
        Vector3 normalMove = transform.position - closestPoint;
        
        Quaternion rotation = Quaternion.FromToRotation(transform.up, normalMove);

        Vector3 moveDirectionProject = rotation * transform.rotation * new Vector3(inputMove.x, 0f, inputMove.y);
        
        Debug.DrawRay(closestPoint, moveDirectionProject, Color.green);

        Vector3 newPos = closestPoint + normalMove.normalized * radius + moveDirectionProject * speed * Time.deltaTime;
        Vector3 newClos = Collider.ClosestPoint(newPos);
        Vector3 newNormal = newPos - newClos;
        
        transform.position = newClos + newNormal.normalized * radius;
        transform.forward = -newNormal;
        //  transform.position = closestPoint + moveDirectionProject * speed * Time.deltaTime;
    }

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


        Quaternion rotation = Quaternion.FromToRotation(transform.up, normalMove);

        Vector3 moveDirectionProject = rotation * transform.forward;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(closestPoint, moveDirectionProject.normalized);
    }
}
