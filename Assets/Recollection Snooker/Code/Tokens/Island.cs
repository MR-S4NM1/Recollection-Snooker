using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MrSanmi.RecollectionSnooker
{
    public class Island : Token
    {
        #region Knobs


        #endregion

        #region References

        #endregion

        #region RuntimeVariables

        [SerializeField] protected Cargo[] _cargos;
        [SerializeField] public List<Cargo> _cargoesLoadedOnIsland;
        protected bool _cargoIsNotOnIsland;
        //List<float> _cargosPos; //classes are normally instanciated at the Start / Awake method

        #endregion

        #region UnityMethods

        void Start()
        {
            base.InitializeToken();
            _cargoesLoadedOnIsland = new List<Cargo>();
            _cargos = GameObject.FindObjectsOfType<Cargo>(true);
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            ValidateReferences();
            #endif
        }

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEvent(other);
            switch (_gameReferee.GameState)
            {
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateIslandHasTouchedTheShipPivotDuringCannonByNavigationTrigger(other);
                    break;
                //case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                //    ValidateTriggerCargoDuringLoadingCargo(other);
                //    break;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            switch (_gameReferee.GameState)
            {
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateIslandHasTouchedTheShipPivotDuringCannonByNavigationCollision(other);
                    break;
            }
        }

        #endregion

        #region RuntimeMethods

        //protected void ValidateTriggerCargoDuringLoadingCargo(Collider other)
        //{
        //    if (other.CompareTag("Cargo") && (!other.gameObject.GetComponent<Cargo>().IsLoadedOnTheShip) &&
        //        (!_cargoesLoadedOnIsland.Contains(other.gameObject.GetComponent<Cargo>())))
        //    {
        //        _cargoesLoadedOnIsland.Add(other.gameObject.GetComponent<Cargo>());
        //    }
        //}

        protected void ValidateIslandHasTouchedTheShipPivotDuringCannonByNavigationTrigger(Collider other)
        {
            if (other.CompareTag("ShipPivot") && (_gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Count > 0))
            {
                _gameReferee._shipPivotHasTouchedTheIsland = true;
            }
        }

        protected void ValidateIslandHasTouchedTheShipPivotDuringCannonByNavigationCollision(Collision other)
        {
            if (other.gameObject.CompareTag("ShipPivot") && (_gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Count > 0))
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
