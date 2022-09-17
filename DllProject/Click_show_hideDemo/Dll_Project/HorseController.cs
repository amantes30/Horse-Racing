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
using System.Timers;
public class HorseInfo
{
    public string user_id { get; set; }
    public bool selcted= false;    
    public int index;
    public float speed = 0;
   
}

public class NewUserInfo
{
    
    public bool ButtonPressed, _gameStarted;
    public string _hostID = "";
}

namespace Dll_Project
{
    public class HorseController : DllGenerateBase 
    {
        public static HorseController _i;

        // Objects From Scene
        public Canvas MainCanvas;
        public Transform table;
        public Transform playerCam;
        public Transform my_horse;
        
        public float speed;
        
        public List<Transform> Horses = new List<Transform>();        // List Of Horse Objects

        

        public bool ButtonPressed, GameStarted = false;
        public string req_inpt = string.Empty;
        public string HostID = "";

        public int curr_index = 0;
        public int NoOfPlayers = 0;

        int touchcount = 0;
        public override void Init()
        {
            table = BaseMono.ExtralDatas[0].Target.transform;
            MainCanvas = BaseMono.ExtralDatas[1].Target.GetComponent<Canvas>();
            
            playerCam = BaseMono.ExtralDatas[2].Target;
            Debug.Log("HorseController Init !");
        }

        public override void Awake()
        {
            
            Debug.Log("HorseController Awake !");            
        }      
      
        public override void Start()
        {
            speed = 5f;
            for (int i = 0; i < table.childCount-1; i++)
            {
                Horses.Add(table.GetChild(i));
                Animator _temp = table.GetChild(i).GetComponent<Animator>();
                _temp.SetInteger("Speed", 0);
            }
            

            MainCanvas.transform.GetChild(0).Find("JoinGame").gameObject.AddComponent<EventTrigger>();
            AddEventTrig(EventTriggerType.PointerEnter, ()=> 
            {
                RawImage img = MainCanvas.transform.GetChild(0).Find("JoinGame").GetChild(0).GetChild(0).GetComponent<RawImage>();
                img.transform.DOScaleX(1, 0.2f);
            });
            AddEventTrig(EventTriggerType.PointerExit, () => 
            {
                RawImage img = MainCanvas.transform.GetChild(0).Find("JoinGame").GetChild(0).GetChild(0).GetComponent<RawImage>();
                img.transform.DOScaleX(0, 0.2f);
            });

            MainCanvas.transform.GetChild(0).Find("JoinGame").GetComponent<Button>().onClick.AddListener(() =>
            {
                MainCanvas.transform.GetChild(0).DOScaleX(0, 0.01f);
                float posZ = 0.0f;
                if (curr_index > 10) { return; }
                mStaticThings.I.StartCoroutine(SelectHorse(curr_index));

                switch (curr_index)
                {
                    case 0:
                        posZ = -61.02f;
                        break;
                    case 1:
                        posZ = -59.31f;
                        break;
                    case 2:
                        posZ = -57.82f;

                        break;
                    case 3:
                        posZ = -56.19f;
                        break;
                    case 4:
                        posZ = -54.61f;
                        break;
                    case 5:
                        posZ = -53.12f;
                        break;
                    case 6:
                        posZ = -51.43f;
                        break;
                    case 7:
                        posZ = -49.9f;
                        break;
                    case 8:
                        posZ = -48.26f;
                        break;
                    case 9:
                        posZ = -46.62f;
                        break;
                    case 10:
                        posZ = -45.07f;
                        break;
                    case 11:
                        posZ = -43.59f;
                        break;
                }
                playerCam.gameObject.SetActive(true);
                playerCam.localPosition = Vector3.Slerp(playerCam.localPosition, new Vector3(-163.218f, 161.953f, posZ), 5);
                curr_index++;                
                
            }); MainCanvas.transform.GetChild(1).DOScaleX(1, 0.01f);
            MainCanvas.transform.GetChild(1).Find("SpeedBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                touchcount++;
                Debug.Log(touchcount);
            });
            _i = this;
           // RoomConnecttt();

            
            Debug.Log("HorseController Start !");
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
                Accelerate();
                
                if (touchcount>=10)
                {
                    AddSpeed();
                    touchcount = 0;                    
                }
                
            }
            
        }
        public override void OnEnable()
        {
            Debug.Log("HorseController OnEnable !");
           // MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), Clicked);
           
        }

        public override void OnDisable()
        {
            Debug.Log("HorseController OnDisable !");
           // MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), Clicked);
        }
      
        public override void OnTriggerEnter(Collider other)
        {
            Debug.LogWarning(other);
        }
       
      
       
        public IEnumerator SelectHorse(int car_index)
        {
           
            my_horse = table.GetChild(car_index);
            my_horse.gameObject.SetActive(true);
        
            

            // text update
           
            MainCanvas.transform.GetChild(0).Find("Selected Car").gameObject.SetActive(true);
            MainCanvas.transform.GetChild(0).Find("Selected Car").GetComponent<Text>().text = "选定的马： " + car_index.ToString();
            MainCanvas.transform.GetChild(0).Find("Speed").gameObject.SetActive(true);
            MainCanvas.transform.GetChild(0).Find("Speed").GetComponent<Text>().text = "速度: 0" + car_index.ToString();
            
            NoOfPlayers++;
            WsCChangeInfo _info = new WsCChangeInfo()
            {
                a = "Select Horse",
                b = car_index.ToString(),
                c = mStaticThings.I.mAvatarID,
                d = NoOfPlayers.ToString()
                
            };            
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), _info);
           

           
            yield return new WaitForSeconds(1);
        }
       
        void Accelerate()
        {
            
            WsCChangeInfo inf = new WsCChangeInfo()
            {
                a = "ACC",
                b = mStaticThings.I.mAvatarID,
                c = my_horse.GetSiblingIndex().ToString(),
                d = speed.ToString()
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), inf);            

           
        }
        void AddSpeed()
        {
            MainCanvas.transform.GetChild(0).Find("Speed").GetComponent<Text>().text = "Speed: " + ((int)(((speed) + 0.01f))).ToString();
                   
            WsCChangeInfo o = new WsCChangeInfo
            {
                a = "SpeedControl",
                b = my_horse.GetSiblingIndex().ToString(),
                c = mStaticThings.I.mAvatarID,
                d = "+"
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), o);
                    //
                
            
        }
        void RoomConnecttt()
        {
            NewUserInfo _info = new NewUserInfo()
            {
                _gameStarted = GameStarted,
                _hostID = HostID,
            };

            string ___info = JsonMapper.ToJson(_info);
            
            WsCChangeInfo y = new WsCChangeInfo
            {
                a = "OK",
                b = mStaticThings.I.mAvatarID,                
                c = ___info
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(),y);
        }

        public void GenerateRandomLetter(GameObject _horse)
        {
            //if (mStaticThings.I.mAvatarID != _horseInfo.user_id) { return; }
            MainCanvas.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            char lt = new char();
            string s = "";
            while (s == "" || s == "a" || s == "w" || s == "s" || s == "d")
            {
                System.Random rnd = new System.Random();
                lt = (char)rnd.Next('a', 'z');
                s = lt.ToString();
            }

            ButtonPressed = false;
            MainCanvas.transform.GetChild(0).Find("Req_Input").GetComponent<Text>().text = s.ToUpper();
            //_horseInfo.req_inpt = s;
            req_inpt = s;
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
            yield return new WaitUntil(() => NoOfPlayers > 1);
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

