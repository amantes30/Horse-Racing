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
        
        public List<Transform> Horses = new List<Transform>();        // List Of Horse Objects
        public List<HorseInfo> WinnerList = new List<HorseInfo>();        // List Of Horse Objects

        public List<HorseInfo> _horsesInfo = new List<HorseInfo>();   // Store Horse Information

        public List<Transform> Doors = new List<Transform>();
        public int activePlayers = 0;

        public bool ButtonPressed, GameStarted = false;
        public string req_inpt = string.Empty;
        public string HostID = "";
        private string user_id;

        public int myhorseIndex;

        public int touchCount;

        public Transform PlayerCamera;

        // WIN POSX -417
        public override void Init()
        {
            _i = this;
            table = BaseMono.ExtralDatas[0].Target.transform;
            MainCanvas = BaseMono.ExtralDatas[1].Target.GetComponent<Canvas>();
            
            Debug.Log("HorseController Init !");
        }

        public override void Awake()
        {
            Debug.Log("HorseController Awake !");            
        }      
      
        public override void Start()
        {
            
            MainCanvas.transform.GetChild(1).Find("SpeedBtn").GetComponent<Button>().onClick.AddListener(() => 
            {
                touchCount++;
            });
            PlayerCamera = BaseMono.ExtralDatas[2].Target;
            
           
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
            MainCanvas.transform.GetChild(0).Find("JoinGame").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("fdssfff");
                
                WsCChangeInfo _info = new WsCChangeInfo()
                {
                    a = "CheckGameStatus",
                    b = user_id,
                    c = activePlayers.ToString()
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), _info);
            }); 
            MainCanvas.transform.GetChild(0).Find("JoinGame").gameObject.AddComponent<EventTrigger>();
            AddEventTrig(EventTriggerType.PointerEnter, () =>
            {
                RawImage img = MainCanvas.transform.GetChild(0).Find("JoinGame").GetChild(0).GetChild(0).GetComponent<RawImage>();
                img.transform.DOScaleX(1, 0.2f);
            });
            AddEventTrig(EventTriggerType.PointerExit, () =>
            {
                RawImage img = MainCanvas.transform.GetChild(0).Find("JoinGame").GetChild(0).GetChild(0).GetComponent<RawImage>();
                img.transform.DOScaleX(0, 0.2f);
            }); 
            RoomConnect();
        }
        void AddEventTrig(EventTriggerType ET, UnityAction UA)
        {
            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener((eventData) => UA()); // you can capture and pass the event data to the listener

            // Create and initialise EventTrigger.Entry using the created TriggerEvent
            EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = ET };

            // Add the EventTrigger.Entry to delegates list on the EventTrigger
            MainCanvas.transform.GetChild(0).Find("JoinGame").gameObject.GetComponent<EventTrigger>().triggers.Add(entry);
        }
        public override void Update()
        {
            if (GameStarted)
            {
                Debug.Log(HostID);
                if (mStaticThings.I.mAvatarID == HostID)
                {
                    Accelerate();
                    foreach (Transform _t in Horses)
                    {
                        WsMovingObj _mov = new WsMovingObj()
                        {
                            id = HostID,
                            islocal = true,
                            position = _t.localPosition,
                            rotation = _t.localRotation,
                            scale = _t.localScale,
                            mark = "i",
                            name = _t.name,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), _mov);
                    }
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
        void HoverIn()
        {
            RawImage img = MainCanvas.transform.GetChild(0).Find("StartGame").GetChild(0).GetChild(0).GetComponent<RawImage>();
            img.transform.DOScaleX(1, 0.2f);
            
        }
        void HoverOut()
        {
            RawImage img = MainCanvas.transform.GetChild(0).Find("StartGame").GetChild(0).GetChild(0).GetComponent<RawImage>();

            img.transform.DOScaleX(0, 0.2f);

        }
        


        
        void Accelerate()
        {
           Debug.Log("ACCCCCCC");
           foreach (HorseInfo i in _horsesInfo)
            {
                if (i.selcted && i.ready)
                {
                    MainCanvas.transform.GetChild(1).Find("Speed").GetComponent<Text>().text = "Speed: " + (i.speed * 100);
                    //Vector3 newPos = new Vector3(PlayerCamera.position.x-2, PlayerCamera.position.y, PlayerCamera.position.z);
                    PlayerCamera.localPosition = new Vector3(Horses[i.index].localPosition.x + 2, PlayerCamera.localPosition.y, PlayerCamera.localPosition.z);
                    Horses[i.index].Translate(Vector3.forward * i.speed);
                    if (Horses[i.index].localPosition.x < -100)
                    {
                        WsCChangeInfo ii = new WsCChangeInfo()
                        {
                            a = "Finished",
                            b = i.index.ToString(),
                            c= mStaticThings.I.mAvatarID,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ii);
                        
                        
                    }
                    
                    else if(touchCount >=10)
                    {
                        AddSpeed();
                        touchCount = 0;
                    }
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
            if (GameStarted)
            {
                PlayerCamera.gameObject.SetActive(true);
                mStaticThings.I.IsThirdCamera = true;
                mStaticThings.I.PCCamra = PlayerCamera;
                PositionCamera(myhorseIndex);
                Debug.Log("DONEEE");
                WsCChangeInfo ms = new WsCChangeInfo
                {
                    a = "StartGame",
                    b = mStaticThings.I.mAvatarID
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ms);


                MainCanvas.transform.GetChild(1).gameObject.SetActive(true);
                MainCanvas.transform.GetChild(0).Find("Timer").DOScaleX(0, 0.5f);
            }
            else
            {
                mStaticThings.I.StartCoroutine( StartCountdown(15));
            }
        }

        void PositionCamera(int _index)
        {
            PlayerCamera.gameObject.SetActive(true);
            float zPos = Horses[_index].localPosition.z;
            
            PlayerCamera.localPosition = new Vector3 (PlayerCamera.transform.localPosition.x , PlayerCamera.localPosition.y, zPos);
        }
    }
}