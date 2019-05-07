using UnityEngine;

public class RotateTest : MonoBehaviour
{
    public float radius;
    public float h;

    public Vector3 rotation;
    public Vector3 test;
    
    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        
        Vector3 bottom = pos + Vector3.up * radius;
        Vector3 top = pos + Vector3.up * (h - radius);
        
        DebugExt.DrawMarker(pos, 1f, Color.blue);
        
        DebugExt.DrawWireCapsule(
            bottom,
            top, 
            radius,
            Color.blue,
            Quaternion.identity);

        Quaternion rot = Quaternion.Euler(rotation);
        
        Vector3 newPos = bottom - rot * Vector3.up * radius;
        
        Vector3 newBottom = newPos + rot * Vector3.up * radius;
        Vector3 newTop = newPos + rot * Vector3.up * (h - radius);
        
        DebugExt.DrawMarker(newPos, 1f, Color.red);
        
        DebugExt.DrawWireCapsule(
            test,
            newTop, 
            radius,
            Color.red,
            Quaternion.identity);
    }
}
