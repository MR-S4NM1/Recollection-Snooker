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
        [SerializeField] protected bool _isLoaded;

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

        private void OnCollisionStay(Collision other)
        {
            ValidateCollisionsByCargo(other);
        }

        private void OnTriggerStay(Collider other)
        {
            switch (_gameReferee.GetGameState)
            {
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    ValidateCargoHasBeenLoaded(other);
                    break;
            }
            
        }

        private void OnCollisionEnter(Collision other)
        {
            switch (_gameReferee.GetGameState)
            {
                case RS_GameStates.CANNON_CARGO:
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateCollisionsByCargo(other);
                    break;
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    ValidateCollisionsByLoadingCargo(other);
                    break;
            }
        }



        #endregion

        #region RuntimeMethods

        protected void ValidateCollisionsByCargo(Collision other)
        {
            if (other.gameObject.CompareTag("Ship") && !_isLoaded)
            {
                _gameReferee.CargoToBeLoaded = this;
            }
            if (_isLoaded)
            {
                if (other.gameObject.CompareTag("Floor"))
                {
                    _gameReferee.DeactivateIsLoadedForAllCargoesOfTheSameType(this.cargoType);
                }
            }
        }

        protected void ValidateCollisionsByLoadingCargo(Collision other)
        {
            if (other.gameObject.CompareTag("CargoSpace") && !_isLoaded)
            {
                print("I have been loaded!");
                _gameReferee.ActivateIsLoadedForAllCargoesOfTheSameType(this.cargoType);
            }
        }

        protected void ValidateCargoHasBeenLoaded(Collider other)
        {
            if (_isLoaded) //If it's loaded, DON'T LOAD IT!
            {
                return;
            }

            if (other.gameObject.CompareTag("CargoSpace"))
            {
                print("I have been loaded!");
                _isLoaded = true; //Set cargo as loaded
                _gameReferee.ActivateIsLoadedForAllCargoesOfTheSameType(this.cargoType); // Sincroniza con el árbitro.

                //Add cargo to the list
                if (!_gameReferee.shipOfTheGame._cargoesLoaded.Contains(this))
                {
                    _gameReferee.shipOfTheGame._cargoesLoaded.Add(this);
                }
            }
        }


        #endregion

        #region PublicMethods

        #endregion

        #region GettersSetters

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; }
        }

        #endregion
    }
}