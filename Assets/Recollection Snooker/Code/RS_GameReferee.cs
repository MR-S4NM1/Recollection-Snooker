using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MrSanmi.Game;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;

namespace MrSanmi.RecollectionSnooker
{
    #region Enum

    public enum RS_GameStates
    {
        //START OF THE GAME
        SHOW_THE_LAYOUT_TO_THE_PLAYER,
        //PLAYER ORDINARY TURN STATES
        CHOOSE_TOKEN_BY_PLAYER,
        CONTACT_POINT_TOKEN_BY_PLAYER,
        FLICK_TOKEN_BY_PLAYER,
        CANNON_BY_NAVIGATION,
        NAVIGATING_SHIP_OF_THE_PLAYER, //Including loading treasures to the island
        ANCHOR_SHIP,
        CANNON_CARGO,
        LOADING_CARGO_BY_PLAYER,
        ORGANIZE_CARGO_BY_PLAYER,
        MOVE_COUNTER_BY_SANCTION,
        //END OF THE TURN
        SHIFT_MONSTER_PARTS,
        //META MECHANICS
        VICTORY_OF_THE_PLAYER,
        FAILURE_OF_THE_PLAYER
    }

    #endregion

    public class RS_GameReferee : GameReferee
    {
        #region References

        //Polymorphism by prefab variants and code
        //for every type of token in the game
        [Header("Token References")]
        [SerializeField] protected Cargo[] allCargoOfTheGame;
        [SerializeField] protected MonsterPart[] allMonsterPartOfTheGame;
        [SerializeField] protected Ship shipOfTheGame;
        [SerializeField] protected ShipPivot shipPivotsOfTheGame;
        [SerializeField] protected MonsterPart monsterHead;

        [Header("Camera References")]
        [SerializeField] protected CinemachineFreeLook tableFreeLookCamera;

        [Header("Flags")]
        [SerializeField] protected GameObject flag;

        [SerializeField] protected TextMeshProUGUI debugText;

        [Header("Random Token Positions")]
        [SerializeField] protected List<Transform> tokenPosList;

        #endregion

        #region RuntimeVariables

        protected new RS_GameStates _gameState;
        protected CinemachineFreeLook _currentFreeLookCamera;
        [SerializeField] protected GameObject _currentFlag;
        protected Token _interactedToken;
        [SerializeField] protected bool _isAllCargoStill;
        protected int _randomTokenPos;
        protected Cargo _nearestCargoToTheShip;

        #endregion

        #region UnityMethods

        void Start()
        {
            InitializeGameReferee();
        }

        private void FixedUpdate()
        {
            print(_gameState);
            switch (_gameState)
            {
                case RS_GameStates.SHOW_THE_LAYOUT_TO_THE_PLAYER:
                    ExecutingShowTheLayoutToThePlayerState();
                    break;
                case RS_GameStates.CHOOSE_TOKEN_BY_PLAYER:
                    ExecutingChooseTokenByPlayerState();
                    break;
                case RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER:
                    ExecutingContactPointTokenByPlayerState();
                    break;
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER:
                    ExecutingFlickTokenByPlayerState();
                    break;
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    ExecutingCannonByNavigationState();
                    break;
                case RS_GameStates.NAVIGATING_SHIP_OF_THE_PLAYER:
                    ExecutingNavigatingShipOfThePlayerState();
                    break;
                case RS_GameStates.ANCHOR_SHIP:
                    ExecutingAnchorShipState();
                    break;
                case RS_GameStates.CANNON_CARGO:
                    ExecutingCannonCargoState();
                    break;
                case RS_GameStates.LOADING_CARGO_BY_PLAYER:
                    ExecutingLoadingCargoByPlayerState();
                    break;
                case RS_GameStates.ORGANIZE_CARGO_BY_PLAYER:
                    ExecutingOrganizeCargoByPlayerState();
                    break;
                case RS_GameStates.MOVE_COUNTER_BY_SANCTION:
                    ExecutingMoveCounterBySanctionState();
                    break;
                case RS_GameStates.SHIFT_MONSTER_PARTS:
                    ExecutingShiftMonsterPartsState();
                    break;
                case RS_GameStates.VICTORY_OF_THE_PLAYER:
                    ExecutingVictoryOfThePlayerState();
                    break;
                case RS_GameStates.FAILURE_OF_THE_PLAYER:
                    ExecutingFailureOfThePlayerState();
                    break;
            }
        }

        #endregion

        #region PublicMethods

        public void DebugInMobile(string value)
        {
            debugText.text = value;
        }

