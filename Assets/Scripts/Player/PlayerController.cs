using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum State
    {
        Grounded,
        Falling,
        Climbing
    }

    [Serializable]
    public class Parameters
    {
        [SerializeField]
        private float moveSpeed = 5f;
        
        [SerializeField]
        private float radius;
        
        [SerializeField]
        private float height;
        
        [SerializeField]
        private float moveStep;
        
        [SerializeField]
        private float groundCheckDistance = 1f;
        
        [SerializeField]
        private bool debugMode;
        
        [SerializeField]
        private Vector2 debugInput;

        [SerializeField]
        private float gravity = -9.81f;
        
        [SerializeField]
        private float minVerticalVelocity = -30f;

        [SerializeField]
        private float groundLimit = 45f;

        [SerializeField]
        private float climbLimit = 150f;
        
        public void OnValidate()
        {
            if (moveSpeed < 0f)
            {
                moveSpeed = 0f;
            }
            
            if (radius < 0f)
            {
                radius = 0f;
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

    [Serializable]
    public struct CurrentTransform
    {
        private Vector3 _direction;
        private Vector2 _inputMove;
        private Vector3 _velocity;
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _up;
        private Vector3 _forward;
        private Vector3 _right;
        private Collider _standingCollider;

        public CurrentTransform(Vector3 direction, Vector2 inputMove, Vector3 velocity, Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right, Collider standingCollider)
        {
            _direction = direction;
            _inputMove = inputMove;
            _velocity = velocity;
            _position = position;
            _rotation = rotation;
            _up = up;
            _forward = forward;
            _right = right;
            _standingCollider = standingCollider;
        }

        public Vector3 Direction => _direction;

        public Vector2 InputMove => _inputMove;

        public Vector3 Velocity => _velocity;

        public Vector3 Position => _position;

        public Quaternion Rotation => _rotation;

        public Vector3 Up => _up;

        public Vector3 Forward => _forward;

        public Vector3 Right => _right;

        public Collider StandingCollider => _standingCollider;
    }

    [SerializeField]
    private Parameters _parameters = null;
    
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
        
        GoToState(State.Falling);
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
        
        _currentState.Update(_parameters, currentTransform);
        
        DebugExt.DrawWireCapsule(
            _parameters.GetCapsuleBottom(_transform.position, currentTransform.Up), 
            _parameters.GetCapsuleTop(_transform.position, currentTransform.Up),
            _parameters.Radius, Color.cyan, Quaternion.identity);
        
        Debug.DrawRay(transform.position, _velocity, Color.cyan);
    }

    private void GoToState(State newState)
    {
        if (_currentState != null)
        {
            _currentState.Exit();
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
        _currentState.Enter(_parameters, GetCurrentTransform());
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
            _standingCollider);
    }
}
