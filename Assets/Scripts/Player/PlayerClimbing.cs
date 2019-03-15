using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    public Collider Collider;
    
    public float radius = 0.5f;

    
    private Vector2 _inputDirection;

    private void OnEnable()
    {
        Command.Instance.Add(EnumCommand.MoveCharacter, MovePlayer);
    }

    public float iterationMove = 0.1f; // must be smaller than radius

    public float speed = 5f;
    
    private void MovePlayer(Vector2 inputMove)
    {
        _inputDirection = inputMove;
    }
    
    private void Update()
    {
        Vector3 closestPoint = Collider.ClosestPoint(transform.position);
        
        DebugExt.DrawWireSphere(closestPoint, 0.1f, Color.red, Quaternion.identity);
        
        Vector3 normalMove = (transform.position - closestPoint).normalized;
        
        Debug.DrawRay(closestPoint, normalMove, Color.blue);
        DebugExt.DrawWireSphere(closestPoint + normalMove * radius, radius, Color.red, Quaternion.identity);
        
        Quaternion rotation = Quaternion.FromToRotation(transform.up, normalMove);

        Vector3 moveDirectionProject = rotation * transform.rotation * new Vector3(_inputDirection.x, 0f, _inputDirection.y);
        
        Debug.DrawRay(closestPoint, moveDirectionProject, Color.green);

        Vector3 newPos = closestPoint + normalMove * radius + moveDirectionProject * speed * Time.deltaTime;
        Vector3 newClos = Collider.ClosestPoint(newPos);
        Vector3 newNormal = newPos - newClos;
        
        transform.position = newClos + newNormal.normalized * radius;
        transform.forward = -newNormal;
        //  transform.position = closestPoint + moveDirectionProject * speed * Time.deltaTime;
    }
}
