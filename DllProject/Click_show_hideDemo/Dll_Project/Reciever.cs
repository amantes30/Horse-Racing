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

        
        
        public override void Init()
        {
            
            Debug.Log("Reciever Is Onhhh"); 
            
        }
        public override void Awake()
        {
            Debug.Log("Reciever Is On");            
        }
        public override void Start()
        {
            
            Debug.Log("Reciever Is On");
        }
        public override void Update()
        {
           if (HorseController._i.GameStarted && mStaticThings.I.mAvatarID == HorseController._i.HostID)
            {
                foreach (Transform _t in HorseController._i.Horses)
                {
                    WsMovingObj m_obj = new WsMovingObj
                    {
                        id = "",
                        name = _t.name,
                        islocal = true,
                        mark = "i",
                        position= _t.position,
                        rotation=_t.rotation,
                        scale=_t.localScale,
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), m_obj);
                }

            }

        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);           
            //MessageDispatcher.AddListener(WsMessageType.RecieveMovingObj.ToString(), RecieveCChangeObj);           
                       
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);            
            //MessageDispatcher.RemoveListener(WsMessageType.RecieveMovingObj.ToString(), RecieveCChangeObj);            
        }
      
        void RecieveCChangeObj(IMessage m)
        {
            WsCChangeInfo ms = m.Data as WsCChangeInfo;
            switch (ms.a)
            { 
                case "RoomConnected":
                    Debug.Log("FVDDDDDDDDDDDDDDD");
                    HorseController._i.HostID = ms.b == "" ? mStaticThings.I.mAvatarID : ms.b;
                    
                    Debug.Log("FVDDDDDDDDDDDDDDD");                  
                    break;
                case "Select Horse":
                    int index = int.Parse(ms.b);
                    int count = 0;
                    // set button Off
                    HorseController._i.Horses[index].GetChild(3).GetChild(0).GetChild(1).gameObject.SetActive(false);
                    HorseController._i.Horses[index].GetChild(3).GetChild(0).GetChild(3).gameObject.SetActive(true);
                    HorseController._i.Horses[index].GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = "Ready";
                    // update horse information
                    HorseController._i._horsesInfo[index].selcted = true;
                    HorseController._i._horsesInfo[index].user_id = ms.c;
                    HorseController._i._horsesInfo[index].NoOfUsers += 1;
                    
                    
                    foreach (HorseInfo i in HorseController._i._horsesInfo)
                    {
                        if (i.selcted) { count++; }
                    }
                    if (count > 0) 
                    {
                        HorseController._i.MainCanvas.gameObject.SetActive(true);
                        /// Turn On StartGame Button
                    }
                    Debug.Log("select Message");
                    break;
                case "StartGame":
                    /// turn OFF startGame btn
                    HorseController._i.MainCanvas.gameObject.SetActive(false);

                    // set hostID
                    HorseController._i.HostID = HorseController._i.HostID == "" ? mStaticThings.I.mAvatarID : ms.b;
                    Debug.Log(ms.b);
                    StartGame();
                    break;
                case "AddSpeed":
                    if (mStaticThings.I.mAvatarID == HorseController._i.HostID)
                    {
                        int ind = int.Parse(ms.b);
                        //HorseController._i.Rand(HorseController._i.Horses[ind].gameObject, HorseController._i._horsesInfo[ind]);
                        Transform T = HorseController._i.Horses[int.Parse(ms.b)];
                        T.Translate(Vector3.forward * 2);

                        WsMovingObj m_ob = new WsMovingObj 
                        {
                            id = HorseController._i.HostID,
                            name = T.name,
                            islocal = true,
                            mark = "s",
                            position = T.position,
                            rotation = T.rotation,
                            scale = T.localScale,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), m_ob);
                    }
                    break;
                case "GameOver":                   
                    break;

                
            }
        }
        void StartGame()
        {
            
            foreach (HorseInfo i in HorseController._i._horsesInfo)
            {
                if (!i.selcted) 
                {
                    HorseController._i.Horses[i.index].gameObject.SetActive(false);
                    
                }
                else { HorseController._i.Rand(HorseController._i.Horses[i.index].gameObject, HorseController._i._horsesInfo[i.index]);}
            }
            HorseController._i.GameStarted = true;

        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
    }
}
