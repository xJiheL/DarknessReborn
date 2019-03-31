using System;
using UnityEngine;

[Serializable]
public class PlayerStateFalling : PlayerState
{
    private float verticalVelocity;
    
    public PlayerStateFalling(Transform t) : base(t)
    {
    }

    public override void Enter()
    {
        verticalVelocity = 0f;
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

        float movement = verticalVelocity * Time.deltaTime;

        Vector3 origin = T.position + Vector3.up * p.Radius;
        
        DebugExt.DrawWireSphere(origin, p.Radius, Color.red, Quaternion.identity);
        DebugExt.DrawWireSphere(origin - Vector3.up * Mathf.Abs(movement), p.Radius, Color.blue, Quaternion.identity);
        
        if (Physics.SphereCast(
            origin,
            p.Radius,
            -Vector3.up,
            out RaycastHit hit,
            Mathf.Abs(movement),
            1 << LayerMask.NameToLayer("Ground"),
            QueryTriggerInteraction.Ignore))
        {
            DebugExt.DrawMarker(hit.point, 1f, Color.red);
            
            OnSetPosition.Invoke(new Vector3(t.Position.x, 100f, t.Position.z));
            Enter();
            return;
            
            //GoToState(State.Grounded);
        }

        Vector3 newPos = T.position + Vector3.up * movement;
        
        OnSetPosition.Invoke(newPos);
    }
}
