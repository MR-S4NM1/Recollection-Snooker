using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrSanmi.RecollectionSnooker
{
    #region Enums


    #endregion

    #region Structs


    #endregion

    public class ShipPivot : Token
    {
        #region Knobs


        #endregion

        #region References

        #endregion

        #region RuntimeVariables

        #endregion

        #region UnityMethods

        void Start()
        {
            base.InitializeToken();
        }

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEvent(other);
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            ValidateReferences();
            #endif
        }

        #endregion

        #region RuntimeMethods

        #endregion

        #region PublicMethods

        #endregion

        #region GettersSetters

        #endregion
    }
}