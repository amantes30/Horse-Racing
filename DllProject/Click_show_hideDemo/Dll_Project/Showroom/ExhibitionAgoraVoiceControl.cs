using com.ootii.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dll_Project
{
    public class ExhibitionAgoraVoiceControl : DllGenerateBase
    {
        public override void Awake()
        {
            base.Awake();
        }
        public override void Start()
        {
            base.Start();
        }

        private void OnChangeVoice(IMessage msg) 
        {
            BaseMono.StartCoroutine(SetClick());
        }
        private IEnumerator SetClick() 
        {
            yield return new WaitForSeconds(5);
            MessageDispatcher.SendMessage(this, VoiceDispMessageType.ConnectVoiceExRoom.ToString(), PlayerPrefs.GetString(mStaticThings.I.nowRoomStartChID + "NowRoomGMVoice"), 0);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), OnTelePortToMesh);
            MessageDispatcher.AddListener(VrDispMessageType.RoomConnected.ToString(), OnChangeVoice);
        }
        public override void OnDisable()
        {
            base.OnDisable();
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), OnTelePortToMesh);
            MessageDispatcher.RemoveListener(VrDispMessageType.RoomConnected.ToString(), OnChangeVoice);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Update()
        {
            base.Update();
        }

        private string recordName;
        void OnTelePortToMesh(IMessage msg)
        {
            string meshname = (string)msg.Data;
            if (string.IsNullOrEmpty(meshname) || !meshname.StartsWith("ground_"))
                return;

            meshname = meshname.Replace("ground_", "");

            if (recordName == meshname)
            {
                return;
            }
            recordName = meshname;
            if (meshname == "jiaban")
            {
                MessageDispatcher.SendMessage(this, VoiceDispMessageType.ConnectVoiceExRoom.ToString(), "", 0);
            }
            else
            {
                MessageDispatcher.SendMessage(this, VoiceDispMessageType.ConnectVoiceExRoom.ToString(), meshname, 0);
                
            }
            PlayerPrefs.SetString(mStaticThings.I.nowRoomStartChID + "NowRoomGMVoice", mStaticThings.I.nowRoomGMEroomExID);
        }
    }
}
