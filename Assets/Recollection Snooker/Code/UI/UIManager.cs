using MrSanmi.RecollectionSnooker;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrSanmi.RecollectionSnooker
{
    public class UIManager : MonoBehaviour
    {
        #region References

        public static UIManager instance;

        [SerializeField] protected RS_GameReferee _gameReferee;
        [SerializeField] protected GameObject _gamePanel;
        [SerializeField] protected GameObject _victoryPanel;
        [SerializeField] protected GameObject _defeatPanel;
        [SerializeField] protected GameObject _pausePanel;
        [SerializeField] protected TextMeshProUGUI _hpNarrator;
        [SerializeField] protected RS_MobileInputHandler _mobileInputHandler;

        #endregion

        #region RuntimeVariables

        [SerializeField] public bool _confirmStateChange;
        [SerializeField] public bool _confirmLoad;


        #endregion

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnDrawGizmos()
        {
            if (_gameReferee == null)
            {
                _gameReferee = GameObject.FindAnyObjectByType<RS_GameReferee>();
            }
        }

        private void Start()
        {
            _gamePanel.SetActive(true);
            _confirmStateChange = false;
            _victoryPanel.gameObject.SetActive(false);
            _defeatPanel.gameObject.SetActive(false);
            _hpNarrator.text = _gameReferee.GetHealthPoints.ToString();
        }

        private void FixedUpdate()
        {
            print(_confirmStateChange + "Has been confirmed change of state");
        }

        public void ChangeBackwardsCurrentGameState()
        {
            switch (_gameReferee.GameState)
            {
                case RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER:
                    _gameReferee.ReversedGameStateMechanic(RS_GameStates.CHOOSE_TOKEN_BY_PLAYER);
                    break;
                case RS_GameStates.FLICK_TOKEN_BY_PLAYER:
                    _gameReferee.ReversedGameStateMechanic(RS_GameStates.CONTACT_POINT_TOKEN_BY_PLAYER);
                    break;
            }
        }

        public void ConfirmToFinishState()
        {
            _confirmStateChange = true;
            _mobileInputHandler.ChangeFromAnchorShipToNextState();
            _mobileInputHandler.ChangeFromLoadingCargoByPlayerStateOnShipToNext();
        }

        public void ConfirmLoad()
        {
            _confirmLoad = true;
        }

        public void ActivateVictoryPanel()
        {
            _gamePanel.gameObject.SetActive(false);
            _victoryPanel.gameObject.SetActive(true);
        }

        public void ActivateDefeatPanel()
        {
            _gamePanel.gameObject.SetActive(false);
            _defeatPanel.gameObject.SetActive(true);
        }

        public void ActivateAndDeactivatePausePanel(bool pausePanelBool)
        {
            switch (pausePanelBool)
            {
                case true:
                    _gamePanel.gameObject.SetActive(false);
                    _pausePanel.gameObject.SetActive(true);
                    break;
                case false:
                    _gamePanel.gameObject.SetActive(true);
                    _pausePanel.gameObject.SetActive(false);
                    break;
            }
        }

        public void UpdatePlayerLife (int playerHP)
        {
            _hpNarrator.text = playerHP.ToString();
        }
    }
}

