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
public class HorseInfo
{
    public string user_id { get; set; }
    public bool selcted= false;
    public int NoOfUsers;
    public int index;
    public float speed = 0;
    public string req_inpt;
}

public class NewUserInfo
{
    public List<HorseInfo> __horseinfo = new List<HorseInfo>();
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
        
        public List<Transform> Horses = new List<Transform>();        // List Of Horse Objects

        public List<HorseInfo> _horsesInfo = new List<HorseInfo>();   // Store Horse Information

        public bool ButtonPressed, GameStarted = false;
        public string req_inpt = string.Empty;
        public string HostID = "";
        
        public override void Init()
        {
            table = BaseMono.ExtralDatas[0].Target.transform;
            MainCanvas = BaseMono.ExtralDatas[1].Target.GetComponent<Canvas>();
            Debug.Log("HorseController Init !");
        }

        public override void Awake()
        {
            RoomConnecttt();
            Debug.Log("HorseController Awake !");            
        }      
      
        public override void Start()
        {           
            for (int i = 0; i < table.GetChild(0).childCount; i++)
            {
                HorseInfo _in = new HorseInfo
                {
                    index = i,
                    user_id = "",
                    selcted = false,
                    NoOfUsers = 0,
                    speed = 0,
                    req_inpt = "",
                };

                Horses.Add(table.GetChild(0).GetChild(i));
                Animator _temp = table.GetChild(0).GetChild(i).Find("HorseObj").GetChild(0).GetComponent<Animator>();
                _temp.SetInteger("Speed_f", 0);
                _temp.SetBool("Eat_b", true);                
                _horsesInfo.Add(_in);

            }


            MainCanvas.transform.GetChild(0).Find("StartGame").gameObject.AddComponent<EventTrigger>();
            AddEventTrig(EventTriggerType.PointerEnter, HoverIn);
            AddEventTrig(EventTriggerType.PointerExit, HoverOut);

            MainCanvas.transform.GetChild(0).Find("StartGame").GetComponent<Button>().onClick.AddListener(() =>
            {
                WsCChangeInfo info = new WsCChangeInfo
                {
                    a = "StartGame",
                    b = mStaticThings.I.mAvatarID,
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), info);
            });
            _i = this;

