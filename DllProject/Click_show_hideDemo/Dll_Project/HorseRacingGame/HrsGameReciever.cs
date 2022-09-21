using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

using DG.Tweening;

class NewUserInfo
{
    public bool _gameStarted = false;
    public string _hostID = "";
    public int activeUsers = 0;
}
namespace Dll_Project.HorseRacingGame
{
    public class HrsGameReciever : DllGenerateBase
    {
        private string HostID;
        public override void Init()
        {
        }

        public override void Awake()
        {
            
            
        }

        public override void Start()
        {
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveMessage);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveMessage);

        }

        public override void Update()
        {
        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }

        void RecieveMessage(IMessage msg)
        {
            WsCChangeInfo wsCChangeInfo = msg as WsCChangeInfo;

            switch (wsCChangeInfo.a)
            {
                case "NewUserConnect":
                    if (HostID!=null && HorseController.i.userID == HostID)
                    {
                        NewUserInfo _info = new NewUserInfo()
                        {
                            _gameStarted = HorseController.i.started,
                            activeUsers = HorseController.i.numberOfPlayers,
                            _hostID = HostID,
                        };
                        string s_info = JsonMapper.ToJson(_info);
                        WsCChangeInfo ws = new WsCChangeInfo() 
                        {
                            a = "SendInfo",
                            b = s_info,
                            c = wsCChangeInfo.b
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ws);
                        for (int i = 0; i <= HorseController.i.numberOfPlayers; i++)
                        {
                            SendPosition(HorseController.i.HorsesParent.GetChild(i), "s");
                        }
                    }
                    break;
                case "SendInfo":
                    if (wsCChangeInfo.c == HorseController.i.userID)
                    {
                        NewUserInfo _info = JsonMapper.ToObject<NewUserInfo>(wsCChangeInfo.b);
                        HorseController.i.started = _info._gameStarted;
                        HorseController.i.numberOfPlayers = _info.activeUsers;
                        HorseController.i.hostID = _info._hostID;

                        for (int i = 0; i <= HorseController.i.numberOfPlayers; i++)
                        {
                            Animator animator = HorseController.i.Doors[i].GetComponent<Animator>();
                            animator.SetTrigger("isOpen");
                        }
                    }
                    break;
            }
        }

        void SendPosition(Transform Horse, string _mark)
        {
            WsMovingObj wsMovingObj = new WsMovingObj()
            {
                id = HostID,
                name = Horse.name,
                islocal = true,
                mark = _mark,
                position = Horse.localPosition,
                rotation = Horse.localRotation,
                scale = Horse.localScale,
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), wsMovingObj);
        }

    }
}
