using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrSanmi.RecollectionSnooker
{
    #region Enums

    public enum MonsterPartType
    {
        LIMB,
        HEAD
    }

    #endregion

    #region Structs


    #endregion

    public class MonsterPart : Token
    {
        #region Knobs

        [Header("Knobs / Parameters")]
        public MonsterPartType monsterPartType;

        #endregion

        #region References

        #endregion

        #region RuntimeVariables

        protected Vector3 _pushDirection;
        protected RaycastHit _raycastHit;
        protected bool canBePlacedAtThePosition;
        Vector3 _randomPosToPlaceToken;

        #endregion

        #region UnityMethods

        void Start()
        {
            base.InitializeToken();
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            ValidateReferences();
            #endif
        }

        void Update()
        {

        }

        //private void OnCollisionEnter(Collision other)
        //{
        //    ValidateCollision(other);
        //}

        private void OnCollisionStay(Collision other)
        {
            if (this.monsterPartType == MonsterPartType.HEAD
                && (other.gameObject.CompareTag("Cargo"))
                )
            {
                //Debug.Log("Push from " + gameObject.name + " to " + other.gameObject.name);
                _pushDirection = (other.transform.position -
                    this.transform.position).normalized;
                other.gameObject.GetComponent<RS_TokenFiniteStateMachine>()?.ThrowToken(_pushDirection * 2.0f);
            }
        }

        #endregion

        #region RuntimeMethods

        #endregion

        #region PublicMethods

        public void ValidateSpaceToSpawnMonsterPart()
        {
            if (monsterPartType == MonsterPartType.HEAD)
            {
                canBePlacedAtThePosition = false;

                while (!canBePlacedAtThePosition)
                {
                    _tokenPhysicalFSM.StateMechanic(TokenStateMechanic.SET_SPOOKY);
                    _randomPosToPlaceToken = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));
                    transform.position = _randomPosToPlaceToken;
                    if (Physics.SphereCast(new Vector3(_randomPosToPlaceToken.x, _randomPosToPlaceToken.y + 10.0f, _randomPosToPlaceToken.z), 5.0f, -transform.up * 1.0f, out _raycastHit, 10.0f))
                    {
                        if (!_raycastHit.collider.gameObject.GetComponent<Token>())
                        {
                            canBePlacedAtThePosition = true;
                            _tokenPhysicalFSM.StateMechanic(TokenStateMechanic.SET_RIGID);
                            transform.position = _randomPosToPlaceToken;
                        }
                    }
                }
                print(canBePlacedAtThePosition);
            }
            else if (monsterPartType == MonsterPartType.LIMB)
            {
                canBePlacedAtThePosition = false;

                while (!canBePlacedAtThePosition)
                {
                    _tokenPhysicalFSM.StateMechanic(TokenStateMechanic.SET_SPOOKY);
                    _randomPosToPlaceToken = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));
                    transform.position = _randomPosToPlaceToken;
                    if (Physics.SphereCast(new Vector3(_randomPosToPlaceToken.x, _randomPosToPlaceToken.y + 5.0f, _randomPosToPlaceToken.z), 3.0f, -transform.up * 1.0f, out _raycastHit, 5.0f))
                    {
                        if (!_raycastHit.collider.gameObject.GetComponent<Token>())
                        {
                            canBePlacedAtThePosition = true;
                            _tokenPhysicalFSM.StateMechanic(TokenStateMechanic.SET_PHYSICS);
                            transform.position = _randomPosToPlaceToken;
                        }
                    }
                }
                print(canBePlacedAtThePosition);
            }
        }

        #endregion

        #region GettersSetters

        public bool CanBePlacedAtThePosition
        {
            get { return canBePlacedAtThePosition; }
        }

        #endregion
    }
}