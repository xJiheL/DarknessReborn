using System;
using UnityEngine;

[Serializable]
public class ControllerDebug
{
    [SerializeField]
    private bool showMoveDirection = false;
    
    [SerializeField]
    private Color moveDirectionColor = Color.yellow;
    
    [SerializeField]
    private bool showGroundNormal = false;
    
    [SerializeField]
    private Color groundNormalColor = Color.red;

    [SerializeField]
    private bool showGroundCheck = false;
    
    [SerializeField]
    private Color groundCheckColor = Color.magenta;

    [SerializeField]
    private bool showComputePenetration = false;
    
    [SerializeField]
    private Color computePenetrationColor = Color.blue;

    public bool ShowMoveDirection => showMoveDirection;

    public Color MoveDirectionColor => moveDirectionColor;

    public bool ShowGroundNormal => showGroundNormal;

    public Color GroundNormalColor => groundNormalColor;

    public bool ShowGroundCheck => showGroundCheck;

    public Color GroundCheckColor => groundCheckColor;

    public bool ShowComputePenetration => showComputePenetration;

    public Color ComputePenetrationColor => computePenetrationColor;
}
