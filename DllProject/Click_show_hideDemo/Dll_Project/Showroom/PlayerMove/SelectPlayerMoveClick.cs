using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.Showroom.PlayerMove
{
    public class SelectPlayerMoveClick : DllGenerateBase
    {
        public override void Init()
        {
        }
        #region 初始化
        public override void Awake()
        {
        }

        public override void Start()
        {
        }
        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }

        public override void Update()
        {
            if (BaseMono.GetComponent<Toggle>().isOn)
            {
                if (!mStaticData.MovePlayerList.Contains(BaseMono.OtherData))
                {
                    mStaticData.MovePlayerList.Add(BaseMono.OtherData);
                }
            }
            else 
            {
                if (mStaticData.MovePlayerList.Contains(BaseMono.OtherData))
                {
                    mStaticData.MovePlayerList.Remove(BaseMono.OtherData);
                }
            }
        }
        #endregion
    }
}
