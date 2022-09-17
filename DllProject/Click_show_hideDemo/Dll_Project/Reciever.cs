using com.ootii.Messages;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

namespace Dll_Project
{
    public class Reciever : DllGenerateBase
    {
        private Canvas canvas;
        
        
        public override void Init()
        {
            
            Debug.Log("Reciever Is ON");
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        }
        public override void Awake()
        {
            Debug.Log("Reciever Is On");            
        }
        public override void Start()
        {
            canvas = HorseController._i.MainCanvas;
            
            Debug.Log("Reciever Is On");
        }
        
        public override void OnEnable()
        {



        }
        void DeselectHorse(mStaticThings user)
        {
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);            
                   
        }
      
        
        void RecieveCChangeObj(IMessage m)
        {
            WsCChangeInfo ms = m.Data as WsCChangeInfo;
            switch (ms.a)
            { 
                case "OK":
                    Debug.Log("dfsssssssssss");
                    if (mStaticThings.I.mAvatarID != ms.b) 
                    {
                        WsCChangeInfo i = new WsCChangeInfo()
                        {
                            a = "UpdateForNewUser",
                            b = mStaticThings.I.mAvatarID,
                            c = ms.b,
                            d = ms.c,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), i);
                    }

                    
                    break;
                case "UpdateForNewUser":
                   
                    break;
                case "Select Horse":
                    int.TryParse(ms.d, out HorseController._i.NoOfPlayers);
                    HorseController._i.Horses[int.Parse(ms.b)].gameObject.SetActive(true);
                    mStaticThings.I.StartCoroutine (HorseController._i.StartCountdown(10));
                    HorseController._i.HostID = HorseController._i.HostID == "" ? mStaticThings.I.mAvatarID : ms.c;

                    Debug.Log("select Message");
                    break;
                case "DeselectHorse":
                    if (mStaticThings.I.mAvatarID == ms.b)
                    {
                        DeselectHorse(mStaticThings.I);
                    }
                    break;
                case "StartGame":
                    /// turn OFF startGame btn
                    
                    
                    Debug.Log(ms.b);
                    StartGame();
                    break;
                case "SpeedControl":
                    if (mStaticThings.I.mAvatarID == ms.c && mStaticThings.I.mAvatarID == HorseController._i.HostID)
                    {
                        HorseController._i.speed += 0.05f;

                        foreach (Transform _t in HorseController._i.table.GetComponentsInChildren<Transform>())
                        {
                            WsMovingObj _mov = new WsMovingObj
                            {
                                id = mStaticThings.I.mAvatarID,
                                name = _t.name,
                                islocal = true,
                                mark = "s",
                                position = _t.localPosition,
                                rotation = _t.localRotation,
                                scale = _t.localScale,
                            };
                            MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), _mov);
                        }
                    }
                    break;
                case "GameOver":
                    //mStaticThings.I.StartCoroutine(ResetGame(int.Parse(ms.b)));
                    break;
                case "ACC":
                    if (HorseController._i.HostID == ms.b)
                    {
                        Transform _t = HorseController._i.table.GetChild(int.Parse(ms.c));
                        _t.Translate(Vector3.forward * float.Parse(ms.d) * Time.deltaTime);
                        Animator o = _t.GetComponent<Animator>();
                        o.SetInteger("IDInt", 12);
                        o.SetBool("Stand", true);

                        Debug.LogError(_t.name);
                        foreach (Transform _tt in HorseController._i.Horses)
                        {
                            WsMovingObj _mov = new WsMovingObj
                            {
                                id = ms.b,
                                name = _tt.name,
                                islocal = true,
                                mark = "i",
                                position = _tt.localPosition,
                                rotation = _tt.localRotation,
                                scale = _tt.localScale

                            }; MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), _mov);
                        }
                    }
                    break;
                
            }
        }
        void StartGame()
        {
            HorseController._i.GameStarted = true;


        }

        /*IEnumerator ResetGame(int winner_index)
        {
            
        }*/
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
    }
}
