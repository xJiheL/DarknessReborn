using UnityEngine;
using System.Collections.Generic;

public class GizmosPosition : MonoBehaviour
{
    List<Vector3> allPositions = new List<Vector3>();
    Vector3 previousSavePosition;

    private void OnDrawGizmos ()
    {
        for (int i = 0; i < allPositions.Count; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(allPositions[i], 0.01f);
            
            if(i == 0)
            {
                continue;
            }

            Gizmos.DrawLine(allPositions[i-1], allPositions[i]);
        }
    }

    private void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            allPositions.Clear();
        }

        if (previousSavePosition != transform.position)
        {
            previousSavePosition = transform.position;
            allPositions.Add(transform.position);
            
            while (allPositions.Count > 300)
            {
                allPositions.RemoveAt(0);
            }
        }
    }
}
