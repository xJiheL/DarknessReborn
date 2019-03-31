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

        public void OnValidate()
        {
            if (moveStep < 0.01f)
            {
                moveStep = 0.01f;
            }

            // le mouvement doit jamais être supérieur au radius pour éviter des problème de pénétration
            if (moveStep > radius)
            {
                moveStep = radius;
            }
        }

        public float MoveSpeed => moveSpeed;

        public float Radius => radius;

        public float Height => height;

        public float MoveStep => moveStep;

        public float GroundCheckDistance => groundCheckDistance;

        public bool DebugMode => debugMode;

        public Vector2 DebugInput => debugInput;

        public float MinVerticalVelocity => minVerticalVelocity;

        public float Gravity => gravity;
    }

    [Serializable]
    public struct CurrentTransform
    {
        private Vector3 _direction;
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _up;
        private Vector3 _forward;
        private Vector3 _right;

        public CurrentTransform(Vector3 direction, Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right)
        {
            _direction = direction;
            _position = position;
            _rotation = rotation;
            _up = up;
            _forward = forward;
            _right = right;
        }

        public Vector3 Direction => _direction;

        public Vector3 Position => _position;

        public Quaternion Rotation => _rotation;

        public Vector3 Up => _up;

        public Vector3 Forward => _forward;

        public Vector3 Right => _right;
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
    
    public Vector3 Direction => _direction;

    
    private void OnValidate()
    {
        _parameters?.OnValidate();
    }

    private void Awake()
    {
        _transform = transform;
        
        _stateGrounded = new PlayerStateGrounded(transform);
        _stateFalling = new PlayerStateFalling(transform);
        _stateClimbing = new PlayerStateClimbing(transform);
        
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

        CurrentTransform currentTransform = new CurrentTransform(
            _direction, 
            _transform.position, 
            _transform.rotation,
            _transform.up,
            _transform.forward,
            _transform.right);
        
        Debug.DrawRay(currentTransform.Position, _direction * 2f, Color.green);
        
        _currentState.Update(_parameters, currentTransform);
    }

    private void GoToState(State newState)
    {
        if (_currentState != null)
        {
            _currentState.OnSetPosition -= OnSetPosition;
            _currentState.OnRequestState -= GoToState;
            
            _currentState.Exit();
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
        
        _currentState.Enter();
        
        _currentState.OnSetPosition += OnSetPosition;
        _currentState.OnRequestState += GoToState;
    }

    private void OnSetPosition(Vector3 position)
    {
        _transform.position = position;
    }
}
