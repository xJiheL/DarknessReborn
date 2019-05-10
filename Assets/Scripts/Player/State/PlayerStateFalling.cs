using System;
using UnityEngine;

[Serializable]
public class PlayerStateFalling : PlayerState
{
    private float _verticalVelocity;
    private Vector3 _velocity;
    private Vector3 _smoothRef;

    public override void Enter(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        _verticalVelocity = 0f;
        _velocity = t.Velocity;
        _smoothRef = Vector3.zero;
    }

    public override void Exit(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        
    }

    public override void Update(PlayerController.Parameters p, PlayerController.CurrentTransform t)
    {
        _verticalVelocity += p.Gravity * t.DeltaTime;

        if (_verticalVelocity < p.MinVerticalVelocity)
        {
            _verticalVelocity = p.MinVerticalVelocity;
        }

        _velocity = Vector3.SmoothDamp(_velocity, /*t.Direction * p.MoveSpeed*/ Vector3.zero, ref _smoothRef, 0.3f); // TODO no move in the air? add airSpeed

        Debug.DrawRay(t.Position, _velocity, Color.magenta);

        Vector3 castDirection = (Vector3.up * _verticalVelocity + _velocity) * t.DeltaTime;
        
        Vector3 bottom = p.GetCapsuleBottom(t.Position, t.Up);
        Vector3 top = p.GetCapsuleTop(t.Position, t.Up);
        
        DebugExt.DrawWireCapsule(
            bottom, 
            top, 
            p.Radius, Color.blue);
        
        DebugExt.DrawWireCapsule(
            bottom + castDirection, 
            top + castDirection,
            p.Radius, Color.red);
        
        if (Physics.CapsuleCast(
            bottom,
            top,
            p.Radius,
            castDirection,
            out RaycastHit hit,
            castDirection.magnitude,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore))
        {
            DebugExt.DrawMarker(hit.point, 1f, Color.red);
            Debug.DrawRay(hit.point, hit.normal, Color.red);

            OnSetPosition.Invoke(t.Position + castDirection.normalized * hit.distance);
            
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            PlayerController.State state = p.GetStateWithAngle(angle);

            if (state != PlayerController.State.Falling)
            {
                OnRequestState.Invoke(state);
                return;
            }
            
            Debug.LogError("it's possible?");
        }
        
        OnSetPosition.Invoke(t.Position + castDirection);
    }
}