        public void GameStateMechanic(RS_GameStates toNextState)
        {
            //let's validate the possibility to go
            //to the suggested next state
            switch (toNextState)
            {
                case RS_GameStates.CHOOSE_TOKEN_BY_PLAYER:
                    if (_gameState == RS_GameStates.CANNON_CARGO ||
                        _gameState == RS_GameStates.SHOW_THE_LAYOUT_TO_THE_PLAYER)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER:
                    if (_gameState == RS_GameStates.CHOOSE_TOKEN_BY_PLAYER ||
                        _gameState == RS_GameStates.FLICK_TOKEN_BY_PLAYER)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER:
                    if (_gameState == RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    if (_gameState == RS_GameStates.FLICK_TOKEN_BY_PLAYER)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.NAVIGATING_SHIP_OF_THE_PLAYER:
                    if (_gameState == RS_GameStates.CANNON_BY_NAVIGATION)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.ANCHOR_SHIP:
                    if (_gameState == RS_GameStates.NAVIGATING_SHIP_OF_THE_PLAYER)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.CANNON_CARGO:
                    if (_gameState == RS_GameStates.FLICK_TOKEN_BY_PLAYER)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.LOADING_CARGO_BY_PLAYER:
                    if (_gameState == RS_GameStates.CANNON_CARGO)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.ORGANIZE_CARGO_BY_PLAYER:
                    if (_gameState == RS_GameStates.LOADING_CARGO_BY_PLAYER)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.MOVE_COUNTER_BY_SANCTION:
                    if (_gameState == RS_GameStates.CANNON_CARGO)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.SHIFT_MONSTER_PARTS:
                    if (_gameState == RS_GameStates.ORGANIZE_CARGO_BY_PLAYER ||
                        _gameState == RS_GameStates.MOVE_COUNTER_BY_SANCTION)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.VICTORY_OF_THE_PLAYER:
                    if (_gameState == RS_GameStates.NAVIGATING_SHIP_OF_THE_PLAYER)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.FAILURE_OF_THE_PLAYER:
                    if (_gameState == RS_GameStates.MOVE_COUNTER_BY_SANCTION)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
            }
        }

        #endregion

        #region RuntimeMethods

        protected void FinalizeCurrentState(RS_GameStates toNextState)
        {
            FinalizeState();
            _gameState = toNextState;
            InitializeState();
        }

        protected bool IsAllCargoStill()
        {
            _isAllCargoStill = true;
            foreach (Token token in allCargoOfTheGame)
            {
                if (!token.IsStill)
                {
                    _isAllCargoStill = false;
                    break;
                }
            }
            return _isAllCargoStill;
        }

        protected override void InitializeGameReferee()
        {
            _gameState = RS_GameStates.SHOW_THE_LAYOUT_TO_THE_PLAYER;  //(RS_GameStates)0;
            //InitializeDropCargoState();
            InitializeState();
        }

        protected void InitializeState()
        {
            switch(_gameState)
            {
                case RS_GameStates.SHOW_THE_LAYOUT_TO_THE_PLAYER:
                    InitializeShowTheLayoutToThePlayerState();
                    break;
                case RS_GameStates.CHOOSE_TOKEN_BY_PLAYER:
                    InitializeChooseTokenByPlayerState();
                    break;
                case RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER:
                    InitializeContactPointTokenByPlayerState();
                    break;
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER:
                    InitializeFlickTokenByPlayerState();
                    break;
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    InitializeCannonByNavigationState();
                    break;
                case RS_GameStates.NAVIGATING_SHIP_OF_THE_PLAYER:
                    InitializeNavigatingShipOfThePlayerState();
                    break;
                case RS_GameStates.ANCHOR_SHIP:
                    InitializeAnchorShipState();
                    break;
                case RS_GameStates.CANNON_CARGO:
                    InitializeCannonCargoState();
                    break;
                case RS_GameStates.LOADING_CARGO_BY_PLAYER:
                    InitializeLoadingCargoByPlayerState();
                    break;
                case RS_GameStates.ORGANIZE_CARGO_BY_PLAYER:
                    InitializeOrganizeCargoByPlayerState();
                    break;
                case RS_GameStates.MOVE_COUNTER_BY_SANCTION:
                    InitializeMoveCounterBySanctionState();
                    break;
                case RS_GameStates.SHIFT_MONSTER_PARTS:
                    InitializeShiftMonsterPartsState();
                    break;
                case RS_GameStates.VICTORY_OF_THE_PLAYER:
                    InitializeVictoryOfThePlayerState();
                    break;
                case RS_GameStates.FAILURE_OF_THE_PLAYER:
                    InitializeFailureOfThePlayerState();
                    break;
            }
        }

        protected void FinalizeState()
        {
            switch (_gameState)
            {
                case RS_GameStates.SHOW_THE_LAYOUT_TO_THE_PLAYER:
                    FinalizeShowTheLayoutToThePlayerState();
                    break;
                case RS_GameStates.CHOOSE_TOKEN_BY_PLAYER:
                    FinalizeChooseTokenByPlayerState();
                    break;
                case RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER:
                    FinalizeContactPointTokenByPlayerState();
                    break;
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER:
                    FinalizeFlickTokenByPlayerState();
                    break;
                case RS_GameStates.CANNON_BY_NAVIGATION:
                    FinalizeCannonByNavigationState();
                    break;
                case RS_GameStates.NAVIGATING_SHIP_OF_THE_PLAYER:
                    FinalizeNavigatingShipOfThePlayerState();
                    break;
                case RS_GameStates.ANCHOR_SHIP:
                    FinalizeAnchorShipState();
                    break;
                case RS_GameStates.CANNON_CARGO:
                    FinalizeCannonCargoState();
                    break;
                case RS_GameStates.LOADING_CARGO_BY_PLAYER:
                    FinalizeLoadingCargoByPlayerState();
                    break;
                case RS_GameStates.ORGANIZE_CARGO_BY_PLAYER:
                    FinalizeOrganizeCargoByPlayerState();
                    break;
                case RS_GameStates.MOVE_COUNTER_BY_SANCTION:
                    FinalizeMoveCounterBySanctionState();
                    break;
                case RS_GameStates.SHIFT_MONSTER_PARTS:
                    FinalizeShiftMonsterPartsState();
                    break;
                case RS_GameStates.VICTORY_OF_THE_PLAYER:
                    FinalizeVictoryOfThePlayerState();
                    break;
                case RS_GameStates.FAILURE_OF_THE_PLAYER:
                    FinalizeFailureOfThePlayerState();
                    break;
            }
        }

        protected void DebugInConsole(string value)
        {
            Debug.Log(
                    gameObject.name + ": " +
                    this.name + " - " +
                    value
                );
        }

        #endregion

        //for every state, we will handle
        //Intialize___State
        //Manage___State
        //Finalize___State
        #region FiniteStateMachineMethods

        #region ShowTheLayoutToThePlayer

        protected void InitializeShowTheLayoutToThePlayerState()
        {
            //TODO: Make the proper initialization of the state

            // Putting all cargo at random positions in order to have some variety at the beginning of each game session

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                _randomTokenPos = Random.Range(0, tokenPosList.Count - 1);
                cargo.gameObject.transform.position = tokenPosList[_randomTokenPos].position;
                tokenPosList.RemoveAt(_randomTokenPos);
            }

            //Setting all cargo in the GHOST state

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }

