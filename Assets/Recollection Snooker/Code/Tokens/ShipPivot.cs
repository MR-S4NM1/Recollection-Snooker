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

            switch (_gameReferee.GameState)
            {
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER:
                    ValidateTriggerWithFlag(other);
                    break;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            switch (_gameReferee.GameState)
            {
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateCollisionOfTheIslandByShipPivot(other);
                    break;
            }
        }

        private void OnCollisionStay(Collision other)
        {
            switch (_gameReferee.GameState)
            {
                case RS_GameStates.CANNON_CARGO:
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateCollisionDuringCannon(other);
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            ValidateReferences();
            #endif
        }

        #endregion

        #region RuntimeMethods

        protected void ValidateCollisionOfTheIslandByShipPivot(Collision other)
        {
            if (other.gameObject.CompareTag("Island") && _gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Count > 0)
            {
                _gameReferee._shipPivotHasTouchedTheIsland = true;
            }
        }

        #endregion

        #region PublicMethods


        #endregion

        #region GettersSetters

        #endregion
    }
}