            Debug.Log("HorseController Start !");
        }
        void AddEventTrig(EventTriggerType ET, UnityAction UA)
        {
            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener((eventData) => UA()); // you can capture and pass the event data to the listener

            // Create and initialise EventTrigger.Entry using the created TriggerEvent
            EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = ET };

            // Add the EventTrigger.Entry to delegates list on the EventTrigger
            MainCanvas.transform.GetChild(0).Find("StartGame").gameObject.GetComponent<EventTrigger>().triggers.Add(entry);
        }
        public override void LateUpdate()
        {
            if (GameStarted)
            {
                if (mStaticThings.I.mAvatarID == HostID)
                {
                    Accelerate();
                }
                if (Input.inputString == req_inpt)
                {
                    AddSpeed();
                }
                else if (Input.inputString != req_inpt && Input.anyKeyDown)
                {
                    foreach (HorseInfo i in _horsesInfo)
                    {
                        if (i.user_id == mStaticThings.I.mAvatarID)
                        {
                            MainCanvas.transform.GetChild(0).Find("Speed").gameObject.SetActive(true);
                            MainCanvas.transform.GetChild(0).Find("Speed").GetComponent<Text>().text = "Speed: " + ((int)(((i.speed)) * 100)).ToString();
                            /*WsCChangeInfo o = new WsCChangeInfo
                            {
                                a = "SpeedControl",
                                b = i.index.ToString(),
                                c = i.user_id,
                                d = "-"
                            };
                            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), o);
                        */
                        }
                    }

                }

            }
        }
        public override void OnEnable()
        {
            Debug.Log("HorseController OnEnable !");
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), Clicked);
            
            if (mStaticThings.I != null)
            {
                
            }
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
            switch ((Obj.name).Split('_')[0])
            {
                case "Button":
                    mStaticThings.I.StartCoroutine(SelectHorse(Obj,8));
                    break;               
                default: break;
            }
            
        }
    
       
        public IEnumerator SelectHorse(GameObject _obj, int _waittime)
        {

            int car_index = int.Parse((_obj.name).Split('_')[1]);
            int count = 0;
            Transform selectedHorse = Horses[car_index];
            HorseInfo selectedInfo = _horsesInfo[car_index];
            foreach (Transform t in Horses)
            {
                t.GetChild(3).GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
            
            selectedHorse.GetChild(3).GetChild(0).GetChild(3).gameObject.SetActive(true);
            selectedHorse.GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = "备好了";
            selectedInfo.selcted = true;
            selectedInfo.user_id = mStaticThings.I.mAvatarID;
            selectedInfo.NoOfUsers += 1;


            if (mStaticThings.I.mAvatarID == car_index.ToString())
            {
                MainCanvas.transform.GetChild(0).Find("Selected Car").gameObject.SetActive(true);
                MainCanvas.transform.GetChild(0).Find("Selected Car").GetComponent<Text>().text = "选定的马： " + car_index.ToString();
                MainCanvas.transform.GetChild(0).Find("Speed").gameObject.SetActive(true);
                MainCanvas.transform.GetChild(0).Find("Speed").GetComponent<Text>().text = "速度: 0" + car_index.ToString();
            }
            foreach (HorseInfo i in _horsesInfo)
            {
                if (i.selcted) { count++; }
            }
            if (count > 0)
            {
                MainCanvas.transform.GetChild(0).Find("StartGame").gameObject.SetActive(true);
                /// Turn On StartGame Button
            }
            WsCChangeInfo _info = new WsCChangeInfo()
            {
                a = "Select Horse",
                b = car_index.ToString(),
                c = mStaticThings.I.mAvatarID,
                d = count.ToString(),
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), _info);

            yield return new WaitForSeconds(_waittime);
        }
        public void GenerateRandomLetter(GameObject _horse, HorseInfo _horseInfo)
        {
            if (mStaticThings.I.mAvatarID != _horseInfo.user_id) { return; }
            MainCanvas.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            char lt = new char();
            string s = "";
            while (s=="" || s =="a" || s=="w" || s == "s" || s == "d")
            {
                System.Random rnd = new System.Random();
                lt = (char)rnd.Next('a', 'z');
                s = lt.ToString();
            }
            

            ButtonPressed = false;
            MainCanvas.transform.GetChild(0).Find("Req_Input").GetComponent<Text>().text = s.ToUpper();            
            _horseInfo.req_inpt = s;
            req_inpt = s;
        }
        void Accelerate()
        {
            foreach (HorseInfo _t in _horsesInfo)
            {
                if (_t.selcted)
                {
                    Horses[_t.index].Translate(Vector3.forward * _t.speed * Time.deltaTime);
                    Horses[_t.index].Find("HorseObj").GetChild(0).GetComponent<Animator>().SetBool("Eat_b", false);
                    Horses[_t.index].Find("HorseObj").GetChild(0).GetComponent<Animator>().SetFloat("Speed_f", 0.5f);
                    WsMovingObj _moveinfo = new WsMovingObj
                    {
                        id = _t.user_id,
                        name = Horses[_t.index].name,
                        islocal = true,
                        mark = "i",
                        position = Horses[_t.index].localPosition,
                        rotation = Horses[_t.index].localRotation,
                        scale = Horses[_t.index].localScale
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), _moveinfo);
                }
                if (Horses[_t.index].localPosition.z > 50)
                {
                    GameStarted = false;
                    if (mStaticThings.I.mAvatarID == _t.user_id)
                    {
                        MainCanvas.transform.GetChild(0).Find("Req_Input").GetComponent<Text>().text = "赢家"; // winner
                    }
                    WsCChangeInfo msg = new WsCChangeInfo
                    {
                        a = "GameOver",
                        b = _t.index.ToString(),
                        c=_t.user_id,

                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), msg);

                }

            }
        }
        void AddSpeed()
        {
            ButtonPressed = true;
            foreach (HorseInfo i in _horsesInfo)
            {
                if (i.user_id == mStaticThings.I.mAvatarID)
                {
                    MainCanvas.transform.GetChild(0).Find("Speed").GetComponent<Text>().text = "Speed: " + ((int)(((i.speed) + 0.01f) * 100)).ToString();
                    GenerateRandomLetter(Horses[i.index].gameObject, i);
                    WsCChangeInfo o = new WsCChangeInfo
                    {
                        a = "SpeedControl",
                        b = i.index.ToString(),
                        c = i.user_id,
                        d = "+"
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), o);
                    //
                }
            }
        }
        void RoomConnecttt()
        {
            NewUserInfo _info = new NewUserInfo()
            {
                __horseinfo = this._horsesInfo,
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

       
    }
}