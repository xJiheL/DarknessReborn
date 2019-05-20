using UnityEngine;

public class TestPenetration : MonoBehaviour
{
    private CapsuleCollider capsule;

    private int iterationMax = 10;

    public float radius = 0.7f;
    public float height = 3f;
    
    void OnDrawGizmos()
    {
        Collider[] results = new Collider[16];
        capsule = GetComponent<CapsuleCollider>();

        Debug.Assert(capsule.center.Equals(Vector3.zero), "Incorrect capsule center");
        Debug.Assert(capsule.radius.Equals(radius), "Incorrect capsule radius");
        Debug.Assert(capsule.height.Equals(height), "Incorrect capsule height");
        Debug.Assert(capsule.direction == 1, "Incorrect capsule direction"); // Y-Axis
        
        Vector3 bottom = transform.position - Vector3.up * (height / 2f - radius);  // TODO Get Capsule Bottom
        Vector3 top = transform.position + Vector3.up * (height / 2f - radius);     // TODO Get Capsule Top

        Vector3 finalBottom = bottom;
        Vector3 finalTop = top;
        
        DebugExt.DrawWireCapsule(finalBottom, finalTop, capsule.radius, Color.blue);

        int resultsNumber = Physics.OverlapCapsuleNonAlloc(
            finalBottom, 
            finalTop, 
            radius, 
            results, 
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);
        
        Vector3 translation = Vector3.zero;
        int iteration = 0;
        
        while (resultsNumber > 0 && iteration < iterationMax)
        {
            if (Physics.ComputePenetration(
                capsule,
                capsule.transform.position + translation,
                capsule.transform.rotation,
                results[0],
                results[0].transform.position,
                results[0].transform.rotation,
                out var direction,
                out var distance))
            {
                distance += Physics.defaultContactOffset;
                translation += direction * distance;
            }
            else
            {
                // possible with floating value...
                translation += Vector3.up * Physics.defaultContactOffset;
                Debug.LogWarning("overlap but no penetration");
            }
            
            DebugExt.DrawWireSphere(finalBottom, 0.05f, Color.red, Quaternion.identity);
            DebugExt.DrawWireSphere(finalBottom + translation, 0.05f, Color.red, Quaternion.identity);
            Debug.DrawLine(finalBottom, finalBottom + translation, Color.red);
            
            finalBottom = bottom + translation;
            finalTop = top + translation;
            
            resultsNumber = Physics.OverlapCapsuleNonAlloc(
                finalBottom, 
                finalTop, 
                capsule.radius, 
                results, 
                PlayerController.GetGroundMask(),
                QueryTriggerInteraction.Ignore);

            iteration++;
        }
        
        DebugExt.DrawWireCapsule(finalBottom, finalTop, capsule.radius, iteration == iterationMax ? Color.red : Color.green);
        GizmosExt.DrawText("Iteration "+iteration, finalTop);
        
        GroundCheck(finalBottom);
    }

    private void GroundCheck(Vector3 Bottom)
    {
        Collider[] results = new Collider[16];
        
        int resultsNumber = Physics.OverlapSphereNonAlloc(
            Bottom,
            capsule.radius,
            results,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore);

        DebugExt.DrawWireSphere(
            Bottom, 
            capsule.radius, 
            resultsNumber == 0 ? Color.yellow : Color.magenta,
            Quaternion.identity);

        if (resultsNumber != 0)
        {
            // assert
            return;
        }

        if (Physics.SphereCast(
            Bottom,
            capsule.radius - Physics.defaultContactOffset,
            -Vector3.up,
            out var hitInfo,
            float.MaxValue,
            PlayerController.GetGroundMask(),
            QueryTriggerInteraction.Ignore))
        {
            DebugExt.DrawMarker(hitInfo.point, 1f, Color.yellow);
            Debug.DrawRay(hitInfo.point, hitInfo.normal * 2f, Color.yellow);
        }
        else
        {
            Debug.LogError("No normal !");
        }
    }
}