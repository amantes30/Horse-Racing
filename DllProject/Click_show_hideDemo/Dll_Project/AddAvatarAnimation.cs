using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slate;
using System;

namespace Dll_Project
{
    public class AddAvatarAnimation : DllGenerateBase
    {

        public override void Init()
        {
            
            foreach(Transform k in BaseMono.ExtralDatas[0].Target.GetComponentInChildren<Transform>())
            {
                k.gameObject.AddComponent<AnimationListener>();
                k.gameObject.GetComponent<AnimationListener>().Start();
                k.gameObject.GetComponent<AnimationListener>().Update();
            }
            
        }
    }
}
