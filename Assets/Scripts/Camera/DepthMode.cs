using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
public class DepthMode : MonoBehaviour
{
    [SerializeField]DepthTextureMode _depthMode;
    void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode = _depthMode;
    }
}
