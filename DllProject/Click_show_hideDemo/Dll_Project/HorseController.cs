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

public class HosrseInfo
{
    public string user_id;
    public int index;
    public float PosX, PosY, PosZ;
    public bool selcted= false;
    public int NoOfUsers;
    public float speed = 0;
    public string req_inpt;
}


namespace Dll_Project
{
    public class HorseController : DllGenerateBase 
    {
        public GameObject Table;
        public GameObject HorsesParent;
        public List<Transform> Horses = new List<Transform>();
        public List<HosrseInfo> HorsesInfo = new List<HosrseInfo>();
        public Transform _canvas;
        public static HorseController _i;

        
        bool Btnpressed =false;
        public bool GameStarted = false;
        public int NoOfHorseActive;
        public string req_inpt;
        public int active_horses=0;

        

        public string HostUID;

        
        public override void Init()
        {
            if (mStaticThings.I.isVRApp) { return; }
           
            Table = BaseMono.ExtralDatas[0].Target.gameObject;
            HorsesParent = Table.transform.GetChild(0).gameObject;
            _canvas = BaseMono.ExtralDatas[1].Target.transform;
            
            
            Debug.Log("HorseController Init !");
            for(int i =0; i< HorsesParent.GetComponentInChildren<Transform>().childCount; i++)
            {
                Transform _h = Table.transform.GetChild(0).GetChild(i);
                Horses.Add(_h);
                HosrseInfo _info = new HosrseInfo()
                {
                    index = _h.GetSiblingIndex(),
                    PosX = _h.localPosition.x,
                    PosY = _h.localPosition.y,
                    PosZ = _h.localPosition.z,                    
                    speed = 0,
                    user_id = "",
                    selcted = false,

                };
                HorsesInfo.Add(_info);
                
            }
            MessageDispatcher.AddListener(VrDispMessageType.RoomConnected.ToString(), (msg)=> 
            {
                WsCChangeInfo ws = new WsCChangeInfo()
                {
                    a = "RoomConnected",
                    b = mStaticThings.I.mAvatarID,
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ws);

            });
            
            
            

        }
        
