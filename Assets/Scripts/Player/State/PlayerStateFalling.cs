using System;
using UnityEngine;

[Serializable]
public class PlayerStateFalling : PlayerState
{
    private float verticalVelocity;
    
    private Vector3 currentDirection;
    private Vector3 currentDirectionVelocity;
    
    public PlayerStateFalling(Transform t) : base(t)
    {
    }

    public override void Enter()
    {
        verticalVelocity = 0f;
        
        currentDirection = Vector3.zero;
        currentDirectionVelocity = Vector3.zero;
    }

    public override void Exit()
    {
        
    }

    public override void Update(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        verticalVelocity += p.Gravity * Time.deltaTime;

        if (verticalVelocity < p.MinVerticalVelocity)
        {
            verticalVelocity = p.MinVerticalVelocity;
        }

        currentDirection = Vector3.SmoothDamp(currentDirection, t.Direction, ref currentDirectionVelocity, 0.3f);

        Debug.DrawRay(t.Position, currentDirection * 2f, Color.magenta);

        Vector3 castDirection = Vector3.up * verticalVelocity * Time.deltaTime + 
                                currentDirection * p.MoveSpeed * Time.deltaTime ;
        
        Vector3 bottom = PlayerController.GetCapsuleBottom(T.position, p.Radius);
        Vector3 top = PlayerController.GetCapsuleTop(T.position, p.Radius, p.Height);
        
        DebugExt.DrawWireCapsule(
            bottom, 
            top, 
            p.Radius, Color.red, Quaternion.identity);
        
        DebugExt.DrawWireCapsule(
            bottom + castDirection, 
            top + castDirection,
            p.Radius, Color.blue, Quaternion.identity);
        
        if (Physics.CapsuleCast(
            bottom,
            top,
            p.Radius,
            castDirection,
            out RaycastHit hit,
            castDirection.magnitude, // todo avoid that
            1 << LayerMask.NameToLayer("Ground"),
            QueryTriggerInteraction.Ignore))
        {
            DebugExt.DrawMarker(hit.point, 1f, Color.red);
            
            OnSetPosition.Invoke(new Vector3(t.Position.x, 100f, t.Position.z));
            Enter();
            return;
            
            //GoToState(State.Grounded);
        }

        Vector3 newPos = T.position + castDirection;
        
        OnSetPosition.Invoke(newPos);
    }
}
