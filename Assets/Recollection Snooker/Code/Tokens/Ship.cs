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

        #endregion

        #region RuntimeMethods


        #endregion

        #region PublicMethods

        public Cargo NearestCargo()
        {
            //_cargosPos.Clear(); //start with a clean slate
            _nearestDistance = 1000000000000000; //Mathf.Infinity;

            //We calculate the distance between all cargo and the ship.
            for (int i = 0; i < _cargos.Length - 1; ++i)
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