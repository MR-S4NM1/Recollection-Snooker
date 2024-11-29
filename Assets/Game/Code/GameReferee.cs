using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MrSanmi.RecollectionSnooker
{
    #region Enums

    public enum GeneralGameStates
    {
        GAME, //= 0
        PAUSE, //= 1
        GAME_OVER, //= 2
        VICTORY
    }

    #endregion

    public class GameReferee : MonoBehaviour
    {
        #region References

        #endregion

        #region RuntimeVariables

        [SerializeField] protected GeneralGameStates _generalGameState;

        #endregion

        #region UnityMethods
        void Start() //Runtime
        {
            InitializeGameState();
        }

        #endregion

        #region PublicMethods

        public void PauseGame()
        {
            //State Mechanics / Actions to move within the Finite State Machine
            if (_generalGameState == GeneralGameStates.GAME)
            {
                FinalizeGameState();
                //I should go to pause
                _generalGameState = GeneralGameStates.PAUSE;
                InitializePauseState();
            }
            else if (_generalGameState == GeneralGameStates.PAUSE)
            {
                FinalizePauseState();
                //I return to the game
                _generalGameState = GeneralGameStates.GAME;
                InitializeGameState();
            }
        }

        public void WinGame()
        {
            //State Mechanics / Actions to move within the Finite State Machine
            if (_generalGameState == GeneralGameStates.GAME)
            {
                FinalizeGameState();
                //I should go to pause
                _generalGameState = GeneralGameStates.VICTORY;
                InitializeWinningState();
            }
        }
        public void LoseGame()
        {
            //State Mechanics / Actions to move within the Finite State Machine
            if (_generalGameState == GeneralGameStates.GAME)
            {
                FinalizeGameState();
                //I should go to pause
                _generalGameState = GeneralGameStates.GAME_OVER;
                InitializeLosingState();
            }
        }

        #endregion

        #region GameStateMethods

        #region GameState

        protected void InitializeGameState()
        {
            Time.timeScale = 1f;
        }

        protected void ExecutingGameState()
        {
            //Nothing to do
        }

        protected void FinalizeGameState()
        {
            Time.timeScale = 0f;
        }

        #endregion

        #region PauseState

        protected void InitializePauseState()
        {
            Time.timeScale = 0f;
            UIManager.instance.ActivateAndDeactivatePausePanel(/*IsOnGame*/ true);
        }

        protected void ExecutingPauseState()
        {
            //TODO: Pending
        }

        protected void FinalizePauseState()
        {
            Time.timeScale = 1f;
            UIManager.instance.ActivateAndDeactivatePausePanel(/*IsOnGame*/ false);
        }

        #endregion

        #region WinningState

        protected void InitializeWinningState()
        {
            UIManager.instance.ActivateVictoryPanel();
        }

        protected void ExecutingWinningState()
        {
            //TODO: Pending
        }

        protected void FinalizeWinningState()
        {

        }

        #endregion

        #region LosingState

        protected void InitializeLosingState()
        {
            UIManager.instance.ActivateDefeatPanel();
        }

        protected void ExecutingLosingState()
        {
            //TODO: Pending
        }

        protected void FinalizeLosingState()
        {

        }

        #endregion

        #region Getters

        public bool IsOnGame
        {
            get { return _generalGameState == GeneralGameStates.GAME; }
        }

        #endregion

        #endregion GameStateMethods
    }
}
