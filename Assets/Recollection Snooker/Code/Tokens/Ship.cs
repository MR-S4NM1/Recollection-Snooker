using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MrSanmi.RecollectionSnooker
{
    #region Enums


    #endregion

    #region Structs


    #endregion

    public class Ship : Token
    {
        #region Knobs


        #endregion

        #region References

        #endregion

        #region RuntimeVariables

        [SerializeField] protected Cargo[] _cargos;
        [SerializeField] public List<Cargo> _cargoesLoadedOnTheShip;
        //List<float> _cargosPos; //classes are normally instanciated at the Start / Awake method
        Cargo _nearestCargo;
        float _nearestDistance;
        float _currentDistance;
        int _indexNearestDistance;

        #endregion

        #region UnityMethods

        void Start()
        {
            base.InitializeToken();
            _cargoesLoadedOnTheShip = new List<Cargo>();
            //We obtain all cargo in scene in order to know which one is the nearest, and then save them in an array.
            _cargos = GameObject.FindObjectsOfType<Cargo>(true);
            //_cargosPos = new List<float>();  //runtime variable
        }

        private void OnCollisionEnter(Collision other)
        {
            switch (_gameReferee.GameState)
            {
                case RS_GameStates.CANNON_CARGO:
                    ValidateShipHasTouchedACargoDuringCannonCollision(other);
                    break;
            }
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
                case RS_GameStates.CANNON_CARGO:
                    ValidateShipHasTouchedACargoDuringCannon(other);
                    break;
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    ValidateTriggerCargoDuringLoadingCargo(other);
                    break;
            }
        }

        #endregion

        #region RuntimeMethods

        void ValidateTriggerCargoDuringLoadingCargo(Collider other)
        {
            if (other.CompareTag("Cargo") && (!other.gameObject.GetComponent<Cargo>().IsLoadedOnTheShip) &&
                (!_cargoesLoadedOnTheShip.Contains(other.gameObject.GetComponent<Cargo>())))
            {
                _cargoesLoadedOnTheShip.Add(other.gameObject.GetComponent<Cargo>());
            }
        }

        void ValidateShipHasTouchedACargoDuringCannon(Collider other)
        {
            if (other.CompareTag("Cargo") && !other.gameObject.GetComponent<Cargo>().IsLoadedOnTheShip &&
                (!_cargoesLoadedOnTheShip.Contains(other.gameObject.GetComponent<Cargo>())))
            {
                _gameReferee._aCargoHasTouchedTheShip = true;
            }
        }

        void ValidateShipHasTouchedACargoDuringCannonCollision(Collision other)
        {
            if (other.gameObject.CompareTag("Cargo") && !other.gameObject.GetComponent<Cargo>().IsLoadedOnTheShip &&
                (!_cargoesLoadedOnTheShip.Contains(other.gameObject.GetComponent<Cargo>())))
            {
                _gameReferee._aCargoHasTouchedTheShip = true;
            }
        }


        #endregion

        #region PublicMethods

        public Cargo NearestCargo()
        {
            //_cargosPos.Clear(); //start with a clean slate
            _nearestDistance = Mathf.Infinity;

            //We calculate the distance between all cargo and the ship.
            for (int i = 0; i < _cargos.Length - 1; ++i)
            {
                if (!_cargos[i].IsLoadedOnTheShip || !_cargos[i]._isLoadedOnTheIsland)
                {
                    _currentDistance = Vector3.SqrMagnitude(this.gameObject.transform.position - _cargos[i].gameObject.transform.position);
                    //_cargosPos.Add(_currentDistance);
                    if (_currentDistance < _nearestDistance)
                    {
                        _nearestDistance = _currentDistance;
                        //we found a possible candidate
                        _indexNearestDistance = i;
                    }
                }
            }

            _nearestCargo = _cargos[_indexNearestDistance];

            //We obtain the nearest distance among the distances.
            //_nearestDistance = _cargosPos.Min();

            //We return the nearest cargo in order to deactivate it by the Game Referee.
            //for (int i = 0; i < _cargos.Length - 1; ++i)
            //{
            //    if (Vector3.Distance(this.gameObject.transform.position, _cargos[i].transform.position) <= _nearestDistance + 0.1f)
            //    {
            //        _nearestCargo = _cargos[i];
            //    }
            //}

            //_cargosPos.Clear();

            return _nearestCargo;
        }

        #endregion

        #region GettersSetters

        #endregion
    }
}