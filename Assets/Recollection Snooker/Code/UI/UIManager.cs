using MrSanmi.Game;
using MrSanmi.RecollectionSnooker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrSanmi.RecollectionSnooker
{
    public class UIManager : MonoBehaviour
    {
        #region

        public static UIManager instance;

        [SerializeField] protected RS_GameReferee _gameReferee;

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
        }
    }

