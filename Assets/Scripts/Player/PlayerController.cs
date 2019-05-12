using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Parameters _parameters = null;
    
    [SerializeField]
    private ControllerDebug _debug = null;
    
    private PlayerState _currentState;
    private PlayerStateGrounded _stateGrounded;
    private PlayerStateFalling _stateFalling;
    private PlayerStateClimbing _stateClimbing;

    private Transform _transform;
    
    private Vector3 _direction;

    private Vector2 _inputMove;
    
    private Vector3 _previousPosition;
    private Vector3 _velocity;

    private Collider _standingCollider;
    
    public Vector3 Direction => _direction;

    
    private void OnValidate()
    {
        _parameters?.OnValidate();
    }

    private void Awake()
    {
        _transform = transform;
        
        _stateGrounded = new PlayerStateGrounded();
        _stateFalling = new PlayerStateFalling();
        _stateClimbing = new PlayerStateClimbing();

        _previousPosition = transform.position;
        
        GoToState(State.Grounded);
    }

    private void OnEnable()
    {
        Command.Instance.Add(EnumCommand.MoveCharacter, MoveCharacter);
        Command.Instance.Add(EnumCommand.Jump, Jump);
    }

    private void OnDisable()
    {
        // TODO isInstanciated
        //Command.Instance.Remove(EnumCommand.MoveCharacter);
        //Command.Instance.Remove(EnumCommand.Jump);
    }
    
    private void MoveCharacter(Vector2 inputMove)
    {
        _inputMove = inputMove;
    }
    
    private void Jump()
    {
        Debug.Log("jump soon!");
    }

    private void Update()
    {
        Vector3 inputMoveNormalized = !_parameters.DebugMode
            ? new Vector3(_inputMove.x, 0f, _inputMove.y)
            : new Vector3(_parameters.DebugInput.x, 0f, _parameters.DebugInput.y);

        if (inputMoveNormalized.sqrMagnitude > 1f)
        {
            inputMoveNormalized = inputMoveNormalized.normalized;
        }

        if (!_parameters.DebugMode)
        {
            _direction = Quaternion.Euler(new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f)) * inputMoveNormalized;
        }
        else
        {
            _direction = inputMoveNormalized;
        }

        CurrentTransform currentTransform = GetCurrentTransform();
        
        Debug.DrawRay(currentTransform.Position, _direction, Color.green);
        
        _currentState.Update(_parameters, currentTransform, _debug);
        
        DebugExt.DrawWireCapsule(
            _parameters.GetCapsuleBottom(_transform.position, currentTransform.Up), 
            _parameters.GetCapsuleTop(_transform.position, currentTransform.Up),
            _parameters.Radius, Color.cyan);
        
        Debug.DrawRay(transform.position, _velocity, Color.cyan);
    }

    private void GoToState(State newState)
    {
        if (_currentState != null)
        {
            _currentState.Exit(_parameters, GetCurrentTransform(), _debug);
            Debug.Log("State Exit " + _currentState);
            
            _currentState.OnSetPosition -= OnSetPosition;
            _currentState.OnSetRotation -= OnSetRotation;
            _currentState.OnSetStandingCollider -= OnSetStandingCollider;
            _currentState.OnRequestState -= GoToState;
        }

        switch (newState)
        {
            case State.Grounded:
                _currentState = _stateGrounded;
                break;
            case State.Falling:
                _currentState = _stateFalling;
                break;
            case State.Climbing:
                _currentState = _stateClimbing;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        _currentState.OnSetPosition += OnSetPosition;
        _currentState.OnSetRotation += OnSetRotation;
        _currentState.OnSetStandingCollider += OnSetStandingCollider;
        _currentState.OnRequestState += GoToState;
        
        Debug.Log("State Enter " + _currentState);
        _currentState.Enter(_parameters, GetCurrentTransform(), _debug);
    }

    private void OnSetPosition(Vector3 position)
    {
        _transform.position = position;

        Vector3 lastMovement = position - _previousPosition;
        _velocity = lastMovement / Time.deltaTime;
        _previousPosition = position;
    }
    
    private void OnSetRotation(Quaternion rotation)
    {
        _transform.rotation = rotation;
    }
    
    private void OnSetStandingCollider(Collider standingCollider)
    {
        _standingCollider = standingCollider;
    }

    public static int GetGroundMask()
    {
        return 1 << LayerMask.NameToLayer("Ground");
    }

    private CurrentTransform GetCurrentTransform()
    {
        return new CurrentTransform(
            _direction, 
            _parameters.DebugMode ? _parameters.DebugInput : _inputMove,
            _velocity,
            _transform.position, 
            _transform.rotation,
            _transform.up,
            _transform.forward,
            _transform.right,
            _standingCollider,
            Time.deltaTime);
    }
}
