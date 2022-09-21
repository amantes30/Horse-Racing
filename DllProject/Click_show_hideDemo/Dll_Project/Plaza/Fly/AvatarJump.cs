using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Dll_Project.Showroom;
using System.Collections;

namespace Dll_Project.Plaza.Fly
{
    public class AvatarJump : DllGenerateBase
    {
        public ExtralData[] jumpEndPointList;
        private Transform ColliderParent;

        private GameObject uiPlane;
        private Button cancelBtn;
        private Button sureBtn;

        private GameObject SpiritObject;
        public override void Init()
        {
            jumpEndPointList = BaseMono.ExtralDatas[0].Info;
            ColliderParent = BaseMono.ExtralDatas[1].Target;

            uiPlane = BaseMono.ExtralDatas[2].Target.gameObject;
            cancelBtn = BaseMono.ExtralDatas[3].Target.GetComponent<Button>();
            sureBtn = BaseMono.ExtralDatas[4].Target.GetComponent<Button>();
            SpiritObject = BaseMono.ExtralDatas[5].Target.gameObject;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            cancelBtn.onClick.AddListener(() => { uiPlane.SetActive(false); });
            sureBtn.onClick.AddListener(() => 
            {
                uiPlane.SetActive(false); 
                Move(jumpEndPointList[9].Target);
                SaveInfo.instance.SaveActionData("SceneTeleport", 3);
                if (mStaticThings.I.isVRApp)
                {
                    uiPlane.transform.parent.parent.gameObject.SetActive(false);
                }
            });
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void Update()
        {
            //if (mStaticThings.I != null)
            //{
            //    ColliderParent.position = mStaticThings.I.Maincamera.position;
            //    ColliderParent.rotation = mStaticThings.I.Maincamera.rotation;

            //    if (ColliderCtrl.Instance.isOpen) 
            //    {
            //        if (control != null)
            //        {
            //            ColliderCtrl.Instance.isOpen = false;
            //            control.enabled = true;
            //            sequence.Kill();
            //        }
            //    }
            //}
            
        }
        #endregion
        #region
        CharacterController control;
        Sequence sequence;
        /// <summary>
        /// 地面检测
        /// </summary>
        /// <param name="msg"></param>
        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("area_jump1"))
            {
                Jump(jumpEndPointList[0].Target);
            }
            else if (name.Equals("area_jump2"))
            {
                Jump(jumpEndPointList[1].Target);
            }
            else if (name.Equals("area_jump11"))
            {
                Jump(jumpEndPointList[10].Target);
            }
            else if (name.Equals("area_jump12"))
            {
                Jump(jumpEndPointList[11].Target);
            }
            else if (name.Equals("area_jump3"))
            {
                SaveInfo.instance.SaveActionData("Jump", 15);
                Jump(jumpEndPointList[2].Target);
            }
            else if (name.Equals("area_jump4"))
            {
                Jump(jumpEndPointList[3].Target);
            }
            else if (name.Equals("area_jump5"))
            {
                Jump(jumpEndPointList[4].Target);
            }
            else if (name.Equals("area_jump6"))
            {
                Jump(jumpEndPointList[5].Target);
            }
            else if (name.Equals("area_jump7"))
            {
                Jump(jumpEndPointList[6].Target);
            }
            else if (name.Equals("area_jump8"))
            {
                Jump(jumpEndPointList[7].Target);
            }
            else if (name.Equals("area_jump9"))
            {
                Jump(jumpEndPointList[8].Target);
            }
            else if (name.Equals("area_jump10"))
            {
                SaveInfo.instance.SaveActionData("Jump", 15);
                Jump(jumpEndPointList[9].Target);
            }
            else if (name.Equals("area_dt"))
            {
                uiPlane.SetActive(true);
                if (mStaticThings.I.isVRApp)
                {
                    uiPlane.transform.parent.parent.gameObject.SetActive(true);
                }
            }
            else 
            {
                uiPlane.SetActive(false);
            }
        }

        private void Jump(Transform target)
        {
            control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
            if (control != null)
            {
                control.enabled = false;
            }
            SpiritObject.SetActive(false);
            sequence = mStaticThings.I.MainVRROOT.DOLocalJump(target.position, 5, 1, 2, false).OnComplete(() =>
            {
                control.enabled = true;
            });
            BaseMono.StartCoroutine(ShowSpirit());
        }
        private IEnumerator ShowSpirit() 
        {
            yield return new WaitForSeconds(2.1f);
            SpiritObject.SetActive(true);
        }
        private void Move(Transform target) 
        {
            control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
            if (control != null)
            {
                control.enabled = false;
            }
            mStaticThings.I.MainVRROOT.position = target.position;
            control.enabled = true;
        }
        #endregion
    }
}
