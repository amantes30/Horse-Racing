using com.ootii.Messages;
using Dll_Project.Showroom;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project
{
    public class BroadCastAgoraVoiceControl : DllGenerateBase
    {
        private string BroadcastAgoraVoice = "BroadcastAgoraVoice";
        public static string BroadcastChanelId = "RADIO";
        private bool bEnterBroadcast = false;
        private GameObject broadcast;
        private AudioSource audio;
        public override void Awake()
        {
            base.Awake();
            //broadcast = BaseMono.ExtralDatas[0].Target.gameObject;
            audio = BaseMono.ExtralDatas[1].Target.GetComponentInChildren<AudioSource>();
        }
        public override void Start()
        {
            base.Start();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            //MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            //MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClick);

            //MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }
        public override void OnDisable()
        {
            base.OnDisable();
            //MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            //MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClick);

            //MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Update()
        {
            base.Update();
            //if (mStaticThings.I!=null) 
            //{
            //    if (!mStaticThings.I.isAdmin && bEnterBroadcast == true)
            //    {
            //        broadcast.GetComponent<Renderer>().material = BaseMono.ExtralDataObjs[0].Target as Material;
            //        EndBroadCastVioce();
            //    }
            //}
        }

        private void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (info.a == BroadcastAgoraVoice)
            {
                int broadcast = int.Parse(info.b);
                if (broadcast > 0)
                {
                    audio.Play();
                    string broadcastid = "";
                    if (info.c == mStaticThings.I.mAvatarID)
                    {
                        broadcastid = BroadcastChanelId + "_0_0_1_0";
                    }
                    else
                    {
                        broadcastid = BroadcastChanelId + "_1_0_0_0";
                    }
                    MessageDispatcher.SendMessage(this, "StartBroadcastVoice", broadcastid, 0);
                }
                else
                {
                    MessageDispatcher.SendMessage(this, "EndBroadcastVoice", BroadcastChanelId, 0);
                }
            }
        }

        public void StartBroadCastVioce()
        {
            bEnterBroadcast = true;
            MessageDispatcher.SendMessage(this, "SetSelfVisibleAlltime", true, 0);
            BroadcastVoice(true);
        }
        public void EndBroadCastVioce()
        {
            bEnterBroadcast = false;
            MessageDispatcher.SendMessage(this, "SetSelfVisibleAlltime", false, 0);
            BroadcastVoice(false);
        }

        public void BroadcastVoice(bool broadcast)
        {
            int nbroad = broadcast ? 1 : 0;
            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = BroadcastAgoraVoice,
                b = nbroad.ToString(),
                c = mStaticThings.I.mAvatarID,
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);
        }
        void OnPointClick(IMessage msg)
        {
            GameObject click = msg.Data as GameObject;
            if (ReferenceEquals(click, broadcast.gameObject))
            {
                if (!bEnterBroadcast)
                {
                    if (mStaticThings.I.isAdmin)
                    {
                        broadcast.GetComponent<Renderer>().material = BaseMono.ExtralDataObjs[1].Target as Material;
                        StartBroadCastVioce();

                    }
                }
                else
                {
                    broadcast.GetComponent<Renderer>().material = BaseMono.ExtralDataObjs[0].Target as Material;
                    EndBroadCastVioce();
                }
                SaveInfo.instance.SaveActionData("ButtonProtectors", 8);
            }
        }

        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("ground_jiaban")&& bEnterBroadcast==true) 
            {
                broadcast.GetComponent<Renderer>().material = BaseMono.ExtralDataObjs[0].Target as Material;
                EndBroadCastVioce();
            }
        }
    }
}
