using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening;

/*
 * ReadMe
 * 
 * for unity scene
 *      the horses need to be child object of the Table object
 *          horses need to have the animator component
 * 
 *      the maincanvas is the canvas that user interacts with to start game
 *      Rename this objects in the scene
 *          button to start game = "StartGame"
 *          timer text when the game starts = "Timer"
 *          speed text when the game starts = "Speed"
 * 
 */
public class HorseInfo
{
    public string user_id { get; set; }
    public bool selcted, ready= false;    
    public int index;
    public int touchCount;
    public float speed = 0;
    
}

public class NewUserInfo
{
    public List<HorseInfo> __horseinfo;
    public bool ButtonPressed, _gameStarted;
    public string _hostID = "";
    public int activeUsers;
}

namespace Dll_Project
{
    public class HorseController : DllGenerateBase 
    {
        public static HorseController _i;

        // Objects From Scene
        public Canvas MainCanvas;
        public Transform table;        
        public Transform PlayerCamera;
        Transform _firstPanel;
        Transform _secondPanel;

        public List<Transform> Horses = new List<Transform>();        // List Of Horse Objects
        public List<HorseInfo> WinnerList = new List<HorseInfo>();       // List Of Horse Objects
        public List<HorseInfo> _horsesInfo = new List<HorseInfo>();   // Store Horse Information
        public List<Transform> Doors = new List<Transform>();

        public int activePlayers = 0;
        public int myhorseIndex;
        public int touchCount;

        public bool ButtonPressed, GameStarted = false;

        public string req_inpt = string.Empty;
        public string HostID = "";
        public string user_id;

        // WIN POSX -417
        public override void Init()
        {
            _i = this;
            table = BaseMono.ExtralDatas[0].Target.transform;
            MainCanvas = BaseMono.ExtralDatas[1].Target.GetComponent<Canvas>();
            PlayerCamera = BaseMono.ExtralDatas[2].Target;
            _firstPanel = MainCanvas.transform.GetChild(0);
            _secondPanel = MainCanvas.transform.GetChild(1);
            Debug.Log("HorseController Init !");
        }

        public override void Awake()
        {
            Debug.Log("HorseController Awake !");            
        }      
      
