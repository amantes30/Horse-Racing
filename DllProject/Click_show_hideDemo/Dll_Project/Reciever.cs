﻿using com.ootii.Messages;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using DG.Tweening;
namespace Dll_Project
{
    public class Reciever : DllGenerateBase
    {
        public Canvas canvas;
        Transform _firstPanel;
        Transform _secondPanel;

        private string HostID;        
        
        private bool game_started = false;

        public override void Init()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            canvas = HorseController._i.MainCanvas;
            _firstPanel = canvas.transform.GetChild(0);
            _secondPanel = canvas.transform.GetChild(1);
            Debug.Log("Reciever Is ON"); 
            
        }
        public override void Awake()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            Debug.Log("Reciever Is On");            
        }
        public override void Start()
        {
            
            
            Debug.Log("Reciever Is On");
        }
        
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);           
                      
                       
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);            
                   
        }
      
        void RecieveCChangeObj(IMessage m)
        {
            WsCChangeInfo ms = m.Data as WsCChangeInfo;
            Debug.LogError(ms.a);
            switch (ms.a)
            { 
                case "RoomConnected":
                    Debug.LogError("RoomConnected");
                    NewUserInfo _info;
                    if (HostID == HorseController._i.user_id)
                    {
                        _info = new NewUserInfo() {
                            __horseinfo = HorseController._i._horsesInfo,
                            _gameStarted = HorseController._i.GameStarted,
                            _hostID = HorseController._i.HostID,
                            activeUsers = HorseController._i.activePlayers,
                        };
                        string s_inf = JsonMapper.ToJson(_info);
                        WsCChangeInfo ii = new WsCChangeInfo()
                        {
                            a = "ForNewUser",
                            b = s_inf,
                            c = ms.b,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ii);
                    }



                    break;
                case "ForNewUser":
                    if (mStaticThings.I.mAvatarID == ms.c)
                    {
                        NewUserInfo p = JsonMapper.ToObject<NewUserInfo>(ms.b);
                        HorseController._i._horsesInfo = p.__horseinfo;
                        HorseController._i.GameStarted = p._gameStarted;
                        HorseController._i.HostID = p._hostID;
                        HorseController._i.activePlayers = p.activeUsers;
                        foreach (HorseInfo i in p.__horseinfo)
                        {
                            if (i.selcted)
                            {
                                WsCChangeInfo io = new WsCChangeInfo()
                                {
                                    a = "SelectHorse",
                                    b = i.index.ToString(),
                                    c = i.user_id,
                                    d = HostID,
                                    e = p.activeUsers.ToString()
                                };
                                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), io);
                            }
                        }
                        if (p._gameStarted)
                        {
                            _firstPanel.Find("JoinGame").gameObject.SetActive(false);
                            // gamestarted UI wait
                            foreach (HorseInfo o in HorseController._i._horsesInfo)
                            {
                                if (o.selcted)
                                {
                                    HorseController._i.Horses[o.index].gameObject.SetActive(true);
                                }
                                else
                                {
                                    HorseController._i.Horses[o.index].gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                    break;
               

                    
                case "SelectHorse":                   
                    
                    HorseInfo selectedInfo = new HorseInfo(); 
                    int index = 0;
                    if (mStaticThings.I.mAvatarID == ms.c)
                    {                        
                        index = int.Parse(ms.b);
                        selectedInfo = HorseController._i._horsesInfo[index];   
                        
                    }
                    if (index == 0) { HostID = mStaticThings.I.mAvatarID; }
                    selectedInfo.selcted = true;
                    selectedInfo.user_id = ms.c;
                    selectedInfo.index = index;
                    HorseController._i._horsesInfo[index] = selectedInfo;
                    HorseController._i.activePlayers = int.Parse(ms.e);
                    Debug.LogError(HorseController._i.activePlayers);

                    Animator DoorAnimator = HorseController._i.Doors[index].GetComponent<Animator>();
                    DoorAnimator.SetTrigger("isOpen");

                    mStaticThings.I.StartCoroutine(HorseController._i.StartCountdown(15));

                    Animator h_animator = HorseController._i.Horses[index].GetComponent<Animator>();
                    h_animator.SetInteger("Speed", 1);
                           
                    mStaticThings.I.StartCoroutine(wait(5, HorseController._i.Horses[index]));
                    selectedInfo.ready = true;
                    
                    
                    Debug.Log("select Message");
                    break;
                case "StartGame":
                    bool canStart = false;
                    
                    
                    canStart = int.Parse(ms.b) > 1 ? true : false;                                    

                    
                    Debug.Log(canStart);
                    if (canStart)
                    {
                        StartGame();
                        HorseController._i.GameStarted = true;
                        game_started = true;
                        
                    }
                    else if (!canStart)
                    {
                        //mStaticThings.I.StartCoroutine(HorseController._i.StartCountdown(15));
                        Debug.Log("WAITING FOR OTHER PLAYERS");
                    }
                    break;
                case "Finished":                    
                    
                    HorseController._i.WinnerList.Add(HorseController._i._horsesInfo[int.Parse(ms.b)]);
                    HorseController._i.Horses[int.Parse(ms.b)].GetComponent<Animator>().SetInteger("Speed", 1);

                    if (HorseController._i.WinnerList.Count == HorseController._i.activePlayers && game_started)
                    {
                        Button ResetBtn = canvas.transform.GetChild(1).Find("ResetButton").GetComponent<Button>();
                        ResetBtn.transform.DOScaleX(1, 0.2f);
                        ResetBtn.onClick.AddListener(() =>
                        {
                            mStaticThings.I.StartCoroutine(ResetGame());
                        });
                        
                    }
                    
                    break;
                

                case "AddSpeed":
                    int indexxx = int.Parse(ms.b);
                    Transform _t = HorseController._i.Horses[indexxx];
                    HorseInfo _i = HorseController._i._horsesInfo[indexxx]; 
                    _i.speed += 0.01f;
                    if (mStaticThings.I.mAvatarID == HostID)
                    {
                       
                        WsMovingObj _mov = new WsMovingObj() 
                        {
                            name =_t.name,
                            id = _i.user_id,
                            islocal = true,
                            mark = "s",
                            position = _t.localPosition,
                            rotation = _t.localRotation,
                            scale = _t.localScale,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), _mov);
                            
                        
                        
                    }
                    break;
                
            }
        }
        IEnumerator wait(float waittime, Transform _t)
        {
            yield return new WaitForSeconds(waittime);
            _t.GetComponent<Animator>().SetInteger("Speed", 0);
        }
        void StartGame()
        {
            
            foreach (HorseInfo i in HorseController._i._horsesInfo)
            {
                if (!i.selcted)
                {
                    HorseController._i.Horses[i.index].gameObject.SetActive(false);

                }
                else
                {
                    _firstPanel.Find("RawImage").DOScaleX(1, 0.2f);
                    Transform _t = HorseController._i.Horses[i.index];
                    i.speed += 0.05f;
                    _t.GetComponent<Animator>().SetInteger("Speed", 2);
                    
                    WsMovingObj _mov = new WsMovingObj()
                    {
                        id = HostID,
                        islocal = true,
                        position = _t.localPosition,
                        rotation = _t.localRotation,
                        scale = _t.localScale,
                        mark = "s",
                        name = _t.name,
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), _mov);

                 

                }


            }
            
        }

        IEnumerator ResetGame()
        {
            //HorseController._i.Horses[y.index].GetComponent<Animator>().SetInteger("Speed", 1);
            yield return new WaitForSeconds(1);
            _secondPanel.gameObject.SetActive(false);
            _firstPanel.gameObject.SetActive(true);
            _firstPanel.Find("JoinGame").gameObject.SetActive(true);
            _firstPanel.Find("JoinGame").DOScaleX(1, 0.5f);
            _firstPanel.Find("Rules").DOScaleX(1, 0.5f);
            _secondPanel.Find("GameOver").DOScaleX(0, 0.2f);
            _secondPanel.Find("ResetButton").DOScaleX(0, 0.2f);

            foreach (HorseInfo i in HorseController._i._horsesInfo)
            {

                HorseController._i.Doors[i.index].GetComponent<Animator>().StopPlayback();
                Transform _t = HorseController._i.Horses[i.index];
                _t.gameObject.SetActive(true);
                _t.localPosition = new Vector3(0, _t.localPosition.y, _t.localPosition.z);

                _t.GetComponent<Animator>().SetInteger("Speed", 0);

                i.selcted = false;
                i.ready = false;
                i.speed = 0;                
                i.user_id = "";
               
            }
            HorseController._i.activePlayers = 0;
            HorseController._i.HostID = "";
            HorseController._i.WinnerList.Clear();
            HorseController._i.GameStarted = false;
            HorseController._i.myhorseIndex = 0;

            HorseController._i.PlayerCamera.gameObject.SetActive(false);
            

            
        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
    }
}
