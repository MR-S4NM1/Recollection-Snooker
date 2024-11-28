using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrSanmi.RecollectionSnooker
{
    #region Enums

    public enum CargoTypes
    {
        CREW_MEMBER = 4,
        FUEL = 3,
        MEDICINE = 2,
        SUPPLIES = 1,
        SCREW_PART = 0
        //SCREW_PART,
        //SUPPLIES,
        //MEDICINE,
        //FUEL,
        //CREW_MEMBER
    }

    #endregion

    #region Structs


    #endregion

    public class Cargo : Token
    {
        #region Knobs

        [Header("Knobs / Parameters")]
        public CargoTypes cargoType;

        #endregion

        #region References

        #endregion

        #region RuntimeVariables

        [Header("Runtime Variables")]
        [SerializeField] protected bool _isLoadedOnTheShip;
        [SerializeField] public bool _isLoadedOnTheIsland;

        #endregion

        #region UnityMethods

        void Start()
        {
            base.InitializeToken();
            _isLoadedOnTheIsland = false;
            _isLoadedOnTheShip = false;
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

        private void OnCollisionStay(Collision other)
        {
            ValidateCollisionsByCargo(other);
        }

        private void OnTriggerEnter(Collider other)
        {
            //base.OnTriggerEnterEvent(other);
            OnTriggerEvent(other);
            //Validation exclusively for the CARGO
            switch (_gameReferee.GameState)
            {
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    if (_gameReferee._aCargoHasTouchedTheShip)
                    {
                        ValidateCargoHasBeenLoadedOnShip(other);
                    }
                    //else if (_gameReferee._shipPivotHasTouchedTheIsland)
                    //{
                    //    ValidateCargoHasBeenLoadedOnIsland(other);
                    //}
                    break;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            switch (_gameReferee.GameState)
            {
                case RS_GameStates.CANNON_CARGO:
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateCollisionsByCargo(other);
                    break;
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    if (_gameReferee._aCargoHasTouchedTheShip)
                    {
                        ValidateCollisionsByLoadingCargoOnShip(other);
                    }
                    //else if (_gameReferee._shipPivotHasTouchedTheIsland)
                    //{
                    //    ValidateCollisionsByLoadingCargoOnIsland(other);
                    //}
                    break;
            }
        }



        #endregion

        #region RuntimeMethods

        //Ship
        protected void ValidateCollisionsByCargo(Collision other)
        {
            if (other.gameObject.CompareTag("Ship") && !_isLoadedOnTheShip)
            {
                _gameReferee.CargoToBeLoaded = this;
            }
            if (_isLoadedOnTheShip)
            {
                if (other.gameObject.CompareTag("Floor"))
                {
                    _gameReferee.DeactivateIsLoadedOnTheShipForAllCargoesOfTheSameType(this.cargoType);
                    _gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Remove(this);
                }
            }
        }

        protected void ValidateCollisionsByLoadingCargoOnShip(Collision other)
        {
            if (other.gameObject.CompareTag("CargoSpace") && !_isLoadedOnTheShip)
            {
                print("I have been loaded!");
                _gameReferee.ActivateIsLoadedOnTheShipForAllCargoesOfTheSameType(this.cargoType);
            }
        }

        protected void ValidateCargoHasBeenLoadedOnShip(Collider other)
        {
            if (_isLoadedOnTheShip) //If it's loaded, DON'T LOAD IT!
            {
                return;
            }

            if (other.gameObject.CompareTag("CargoSpace"))
            {
                _isLoadedOnTheShip = true; //Set cargo as loaded
                _gameplayAttributes.isAvailableForFlicking = false;
                _gameReferee.ActivateIsLoadedOnTheShipForAllCargoesOfTheSameType(this.cargoType);

                //Add cargo to the list
                if (!_gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Contains(this))
                {
                    _gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Add(this);
                }
            }
        }

        //Island
        //protected void ValidateCollisionsByLoadingCargoOnIsland(Collision other)
        //{
        //    if (_isLoadedOnTheIsland) //If it's loaded, DON'T LOAD IT!
        //    {
        //        return;
        //    }

        //    if (other.gameObject.CompareTag("IslandCargoSpace"))
        //    {

        //        _isLoadedOnTheShip = false;
        //        _isLoadedOnTheIsland = true;
        //        _gameplayAttributes.isAvailableForFlicking = false;
        //        _gameReferee.ActivateIsLoadedOnTheIslandForAllCargoesOfTheSameType(this.cargoType);
        //        _gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Remove(this);

        //        Add cargo to the list
        //        if (!_gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Contains(this))
        //        {
        //            _gameReferee.islandOfTheGame._cargoesLoadedOnIsland.Add(this);
        //        }
        //    }
        //}

        //protected void ValidateCargoHasBeenLoadedOnIsland(Collider other)
        //{
        //    if (_isLoadedOnTheIsland) //If it's loaded, DON'T LOAD IT!
        //    {
        //        return;
        //    }

        //    if (other.gameObject.CompareTag("IslandCargoSpace"))
        //    {

        //        _isLoadedOnTheShip = false;
        //        _isLoadedOnTheIsland = true;

        //        _gameReferee.ActivateIsLoadedOnTheIslandForAllCargoesOfTheSameType(this.cargoType);
        //        _gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Remove(this);

        //        Add cargo to the list
        //        if (!_gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Contains(this))
        //        {
        //            _gameReferee.islandOfTheGame._cargoesLoadedOnIsland.Add(this);
        //        }
        //    }
        //}


        #endregion

        #region PublicMethods

        #endregion

        #region GettersSetters

        public bool IsLoadedOnTheShip
        {
            get { return _isLoadedOnTheShip; }
            set { _isLoadedOnTheShip = value; }
        }

        #endregion
    }
}