using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrSanmi.RecollectionSnooker
{
    public class RS_CinemachineTargetGroup : CinemachineTargetGroup
    {
        public void ClearTargets()
        {
            m_Targets = new Target[0];
        }
    }
}