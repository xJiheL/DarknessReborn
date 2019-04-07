using System;
using UnityEngine;

public abstract class PlayerState
{
    public Action<PlayerController.State> OnRequestState;
    public Action<Vector3> OnSetPosition;
    public Action<Quaternion> OnSetRotation;
    public Action<Collider> OnSetStandingCollider;
    
    private Transform _t;

    public Transform T => _t;

    public PlayerState(Transform t)
    {
        _t = t;
    }

    public abstract void Enter(PlayerController.Parameters p, PlayerController.CurrentTransform t);
    public abstract void Exit();
    
    public abstract void Update(PlayerController.Parameters p, PlayerController.CurrentTransform t);
}
