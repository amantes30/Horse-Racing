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
            WsCChangeInfo ws = msg.Data as WsCChangeInfo;

            switch (ws.a)
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
                        WsCChangeInfo s = new WsCChangeInfo() 
                        {
                            a = "SendInfo",
                            b = s_info,
                            c = ws.b
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ws);
                        for (int i = 0; i <= HorseController.i.numberOfPlayers; i++)
                        {
                            SendPosition(HorseController.i.HorsesParent.GetChild(i), "s");
                        }
                    }
                    break;
                case "SendInfo":
                    if (ws.c == HorseController.i.userID)
                    {
                        NewUserInfo _info = JsonMapper.ToObject<NewUserInfo>(ws.b);
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
                case "iselect":                   
                    HorseController.i.numberOfPlayers = int.Parse(ws.b);                   
                    
                    HorseController.i.hostID = ws.d;
                    HostID = ws.d;
                    int index = int.Parse(ws.e);
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
                        Text txt = Canvas.GetChild(1).Find("Timer").GetComponent<Text>();
                        txt.transform.DOScaleX(1, 0.2f);
                        txt.text = ws.b;
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
