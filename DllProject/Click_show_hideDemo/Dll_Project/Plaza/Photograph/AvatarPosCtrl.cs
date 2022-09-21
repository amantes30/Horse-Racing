using com.ootii.Messages;
using DG.Tweening;
using LitJson;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.Plaza.Photograph
{
    public class AvatarPosCtrl : DllGenerateBase
    {
        private Transform groupPlayer;
        private Button standBtn;

        private List<string> avatarList = new List<string>();
        private bool isPhoto = false;

        public override void Init()
        {
            groupPlayer = BaseMono.ExtralDatas[0].Target;
            standBtn = BaseMono.ExtralDatas[1].Target.GetComponent<Button>();
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            standBtn.onClick.AddListener(GetAvatarList);
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        }

        public override void Update()
        {
            //if (Input.GetKeyUp(KeyCode.Z)) 
            //{
            //    GetAvatarList();
            //}
        }
        #endregion
        /// <summary>
        /// 地面检测
        /// </summary>
        /// <param name="msg"></param>
        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("area_photo"))
            {
                isPhoto = true;
                if (mStaticData.Identity.Camera.Contains(mStaticThings.I.mAvatarID))
                {
                    standBtn.gameObject.SetActive(true);
                }
            }
            else
            {
                isPhoto = false;

                standBtn.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 接受所有人员发送消息
        /// </summary>
        /// <param name="msg"></param>
        private void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (info.a == "PhotoPosClick") 
            {
                if (isPhoto) 
                {
                    var temp = JsonMapper.ToObject<List<string>>(info.b);
                    if (temp.Contains(mStaticThings.I.mAvatarID)) 
                    {
                        var sort = temp.IndexOf(mStaticThings.I.mAvatarID);
                        JumpPos(groupPlayer, sort);
                    }
                }
            }
        }
        private void JumpPos(Transform group,int index)
        {
            int max = group.GetComponent<VRPlayceGroup>()._VRPlayceDots.Count - 1;
            int playcenum = Mathf.Clamp(index, 0, max);

            CharacterController control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
            if (control != null)
                control.enabled = false;

            mStaticThings.I.MainVRROOT.position = group.GetComponent<VRPlayceGroup>()._VRPlayceDots[playcenum].transform.position;
            mStaticThings.I.MainVRROOT.rotation = group.GetComponent<VRPlayceGroup>()._VRPlayceDots[playcenum].transform.rotation;
            if (control != null)
                control.enabled = true;
        }
        /// <summary>
        /// 获取拍照平台人员列表
        /// </summary>
        private void GetAvatarList() 
        {
            avatarList.Clear();
            Collider[] cols = Physics.OverlapSphere(mStaticThings.I.Maincamera.position, 7);
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].name == "Canvas") 
                {
                    var temp = cols[i].transform.parent.parent.parent;
                    if (temp.parent.name == "_WsAvatarsRoot") 
                    {
                        avatarList.Add(temp.name);
                    }
                }
            }

            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = "PhotoPosClick",
                b = JsonMapper.ToJson(avatarList)
            };
            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
        }
    }
}
