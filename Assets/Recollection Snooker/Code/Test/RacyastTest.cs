using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SphereCastTest))]
public class SphereCastTest_Editor : Editor
{
    protected SphereCastTest _target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        _target = (SphereCastTest)target;
        if (GUILayout.Button("Project Sphere Cast"))
        {
            _target.SphereCast();
        }
    }

}
