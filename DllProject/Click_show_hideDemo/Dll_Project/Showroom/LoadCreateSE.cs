using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dll_Project.Showroom
{
    public class LoadCreateSE : DllGenerateBase
    {
        private GameObject SEPrefab;
        public override void Init()
        {
            SEPrefab = BaseMono.ExtralDataObjs[0].Target as GameObject;
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
            MessageDispatcher.AddListener(VrDispMessageType.VRLoadAvatarDone.ToString(), VrLoadAvavtar);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.VRLoadAvatarDone.ToString(), VrLoadAvavtar);
        }

        public override void Update()
        {
        }
        #endregion

        private void VrLoadAvavtar(IMessage msg)
        {
            Transform go = msg.Data as Transform;
            if (go != null) 
            {
                var temp = GameObject.Instantiate(SEPrefab, go.transform);
                GameObject.Destroy(temp, 2f);
            }
        }
    }
}
