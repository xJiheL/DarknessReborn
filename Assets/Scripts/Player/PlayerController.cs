using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;
    
    private Rigidbody _rb;

    private Vector3 _direction;

    private Vector2 _inputDirection;
    
    public Vector3 Direction => _direction;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        Command.Instance.Add(EnumCommand.MoveCharacter, MovePlayer);
        Command.Instance.Add(EnumCommand.Jump, Jump);
    }

    private void OnDisable()
    {
        // TODO isInstanciated
        //Command.Instance.Remove(EnumCommand.MoveCharacter);
        //Command.Instance.Remove(EnumCommand.Jump);
    }
    
    private void MovePlayer(Vector2 inputMove)
    {
        _inputDirection = inputMove;
    }
    
    private void Jump()
    {
        Debug.Log("jump soon!");
    }

    private void FixedUpdate()
    {
        // TODO temp
        Vector3 inputDirection = new Vector3(
            _inputDirection.x,
            0f,
            _inputDirection.y);

        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection = inputDirection.normalized;
        }

        _direction = Quaternion.Euler(new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f)) * inputDirection;
        
        _rb.velocity = _direction * moveSpeed;

        if (_direction.sqrMagnitude > 0.2f)
        {
            transform.forward = -_direction; // TODO ultra temp wtf
        }
        
    }
}
