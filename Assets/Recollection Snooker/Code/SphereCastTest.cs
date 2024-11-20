using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCastTest : MonoBehaviour
{
    protected RaycastHit _raycastHit;

    public void SphereCast()
    {
        Debug.DrawRay(transform.position, -transform.up * 1.0f, Color.red, 1f);
        if (Physics.SphereCast(transform.position, 0.5f, -transform.up, out _raycastHit, 1.0f))
        {
            Debug.LogWarning("Hitted with the Sphere Cast " + _raycastHit.collider.gameObject.name);
            //Debug.Break(); //Pause on the editor
        }
        else
        {
            Debug.LogWarning("Hitted NOTHING");
        }
    }
}

