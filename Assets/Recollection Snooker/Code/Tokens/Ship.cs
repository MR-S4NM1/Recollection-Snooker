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
        [SerializeField] public List<Cargo> _cargoesLoaded;
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
            _cargoesLoaded = new List<Cargo>();
            //We obtain all cargo in scene in order to know which one is the nearest, and then save them in an array.
            _cargos = GameObject.FindObjectsOfType<Cargo>(true);
            //_cargosPos = new List<float>();  //runtime variable
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

        //private void OnCollisionEnter(Collision other)
        //{
        //    ValidateCollisionCargo(other);
        //}

        //private void OnCollisionStay(Collision other)
        //{
        //    ValidateCollisionCargo(other);
        //}

        private void OnTriggerEnter(Collider other)
        {
            ValidateTriggerCargo(other);
        }

        #endregion

        #region RuntimeMethods

        //void ValidateCollisionCargo(Collision other)
        //{
        //    if (other.collider.CompareTag("Cargo"))
        //    {
        //        switch (_gameReferee.GetGameState)
        //        {
        //            case RS_GameStates.CANNON_CARGO:
        //                _gameReferee.SetACargoHasTouchedTheShip = true;
        //                break;
        //            case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
        //            case RS_GameStates.SHIFT_MONSTER_PARTS:
        //                if ((!other.gameObject.GetComponent<Cargo>().IsLoaded) && (other.gameObject.name == _gameReferee._lastCargoLoaded))
        //                {
        //                    _cargoesLoaded.Add(other.gameObject.GetComponent<Cargo>());
        //                }
        //                break;
        //        }
        //    }
        //}

        void ValidateTriggerCargo(Collider other)
        {
            if (other.CompareTag("Cargo"))
            {
                switch (_gameReferee.GetGameState)
                {
                    case RS_GameStates.CANNON_CARGO:
                        _gameReferee.SetACargoHasTouchedTheShip = true;
                        break;
                    case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                        if ((!other.gameObject.GetComponent<Cargo>().IsLoaded) &&
                            !_cargoesLoaded.Contains(other.gameObject.GetComponent<Cargo>()))
                        {
                            _cargoesLoaded.Add(other.gameObject.GetComponent<Cargo>());
                        }
                        break;
                }
            }
        }


        #endregion

        #region PublicMethods

        public Cargo NearestCargo()
        {
            //_cargosPos.Clear(); //start with a clean slate
            _nearestDistance = 1000000000000000; //Mathf.Infinity;

            //We calculate the distance between all cargo and the ship.
            for (int i = 0; i < _cargos.Length - 1; ++i)
            {
                if (!_cargos[i].IsLoaded)
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