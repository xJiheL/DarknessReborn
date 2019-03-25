using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    enum State
    {
        Grounded,
        Falling
    }
    
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float moveStep;
    
    [SerializeField]
    private float radius;

    [SerializeField]
    private float groundCheckDistance = 1f;
    
    private Vector3 _direction;

    private Vector2 _inputDirection;
    
    public Vector3 Direction => _direction;

    private Collider[] overlapCapsule;
    public Collider closestCollider;

    [SerializeField] private bool debugMode;
    [SerializeField] private Vector2 debugInput;

    private State currentState = State.Falling;
    
    private float verticalVelocity;

    [SerializeField] private float maxVerticalVelocity = 10f;

    [SerializeField] private float gravity = 9.81f;
    
    private void OnValidate()
    {
        if (moveStep < 0.01f)
        {
            moveStep = 0.01f;
        }

        // TODO le mouvement doit jamais être supérieur au radius pour éviter des problème de pénétration
        /*if (moveStep > radius)
        {
            moveStep = radius;
        }*/
    }

    private void Awake()
    {
        overlapCapsule = new Collider[16];
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

    private void Update()
    {
        switch (currentState)
        {
            case State.Grounded:
                UpdateGrounded();
                break;
            
            case State.Falling:
                UpdateFalling();
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }


        
    }

    private void UpdateGrounded()
    {
// TODO temp
        Vector3 inputDirection = !debugMode
            ? new Vector3(_inputDirection.x, 0f, _inputDirection.y)
            : new Vector3(debugInput.x, 0f, debugInput.y);

        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection = inputDirection.normalized;
        }

        if (!debugMode)
        {
            _direction = Quaternion.Euler(new Vector3(0f, Camera.main.transform.eulerAngles.y, 0f)) * inputDirection;
        }
        else
        {
            _direction = inputDirection;
        }

        Debug.DrawRay(transform.position, _direction * 2f, Color.green);

        Vector3 newPosition = transform.position;

        float distanceToTravel = moveSpeed * Time.deltaTime;

        while (distanceToTravel > 0)
        {
            float currentMoveStep = moveStep;

            if (distanceToTravel < moveStep)
            {
                currentMoveStep = distanceToTravel;
            }

            newPosition += UpdateController(newPosition, currentMoveStep);
            distanceToTravel -= currentMoveStep;
        }

        transform.position = newPosition;

        if (_direction.sqrMagnitude > 0.2f)
        {
            transform.forward = -_direction; // TODO ultra temp wtf
        }
    }

    private void UpdateFalling()
    {
        Vector3 basePos = transform.position;

        // TODO d'la bite
       /* Vector3 inputDirection = !debugMode
            ? new Vector3(_inputDirection.x, 0f, _inputDirection.y)
            : new Vector3(debugInput.x, 0f, debugInput.y);

        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection = inputDirection.normalized;
        }

        basePos += inputDirection * moveSpeed * Time.deltaTime;*/ // TODO handle that better
        
        
        verticalVelocity = Mathf.Clamp(verticalVelocity + gravity * Time.deltaTime, 0, maxVerticalVelocity);

        if (Physics.SphereCast(
            basePos,
            radius,
            -Vector3.up,
            out var hit,
            verticalVelocity,
            1 << LayerMask.NameToLayer("Ground"),
            QueryTriggerInteraction.Ignore))
        {
            verticalVelocity = hit.distance;

            GoToState(State.Grounded);

            DebugExt.DrawMarker(hit.point, 1f, Color.red, 10f);
        }

        Vector3 newPos = basePos - Vector3.up * verticalVelocity;

        transform.position = newPos;
    }

    private Vector3 UpdateController(Vector3 startPosition, float moveDistance)
    {
        /* ---------------- */

        Vector3 groundNormal = Vector3.up;

        /* ---------------- Ground Test ---------------- */

        {
            Vector3 capsuleBottom = startPosition + Vector3.up * radius;
            Vector3 vector3 = capsuleBottom - Vector3.up * groundCheckDistance;
            
            DebugExt.DrawWireSphere(capsuleBottom, radius, Color.white, transform.rotation);
            DebugExt.DrawWireSphere(vector3, radius, Color.blue, transform.rotation);

            int colliderTouched = Physics.OverlapCapsuleNonAlloc(
                capsuleBottom,
                vector3,
                radius,
                overlapCapsule,
                1 << LayerMask.NameToLayer("Ground"),
                QueryTriggerInteraction.Ignore);

            float minDistance = float.MaxValue;
            closestCollider = null; // TODO : handle when there are no colliders
            Vector3 closestPoint = Vector3.zero; // same

            for (int i = 0; i < colliderTouched; i++)
            {
                Vector3
                    newClosestPoint =
                        overlapCapsule[i]
                            .ClosestPoint(
                                capsuleBottom); // TODO check si le collider est bien compatible (donc tout sauf les mesh collider pas convex : https://docs.unity3d.com/ScriptReference/Physics.ClosestPoint.html)

                float distance = Vector3.Distance(newClosestPoint, capsuleBottom); // TODO use sqrMagnitude instead
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = overlapCapsule[i];
                    closestPoint = newClosestPoint;
                }
            }

            if (closestCollider == null)
            {
                GoToState(State.Falling);
            }

            groundNormal = (capsuleBottom - closestPoint).normalized;
            // TODO marche pas du coup !!!!!
            transform.position = closestPoint + groundNormal * radius - Vector3.up * radius; // ATTENTION A PAS LE POUSSER DANS UN AUTRE COLL

            DebugExt.DrawMarker(closestPoint, 1f, Color.red);

            Debug.DrawRay(closestPoint, groundNormal * 2f, Color.magenta);
        }

        /* ---------------- */

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
        Vector3 moveDirectionProject = rotation * _direction;

        Debug.DrawRay(transform.position, moveDirectionProject * 2f, Color.yellow);

        // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! je dois gérer la pénétration!

        return moveDirectionProject * moveDistance;


    }

    private void GoToState(State newState)
    {
        verticalVelocity = 0f;
        
        currentState = newState;
    }
}
