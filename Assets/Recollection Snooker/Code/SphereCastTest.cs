using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCastTest : MonoBehaviour
{
    protected RaycastHit _raycastHit;

    public void SphereCast()
    {
        if(Physics.SphereCast(transform.position, 1.0f, Vector3.zero, out _raycastHit))
        {
            Debug.LogWarning("Hit with the Sphere cast");
            Debug.Break();
        }
    }
}
