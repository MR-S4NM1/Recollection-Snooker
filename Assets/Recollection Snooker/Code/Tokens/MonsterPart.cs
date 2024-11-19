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
                && (other.gameObject.tag == "Dice" || other.gameObject.tag == "Cargo")
                )
            {
                //Debug.Log("Push from " + gameObject.name + " to " + other.gameObject.name);
                _pushDirection = (other.transform.position -
                    this.transform.position).normalized;
                other.gameObject.GetComponent<RS_TokenFiniteStateMachine>()?.ThrowToken(_pushDirection * 2.0f);
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


        #endregion

        #region PublicMethods

        public void ValidateSpaceToSpawnMonsterPart()
        {
            if(monsterPartType == MonsterPartType.LIMB)
            {
                //_randomPosToPlaceToken = new Vector3();
                if (Physics.SphereCast(transform.position, 0.5f, -transform.up, out _raycastHit, 1.0f))
                {

                }
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