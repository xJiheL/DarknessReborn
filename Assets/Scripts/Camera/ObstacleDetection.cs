using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO : need improvment to get the correct material back and not instanced material
public class ObstacleDetection : MonoBehaviour
{
    [SerializeField] Texture _dissolveMask;
    [Range (0f,1f)]
    [SerializeField] float _cutout = .5f;

    private int _layerMask = 1 << 16;

    private string _dissolveName = "DarknessReborn/DissolveObstacle";
    private string _standardName = "DarknessReborn/Standard";

    private Shader _dissolveShader;
    private Shader _standardShader;

    void Start ()
    {
        _dissolveShader = Shader.Find(_dissolveName);
        _standardShader = Shader.Find(_standardName);
    }

    private bool CheckLayerMask(int layer)
    {
        return _layerMask == (_layerMask | (1 << layer));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CheckLayerMask(other.gameObject.layer))
        {
            CheckRenderer(other, _dissolveShader);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (CheckLayerMask (other.gameObject.layer))
        {
            CheckRenderer(other, _standardShader);
        }
    }

    private void CheckRenderer (Collider other, Shader shader)
    {
        MeshRenderer[] rend = other.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in rend)
        {
            SwitchShader(r, shader);
        }
    }

    private void SwitchShader(MeshRenderer meshRenderer, Shader shader)
    {
        Material[] mat = meshRenderer.materials;
        foreach (Material m in mat)
        {
            m.shader = shader;
            if (shader == _dissolveShader)
            {
                m.SetTexture("_DissolveTex", _dissolveMask);
                m.SetFloat("_Cutout", _cutout);
            }
        }
        meshRenderer.materials = mat;
    }
}
