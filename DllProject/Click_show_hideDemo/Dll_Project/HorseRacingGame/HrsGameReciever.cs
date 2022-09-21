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
        private Transform Canvas;
        public override void Init()
        {
        }

        public override void Awake()
        {
            
            
        }

        public override void Start()
        {
            Canvas = BaseMono.ExtralDatas[0].Target;
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
                case "i start":                   
                    HorseController.i.numberOfPlayers = int.Parse(wsCChangeInfo.b);                   
                    HorseController.i.userID = wsCChangeInfo.c;
                    HorseController.i.hostID = wsCChangeInfo.d;
                    HostID = wsCChangeInfo.d;
                    int index = int.Parse(wsCChangeInfo.e);
                    mStaticThings.I.StartCoroutine(PrepareHorse(HorseController.i.HorsesParent.GetChild(index), 4));

                    if (HostID == mStaticThings.I.mAvatarID)
                    {
                        HorseController.i.timer = 15;
                        mStaticThings.I.StartCoroutine(HorseController.i.StartCountdown(HorseController.i.timer));
                    }
                    
                    break;
                case "Timer":
                    if (HorseController.i.selected)
                    {
                        Text txt = Canvas.GetChild(0).Find("Timer").GetChild(1).GetComponent<Text>();
                        txt.text = wsCChangeInfo.b;
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

        IEnumerator PrepareHorse(Transform tt, float _waittime)
        {
            tt.GetComponent<Animator>().SetInteger("Speed", 1);
            yield return new WaitForSeconds(_waittime);
            tt.GetComponent<Animator>().SetInteger("Speed", 0);
        }
    }
}
