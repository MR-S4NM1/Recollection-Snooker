using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
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

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEvent(other);
            switch (_gameReferee.GetGameState)
            {
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateIslandHasTouchedTheShipPivotDuringCannonByNavigationTrigger(other);
                    break;
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    ValidateTriggerCargoDuringLoadingCargo(other);
                    break;
            }
        }

        #endregion

        #region RuntimeMethods

        protected void ValidateTriggerCargoDuringLoadingCargo(Collider other)
        {
            if (other.CompareTag("Cargo") && (!other.gameObject.GetComponent<Cargo>().IsLoadedOnTheShip) &&
                (!_cargoesLoadedOnIsland.Contains(other.gameObject.GetComponent<Cargo>())))
            {
                _cargoesLoadedOnIsland.Add(other.gameObject.GetComponent<Cargo>());
            }
        }

        protected void ValidateIslandHasTouchedTheShipPivotDuringCannonByNavigationTrigger(Collider other)
        {
            if (other.CompareTag("ShipPivot") && (_gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Count > 0))
            {
                _gameReferee._shipPivotHasTouchedTheIsland = true;
            }
        }


        #endregion

        #region PublicMethods

        public bool CheckIsATokenFromTheShipIsNotOnTheIsland()
        {
            _cargoIsNotOnIsland = false;

            foreach (Cargo cargo in _gameReferee.shipOfTheGame._cargoesLoadedOnTheShip)
            {
                if (!_cargoesLoadedOnIsland.Contains(cargo))
                {
                    _cargoIsNotOnIsland = true;
                    break;
                }
            }
            return _cargoIsNotOnIsland;
        }

        #endregion

        #region GettersSetters

        #endregion
    }
}
