using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dll_Project.Plaza.Fly
{
    public class AnimSync : DllGenerateBase
    {
        public static AnimSync Instance;
        private Transform animTf;
        private Animator anim;

        public override void Init()
        {
            animTf = BaseMono.ExtralDatas[0].Target;
            anim = animTf.GetComponent<Animator>();
        }
        #region 初始
        public override void Awake()
        {
            Instance = this;
        }

        public override void Start()
        {
            GetAnimatorPCT();
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void Update()
        {
            
            //if (Input.GetKeyUp(KeyCode.P))
            //{
            //    anim.Play("yuanhuanzhuan", 0, 0.2f);
            //}
        }
        #endregion

        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("dimian_collider"))
            {
                GetAnimatorPCT();
            }
        }
        /// <summary>
        /// 给所有人发送消息
        /// </summary>
        public void GetAnimatorPCT()
        {
            if (mStaticThings.AllActiveAvatarList.Count>0) 
            {
                WsCChangeInfo wsinfo = new WsCChangeInfo()
                {
                    a = "GetAnimPCT",
                    b = mStaticThings.I.mAvatarID,
                    c = mStaticThings.AllActiveAvatarList[0]
                };
                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
            }
        }
        /// <summary>
        /// 接受所有人员发送消息
        /// </summary>
        /// <param name="msg"></param>
        private void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (info.a.Equals("GetAnimPCT"))
            {
                if (info.c == mStaticThings.I.mAvatarID)
                {
                    GetAnimPCT(info.b);
                }
            }
            else if (info.a.Equals("ShowAnimPCT")) 
            {
                if (info.b == mStaticThings.I.mAvatarID) 
                {
                    anim.Play("yuanhuanzhuan", 0, float.Parse(info.c));
                }
            }
        }

        #region 获取动画百分比
        float length;
        float frameRate;
        float totalFrame=1796;
        float currentTime;
        int currentFrame;

        float animPct;//现在帧百分比
        private void GetData() 
        {
            //动画片段长度
            //length = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            //length = 29.93333f;
            //获取动画片段帧频
            //frameRate = anim.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;
            //frameRate = 60;
            //计算动画片段总帧数
            //totalFrame = length / (1 / frameRate);
            totalFrame = 1796;
        }
        /// <summary>
        /// 获取中心塔动画百分比
        /// </summary>
        private void GetAnimPCT(string mavatorid) 
        {
            //当前动画机播放时长
            currentTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime; 
            //计算当前播放的动画片段运行至哪一帧
            currentFrame = (int)(Mathf.Floor(totalFrame * currentTime) % totalFrame);

            animPct = currentFrame / totalFrame;

            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = "ShowAnimPCT",
                b = mavatorid,
                c = animPct.ToString()
            };
            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
        }
        #endregion
    }
}