        public override void Start()
        {
            Button SpeedUpBtn = _secondPanel.Find("SpeedBtn").GetComponent<Button>();
            SpeedUpBtn.onClick.AddListener(() => 
            {
                touchCount++;
            });

            user_id = mStaticThings.I.mAvatarID;
            
            // INITIALIZE HORSES AND HORSE STATUS IN A LIST
            for (int i = 0; i < table.childCount - 1; i++)
            {
                
                Horses.Add(table.GetChild(i));
                HorseInfo h_inf = new HorseInfo {
                    index = i,
                    selcted = false,
                    user_id = "",
                    touchCount = 0,
                    speed = 0,
                };
                _horsesInfo.Add(h_inf);
                Doors.Add(BaseMono.ExtralDatas[3].Info[i].Target);
            }
            _firstPanel.Find("JoinGame").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("JoinGame Clicked");
                if(activePlayers == 0) { HostID = user_id; }
                if (activePlayers <= 10)
                {
                    _firstPanel.Find("Rules").DOScaleX(0, 0.5f);
                    _firstPanel.Find("JoinGame").DOScaleX(0, 0.5f);
                    _firstPanel.Find("Timer").DOScaleX(1, 0.5f);

                    
                    Debug.Log(mStaticThings.I.mAvatarID + " h_index: " + activePlayers);

                    myhorseIndex = activePlayers;
                    Transform selectedhorse = Horses[myhorseIndex];
                    GameStarted = true;
                    WsCChangeInfo _info = new WsCChangeInfo()
                    {
                        a = "SelectHorse",
                        b = myhorseIndex.ToString(),
                        c = user_id,
                        d = HostID,
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), _info);
                }
                else if(activePlayers > 10)
                {
                    GameStarted = false;
                    // FULL,  WAIT FEW MINUTES UI
                }

            }); 
            _firstPanel.Find("JoinGame").gameObject.AddComponent<EventTrigger>();
            AddEventTrig(EventTriggerType.PointerEnter, () =>
            {
                RawImage img = _firstPanel.Find("JoinGame").GetChild(0).GetChild(0).GetComponent<RawImage>();
                img.transform.DOScaleX(1, 0.2f);
            });
            AddEventTrig(EventTriggerType.PointerExit, () =>
            {
                RawImage img = _firstPanel.Find("JoinGame").GetChild(0).GetChild(0).GetComponent<RawImage>();
                img.transform.DOScaleX(0, 0.2f);
            });


            // PLEASE CHECK ROOM CONNECT FOR ME
            RoomConnect();

        }
        

        public override void Update()
        {
            if (GameStarted)
            {
                Text SpeedText = _secondPanel.Find("Speed").GetComponent<Text>();
                SpeedText.text = "Speed: " + (_horsesInfo[myhorseIndex].speed * 100);                
                
                PlayerCamera.localPosition = (Vector3.right * 2) + PlayerCamera.localPosition; ;
                Debug.Log(HostID);
                
                Accelerate(); 
                
                if (touchCount >= 10)
                {
                    AddSpeed();
                    touchCount = 0;
                }
                if (Horses[myhorseIndex].localPosition.x < -100)
                {
                    WsCChangeInfo ii = new WsCChangeInfo()
                    {
                        a = "Finished",
                        b = myhorseIndex.ToString(),
                        c = mStaticThings.I.mAvatarID,
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ii);
                    _secondPanel.Find("GameOver").DOScaleX(1, 0.2f);
                    GameStarted = false;

                    _secondPanel.Find("GameOver").GetChild(0).GetComponent<Text>().text =
                        "比赛结束 \n 当前排名为：第 " + WinnerList.Count + "名";
                    Transform cam = PlayerCamera;
                    cam.localPosition = new Vector3(-3.9f, cam.localPosition.y, cam.localPosition.z);
                }

            }
        }
        public override void OnEnable()
        {
            Debug.Log("HorseController OnEnable !");
        }

        public override void OnDisable()
        {
            Debug.Log("HorseController OnDisable !");
            
        }

        public override void OnTriggerEnter(Collider other)
        {
            Debug.LogWarning(other);
        }
        void Accelerate()
        {
            if (mStaticThings.I.mAvatarID != HostID) { return; }
           Debug.Log("ACCCCCCC");
           foreach (HorseInfo i in _horsesInfo)
            {
                if (i.selcted && i.ready)
                {                    
                    Horses[i.index].Translate(Vector3.forward * i.speed);
                    WsMovingObj _mov = new WsMovingObj() 
                    {
                        id = i.index.ToString(),
                        islocal = true,
                        mark = "i",
                        name = Horses[i.index].name,
                        position = Horses[i.index].localPosition,
                        rotation = Horses[i.index].localRotation,
                        scale = Horses[i.index].localScale,
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), _mov);
                    
                }
            }
        }
        void AddSpeed()
        {
            WsCChangeInfo p = new WsCChangeInfo()
            {
                a = "AddSpeed",
                b = myhorseIndex.ToString(),                
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), p);
        }
        void RoomConnect()
        {

            Debug.LogError("econ");
            
            MainCanvas.transform.GetChild(0).DOScaleX(1, 0.2f);
            
            WsCChangeInfo wsCChangeInfo = new WsCChangeInfo
            {
                a = "RoomConnected",
                b = user_id,                
               
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), wsCChangeInfo);
        }
        float currCountdownValue;
        public IEnumerator StartCountdown(float countdownValue)
        {
            currCountdownValue = countdownValue;
            while (currCountdownValue >= 0)
            {
                MainCanvas.transform.GetChild(0).Find("Timer").GetComponent<Text>().text =
                    "目前玩家" + activePlayers.ToString() + "人，游戏还剩" + currCountdownValue.ToString() + "秒开始";
                currCountdownValue.ToString();
                //Debug.Log("Countdown: " + currCountdownValue);
                yield return new WaitForSeconds(1.0f);

                currCountdownValue--;
            }
            
           
                PlayerCamera.gameObject.SetActive(true);
                mStaticThings.I.IsThirdCamera = true;
                mStaticThings.I.PCCamra = PlayerCamera;
                PositionCamera(myhorseIndex);
                Debug.Log("DONEEE");
                WsCChangeInfo ms = new WsCChangeInfo
                {
                    a = "StartGame",
                    b = activePlayers.ToString(),
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ms);


                _secondPanel.gameObject.SetActive(true);
                _firstPanel.Find("Timer").DOScaleX(0, 0.5f);
            
           
        }

        void PositionCamera(int _index)
        {
            PlayerCamera.gameObject.SetActive(true);
            float zPos = Horses[_index].localPosition.z;
            
            PlayerCamera.localPosition = new Vector3 (PlayerCamera.transform.localPosition.x , PlayerCamera.localPosition.y, zPos);
        }
        void AddEventTrig(EventTriggerType ET, UnityAction UA)
        {
            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener((eventData) => UA()); // you can capture and pass the event data to the listener

            // Create and initialise EventTrigger.Entry using the created TriggerEvent
            EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = ET };

            // Add the EventTrigger.Entry to delegates list on the EventTrigger
            _firstPanel.Find("JoinGame").gameObject.GetComponent<EventTrigger>().triggers.Add(entry);
        }
    }
}