        public override void Awake()
        {
            Debug.Log("HorseController Awake !");
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), Clicked);
        }

        public override void Start()
        {
        
            _i = this;
            Debug.Log("HorseController Start !");
        }
        void SendCChangeObj(IMessage msg)
        {
            WsChangeInfo changeInfo = msg.Data as WsChangeInfo;

            MessageDispatcher.SendMessageData(WsMessageType.RecieveChangeObj.ToString(), changeInfo, 0);

        }
        public override void OnEnable()
        {
            Debug.Log("HorseController OnEnable !");
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), Clicked);
            //MessageDispatcher.AddListener(WsMessageType.SendCChangeObj.ToString(), SendCChangeObj);
        }
       
        public override void OnDisable()
        {
            Debug.Log("HorseController OnDisable !");
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), Clicked);
            //MessageDispatcher.RemoveListener(WsMessageType.SendCChangeObj.ToString(), SendCChangeObj);


        }
        IEnumerator Wait(int r)
        {
            yield return new WaitForSeconds(r);
        }

        
        void Clicked(IMessage msg)
        {
           
            GameObject Obj = msg.Data as GameObject;
            Debug.Log((Obj.name).Split('_')[0]);
            switch ( (Obj.name).Split('_')[0])
            {
                case "Button":
                    
                    int indexx = int.Parse((msg.Data as GameObject).name.Split('_')[1]);
                    
                    Button c = Obj.GetComponent<Button>();
                    c.transform.GetChild(0).GetComponent<Text>().text = "OK";
                    
                    if (HorsesInfo[indexx].user_id == "") { active_horses += 1; }
                    foreach(Transform _t in Horses)
                    {
                        _t.GetChild(3).gameObject.SetActive(false);
                    }
                    Horses[indexx].GetChild(3).gameObject.SetActive(true);

                    HosrseInfo _info = new HosrseInfo()
                    {
                        index = indexx,
                        speed = 0,
                        user_id = mStaticThings.I.mAvatarID,
                        selcted = true,
                        PosX = 0,
                        PosY = 0,
                        PosZ = 0,
                        NoOfUsers = 0,
                        req_inpt = "",
                        
                        
                    };

                    
                    _info.NoOfUsers += 1;
                    string s = JsonMapper.ToJson(_info);

                    WsCChangeInfo _infoo = new WsCChangeInfo()
                    {
                        a = "SelectHorse",
                        b = s,                        
                        
                        c = active_horses.ToString(),
                      };
                      MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString() , _infoo);                    
                    break;
                case "StartGame":
                    Obj.SetActive(false);
                    HostUID = mStaticThings.I.mAvatarID;
                    WsCChangeInfo im = new WsCChangeInfo()
                    {
                        a = "StartGame",
                        b = HostUID,

                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), im);
                    break;
                default:break;
            }
        }

        public float speed = 0.01f;
        public override void LateUpdate()
        {
            if (GameStarted)
            {
                foreach(Transform _t in Horses)
                { 
                    _t.GetChild(3).GetChild(0).GetChild(2).GetComponent<Text>().text = "speed = " + speed.ToString();
                    if (mStaticThings.I.mAvatarID == HostUID)
                    {
                        WsMovingObj m = new WsMovingObj()
                        {
                            id = mStaticThings.I.mAvatarID,
                            name = _t.name,
                            mark = "i",
                            position = _t.transform.localPosition,
                            islocal = true,
                            rotation = _t.transform.rotation,
                            scale = _t.transform.localScale,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), m);
                    }

                }
                Rand();
                Debug.LogError(req_inpt);               
                if (req_inpt == Input.inputString)
                {
                    Btnpressed = true;
                    Rand();
                    foreach (HosrseInfo _h in HorsesInfo)
                    {
                        if (mStaticThings.I.mAvatarID == _h.user_id)
                        {
                            
                            WsCChangeInfo ws = new WsCChangeInfo()
                            {
                                a = "AddSpeed",
                                b = req_inpt,                                
                                c = _h.index.ToString(),


                            };
                            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ws);
                        }
                    }
                   

                }

            }

        }
       
        public override void OnTriggerEnter(Collider other)
        {
            Debug.LogWarning(other);
        }
       
        public void Rand()
        {
            char lt = new char();
            string s = "";
            if (!Btnpressed)
                return;
            //if (Btnpressed)
            {
                System.Random rnd = new System.Random();
                lt = (char)rnd.Next('a', 'z');
                s = lt.ToString();
                Btnpressed = false;
                
                
            }
            req_inpt = s;
            foreach (var go in Horses)
            {
                go.gameObject.transform.GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = req_inpt;
            }
        }

        public void AllowStartGame()
        {
            Debug.Log("alllllow");
            Button b = _canvas.transform.GetChild(0).GetChild(1).GetComponent<Button>();
            b.gameObject.SetActive(true);
            
            
        }
        public void StartGame()
        {
            
            Debug.Log("starting");
            
            for (int i = 0; i < Horses.Count; i++)
            {
                if (!HorsesInfo[i].selcted)
                {

                    Horses[i].gameObject.SetActive(false);
                    Horses[i].GetChild(3).GetChild(0).GetChild(1).gameObject.SetActive(false);

                }
                else
                {
                    Horses[i].GetChild(3).GetChild(0).GetChild(1).gameObject.SetActive(false);
                    Horses[i].gameObject.SetActive(true);
                    
                    Horses[i].transform.GetChild(3).GetChild(0).GetChild(3).gameObject.SetActive(true);
                    Horses[i].gameObject.SetActive(true);
                    
                    

                    Rand();
                    
                }
            }
                
            GameStarted = true;
            Btnpressed = true;
            
        }
    }
}