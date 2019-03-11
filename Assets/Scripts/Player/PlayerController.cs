using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float radius;

    [SerializeField]
    private float groundCheckDistance = 1f;
    
    private Vector3 _direction;

    private Vector2 _inputDirection;
    
    public Vector3 Direction => _direction;

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
        Debug.DrawRay(transform.position, _direction * 2f, Color.green);
        
        /* ---------------- */

        Vector3 groundNormal = Vector3.up;
        Vector3 groundPos = Vector3.up;
        
        /* ---------------- */

        {
            Vector3 capsuleBottom = GetCapsuleBottom();

            Collider[] overlapCapsule = Physics.OverlapCapsule(
                capsuleBottom,
                capsuleBottom - Vector3.up * groundCheckDistance,
                radius/*,
                LayerMask.NameToLayer("Ground"), // TODO WTF? pourquoi ca marche pas?
                QueryTriggerInteraction.Ignore*/);

            float minDistance = float.MaxValue;
            Collider closestCollider = null; // TODO : handle when there are no colliders
            Vector3 closestPoint = Vector3.zero; // same
            
            foreach (Collider coll in overlapCapsule)
            {
                Vector3 newClosestPoint = coll.ClosestPoint(capsuleBottom);

                float distance = Vector3.Distance(newClosestPoint, capsuleBottom); // TODO use sqrMagnitude instead
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = coll;
                    closestPoint = newClosestPoint;
                }
            }

            if (closestCollider == null)
            {
                Debug.LogError("NO COLLIDER FOR GROUND");
            }

            groundNormal = (capsuleBottom - closestPoint).normalized;
            transform.position = closestPoint + groundNormal * radius - Vector3.up * radius; // ATTENTION A PAS LE POUSSER DANS UN AUTRE COLL
            
            Debug.DrawRay(closestPoint, groundNormal * 2f, Color.magenta);
        }

        /* ---------------- */
        
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
        Vector3 moveDirectionProject = rotation * _direction;
        
        Debug.DrawRay(transform.position, moveDirectionProject * 2f, Color.yellow);
        
        transform.position += moveDirectionProject * moveSpeed * Time.deltaTime; // le mouvement doit jamais être supérieur au radius pour éviter des problème de pénétration

        if (_direction.sqrMagnitude > 0.2f)
        {
            transform.forward = -_direction; // TODO ultra temp wtf
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GetCapsuleBottom(), radius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(GetCapsuleBottom() - Vector3.up * groundCheckDistance, radius);
    }

    private Vector3 GetCapsuleBottom()
    {
        return transform.position + Vector3.up * radius;
    }
}
