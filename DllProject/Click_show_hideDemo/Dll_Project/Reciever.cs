using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slate;
using System.Threading.Tasks;
using System.Numerics;
using System;
using LitJson;
namespace Dll_Project
{
    public class Reciever : DllGenerateBase
    {
        List<Transform> Horses = new List<Transform>();

        public override void Init()
        {
            if (mStaticThings.I.isVRApp) { return; }

            Debug.Log("Reciever Is On");
        }
        public override void Awake()
        {
            Debug.Log("Reciever Is On");
            
        }
        public override void Start()
        {
            Horses = HorseController._i.Horses;
            Debug.Log("Reciever Is On");
        }
        public override void Update()
        {
           
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), MessageRcv); 
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), MessageRcv);

        }
        void MessageRcv(IMessage m)
        {
            WsCChangeInfo ms = m.Data as WsCChangeInfo;
            switch (ms.a)
            {
                case "RoomConnected":
                    Debug.Log("recvd");
                  
                    break;
                case "SelectHorse":
                    Debug.LogError("selected");
                    
                    HosrseInfo _in = JsonMapper.ToObject<HosrseInfo>(ms.b);
                    //Debug.Log (HorseController._i.HorsesInfo[ _in.index].selcted);
                    HorseController._i.HorsesInfo[_in.index] = _in;
                    if (_in.user_id != mStaticThings.I.mAvatarID)
                    {
                        HorseController._i.Horses[_in.index].GetChild(3).GetChild(0).GetChild(1).gameObject.SetActive(false);
                    }
                    HorseController._i.active_horses =int.Parse( ms.c);
                    Debug.Log(ms.c + " horses active");
                    if (HorseController._i.active_horses > 0)
                    {
                        HorseController._i.AllowStartGame();
                    }
                    break;
                case "StartGame":
                    HorseController._i.HostUID = ms.b;
                    HorseController._i._canvas.GetChild(0).Find("StartBtn").gameObject.SetActive(false);
                    HorseController._i.StartGame();

                    break;
                case "AddSpeed":
                    Debug.Log("addspeed");
                    if (mStaticThings.I.mAvatarID == ms.c)
                    {
                        Transform HorseObj = HorseController._i.Horses[int.Parse(ms.d)];
                        
                        HorseObj.transform.GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = ms.b;
                        HorseObj.Translate(Vector3.forward * 2);

                        WsMovingObj inff = new WsMovingObj()
                        {
                            id = mStaticThings.I.mAvatarID,
                            name = HorseObj.name,
                            mark = "s",
                            position = HorseObj.transform.localPosition,
                            islocal = true,
                            rotation = HorseObj.transform.rotation,
                            scale = HorseObj.transform.localScale,

                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), inff);
                        Debug.Log("Control Obj Sent!");
                        int temp = IsGameOver(HorseController._i.HorsesInfo);
                        if (temp != 0)
                        {
                            WsCChangeInfo w = new WsCChangeInfo() 
                            {
                                a = "GameOver",
                                b = temp.ToString(),
                            };
                            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), w);
                        }
                    }

                   

                    break;

                case "GameOver":
                    GameObject winner_horse = HorseController._i.Horses[int.Parse(ms.b)].gameObject;
                    winner_horse.transform.GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = "Winner";
                    break;

                
            }
        }
        int IsGameOver(List<HosrseInfo> _info)
        {
            int Winner_index = 0;
            
            foreach(HosrseInfo i in _info) 
            {
                if(HorseController._i.Horses[i.index].localPosition.z > 130)
                {
                    Debug.Log(i.index + " is winner");
                    Winner_index = i.index;
                }
                Winner_index = 0;
            }
            return Winner_index;
            
        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
    }
}