            if (!IsAllCargoStill())
            {
                GameStateMechanic(RS_GameStates.CHOOSE_TOKEN_BY_PLAYER);
            }
        }

        protected void ExecutingShowTheLayoutToThePlayerState()
        {

        }

        protected void FinalizeShowTheLayoutToThePlayerState()
        {

        }

        #endregion ShowTheLayoutToThePlayer

        #region ChooseTokenByPlayer

        protected void InitializeChooseTokenByPlayerState()
        {
            _nearestCargoToTheShip = shipOfTheGame.NearestCargo();

            _nearestCargoToTheShip.gameObject.SetActive(false);

            //All cargo is set to Spooky
            foreach (Token cargo in allCargoOfTheGame)
            {
                cargo.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }
            //TODO: Set Spooky for ships, monster parts and ship pivots

            //Activate the table camera (with the highest priority)
            if (_currentFreeLookCamera != null)
            {
                _currentFreeLookCamera.Priority = 1;
            }
            _currentFreeLookCamera = tableFreeLookCamera;
            _currentFreeLookCamera.Priority = 1000;

            //Check available cargo for flicking
            foreach (Cargo cargo in allCargoOfTheGame)
            {
                if (!cargo.IsLoaded)
                {
                    cargo.SetHighlight(true);
                    cargo.IsAvalaibleForFlicking = true;
                }
            }
        }

        protected void ExecutingChooseTokenByPlayerState()
        {

        }

        protected void FinalizeChooseTokenByPlayerState()
        {
            //table free look camera
            _currentFreeLookCamera.Priority = 1;
        }

        #endregion ChooseTokenByPlayer

        #region ContactPointTokenByPlayer

        protected void InitializeContactPointTokenByPlayerState()
        {
            _interactedToken.StateMechanic(TokenStateMechanic.SET_PHYSICS);
            //Focus to the camera rig of the selected token
            _currentFreeLookCamera = _interactedToken.GetFreeLookCamera;
            _currentFreeLookCamera.Priority = 1000;
        }

