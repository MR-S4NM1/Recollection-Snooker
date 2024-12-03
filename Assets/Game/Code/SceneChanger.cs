using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MrSanmi.RecollectionSnooker
{
    public class SceneChanger : MonoBehaviour
    {
        public static SceneChanger instance; // Singleton :P

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

        public void ChangeSceneTo(int sceneID)
        {
            SceneManager.LoadScene(sceneID);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}