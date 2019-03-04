using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController = null;
    
    [SerializeField]
    private Vector2 rotateSpeed = new Vector2(90f, -90f);

    [SerializeField]
    private float distanceToPlayer = 8f;
    
    [SerializeField]
    private float pivotOffsetDistance = 2f;
    
    [SerializeField]
    private float pivotOffsetSmoothTime = 0.6f;
    
    private Vector2 _currentRotation;

    private Vector3 _pivotOffset;
    private Vector3 _pivotOffsetVelocity;

    private bool _updateCamera;
    
    private void OnEnable()
    {
        Command.Instance.Add(EnumCommand.OrbitCamera, Rotate);
    }

    private void OnDisable()
    {
        // TODO isInstanciated
        //Command.Instance.Remove(EnumCommand.OrbitCamera);
    }

    private void Rotate(Vector2 inputValue) 
    {
        _currentRotation.x = Mathf.Clamp(
            _currentRotation.x - inputValue.y * (Command.Instance.joystickConnected ? Time.deltaTime * rotateSpeed.y : 1f),
            -90f,
            90f);
        
        _currentRotation.y = Mathf.Repeat(
            _currentRotation.y + inputValue.x * (Command.Instance.joystickConnected ? Time.deltaTime * rotateSpeed.x : 1f),
            360f);

        _updateCamera = true;
    }

    private void LateUpdate()
    {
        if (!_updateCamera)
        {
            return;
        }
        
        /* Position */
        
        _pivotOffset = Vector3.SmoothDamp(
            _pivotOffset, 
            playerController.Direction, 
            ref _pivotOffsetVelocity, 
            pivotOffsetSmoothTime, 
            float.MaxValue, 
            Time.fixedDeltaTime);
        
        /* Rotation */
        
        Quaternion cameraDirection = Quaternion.Euler(_currentRotation);
        Vector3 direction = cameraDirection * Vector3.back;

        /* End */
        
        Transform cameraTransform = transform;
        cameraTransform.position = playerController.transform.position + _pivotOffset * pivotOffsetDistance + direction * distanceToPlayer;
        cameraTransform.rotation = cameraDirection;
        
        _updateCamera = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerController.transform.position + playerController.Direction * pivotOffsetDistance, 0.25f);
    }
}
