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
    public bool selcted= false;    
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

        public List<HorseInfo> _horsesInfo = new List<HorseInfo>();   // Store Horse Information

        public int activePlayers = 0;

        public bool ButtonPressed, GameStarted = false;
        public string req_inpt = string.Empty;
        public string HostID = "";
        private string user_id;

        public int myhorseIndex;

        public int touchCount;

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
            
            MainCanvas.transform.GetChild(1).Find("SpeedBtn").GetComponent<Button>().onClick.AddListener(() => { });
            
           
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
            }); RoomConnect();

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
        public override void LateUpdate()
        {
            if (GameStarted)
            {
                if (mStaticThings.I.mAvatarID == HostID)
                {
                    Accelerate();
                }
                
                
                
                
                

            }
        }
        public override void OnEnable()
        {
            Debug.Log("HorseController OnEnable !");
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), Clicked);
            
           
        }

        public override void OnDisable()
        {
            Debug.Log("HorseController OnDisable !");
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), Clicked);
            
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
        void Clicked(IMessage msg)
        {
            GameObject Obj = msg.Data as GameObject;
            Debug.Log((Obj.name).Split('_')[0]);
            /*switch ((Obj.name).Split('_')[0])
            {
                case "Button":
                    mStaticThings.I.StartCoroutine(SelectHorse(Obj,8));
                    break;               
                default: break;
            }*/
            
        }
    
       
        IEnumerator SelectHorse(GameObject _obj, int _waittime)
        {

            

            yield return new WaitForSeconds(_waittime);
        }
        
        void Accelerate()
        {
           foreach (HorseInfo i in _horsesInfo)
            {
                if (i.selcted)
                {
                    Horses[i.index].Translate(Vector3.forward * i.speed);
                    if (i.touchCount >= 10)
                    {
                        AddSpeed();
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
        }
        void RoomConnect()
        {
            NewUserInfo _info = new NewUserInfo()
            {
                __horseinfo = _horsesInfo,
                _gameStarted = GameStarted,
                _hostID = HostID,
                activeUsers = activePlayers,
            };
            MainCanvas.transform.GetChild(0).DOScaleX(1, 0.2f);
            string ___info = JsonMapper.ToJson(_info);
            WsCChangeInfo wsCChangeInfo = new WsCChangeInfo
            {
                a = "RoomConnected",
                b = user_id,                
                c = ___info,
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), wsCChangeInfo);
        }
        float currCountdownValue;
        public IEnumerator StartCountdown(float countdownValue)
        {
            currCountdownValue = countdownValue;
            while (currCountdownValue >= 0)
            {
                MainCanvas.transform.GetChild(0).Find("Timer").GetComponent<Text>().text = currCountdownValue.ToString();
                //Debug.Log("Countdown: " + currCountdownValue);
                yield return new WaitForSeconds(1.0f);
                currCountdownValue--;
            }
            yield return new WaitUntil(() => activePlayers > 1);
            Debug.Log("DONEEE");
            WsCChangeInfo ms = new WsCChangeInfo
            {
                a = "StartGame",
                b = mStaticThings.I.mAvatarID
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ms);
            MainCanvas.transform.GetChild(0).Find("JoinGame").gameObject.SetActive(false);

            MainCanvas.transform.GetChild(0).Find("Rules").gameObject.SetActive(false);
            MainCanvas.transform.GetChild(1).gameObject.SetActive(true);
        }

    }
}