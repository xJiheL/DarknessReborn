using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanciateEvent : MonoBehaviour
{
    public void SpawnObject(GameObject obj)
    {
        GameObject clone = Instantiate(obj, this.transform);
        clone.transform.localPosition = Vector3.zero;
    }
}
