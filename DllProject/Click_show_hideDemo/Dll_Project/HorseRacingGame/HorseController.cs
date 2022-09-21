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
            GameCanvas.GetChild(0).GetChild(1).DOScaleX(0, 0.2f);
            GameCanvas.GetChild(1).GetChild(0).DOScaleX(0, 0.2f);
        }
        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }

        public override void Update()
        {
        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
    }
}