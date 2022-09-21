using com.ootii.Messages;
using DG.Tweening;
using Dll_Project.Plaza.Music;
using Dll_Project.Showroom;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace Dll_Project.Plaza.Fly
{
    public class AvatarFly : DllGenerateBase
    {
        private Transform pointParent;
        public Vector3[] Pos;

        private GameObject uiPlane;
        private Button cancelBtn;
        private Button sureBtn;

        private GameObject SpiritObject;
        public override void Init()
        {
            pointParent = BaseMono.ExtralDatas[0].Target;
            Pos = new Vector3[pointParent.childCount];
            for (int i = 0; i < pointParent.childCount; i++)
            {
                Pos[i] = pointParent.GetChild(i).position;
            }

            uiPlane = BaseMono.ExtralDatas[1].Target.gameObject;
            cancelBtn = BaseMono.ExtralDatas[2].Target.GetComponent<Button>();
            sureBtn = BaseMono.ExtralDatas[3].Target.GetComponent<Button>();
            SpiritObject = BaseMono.ExtralDatas[4].Target.gameObject;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            for (int i = 1; i < Pos.Length; i++)
            {
                Pos[i] = new Vector3(Pos[i].x, UnityEngine.Random.Range(25f, 70f), Pos[i].z);
            }

            cancelBtn.onClick.AddListener(() => { uiPlane.SetActive(false); });
            sureBtn.onClick.AddListener(() =>
            {
                uiPlane.SetActive(false);
                Fly();
                SaveInfo.instance.SaveActionData("Fly", 3);
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
        }
        #endregion

        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("area_two"))
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

        private bool isOpen = true;
        /// <summary>
        /// 人物飞行
        /// </summary>
        private void Fly()
        {
            if (isOpen == false)
                return;
            MusicCtrl.Instance.PlayBgMusic(false);
            MusicCtrl.Instance.PlayFlyMusic(true);
            isOpen = false;
            CharacterController control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
            if (control != null)
                control.enabled = false;
            SpiritObject.SetActive(false);
            mStaticThings.I.MainVRROOT.position = Pos[0];
            float ti = UnityEngine.Random.Range(50, 80);
            mStaticThings.I.MainVRROOT.DOPath(Pos, ti, PathType.CatmullRom, PathMode.Full3D, 100, Color.yellow).SetOptions(true).OnComplete(() =>
            {
                if (control != null)
                    control.enabled = true;
                isOpen = true;
                SpiritObject.SetActive(true);
                mStaticThings.I.MainVRROOT.rotation = new Quaternion(0, 0, 0, 0);
                MusicCtrl.Instance.PlayBgMusic(true);
                MusicCtrl.Instance.PlayFlyMusic(false);
            }).OnWaypointChange(p=>{ MoverOver(ti); });
        }
        /// <summary>
        /// 看向目标点
        /// </summary>
        private void MoverOver(float ti) 
        {
            if (!mStaticThings.I.isVRApp) 
            {
                mStaticThings.I.MainVRROOT.DOLookAt(new Vector3(-102.8935f, 34.24763f, 3.81422f), ti/Pos.Length, AxisConstraint.Y, null);
            }
        }
    }
}
