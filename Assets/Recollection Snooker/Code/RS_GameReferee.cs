using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using System;
using UnityEngine.UIElements;

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
        LOADING_AND_ORGANIZING_CARGO_BY_PLAYER,
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

        [Header("General Game Referee")]
        [SerializeField] protected GameReferee _generalGameReferee;

        //Polymorphism by prefab variants and code
        //for every type of token in the game
        [Header("Token References")]
        [SerializeField] protected Cargo[] allCargoOfTheGame;
        [SerializeField] protected MonsterPart[] allMonsterPartOfTheGame;
        [SerializeField] public Ship shipOfTheGame;
        [SerializeField] protected ShipPivot shipPivotOfTheGame;
        [SerializeField] protected MonsterPart monsterHead;
        [SerializeField] public Island islandOfTheGame;
        [SerializeField] protected Transform[] cargoPositionsOnShip;
        [SerializeField] protected Transform[] cargoPositionsOnIsland;
        [SerializeField] protected GameObject cargoSpaceOnShip;
        [SerializeField] protected GameObject anchorShipSpace;
        [SerializeField] protected GameObject islandWall;
        [SerializeField] protected RS_CinemachineTargetGroup _rs_CinemachineTargetGroup;
        [SerializeField] protected CinemachineFreeLook _cinemachineTargetGroupCamera;

        [Header("Camera References")]
        [SerializeField] protected CinemachineFreeLook tableFreeLookCamera;
        [SerializeField] protected CinemachineVirtualCamera shipVirtualCamera;
        [SerializeField] protected CinemachineVirtualCamera anchorShipVirtualCamera;
        //[SerializeField] protected CinemachineVirtualCamera islandVirtualCamera;

        [Header("Flags")]
        [SerializeField] protected GameObject flag;

        [SerializeField] protected TextMeshProUGUI debugText;

        [Header("Random Token Positions")]
        [SerializeField] protected List<Transform> tokenPosList;

        [Header("Health Points")]
        [SerializeField] protected int healthPoints;

        #endregion

        #region RuntimeVariables

        protected RS_GameStates _gameState;
        protected CinemachineVirtualCameraBase _currentVirtualCameraBase;
        protected Token _interactedToken;
        [SerializeField] protected bool _isAllCargoStill;
        protected int _randomTokenPos;
        protected Cargo _nearestCargoToTheShip;
        protected bool _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart;
        [SerializeField] protected Cargo _cargoToBeLoaded;
        public bool _aCargoHasTouchedTheShip;
        protected Vector3 _originalPositionOfTheFlag;
        public bool _shipPivotHasTouchedTheIsland;
        protected float distanceBetweenShipAndPivot;


        #endregion

        #region UnityMethods

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            if(_generalGameReferee == null)
            {
                _generalGameReferee = GameObject.FindObjectOfType<GameReferee>(true);
            }
            #endif
        }

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
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    ExecutingLoadingAndOrganizingCargoByPlayerState();
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
                    if (_gameState == RS_GameStates.SHIFT_MONSTER_PARTS ||
                        _gameState == RS_GameStates.SHOW_THE_LAYOUT_TO_THE_PLAYER)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER:
                    if (_gameState == RS_GameStates.CHOOSE_TOKEN_BY_PLAYER)
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
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    if (_gameState == RS_GameStates.CANNON_CARGO ||
                        _gameState == RS_GameStates.ANCHOR_SHIP
                        /*_gameState == RS_GameStates.CANNON_BY_NAVIGATION*/) // This was just for prototype
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.MOVE_COUNTER_BY_SANCTION:
                    if (_gameState == RS_GameStates.CANNON_CARGO ||
                        _gameState == RS_GameStates.CANNON_BY_NAVIGATION)
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.SHIFT_MONSTER_PARTS:
                    if (_gameState == RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER ||
                        _gameState == RS_GameStates.MOVE_COUNTER_BY_SANCTION ||
                        _gameState == RS_GameStates.CANNON_CARGO ||
                        _gameState == RS_GameStates.ANCHOR_SHIP
                        /*_gameState == RS_GameStates.CANNON_BY_NAVIGATION*/) //Prototype
                    {
                        FinalizeCurrentState(toNextState);
                    }
                    break;
                case RS_GameStates.VICTORY_OF_THE_PLAYER:
                    if (_gameState == RS_GameStates.CHOOSE_TOKEN_BY_PLAYER)
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

        public void ReversedGameStateMechanic(RS_GameStates toPreviousState)
        {
            switch (toPreviousState)
            {
                case RS_GameStates.CHOOSE_TOKEN_BY_PLAYER:
                    if (_gameState == RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER)
                    {
                        FinalizeCurrentStateAndGoToPrevious(toPreviousState);
                    }
                    break;
                case RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER:
                    if (_gameState == RS_GameStates.FLICK_TOKEN_BY_PLAYER)
                    {
                        FinalizeCurrentStateAndGoToPrevious(toPreviousState);
                    }
                    break;
            }
        }

        //Ship
        public void ActivateIsLoadedOnTheShipForAllCargoesOfTheSameType(CargoTypes cargoType)
        {
            switch (cargoType)
            {
                case CargoTypes.SCREW_PART:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.SCREW_PART)
                        {
                            cargoLoaded.IsLoadedOnTheShip = true;
                            cargoLoaded.IsAvalaibleForFlicking = false;
                            cargoLoaded.SetHighlight(false);
                        }
                    }
                    break;
                case CargoTypes.CREW_MEMBER:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.CREW_MEMBER)
                        {
                            cargoLoaded.IsLoadedOnTheShip = true;
                            cargoLoaded.IsAvalaibleForFlicking = false;
                            cargoLoaded.SetHighlight(false);
                        }
                    }
                    break;
                case CargoTypes.FUEL:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.FUEL)
                        {
                            cargoLoaded.IsLoadedOnTheShip = true;
                            cargoLoaded.IsAvalaibleForFlicking = false;
                            cargoLoaded.SetHighlight(false);
                        }
                    }
                    break;
                case CargoTypes.MEDICINE:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.MEDICINE)
                        {
                            cargoLoaded.IsLoadedOnTheShip = true;
                            cargoLoaded.IsAvalaibleForFlicking = false;
                            cargoLoaded.SetHighlight(false);
                        }
                    }
                    break;
                case CargoTypes.SUPPLIES:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.SUPPLIES)
                        {
                            cargoLoaded.IsLoadedOnTheShip = true;
                            cargoLoaded.IsAvalaibleForFlicking = false;
                            cargoLoaded.SetHighlight(false);
                        }
                    }
                    break;
            }
        }

        public void DeactivateIsLoadedOnTheShipForAllCargoesOfTheSameType(CargoTypes cargoType)
        {
            switch (cargoType)
            {
                case CargoTypes.SCREW_PART:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.SCREW_PART)
                        {
                            cargoLoaded.IsLoadedOnTheShip = false;
                            cargoLoaded.IsAvalaibleForFlicking = true;
                        }
                    }
                    break;
                case CargoTypes.CREW_MEMBER:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.CREW_MEMBER)
                        {
                            cargoLoaded.IsLoadedOnTheShip = false;
                            cargoLoaded.IsAvalaibleForFlicking = true;
                        }
                    }
                    break;
                case CargoTypes.FUEL:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.FUEL)
                        {
                            cargoLoaded.IsLoadedOnTheShip = false;
                            cargoLoaded.IsAvalaibleForFlicking = true;
                        }
                    }
                    break;
                case CargoTypes.MEDICINE:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.MEDICINE)
                        {
                            cargoLoaded.IsLoadedOnTheShip = false;
                            cargoLoaded.IsAvalaibleForFlicking = true;
                        }
                    }
                    break;
                case CargoTypes.SUPPLIES:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.SUPPLIES)
                        {
                            cargoLoaded.IsLoadedOnTheShip = false;
                            cargoLoaded.IsAvalaibleForFlicking = true;
                        }
                    }
                    break;
            }
        }

        //Island
        public void ActivateIsLoadedOnTheIslandForAllCargoesOfTheSameType(CargoTypes cargoType)
        {
            switch (cargoType)
            {
                case CargoTypes.SCREW_PART:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.SCREW_PART)
                        {
                            cargoLoaded._isLoadedOnTheIsland = true;
                            cargoLoaded.SetHighlight(false);
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.CREW_MEMBER:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.CREW_MEMBER)
                        {
                            cargoLoaded._isLoadedOnTheIsland = true;
                            cargoLoaded.SetHighlight(false);
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.FUEL:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.FUEL)
                        {
                            cargoLoaded._isLoadedOnTheIsland = true;
                            cargoLoaded.SetHighlight(false);
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.MEDICINE:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.MEDICINE)
                        {
                            cargoLoaded._isLoadedOnTheIsland = true;
                            cargoLoaded.SetHighlight(false);
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.SUPPLIES:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.SUPPLIES)
                        {
                            cargoLoaded._isLoadedOnTheIsland = true;
                            cargoLoaded.SetHighlight(false);
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
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

        protected void FinalizeCurrentStateAndGoToPrevious(RS_GameStates toPreviousState)
        {
            FinalizeStateAndGoToPreviousState();
            _gameState = toPreviousState;
            InitializeState();
        }

        protected bool IsAllCargoAndShipPivotStill()
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
            if (!shipPivotOfTheGame.IsStill)
            {
                _isAllCargoStill = false;
            }
            return _isAllCargoStill;
        }

        protected void InitializeGameReferee()
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
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    InitializeLoadingAndOrganizingCargoByPlayerState();
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
                case RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER:
                    FinalizeLoadingAndOrganizingCargoByPlayerState();
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

        protected void FinalizeStateAndGoToPreviousState()
        {
            switch (_gameState)
            {
                case RS_GameStates.CHOOSE_TOKEN_BY_PLAYER:
                    FinalizeContactPointTokenByPlayerState();
                    break;
                case RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER:
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER:
                    FinalizeFlickTokenByPlayerState();
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

        protected void ChangeCameraTo(CinemachineVirtualCameraBase nextCamera)
        {
            if (_currentVirtualCameraBase != null)
            {
                _currentVirtualCameraBase.Priority = 1;
            }
            _currentVirtualCameraBase = nextCamera;
            _currentVirtualCameraBase.Priority = 1000;
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
            UIManager.instance._confirmStateChange = false;
            //UIManager.instance._confirmLoad = false;

            //_originalPositionOfTheFlag = flag.transform.localPosition;
            _aCargoHasTouchedTheShip = false;
            _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;
            _shipPivotHasTouchedTheIsland = false;

            anchorShipSpace.SetActive(false);

            //TODO: Make the proper initialization of the state

            // Putting all cargo at random positions in order to have some variety at the beginning of each game session

            //Setting all cargo in the GHOST state
            foreach (Cargo cargo in allCargoOfTheGame)
            {
                _randomTokenPos = UnityEngine.Random.Range(0, tokenPosList.Count - 1);
                cargo.gameObject.transform.position = tokenPosList[_randomTokenPos].position;
                tokenPosList.RemoveAt(_randomTokenPos);
                cargo.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }

            foreach (MonsterPart monsterPart in allMonsterPartOfTheGame)
            {
                monsterPart.StateMechanic(TokenStateMechanic.SET_SPOOKY);
                monsterPart.IsAvalaibleForFlicking = false;
            }

            if (!IsAllCargoAndShipPivotStill())
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
            ChangeCameraTo(tableFreeLookCamera);

            if(islandOfTheGame._cargoesLoadedOnIsland.Count >= 3)
            {
                GameStateMechanic(RS_GameStates.VICTORY_OF_THE_PLAYER);
            }

            cargoSpaceOnShip.SetActive(false);
            //_originalPositionOfTheFlag = flag.transform.localPosition;
            _aCargoHasTouchedTheShip = false;
            _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;
            _shipPivotHasTouchedTheIsland = false;

            shipPivotOfTheGame.IsAvalaibleForFlicking = true;
            shipPivotOfTheGame.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            shipPivotOfTheGame.SetHighlight(true);

            shipOfTheGame.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            shipOfTheGame.gameObject.transform.position = new Vector3(shipOfTheGame.gameObject.transform.position.x,
                0.0f, shipOfTheGame.gameObject.transform.position.z);

            foreach(Cargo cargo in allCargoOfTheGame)
            {
                if (!cargo.IsLoadedOnTheShip && !cargo._isLoadedOnTheIsland)
                {
                    cargo.StateMechanic(TokenStateMechanic.SET_SPOOKY);
                    cargo.SetHighlight(true);
                    cargo.IsAvalaibleForFlicking = true;
                }
            }

            foreach (Cargo cargoLoaded in islandOfTheGame._cargoesLoadedOnIsland)
            {
                ActivateIsLoadedOnTheIslandForAllCargoesOfTheSameType(cargoLoaded.cargoType);
                cargoLoaded.IsAvalaibleForFlicking = false;
                cargoLoaded.StateMechanic(TokenStateMechanic.SET_RIGID);
                cargoLoaded.SetHighlight(false);
                cargoLoaded.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

                if (cargoLoaded.cargoType == CargoTypes.SCREW_PART)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnIsland[0].gameObject.transform.position;
                }
                if (cargoLoaded.cargoType == CargoTypes.SUPPLIES)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnIsland[1].gameObject.transform.position;
                }
                if (cargoLoaded.cargoType == CargoTypes.MEDICINE)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnIsland[2].gameObject.transform.position;
                }
                if (cargoLoaded.cargoType == CargoTypes.FUEL)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnIsland[3].gameObject.transform.position;
                }
                if (cargoLoaded.cargoType == CargoTypes.CREW_MEMBER)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnIsland[4].gameObject.transform.position;
                }
            }

            foreach (Cargo cargoLoaded in shipOfTheGame._cargoesLoadedOnTheShip)
            {
                ActivateIsLoadedOnTheShipForAllCargoesOfTheSameType(cargoLoaded.cargoType);
                cargoLoaded.IsAvalaibleForFlicking = false;
                cargoLoaded.StateMechanic(TokenStateMechanic.SET_RIGID);
                cargoLoaded.SetHighlight(false);
                cargoLoaded.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

                if (cargoLoaded.cargoType == CargoTypes.SCREW_PART)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnShip[0].gameObject.transform.position;
                }
                if (cargoLoaded.cargoType == CargoTypes.SUPPLIES)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnShip[1].gameObject.transform.position;
                }
                if (cargoLoaded.cargoType == CargoTypes.MEDICINE)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnShip[2].gameObject.transform.position;
                }
                if (cargoLoaded.cargoType == CargoTypes.FUEL)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnShip[3].gameObject.transform.position;
                }
                if (cargoLoaded.cargoType == CargoTypes.CREW_MEMBER)
                {
                    cargoLoaded.gameObject.transform.position = cargoPositionsOnShip[4].gameObject.transform.position;
                }
            }

            foreach(MonsterPart monsterPart in allMonsterPartOfTheGame)
            {
                monsterPart.StateMechanic(TokenStateMechanic.SET_SPOOKY);
                monsterPart.IsAvalaibleForFlicking = false;
            }

            _nearestCargoToTheShip = shipOfTheGame.NearestCargo();
            _nearestCargoToTheShip.IsAvalaibleForFlicking = false;
            _nearestCargoToTheShip.SetHighlight(false);
            print(_nearestCargoToTheShip.gameObject.name);


            //TODO: Set Spooky for ships, monster parts and ship pivots

            //Activate the table camera (with the highest priority)

            //Check available cargo for flicking
        }

        protected void ExecutingChooseTokenByPlayerState()
        {

        }

        protected void FinalizeChooseTokenByPlayerState()
        {
            //table free look camera
            //_currentVirtualCameraBase.Priority = 1;
        }

        #endregion ChooseTokenByPlayer

        #region ContactPointTokenByPlayer

        protected void InitializeContactPointTokenByPlayerState()
        {
            _interactedToken.StateMechanic(TokenStateMechanic.SET_PHYSICS);
            //Focus to the camera rig of the selected token
            //_currentVirtualCameraBase = _interactedToken.GetFreeLookCamera;
            //_currentVirtualCameraBase.Priority = 1000;

            ChangeCameraTo(_interactedToken.GetFreeLookCamera);
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
            _currentVirtualCameraBase.gameObject.GetComponent<CinemachineMobileInputProvider>().enableCameraRig = false;
            CinemachineFreeLook tempCam = (CinemachineFreeLook)_currentVirtualCameraBase;
            tempCam.m_YAxis.Value = 0.0f;
            flag.gameObject.SetActive(true);
        }

        protected void ExecutingFlickTokenByPlayerState()
        {

        }

        protected void FinalizeFlickTokenByPlayerState()
        {
            flag.gameObject.SetActive(false);
            flag.transform.localRotation = Quaternion.identity;
            //flag.gameObject.transform.localPosition = _originalPositionOfTheFlag;
            _nearestCargoToTheShip?.gameObject.SetActive(true);
            _nearestCargoToTheShip = null;
        }

        #endregion

        #region CannonByNavigation

        protected void InitializeCannonByNavigationState()
        {
            islandWall.gameObject.SetActive(true);

            shipOfTheGame.StateMechanic(TokenStateMechanic.SET_PHYSICS);

            //_nearestCargoToTheShip?.gameObject.SetActive(true);
            _nearestCargoToTheShip = null;
            _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                if(cargo.IsLoadedOnTheShip || cargo._isLoadedOnTheIsland)
                {
                    cargo.StateMechanic(TokenStateMechanic.SET_RIGID);
                }
                else
                {
                    cargo.StateMechanic(TokenStateMechanic.SET_PHYSICS);
                }
            }

            foreach (MonsterPart monsterPart in allMonsterPartOfTheGame)
            {
                monsterPart.StateMechanic(TokenStateMechanic.SET_PHYSICS);
            }

            shipPivotOfTheGame.StateMechanic(TokenStateMechanic.SET_PHYSICS);
        }

        protected void ExecutingCannonByNavigationState()
        {
            if (IsAllCargoAndShipPivotStill())
            {
                if (_gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart)
                {
                    GameStateMechanic(RS_GameStates.MOVE_COUNTER_BY_SANCTION);
                }
                else
                {
                    //if (!_shipPivotHasTouchedTheIsland)
                    //{
                    //    GameStateMechanic(RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER);
                    //}
                    GameStateMechanic(RS_GameStates.NAVIGATING_SHIP_OF_THE_PLAYER);
                }
            }
        }

        protected void FinalizeCannonByNavigationState()
        {
            islandWall.gameObject.SetActive(false);

            //_rs_CinemachineTargetGroup.ClearTargets();

            //_aCargoHasTouchedTheShip = false;
            //_gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;
            //_shipPivotHasTouchedTheIsland = false;

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }

            shipPivotOfTheGame.StateMechanic(TokenStateMechanic.SET_SPOOKY);

            shipOfTheGame.StateMechanic(TokenStateMechanic.SET_SPOOKY);
        }

        #endregion

        #region NavigatingShipOfThePlayer

        protected void InitializeNavigatingShipOfThePlayerState()
        {
            foreach(MonsterPart monsterPart in allMonsterPartOfTheGame)
            {
                monsterPart.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }

            //LERP
            distanceBetweenShipAndPivot = 0;
            distanceBetweenShipAndPivot = Vector3.SqrMagnitude(shipOfTheGame.gameObject.transform.position - shipPivotOfTheGame.gameObject.transform.position);

            //while (distanceBetweenShipAndPivot > 3.0f)
            //{
            //    shipOfTheGame.gameObject.transform.position = Vector3.Lerp(shipOfTheGame.gameObject.transform.position,
            //        shipPivotOfTheGame.gameObject.transform.position, Time.fixedDeltaTime * 2.0f);
            //}
            //GameStateMechanic(RS_GameStates.ANCHOR_SHIP);
        }

        protected void ExecutingNavigatingShipOfThePlayerState()
        {
            distanceBetweenShipAndPivot = Vector3.SqrMagnitude(shipOfTheGame.gameObject.transform.position - shipPivotOfTheGame.gameObject.transform.position);

            if(distanceBetweenShipAndPivot >= 20.0f)
            {
                shipOfTheGame.gameObject.transform.position = Vector3.MoveTowards(shipOfTheGame.gameObject.transform.position,
                    shipPivotOfTheGame.gameObject.transform.position, Time.fixedDeltaTime * 3.0f);
            }
            else if (distanceBetweenShipAndPivot <= 19.99f)
            {
                GameStateMechanic(RS_GameStates.ANCHOR_SHIP);
            }

            //while (distanceBetweenShipAndPivot > 3.0f)
            //{
            //    shipOfTheGame.gameObject.transform.position = Vector3.Lerp(shipOfTheGame.gameObject.transform.position,
            //        shipPivotOfTheGame.gameObject.transform.position, Time.fixedDeltaTime * 2.0f);
            //}
            //if(distanceBetweenShipAndPivot <= 2.95f)
            //{
            //    GameStateMechanic(RS_GameStates.ANCHOR_SHIP);
            //}
        }

        protected void FinalizeNavigatingShipOfThePlayerState()
        {

        }

        #endregion

        #region AnchorShip

        protected void InitializeAnchorShipState()
        {
            UIManager.instance._confirmStateChange = false;
            //UIManager.instance._confirmLoad = false;

            //if (_shipPivotHasTouchedTheIsland) //Prototype in order to test states changes
            //{
            //    print("HEAR MEEEEEEEEEEEEEEEEEEEEEE!");
            //    GameStateMechanic(RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER);
            //}
            //else
            //{
            //    GameStateMechanic(RS_GameStates.SHIFT_MONSTER_PARTS);
            //}

            // Continues in RS_MobileInputHandler

            ChangeCameraTo(anchorShipVirtualCamera);
            shipPivotOfTheGame.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            anchorShipSpace.gameObject.SetActive(true);

        }

        protected void ExecutingAnchorShipState()
        {

        }

        protected void FinalizeAnchorShipState()
        {
            anchorShipSpace.SetActive(false);
        }

        #endregion

        #region CannonCargo

        protected void InitializeCannonCargoState()
        {
            islandWall.gameObject.SetActive(true);

            //ChangeCameraTo(_cinemachineTargetGroupCamera);

            shipOfTheGame.StateMechanic(TokenStateMechanic.SET_PHYSICS);

            foreach (MonsterPart monsterPart in allMonsterPartOfTheGame)
            {
                monsterPart.StateMechanic(TokenStateMechanic.SET_PHYSICS);
            }

            _nearestCargoToTheShip?.gameObject.SetActive(true);
            _nearestCargoToTheShip = null;
            _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                if (cargo.IsLoadedOnTheShip || cargo._isLoadedOnTheIsland)
                {
                    cargo.StateMechanic(TokenStateMechanic.SET_RIGID);
                }
                else
                {
                    cargo.StateMechanic(TokenStateMechanic.SET_PHYSICS);
                }
            }

            shipPivotOfTheGame.StateMechanic(TokenStateMechanic.SET_PHYSICS);
        }

        protected void ExecutingCannonCargoState()
        {
            if (IsAllCargoAndShipPivotStill())
            {
                if (_gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart)
                {
                    GameStateMechanic(RS_GameStates.MOVE_COUNTER_BY_SANCTION);
                }
                else
                {
                    if (_aCargoHasTouchedTheShip && (_cargoToBeLoaded != null && !shipOfTheGame._cargoesLoadedOnTheShip.Contains(_cargoToBeLoaded)))
                    {
                        GameStateMechanic(RS_GameStates.LOADING_AND_ORGANIZING_CARGO_BY_PLAYER);
                    }
                    else
                    {
                        GameStateMechanic(RS_GameStates.SHIFT_MONSTER_PARTS);
                    }
                }
            }
        }

        protected void FinalizeCannonCargoState()
        {
            islandWall.gameObject.SetActive(false);

            //_rs_CinemachineTargetGroup.ClearTargets();

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }

            shipPivotOfTheGame.StateMechanic(TokenStateMechanic.SET_SPOOKY);
        }

        #endregion

        #region LoadingAndOrganizingCargoByPlayer

        protected void InitializeLoadingAndOrganizingCargoByPlayerState()
        {
            UIManager.instance._confirmStateChange = false;

            if (_aCargoHasTouchedTheShip)
            {
                ChangeCameraTo(shipVirtualCamera);

                cargoSpaceOnShip.SetActive(true);

                foreach (Cargo cargo in allCargoOfTheGame)
                {
                    if (cargo != _cargoToBeLoaded && !cargo.IsLoadedOnTheShip)
                    {
                        cargo.gameObject.SetActive(false);
                    }
                }

                _cargoToBeLoaded.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }
            if (_shipPivotHasTouchedTheIsland)
            {
                print("LOAD MEEEEEEEEEEEEE!");
                foreach (Cargo cargo in shipOfTheGame._cargoesLoadedOnTheShip)
                {
                    if (!islandOfTheGame._cargoesLoadedOnIsland.Contains(cargo))
                    {
                        cargo._isLoadedOnTheIsland = true;
                        cargo.IsLoadedOnTheShip = false;
                        islandOfTheGame._cargoesLoadedOnIsland.Add(cargo);
                        shipOfTheGame._cargoesLoadedOnTheShip.Remove(cargo);
                        cargo.gameObject.transform.position = cargoPositionsOnIsland[0].position;
                        ActivateIsLoadedOnTheIslandForAllCargoesOfTheSameType(cargo.cargoType);
                    }
                }

                GameStateMechanic(RS_GameStates.SHIFT_MONSTER_PARTS);
            }
            //else
            //{
            //    GameStateMechanic(RS_GameStates.SHIFT_MONSTER_PARTS);
            //}
        }

        protected void ExecutingLoadingAndOrganizingCargoByPlayerState()
        {
            if (_aCargoHasTouchedTheShip)
            {
                ChangeCameraTo(shipVirtualCamera);

                cargoSpaceOnShip.SetActive(true);

                foreach (Cargo cargo in allCargoOfTheGame)
                {
                    if (cargo != _cargoToBeLoaded && !cargo.IsLoadedOnTheShip)
                    {
                        cargo.gameObject.SetActive(false);
                    }
                }

                _cargoToBeLoaded.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }
            if (_shipPivotHasTouchedTheIsland)
            {
                print("LOAD MEEEEEEEEEEEEE!");
                foreach (Cargo cargo in shipOfTheGame._cargoesLoadedOnTheShip)
                {
                    if (!islandOfTheGame._cargoesLoadedOnIsland.Contains(cargo))
                    {
                        cargo._isLoadedOnTheIsland = true;
                        cargo.IsLoadedOnTheShip = false;
                        islandOfTheGame._cargoesLoadedOnIsland.Add(cargo);
                        shipOfTheGame._cargoesLoadedOnTheShip.Remove(cargo);
                        cargo.gameObject.transform.position = cargoPositionsOnIsland[0].position;
                        ActivateIsLoadedOnTheIslandForAllCargoesOfTheSameType(cargo.cargoType);
                    }
                }

                GameStateMechanic(RS_GameStates.SHIFT_MONSTER_PARTS);
            }
        }

        protected void FinalizeLoadingAndOrganizingCargoByPlayerState()
        {
            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.gameObject.SetActive(true);
            }

            _cargoToBeLoaded?.gameObject.SetActive(true);

            _cargoToBeLoaded = null;

            cargoSpaceOnShip.SetActive(false);
        }

        #endregion

        #region MoveCounterBySanction

        protected void InitializeMoveCounterBySanctionState()
        {
            healthPoints -= 1;

            if(healthPoints <= 0)
            {
                UIManager.instance.UpdatePlayerLife(healthPoints);
                GameStateMechanic(RS_GameStates.FAILURE_OF_THE_PLAYER);
            }
            else if(healthPoints >= 1)
            {
                UIManager.instance.UpdatePlayerLife(healthPoints);
                GameStateMechanic(RS_GameStates.SHIFT_MONSTER_PARTS);
            }
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
            foreach (MonsterPart monsterPart in allMonsterPartOfTheGame)
            {
                monsterPart.ValidateSpaceToSpawnMonsterPart();
            }

            //for(int i = 0; i < allMonsterPartOfTheGame.Length; i++)
            //{
            //    allMonsterPartOfTheGame[i].ValidateSpaceToSpawnMonsterPart();
            //}

            GameStateMechanic(RS_GameStates.CHOOSE_TOKEN_BY_PLAYER);
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
            //print("Player has won!");
            _generalGameReferee.WinGame();
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
            //print("Player has lost!");
            _generalGameReferee.LoseGame();
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

        public RS_GameStates GameState
        {
            get { return _gameState; }
            set { _gameState = value; }
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
            get { return flag; }
        }

        public bool SetGameRefereeHasConfirmedThatNoCargoOrShipPivotHaveTouchedAMonsterPart
        {
            set { _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = value; }
        }

        public Cargo CargoToBeLoaded
        {
            set { _cargoToBeLoaded = value; }
            get { return _cargoToBeLoaded; }
        }

        public int GetHealthPoints
        {
            get { return healthPoints; }
        }

        public RS_CinemachineTargetGroup GetCMTargetGroup
        {
            get { return _rs_CinemachineTargetGroup; }
        }

        #endregion
    }
}