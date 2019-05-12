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

    public bool ShowMoveDirection => showMoveDirection;

    public Color MoveDirectionColor => moveDirectionColor;

    public bool ShowGroundNormal => showGroundNormal;

    public Color GroundNormalColor => groundNormalColor;

    public bool ShowGroundCheck => showGroundCheck;

    public Color GroundCheckColor => groundCheckColor;
}
