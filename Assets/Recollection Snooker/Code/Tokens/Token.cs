using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

namespace MrSanmi.RecollectionSnooker
{
    #region Enums


    #endregion

    #region Structs

    [System.Serializable]
    public struct GameplayAttributes
    {
        public bool isAvailableForFlicking;
        public bool isOutOfTheBoard;
        public bool isBeingDragged;
    }

    #endregion

    [RequireComponent(typeof(RS_TokenFiniteStateMachine))]
    public class Token : MonoBehaviour
    {
        #region Knobs


        #endregion

        #region References

        [SerializeField,HideInInspector] protected RS_TokenFiniteStateMachine _tokenPhysicalFSM;
        //TODO:Assign remaining references to other Token child prefabs
        [SerializeField] protected CinemachineFreeLook _freeLookCamera;
        [SerializeField] protected RS_GameReferee _gameReferee;

        #endregion

        #region RuntimeVariables

        [Header("Runtime Variables")]
        [SerializeField] protected GameplayAttributes _gameplayAttributes;
        protected Flag _contactedFlag;
        protected bool _cargoOrShipPivotHasTouchedAMonsterPart;
        protected bool _shipPivotHasTouchedACargo;

        #endregion

        #region UnityMethods

        void Start()
        {
            InitializeToken();
        }

        private void OnCollisionEnter(Collision other)
        {
            _gameReferee.DebugInMobile(gameObject.name +
                " OnCollisionEnter() - Detected collision with " +
                other.gameObject.name);
            switch (_gameReferee.GameState)
            {
                case RS_GameStates.CANNON_CARGO:
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateCollisionDuringCannon(other);
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

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEvent(other);
        }

        //void UpdateInEditor()
        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            ValidateReferences();
            #endif
        }

        #endregion

        #region RuntimeMethods

        protected void OnTriggerEvent(Collider other)
        {
            _gameReferee.DebugInMobile(gameObject.name +
                " OnTriggerEnter() - Detected collision with " +
                other.gameObject.name);
            ValidateTrigger(other);
        }

        protected virtual void ValidateReferences()
        {
            if (_tokenPhysicalFSM == null)
            {
                _tokenPhysicalFSM = gameObject.GetComponent<RS_TokenFiniteStateMachine>();
            }
            if (_gameReferee == null)
            {
                _gameReferee = GameObject.FindAnyObjectByType<RS_GameReferee>();
            }
            if (_freeLookCamera == null)
            {
                _freeLookCamera = transform.GetChild(0).GetChild(0).GetComponent<CinemachineFreeLook>();
                //_freeLookCamera = transform.GetComponentInChildren<CinemachineFreeLook>();
            }
        }

        public virtual void ValidateCollisionDuringCannon(Collision other)
        {
            if (this as ShipPivot || this as Cargo)
            {
                if (other.gameObject.CompareTag("MonsterLimb"))
                {
                    _gameReferee.SetGameRefereeHasConfirmedThatNoCargoOrShipPivotHaveTouchedAMonsterPart = true;
                }
            }
        }

        public virtual void ValidateTriggerDuringCannon(Collider other)
        {
            if (this as ShipPivot || this as Cargo)
            {
                if (other.gameObject.CompareTag("MonsterLimb"))
                {
                    _gameReferee.SetGameRefereeHasConfirmedThatNoCargoOrShipPivotHaveTouchedAMonsterPart = true;
                }
            }
        }

        protected virtual void ValidateTrigger(Collider other)
        {
            _gameReferee.DebugInMobile(" Validate Collision " + other.gameObject.name);
            //cases for every type of TOKEN
            switch (_gameReferee.GameState)
            { 
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER:
                    _gameReferee.DebugInMobile("FLICK_TOKEN_BY_PLAYER " + other.gameObject.name);
                    ValidateTriggerWithFlag(other);
                    break;
                case RS_GameStates.CANNON_CARGO:
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ValidateTriggerDuringCannon(other);
                    break;
            }
        }

        protected virtual void ValidateTriggerWithFlag(Collider other)
        {
            if (this as Cargo || this as ShipPivot) //Polymorphism
            {
                _gameReferee.DebugInMobile("I am a Cargo or ShipPivot " + other.gameObject.name);
                if (other.gameObject.CompareTag("Flag")) //other = flag
                {
                    _contactedFlag = other.gameObject.GetComponent<Flag>();
                    _tokenPhysicalFSM.ThrowTokenAtSpecificPosition(other.gameObject.transform.GetChild(0).forward * (Mathf.Abs(_contactedFlag.DeltaXDegrees + 1f * 5)),
                        other.gameObject.transform.position);

                    if (this as Cargo)
                    {
                        //_gameReferee.GetCMTargetGroup.AddMember(this.gameObject.transform, 1, 0);
                        _gameReferee.GameStateMechanic(RS_GameStates.CANNON_CARGO);
                        _gameReferee.DebugInMobile("CANNON " + other.gameObject.name);
                    }
                    else if (this as ShipPivot)
                    {
                        //_gameReferee.GetCMTargetGroup.AddMember(this.gameObject.transform, 1, 0);
                        _gameReferee.GameStateMechanic(RS_GameStates.CANNON_BY_NAVIGATION);
                    }
                }
            }
        }

        protected virtual void InitializeToken()
        {
            //if by any circumstance, the reference is lost,
            //we retreive it
            if (_tokenPhysicalFSM == null)
            {
                _tokenPhysicalFSM = GetComponent<RS_TokenFiniteStateMachine>();
            }
            ValidateReferences();

            _cargoOrShipPivotHasTouchedAMonsterPart = false;
        }

        #endregion

        #region PublicMethods

        public void StateMechanic(TokenStateMechanic value)
        {
            _tokenPhysicalFSM.StateMechanic(value);
        }

        //Broker
        public void SetHighlight(bool value)
        {
            _tokenPhysicalFSM.SetHighlight(value);
        }

        #endregion

        #region GettersSetters

        public bool IsStill
        {
            get { return _tokenPhysicalFSM.IsStill; }
        }

        public bool IsAvalaibleForFlicking
        {
            get { return _gameplayAttributes.isAvailableForFlicking; }
            set { _gameplayAttributes.isAvailableForFlicking = value; }
        }

        public CinemachineFreeLook GetFreeLookCamera
        {
            get { return _freeLookCamera; }
        }

        public bool GetCargoOrShipPivotHaveTouchedAMonsterPart
        {
            get { return _cargoOrShipPivotHasTouchedAMonsterPart; }
        }

        #endregion
    }
}