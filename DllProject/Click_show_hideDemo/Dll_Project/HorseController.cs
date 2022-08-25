using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slate;
using System.Threading.Tasks;
using System.Numerics;
using System;
using TMPro.Examples;
using LitJson;

public class HorseInfo
{
    public string user_id { get; set; }
    public bool selcted= false;
    public int NoOfUsers;
    public int index;
    public float speed = 0;
    public string req_inpt;
}


namespace Dll_Project
{
    public class HorseController : DllGenerateBase
    {
        // Objects From Scene
        public Canvas MainCanvas;
        public Transform table;

        // List Of Horse Objects
        public List<Transform> Horses = new List<Transform>();

        // Store Horse Information
        public List<HorseInfo> _horsesInfo = new List<HorseInfo>();
        public string HostID = "";
        public bool ButtonPressed, GameStarted = false;
        public string req_inpt = string.Empty;
        public static HorseController _i;        
        public override void Init()
        {
            table = BaseMono.ExtralDatas[0].Target.transform;
            MainCanvas = BaseMono.ExtralDatas[1].Target.GetComponent<Canvas>();
            Debug.Log("HorseController Init !");
        }

        public override void Awake()
        {
            Debug.Log("HorseController Awake !");
            if (mStaticThings.I != null)
            {
                RoomConnect();
            }
        }
        void RoomConnect()
        {
            WsCChangeInfo wsCChangeInfo = new WsCChangeInfo 
            {
                a = "RoomConnected",
                b = HostID,
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), wsCChangeInfo); 
        }
        public override void Start()
        {
            for (int i = 0; i < table.GetChild(0).childCount; i++)
            {
               
                HorseInfo _in = new HorseInfo
                {
                    index  = i,
                    user_id = "",
                    selcted = false,
                    NoOfUsers = 0,
                    speed = 0,
                    req_inpt = "",
                };
                Horses.Add(table.GetChild(0).GetChild(i));
                _horsesInfo.Add(_in);

            }

            foreach (Transform i in Horses) { Debug.LogError(i.name); }


            _i = this;
            Debug.Log("HorseController Start !");
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
        IEnumerator Wait(int r)
        {
            yield return new WaitForSeconds(r);
        }


        void Clicked(IMessage msg)
        {
            GameObject Obj = msg.Data as GameObject;
            Debug.Log((Obj.name).Split('_')[0]);
            switch ((Obj.name).Split('_')[0])
            {
                case "Button":
                    int car_index = int.Parse((Obj.name).Split('_')[1]);
                    
                    WsCChangeInfo _info = new WsCChangeInfo() 
                    {
                        a = "Select Horse",
                        b = car_index.ToString(),
                        c = mStaticThings.I.mAvatarID,
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(),_info);
                    break;
                case "StartGame":
                    WsCChangeInfo info = new WsCChangeInfo
                    {
                        a = "StartGame",
                        b = mStaticThings.I.mAvatarID,
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), info);
                    
                    break;
                default: break;
            }

            Wait(1);
        }
        public override void Update()
        {
            if (GameStarted)
            {
                if (Input.inputString == req_inpt) { 
                    ButtonPressed = true;
                    foreach (HorseInfo i in _horsesInfo)
                    {
                        if (i.user_id == mStaticThings.I.mAvatarID)
                        {
                            Rand(Horses[i.index].gameObject, i);
                            WsCChangeInfo o = new WsCChangeInfo
                            {
                                a = "AddSpeed",
                                b = i.index.ToString(),
                            };
                            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), o);
                            //
                        }
                    }
                }
               
            }
        }

        public override void OnTriggerEnter(Collider other)
        {
            Debug.LogWarning(other);
        }

        public void Rand(GameObject _horse, HorseInfo _horseInfo)
        {
            char lt = new char();
            string s = "";
          
            //if (Btnpressed)
            {
                System.Random rnd = new System.Random();
                lt = (char)rnd.Next('a', 'z');
                s = lt.ToString();
                ButtonPressed = false;


            }
            _horse.transform.GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = s;
            _horseInfo.req_inpt = s;
            req_inpt = s;
        }
    }
}