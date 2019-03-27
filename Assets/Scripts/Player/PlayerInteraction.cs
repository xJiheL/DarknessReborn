using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform positionLightweightObject;
    [SerializeField] private float _maxDistance = 2f;
    [SerializeField] private bool _debug;

    private Transform _player;
    private Transform _targetObject;
    private bool _grabbed;
    private bool _lightWeight;
    private float _originalSpeed;
    private Coroutine _co;
    private Vector3 _oldPosPlayer;

    private void Start()
    {
        _player = transform;
    }
    private void Update()
    {
        if (Input.GetButtonDown("B_Xbox"))
        {
            if (_grabbed)
            {
                FreeObject();
            }
            else
            {
                DetectObject();  
            }
        }
    }

    private void DetectObject()
    {
        var hit = new RaycastHit();
        var ray = new Ray(_player.position, -_player.forward);

        if (_debug)
        {
            Debug.DrawRay(_player.position,-_player.forward * _maxDistance, Color.red);
        }
       
        if (Physics.Raycast(ray,out hit, _maxDistance))
        {
            var target = hit.transform;
            switch (target.tag)
            {
                case "HeavyObject":
                    _grabbed = true;
                    DetectedHeavy(target);
                    break;
                
                case "LightweightObject":
                    _grabbed = true;
                    _lightWeight = true;
                    DetectedLightweight(target);
                    break;
                
                default:
                    if (_debug)
                    {
                        Debug.Log(string.Format( "Object grab not supported on {0}", hit.transform.name));
                    }
                    break;
            }
        }
    }

    private void DetectedLightweight(Transform target)
    {
        _targetObject = target;
        _targetObject.transform.parent = positionLightweightObject;
        _targetObject.localPosition = Vector3.zero;
        _targetObject.localEulerAngles = Vector3.zero;
        
        var rigidBody = _targetObject.GetComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
    }
    
    private void DetectedHeavy(Transform target)
    {
        _targetObject = target;

        var playerController = GetComponent<PlayerController>();
        _originalSpeed = playerController.MoveSpeed;
        playerController.MoveSpeed = 1f;
        _oldPosPlayer = _player.position;
        _co = StartCoroutine(MoveHeavyObject());
    }

    private void FreeObject()
    {
        _grabbed = false;

        if (_lightWeight)
        {
            var rigidBody = _targetObject.GetComponent<Rigidbody>();
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
            _lightWeight = false;
        }
        else
        {
            var playerController = GetComponent<PlayerController>();
            playerController.MoveSpeed = _originalSpeed;
            StopCoroutine(_co);
        }

        _targetObject.parent = null;
        _targetObject = null;
    }

    private IEnumerator MoveHeavyObject()
    {
        while (true)
        {
            var direction = _player.position - _oldPosPlayer;
            _targetObject.position += direction ;
            _oldPosPlayer = _player.position;
            yield return null;
        }
    }
}
