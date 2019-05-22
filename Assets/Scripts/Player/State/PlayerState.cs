using System;
using UnityEngine;

public abstract class PlayerState
{
    public Action<State> OnRequestState;
    public Action<Vector3> OnSetPosition;
    public Action<Quaternion> OnSetRotation;
    public Action<Collider> OnSetStandingCollider;
    
    public abstract void Enter(
        Parameters p,
        CurrentTransform t,
        ControllerDebug d);
    
    public abstract void Exit(
        Parameters p,
        CurrentTransform t,
        ControllerDebug d);
    
    public abstract void Update(
        Parameters p,
        CurrentTransform t,
        ControllerDebug d);
}