        protected void ExecutingContactPointTokenByPlayerState()
        {
            
        }

        protected void FinalizeContactPointTokenByPlayerState()
        {

        }

        #endregion ContactPointTokenByPlayer

        #region FlickTokenByPlayer

        protected void InitializeFlickTokenByPlayerState()
        {
            //this virtual camera hasn't changed from the previous
            //state, so this is the camera from the selected token
            _currentFreeLookCamera.gameObject.GetComponent<CinemachineMobileInputProvider>().enableCameraRig = false;
            _currentFreeLookCamera.m_YAxis.Value = 0.0f;
            _currentFlag.gameObject.SetActive(true);
        }

        protected void ExecutingFlickTokenByPlayerState()
        {

        }

        protected void FinalizeFlickTokenByPlayerState()
        {
            _currentFlag.transform.localRotation = Quaternion.identity;
            _currentFlag.gameObject.SetActive(false);
            _nearestCargoToTheShip.gameObject.SetActive(true);
            _nearestCargoToTheShip = null;
        }

        #endregion

        #region CannonByNavigation

        protected void InitializeCannonByNavigationState()
        {

        }

        protected void ExecutingCannonByNavigationState()
        {

        }

        protected void FinalizeCannonByNavigationState()
        {

        }

        #endregion

        #region NavigatingShipOfThePlayer

        protected void InitializeNavigatingShipOfThePlayerState()
        {

        }

        protected void ExecutingNavigatingShipOfThePlayerState()
        {

        }

        protected void FinalizeNavigatingShipOfThePlayerState()
        {

        }

        #endregion

        #region AnchorShip

        protected void InitializeAnchorShipState()
        {

        }

        protected void ExecutingAnchorShipState()
        {

        }

        protected void FinalizeAnchorShipState()
        {

        }

        #endregion

        #region CannonCargo

        protected void InitializeCannonCargoState()
        {
            _nearestCargoToTheShip.gameObject.SetActive(true);
            _nearestCargoToTheShip = null;

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.StateMechanic(TokenStateMechanic.SET_PHYSICS);
            }
        }

        protected void ExecutingCannonCargoState()
        {
            if (!IsAllCargoStill())
            {
                GameStateMechanic(RS_GameStates.CHOOSE_TOKEN_BY_PLAYER);
                //TODO: Pending validation events while the cannon was executing
                //A) LOAD_CARGO_BY_PLAYER
                //B) MOVE_COUNTER
                //C) FINALIZE_TURN (Respawn of the monster parts)
            }
        }

        protected void FinalizeCannonCargoState()
        {

        }

        #endregion

        #region LoadingCargoByPlayer

        protected void InitializeLoadingCargoByPlayerState()
        {

        }

        protected void ExecutingLoadingCargoByPlayerState()
        {

        }

        protected void FinalizeLoadingCargoByPlayerState()
        {

        }

        #endregion

        #region OrganizeCargoByPlayer

        protected void InitializeOrganizeCargoByPlayerState()
        {

        }

        protected void ExecutingOrganizeCargoByPlayerState()
        {

        }

        protected void FinalizeOrganizeCargoByPlayerState()
        {

        }

        #endregion

        #region MoveCounterBySanction

        protected void InitializeMoveCounterBySanctionState()
        {

        }

        protected void ExecutingMoveCounterBySanctionState()
        {

        }

        protected void FinalizeMoveCounterBySanctionState()
        {

        }

        #endregion

        #region ShiftMonsterParts

        protected void InitializeShiftMonsterPartsState()
        {

        }

        protected void ExecutingShiftMonsterPartsState()
        {

        }

        protected void FinalizeShiftMonsterPartsState()
        {

        }

        #endregion

        #region VictoryOfThePlayer

        protected void InitializeVictoryOfThePlayerState()
        {

        }

        protected void ExecutingVictoryOfThePlayerState()
        {

        }

        protected void FinalizeVictoryOfThePlayerState()
        {

        }

        #endregion

        #region FailureOfThePlayer

        protected void InitializeFailureOfThePlayerState()
        {

        }

        protected void ExecutingFailureOfThePlayerState()
        {

        }

        protected void FinalizeFailureOfThePlayerState()
        {

        }

        #endregion

        #endregion FiniteStateMachineMethods

        #region GettersAndSetters

        public RS_GameStates GetGameState
        {
            get { return _gameState; }
        }

        public Token SetInteractedToken
        {
            set {
                _interactedToken = value;
                //DebugInConsole("SetInteractedToken - " + _interactedToken.gameObject.name);
            }
        }

        public GameObject GetCurrentFlag
        {
            get { return _currentFlag; }
        }

        #endregion
    }
}