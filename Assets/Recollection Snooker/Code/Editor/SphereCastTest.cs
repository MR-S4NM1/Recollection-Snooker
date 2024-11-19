using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SphereCastTest))]
public class SphereCastEditor : Editor
{
    protected SphereCastTest target;

    void OnInspectorGUI()
    {
        target = (SphereCastTest)target;
        //if(GUILayout.Button)
    }
}
