using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MrSanmi.Game;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using System;

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

        [Header("Camera References")]
        [SerializeField] protected CinemachineFreeLook tableFreeLookCamera;
        [SerializeField] protected CinemachineVirtualCamera shipVirtualCamera;
        [SerializeField] protected CinemachineVirtualCamera islandVirtualCamera;

        [Header("Flags")]
        [SerializeField] protected GameObject flag;

        [SerializeField] protected TextMeshProUGUI debugText;

        [Header("Random Token Positions")]
        [SerializeField] protected List<Transform> tokenPosList;

        [Header("Health Points")]
        [SerializeField] protected int healthPoints;

        #endregion

        #region RuntimeVariables

        protected new RS_GameStates _gameState;
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
                    Debug.Log(_gameState + " entra a flick token by player");
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
                        _gameState == RS_GameStates.CANNON_BY_NAVIGATION)
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
                        _gameState == RS_GameStates.CANNON_BY_NAVIGATION)
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

        //Ship
        public void ActivateIsLoadedOnTheShipForAllCargoesOfTheSameType(CargoTypes cargoType)
        {
            //if(shipOfTheGame._cargoesLoadedOnTheShip.Count > 0)
            //{
            //    foreach(Cargo cargo in shipOfTheGame._cargoesLoadedOnTheShip)
            //    {
            //        if (cargo.cargoType == cargoType)
            //        {

            //        }
            //    }
            //}

            switch (cargoType)
            {
                case CargoTypes.SCREW_PART:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.SCREW_PART)
                        {
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.CREW_MEMBER:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.CREW_MEMBER)
                        {
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.FUEL:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.FUEL)
                        {
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.MEDICINE:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.MEDICINE)
                        {
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.SUPPLIES:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.SUPPLIES)
                        {
                            cargoLoaded.IsAvalaibleForFlicking = false;
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
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.CREW_MEMBER:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.CREW_MEMBER)
                        {
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.FUEL:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.FUEL)
                        {
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.MEDICINE:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.MEDICINE)
                        {
                            cargoLoaded.IsAvalaibleForFlicking = false;
                        }
                    }
                    break;
                case CargoTypes.SUPPLIES:
                    foreach (Cargo cargoLoaded in allCargoOfTheGame)
                    {
                        if (cargoLoaded.cargoType == CargoTypes.SUPPLIES)
                        {
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
            //_originalPositionOfTheFlag = flag.transform.localPosition;
            _aCargoHasTouchedTheShip = false;
            _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;
            _shipPivotHasTouchedTheIsland = false;

            //TODO: Make the proper initialization of the state

            // Putting all cargo at random positions in order to have some variety at the beginning of each game session

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                _randomTokenPos = UnityEngine.Random.Range(0, tokenPosList.Count - 1);
                cargo.gameObject.transform.position = tokenPosList[_randomTokenPos].position;
                tokenPosList.RemoveAt(_randomTokenPos);
            }

            //Setting all cargo in the GHOST state

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.StateMechanic(TokenStateMechanic.SET_SPOOKY);
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

            cargoSpaceOnShip.SetActive(false);
            //_originalPositionOfTheFlag = flag.transform.localPosition;
            _aCargoHasTouchedTheShip = false;
            _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;
            _shipPivotHasTouchedTheIsland = false;

            shipPivotOfTheGame.IsAvalaibleForFlicking = true;

            shipPivotOfTheGame.StateMechanic(TokenStateMechanic.SET_SPOOKY);

            for(int i = 0; i < shipOfTheGame._cargoesLoadedOnTheShip.Count; ++i)
            {
                ActivateIsLoadedOnTheShipForAllCargoesOfTheSameType(shipOfTheGame._cargoesLoadedOnTheShip[i].cargoType);
                shipOfTheGame._cargoesLoadedOnTheShip[i].gameObject.transform.position = cargoPositionsOnShip[i].position;
                shipOfTheGame._cargoesLoadedOnTheShip[i].StateMechanic(TokenStateMechanic.SET_RIGID);
            }

            for (int i = 0; i < islandOfTheGame._cargoesLoadedOnIsland.Count; ++i)
            {
                ActivateIsLoadedOnTheIslandForAllCargoesOfTheSameType(islandOfTheGame._cargoesLoadedOnIsland[i].cargoType);
                islandOfTheGame._cargoesLoadedOnIsland[i].gameObject.transform.position = cargoPositionsOnIsland[i].position;
                islandOfTheGame._cargoesLoadedOnIsland[i].StateMechanic(TokenStateMechanic.SET_RIGID);
            }

            _nearestCargoToTheShip = shipOfTheGame.NearestCargo();

            _nearestCargoToTheShip.gameObject.SetActive(false);

            //All cargo is set to Spooky
            //for (int i = 0; i < allCargoOfTheGame.Length - 1; ++i)
            //{
            //    if (allCargoOfTheGame[i].IsLoadedOnTheShip || allCargoOfTheGame[i]._isLoadedOnTheIsland)
            //    {
            //        allCargoOfTheGame[i].IsAvalaibleForFlicking = false;
            //    }
            //    else
            //    {
            //        allCargoOfTheGame[i].StateMechanic(TokenStateMechanic.SET_SPOOKY);
            //    }
            //}

                //}
                //TODO: Set Spooky for ships, monster parts and ship pivots

                //Activate the table camera (with the highest priority)

            //Check available cargo for flicking
            foreach (Cargo cargo in allCargoOfTheGame)
            {
                if (!cargo.IsLoadedOnTheShip || !cargo._isLoadedOnTheIsland)
                {
                    cargo.SetHighlight(true);
                    cargo.IsAvalaibleForFlicking = true;
                }
                else
                {
                    cargo.SetHighlight(false);
                    cargo.IsAvalaibleForFlicking = false;
                }
            }

            _nearestCargoToTheShip.IsAvalaibleForFlicking = false;
        }

        protected void ExecutingChooseTokenByPlayerState()
        {

        }

        protected void FinalizeChooseTokenByPlayerState()
        {
            //table free look camera
            _currentVirtualCameraBase.Priority = 1;
        }

        #endregion ChooseTokenByPlayer

        #region ContactPointTokenByPlayer

        protected void InitializeContactPointTokenByPlayerState()
        {
            _interactedToken.StateMechanic(TokenStateMechanic.SET_PHYSICS);
            //Focus to the camera rig of the selected token
            _currentVirtualCameraBase = _interactedToken.GetFreeLookCamera;
            _currentVirtualCameraBase.Priority = 1000;
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
            _nearestCargoToTheShip.gameObject.SetActive(true);
            _nearestCargoToTheShip = null;
        }

        #endregion

        #region CannonByNavigation

        protected void InitializeCannonByNavigationState()
        {
            _nearestCargoToTheShip?.gameObject.SetActive(true);
            _nearestCargoToTheShip = null;
            _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.StateMechanic(TokenStateMechanic.SET_PHYSICS);
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
                    if (_shipPivotHasTouchedTheIsland)
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

        protected void FinalizeCannonByNavigationState()
        {
            _aCargoHasTouchedTheShip = false;
            _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;
            _shipPivotHasTouchedTheIsland = false;

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }

            shipPivotOfTheGame.StateMechanic(TokenStateMechanic.SET_SPOOKY);
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
            _nearestCargoToTheShip?.gameObject.SetActive(true);
            _nearestCargoToTheShip = null;
            _gameRefereeHasConfirmedThatACargoOrShipPivotHasTouchedAMonsterPart = false;

            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.StateMechanic(TokenStateMechanic.SET_PHYSICS);
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
            else if (_shipPivotHasTouchedTheIsland)
            {
                ChangeCameraTo(islandVirtualCamera);

                foreach (Cargo cargo in allCargoOfTheGame)
                {
                    if (cargo != _cargoToBeLoaded && !cargo.IsLoadedOnTheShip)
                    {
                        cargo.gameObject.SetActive(false);
                    }
                    else if (cargo.IsLoadedOnTheShip)
                    {
                        cargo.gameObject.SetActive(false);
                    }
                }

                _cargoToBeLoaded.StateMechanic(TokenStateMechanic.SET_SPOOKY);
            }
        }

        protected void ExecutingLoadingAndOrganizingCargoByPlayerState()
        {

        }

        protected void FinalizeLoadingAndOrganizingCargoByPlayerState()
        {
            foreach (Cargo cargo in allCargoOfTheGame)
            {
                cargo.gameObject.SetActive(true);
            }

            _cargoToBeLoaded.gameObject.SetActive(true);

            _cargoToBeLoaded = null;

            cargoSpaceOnShip.SetActive(false);
        }

        #endregion

        #region MoveCounterBySanction

        protected void InitializeMoveCounterBySanctionState()
        {
            if(healthPoints <= 1)
            {
                print("You lose the game");
                GameStateMechanic(RS_GameStates.FAILURE_OF_THE_PLAYER);
            }
            else if(healthPoints > 1)
            {
                healthPoints -= 1;
                print("You lose a life");
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

            for(int i = 0; i < allMonsterPartOfTheGame.Length; i++)
            {
                allMonsterPartOfTheGame[i].ValidateSpaceToSpawnMonsterPart();
            }

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

        #endregion
    }
}