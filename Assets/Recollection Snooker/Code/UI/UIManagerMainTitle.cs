using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrSanmi.RecollectionSnooker
{
    public class UIManagerMainTitle : MonoBehaviour
    {
        #region References

        public static UIManagerMainTitle instance;

        [SerializeField] protected GameObject mainPanel;
        [SerializeField] protected GameObject creditsPanel;

        #endregion

        #region UnityMethods

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

        #endregion

        #region PublicMethods

        public void ActivateCreditsPanel()
        {
            mainPanel.SetActive(false);
            creditsPanel.SetActive(true);
        }

        public void ActivateMainPanel()
        {
            creditsPanel.SetActive(false);
            mainPanel.SetActive(true);
        }

        #endregion
    }

}