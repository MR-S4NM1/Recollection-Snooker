using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MrSanmi.RecollectionSnooker
{
    public class RS_MobileInputHandler : MobileInputHandler
    {
        #region References

        [SerializeField] protected GameObject _goTouchCursor;
        [SerializeField] protected Camera _camera;
        [SerializeField] protected RS_GameReferee _gameReferee;

        #endregion

        #region RuntimeVariables

        protected RaycastHit _raycastHit;
        [SerializeField] protected Token _chosenToken;
        [SerializeField] protected Token _contactToken;

        #endregion

        #region UnityMethods

        void Start()
        {
            InitializeMobileInputHandler();
        }

        private void FixedUpdate()
        {

        }

        #endregion

        #region LocalMethods

        protected override void HandleTouchInputAction(InputAction.CallbackContext value)
        {
            switch (_gameReferee.GetGameState)
            {
                case RS_GameStates.CHOOSE_TOKEN_BY_PLAYER:
                    HandleTouchInChooseTokenByPlayer(value);
                    break;
                case RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER:
                    HandleTouchInContactPointTokenByPlayer(value);
                    break;
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    HandleTouchInLoadingCargoByPlayer(value);
                    break;
            }
        }

        protected override void HandleRotateInputAction(InputAction.CallbackContext value)
        {
            switch (_gameReferee.GetGameState)
            {
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER: //manage the flags
                    HandleRotationInFlickTokenByPlayer(value);
                    break;
            }
        }

        protected override void HandleTiltInputAction(InputAction.CallbackContext value)
        {
            Debug.Log("TIIIIIIIILTTTTTTTT");
            switch (_gameReferee.GetGameState)
            {
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER: //manage the flags
                    HandleRotationInFlickTokenByPlayer(value);
                    break;
            }
        }

        protected override void HandleTranslateInputAction(InputAction.CallbackContext value)
        {
            if (value.performed)
            {

            }
            else if (value.canceled)
            {

            }
        }

        //protected override void HandleLoadingCargoTranslateInputAction(InputAction.CallbackContext value)
        //{
            
        //}

        #endregion

        #region HandleTouchActions

        protected void HandleTouchInChooseTokenByPlayer(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                if (Physics.Raycast(_camera.ScreenPointToRay(value.ReadValue<Vector2>()),out _raycastHit,
                        100.0f, LayerMask.GetMask("Token")))
                {
                    _chosenToken = _raycastHit.collider.gameObject.GetComponent<Token>();
                    if (_chosenToken.IsAvalaibleForFlicking) {
                        _goTouchCursor.SetActive(true);
                        _goTouchCursor.transform.position = _raycastHit.point;
                        //set the parameter prior to the initialization method
                        //of the Contact Point Token by Player State
                        _gameReferee.SetInteractedToken = _chosenToken;
                        _gameReferee.GameStateMechanic(RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER);
                    }
                }
                else
                {
                    _goTouchCursor.SetActive(false);
                }
            }
            else if (value.canceled)
            {
                _goTouchCursor.SetActive(false);
            }
        }

        protected void HandleTouchInContactPointTokenByPlayer(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                if (
                    Physics.Raycast(
                        _camera.ScreenPointToRay(value.ReadValue<Vector2>()),
                        out _raycastHit,
                        100.0f,
                        LayerMask.GetMask("Token")
                        )
                    )
                {
                    _contactToken = _raycastHit.collider.gameObject.GetComponent<Token>();
                    //compare between the last state and this current state
                    //to check if they are the same ;)
                    if (_contactToken == _chosenToken) 
                    {
                        _goTouchCursor.SetActive(true);
                        _goTouchCursor.transform.position = _raycastHit.point;
                        _gameReferee.GameStateMechanic(RS_GameStates.FLICK_TOKEN_BY_PLAYER);
                    }
                }
                else
                {
                    _goTouchCursor.SetActive(false);
                }
            }
        }

        protected void HandleTouchInLoadingCargoByPlayer(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                if (_gameReferee._aCargoHasTouchedTheShip)
                {
                    if (Physics.Raycast(_camera.ScreenPointToRay(value.ReadValue<Vector2>()),
                    out _raycastHit, 100.0f, LayerMask.GetMask("CargoSpace")))
                    {
                        _goTouchCursor.SetActive(true);
                        _goTouchCursor.transform.position = _raycastHit.point;
                        _gameReferee.CargoToBeLoaded.gameObject.transform.position = Vector3.Lerp(_gameReferee.CargoToBeLoaded.gameObject.transform.position,
                            _raycastHit.point, 0.5f);
                    }
                    else
                    {
                        _goTouchCursor.SetActive(false);
                    }
                }
                else if (_gameReferee._shipPivotHasTouchedTheIsland)
                {
                    if (Physics.Raycast(_camera.ScreenPointToRay(value.ReadValue<Vector2>()),
                        out _raycastHit, 100.0f, LayerMask.GetMask("IslandCargoSpace")))
                    {
                        _goTouchCursor.SetActive(true);
                        _goTouchCursor.transform.position = _raycastHit.point;

                        for (int i = 0; i < _gameReferee.shipOfTheGame._cargoesLoadedOnTheShip.Count; ++i)
                        {
                            _gameReferee.shipOfTheGame._cargoesLoadedOnTheShip[i].gameObject.transform.position = 
                                Vector3.Lerp(_gameReferee.CargoToBeLoaded.gameObject.transform.position,
                                _raycastHit.point, 0.5f);
                        }
                    }
                    else
                    {
                        _goTouchCursor.SetActive(false);
                    }
                }
            }
            else if (value.canceled)
            {
                _goTouchCursor.SetActive(false);
                _gameReferee.GameStateMechanic(RS_GameStates.SHIFT_MONSTER_PARTS);
            }
        }

        #endregion HandleTouchActions

        #region HandleRotationActions

        protected void HandleRotationInFlickTokenByPlayer(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                _gameReferee.GetCurrentFlag.transform.Rotate(
                new Vector3((value.ReadValue<Vector3>() != null ? 
                value.ReadValue<Vector3>().x : value.ReadValue<Vector2>().x) * 
                -4f, 0f, 0f), Space.Self);
            }
            else if (value.canceled)
            {

            }
        }

        #endregion HandleRotationActions
    }
}