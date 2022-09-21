using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dll_Project.Plaza.Fly
{
    public class ColliderCtrl : DllGenerateBase
    {
        public static ColliderCtrl Instance;
        public bool isOpen = false;
        public override void Init()
        {
            
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            Instance = this;
        }
        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }

        public override void Update()
        {
        }
        #endregion

        public override void OnTriggerEnter(Collider other)
        {
            if (other.name== "collider") 
            {
                isOpen = true;
            }
        }
    }
}
