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

        void Update()
        {

        }

        //private void OnCollisionEnter(Collision other)
        //{
        //    ValidateCollision(other);
        //}

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            ValidateReferences();
            #endif
        }

        private void OnCollisionEnter(Collision other)
        {
            switch (_gameReferee.GetGameState)
            {
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateCollisionDuringCannonByNavigation(other);
                    break;
            }
        }

        #endregion

        #region RuntimeMethods

        protected void ValidateCollisionDuringCannonByNavigation(Collision other)
        {
            if (other.gameObject.CompareTag("Island"))
            {
                _gameReferee.SetShipPivotHasTouchedTheIsland = true;
            }
        }


        #endregion

        #region PublicMethods

        #endregion

        #region GettersSetters

        #endregion
    }
}