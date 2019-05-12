using System;
using UnityEngine;

[Serializable]
public class Parameters
{
    [SerializeField]
    private float moveSpeed = 5f;
    
    [SerializeField]
    private float radius = 0.25f;
    
    [SerializeField]
    private float height = 1.7f;

    [SerializeField]
    private float moveStep = 0.2f;
    
    [SerializeField]
    private float groundCheckDistance = 0.3f;
    
    [SerializeField]
    private bool debugMode = false;
    
    [SerializeField]
    private Vector2 debugInput = Vector2.zero;

    [SerializeField]
    private float gravity = -9.81f;
    
    [SerializeField]
    private float minVerticalVelocity = -30f;

    [SerializeField]
    private float groundLimit = 75f;

    [SerializeField]
    private float climbLimit = 150f;
    
    public void OnValidate()
    {
        if (moveSpeed < 0f)
        {
            moveSpeed = 0f;
        }
        
        if (radius < 0.1f)
        {
            radius = 0.1f;
        }

        if (height < radius * 2f)
        {
            height = radius * 2f;
        }
        
        if (moveStep < 0.01f)
        {
            moveStep = 0.01f;
        }

        if (moveStep > radius)
        {
            moveStep = radius;
        }
        
        if (groundCheckDistance < 0f)
        {
            groundCheckDistance = 0f;
        }

        if (groundLimit < 0f)
        {
            groundLimit = 0f;
        }
        
        if (climbLimit < 0f)
        {
            climbLimit = 0f;
        }
        
        if (climbLimit > 180f)
        {
            climbLimit = 180f;
        }
        
        if (groundLimit > climbLimit)
        {
            groundLimit = climbLimit;
        }
    }

    public float MoveSpeed => moveSpeed;

    public float Radius => radius;

    public float Height => height;

    public float MoveStep => moveStep;

    public float GroundCheckDistance => groundCheckDistance;

    public bool DebugMode => debugMode;

    public Vector2 DebugInput => debugInput;

    public float Gravity => gravity;

    public float MinVerticalVelocity => minVerticalVelocity;

    public float GroundLimit => groundLimit;

    public float ClimbLimit => climbLimit;

    public Vector3 GetCapsuleBottom(Vector3 position, Vector3 up)
    {
        return position + up * radius;
    }

    public Vector3 GetCapsuleTop(Vector3 position, Vector3 up)
    {
        return position + up * (height - radius);
    }

    public State GetStateWithAngle(float angle)
    {
        if (angle <= groundLimit)
        {
            return State.Grounded;
        }
        
        if (angle <= climbLimit)
        {
            return State.Climbing;
        }

        return State.Falling;
    }
}