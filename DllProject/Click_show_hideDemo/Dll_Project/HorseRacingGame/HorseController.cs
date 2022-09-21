using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

using DG.Tweening;

public class HorseInfo
{
    public string user_id { get; set; }
    public bool selcted, ready = false;
    public int index;
    public float speed = 0;
}

public class MainCanvas 
{
    public Canvas Main;
    public WaitUI _waitUI;
    public RulesUI _rulesUI;
    public Transform GamePlayPanel, StartPanel;

}

public class RulesUI
{
    public Transform panel;
    public Button joinGameBtn;
    public Button closeBtn;
}
public class WaitUI
{
    public Transform panel;
    public Text numberOfPlayers;
    public Text waitTime;
    public Button closeBtn;
    public Button OkBtn;
}

namespace Dll_Project.HorseRacingGame
{
    public class HorseController : DllGenerateBase
    {
        public static HorseController i;

        public Transform HorsesParent;
        private Transform GameCanvas;
        private Transform GameCamera;
        public List<Transform> Doors = new List<Transform>();

        public bool started, selected, ready;
        public string hostID, userID;


        public int numberOfPlayers = 0;
        public int horseIndex = 0;
        public int Rank = 1;
        public int timer;
        public float speed;
        public int touchCount;

        public override void Init()
        {
            i = this;
            HorsesParent = BaseMono.ExtralDatas[0].Target;
            GameCanvas = BaseMono.ExtralDatas[1].Target;
            GameCamera = BaseMono.ExtralDatas[2].Target;
            for (int i = 0; i <=11; i++)
            {
                Doors.Add(BaseMono.ExtralDatas[3].Info[i].Target);
            }
        }

        public override void Awake()
        {
            numberOfPlayers = 0;
            horseIndex = 0;
            Rank = 1;
            started = false; selected = false; ready = false;
        }

        public override void Start()
        {
            if (mStaticThings.I != null)
            {
                userID = mStaticThings.I.mAvatarID;
            }
            // send user ID to check information
            WsCChangeInfo info = new WsCChangeInfo()
            {
                a = "NewUserConnect",
                b = userID,
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), info);
            GameCanvas.GetChild(0).GetChild(0).DOScaleX(1, 0.2f);
            GameCanvas.GetChild(0).GetChild(0).Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() => 
            {
                GameCanvas.GetChild(0).GetChild(0).DOScaleX(0, 0.2f);
            });
            GameCanvas.GetChild(0).GetChild(0).Find("JoinGame").GetComponent<Button>().onClick.AddListener(() =>
            {
                GameCanvas.GetChild(0).GetChild(0).DOScaleX(0, 0.2f);

                if (numberOfPlayers == 0) { hostID = userID; }

                if (numberOfPlayers > 10)
                {
                    GameCanvas.GetChild(0).GetChild(1).DOScaleX(0, 0.2f);
                    GameCanvas.GetChild(0).GetChild(1).GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameCanvas.GetChild(0).GetChild(0).DOScaleX(1, 0.2f);
                        GameCanvas.GetChild(0).GetChild(1).DOScaleX(0, 0.2f);
                        WsCChangeInfo i = new WsCChangeInfo()
                        {
                            a = "NewUserConnect",
                            b = userID,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), i);
                    });
                    GameCanvas.GetChild(0).GetChild(1).GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameCanvas.GetChild(0).GetChild(0).DOScaleX(1, 0.2f);
                        GameCanvas.GetChild(0).GetChild(1).DOScaleX(0, 0.2f);
                        WsCChangeInfo i = new WsCChangeInfo()
                        {
                            a = "NewUserConnect",
                            b = userID,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), i);
                    });
                    GameCanvas.GetChild(0).GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = numberOfPlayers.ToString();
                    GameCanvas.GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetComponent<Text>().text = 4.ToString();

                }
                if (numberOfPlayers < 10)
                {
                    GameCanvas.GetChild(0).DOScaleX(0, 0.2f);
                    GameCanvas.GetChild(1).DOScaleX(1, 0.2f);
                    horseIndex = numberOfPlayers;
                    numberOfPlayers++;
                    selected = true;
                    speed = 5;
                    WsCChangeInfo startt = new WsCChangeInfo
                    {
                        a = "iselect",
                        b = numberOfPlayers.ToString(),
                        c = userID,
                        d = hostID,
                        e = horseIndex.ToString(),
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), startt);
                    
                }
            });
            GameCanvas.GetChild(0).GetChild(1).DOScaleX(0, 0.2f);
            GameCanvas.GetChild(1).DOScaleX(0, 0.2f);
            GameCanvas.GetChild(1).GetChild(5).GetComponent<Button>().onClick.AddListener(() =>
            {
                GameCanvas.GetChild(1).GetChild(4).DOScaleX(0, 0.2f);
                WsCChangeInfo inn = new WsCChangeInfo()
                {
                    a = "ireset",

                };MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), inn);
            });
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), Click);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), Click);

        }

        public override void Update()
        {
            if (started && selected)
            {
                WsCChangeInfo inf = new WsCChangeInfo()
                {
                    a = "acclerate",
                    b = userID,
                    c = speed.ToString(),
                    d = horseIndex.ToString(),
                };MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), inf);
                if (touchCount > 10)
                {
                    speed += 0.5f;
                    touchCount = 0;
                }
                if (HorsesParent.GetChild(horseIndex).localPosition.x < -100)
                {
                    GameCanvas.GetChild(1).GetChild(4).DOScaleX(1, 0.2f);
                    GameCanvas.GetChild(1).GetChild(4).GetChild(0).GetComponent<Text>().text = Rank.ToString();
                    Rank++;
                    WsCChangeInfo o = new WsCChangeInfo()
                    {
                        a = "ifinish",
                        b = horseIndex.ToString(),
                        c = Rank.ToString()
                    };MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), o);
                    speed = 2;
                }
            }
        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
        void Click(IMessage msg)
        {
            if ((msg.Data as GameObject).name == "HorsesParent")
            {
                GameCanvas.GetChild(0).GetChild(0).DOScaleX(1, 0.2f);
            }
        }
        public IEnumerator StartCountdown(int _seconds)
        {
            timer = _seconds;
            while (timer > 0)
            {
                WsCChangeInfo _info = new WsCChangeInfo() 
                {
                    a = "Timer",
                    b = timer.ToString(),
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), _info);
                yield return new WaitForSeconds(1);

                timer--;
            }
            if (numberOfPlayers == 1)
            {
                mStaticThings.I.StartCoroutine(StartCountdown(15));
                GameCanvas.GetChild(2).GetChild(0).DOScaleX(1, 0.2f);
            }
            else if (numberOfPlayers > 1)
            {
                GameCanvas.GetChild(2).GetChild(0).DOScaleX(0, 0.2f);
                GameCanvas.GetChild(1).Find("Timer").DOScaleX(0, 0.2f);
                WsCChangeInfo i = new WsCChangeInfo()
                {
                    a = "istart",
                    b= numberOfPlayers.ToString()

                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), i);
            }
            
        }
    }
}