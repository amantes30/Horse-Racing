using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
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

        HorseController Game = new HorseController();
        public override void Init()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            canvas = HorseController._i.MainCanvas;
            _firstPanel = canvas.transform.GetChild(0);
            _secondPanel = canvas.transform.GetChild(1);
            Debug.Log("Reciever Is ON");

            Game = HorseController._i;
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
                    if (HostID == Game.user_id)
                    {
                        _info = new NewUserInfo()
                        {
                            __horseinfo = Game._horsesInfo,
                            _gameStarted = Game.GameStarted,
                            _hostID = Game.HostID,
                            activeUsers = Game.activePlayers,
                        };
                        string s_inf = JsonMapper.ToJson(_info);
                        WsCChangeInfo ii = new WsCChangeInfo()
                        {
                            a = "ForNewUser",
                            b = s_inf,
                            c = ms.b,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ii);
                        foreach (HorseInfo i in _info.__horseinfo)
                        {
                            if (i.selcted)
                            {
                                Transform _tt = HorseController._i.Horses[i.index];
                                SendPosition(_tt, "s");
                            }
                        }
                    }


                    break;
                case "ForNewUser":

                    if (mStaticThings.I.mAvatarID == ms.c)
                    {
                        List<Transform> newUserHorses = Game.Horses;
                        NewUserInfo p = JsonMapper.ToObject<NewUserInfo>(ms.b);
                        Game._horsesInfo = p.__horseinfo;
                        Game.GameStarted = p._gameStarted;
                        Game.HostID = p._hostID;
                        Game.activePlayers = p.activeUsers;
                        foreach (HorseInfo i in p.__horseinfo)
                        {
                            if (i.selcted)
                            {
                                Game._horsesInfo[i.index] = i;
                                Game.activePlayers = int.Parse(ms.e);
                                Debug.LogError(Game.activePlayers);

                                Animator _DoorAnimator = Game.Doors[i.index].GetComponent<Animator>();
                                _DoorAnimator.SetTrigger("isOpen");
                                
                            }
                        }
                        if (p._gameStarted)
                        {
                            _firstPanel.Find("JoinGame").gameObject.SetActive(false);
                            // gamestarted UI wait
                            foreach (HorseInfo o in Game._horsesInfo)
                            {
                                if (o.selcted)
                                {
                                    Game.Horses[o.index].gameObject.SetActive(true);
                                }
                                else
                                {
                                    Game.Horses[o.index].gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                    break;
               

                    
                case "SelectHorse":

                    
                    int _index = 0;
                                           
                    _index = int.Parse(ms.b);
                    HorseInfo selectedInfo = new HorseInfo()
                    {
                        selcted = true,
                        user_id = ms.c,
                        index = _index,                        
                    };
                    HostID = ms.d;
                    Game._horsesInfo[_index] = selectedInfo;
                    Game.activePlayers = int.Parse(ms.e);
                    Game.activePlayers++;
                    Debug.LogError(Game.activePlayers);

                    Animator DoorAnimator = Game.Doors[_index].GetComponent<Animator>();
                    DoorAnimator.SetTrigger("isOpen");
                    
                    if (ms.c == HostID)
                    {
                        mStaticThings.I.StartCoroutine(Game.StartCountdown(15));
                            
                    }
                    
                    

                    Animator h_animator = Game.Horses[_index].GetComponent<Animator>();
                    h_animator.SetInteger("Speed", 1);
                           
                    mStaticThings.I.StartCoroutine(wait(5, Game.Horses[_index], selectedInfo));
                   
                    Debug.Log("select Message");
                    break;
                case "countdown":
                    int currCountdownValue = int.Parse(ms.b);
                    int activePlayers = int.Parse(ms.c);
                    foreach (HorseInfo i in Game._horsesInfo)
                    {
                        if (i.selcted)
                        {

                            _firstPanel.Find("Timer").GetComponent<Text>().text =
                            "目前玩家" + activePlayers + "人，游戏还剩 " + currCountdownValue + " 秒开始";
                        }

                    }
                    break;
                case "StartGame":
                    _secondPanel.gameObject.SetActive(true);
                    _firstPanel.Find("Timer").DOScaleX(0, 0.5f);
                    foreach (HorseInfo i in Game._horsesInfo)
                    {
                        if (i.selcted && mStaticThings.I.mAvatarID == i.user_id)
                        {
                            Game.PositionCamera(i.index);
                        }
                    }
                        
                    StartGame();
                   
                   
                        
                  
                    break;
                case "Finished":

                    Game.WinnerList.Add(Game._horsesInfo[int.Parse(ms.b)]);
                    Game.Horses[int.Parse(ms.b)].GetComponent<Animator>().SetInteger("Speed", 1);

                    if (Game.WinnerList.Count == Game.activePlayers && game_started)
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
                   
                    if (mStaticThings.I.mAvatarID == HostID)
                    {
                        int indexxx = int.Parse(ms.b);
                        Transform _t = Game.Horses[indexxx];
                        HorseInfo _i = Game._horsesInfo[indexxx];
                        _i.speed += 0.01f;
                        SendPosition(_t, "s");
                    }
                    break;
                
            }
        }
        IEnumerator wait(float waittime, Transform _t , HorseInfo h_inf)
        {
            yield return new WaitForSeconds(waittime);
            _t.GetComponent<Animator>().SetInteger("Speed", 0);
            h_inf.ready = true;
        }
        void StartGame()
        {
            HorseController._i.GameStarted = true;
            game_started = true;

            Transform _horseObj;
            foreach (HorseInfo i in Game._horsesInfo)
            {
                _horseObj = Game.Horses[i.index];
                if (!i.selcted)
                {
                    _horseObj.gameObject.SetActive(false);

                }
                else
                {
                    _firstPanel.Find("RawImage").DOScaleX(1, 0.2f);
                    
                    i.speed += 0.05f;
                    _horseObj.GetComponent<Animator>().SetInteger("Speed", 2);

                    SendPosition(_horseObj, "s");
                }


            }
            
        }
        void SendPosition(Transform _t, string _mark)
        {
            WsMovingObj _mov = new WsMovingObj()
            {
                id = HostID,
                islocal = true,
                position = _t.localPosition,
                rotation = _t.localRotation,
                scale = _t.localScale,
                mark = _mark,
                name = _t.name,
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), _mov);

        }
        IEnumerator ResetGame()
        {
            //HorseController._i.Horses[y.index].GetComponent<Animator>().SetInteger("Speed", 1);
            yield return new WaitForSeconds(2);
            _secondPanel.gameObject.SetActive(false);
            _secondPanel.Find("GameOver").DOScaleX(0, 0.2f);
            _secondPanel.Find("ResetButton").DOScaleX(0, 0.2f);
            _firstPanel.gameObject.SetActive(true);
            _firstPanel.Find("JoinGame").gameObject.SetActive(true);
            _firstPanel.Find("JoinGame").DOScaleX(1, 0.5f);
            _firstPanel.Find("Rules").DOScaleX(1, 0.5f);            
            _firstPanel.Find("RawImage").DOScaleX(1, 0.2f);

            foreach (HorseInfo i in HorseController._i._horsesInfo)
            {
                Game.Doors[i.index].GetComponent<Animator>().Rebind();
                Transform _t = Game.Horses[i.index];
                _t.gameObject.SetActive(true);
                _t.localPosition = new Vector3(0, _t.localPosition.y, _t.localPosition.z);

                _t.GetComponent<Animator>().SetInteger("Speed", 0);

                i.selcted = false;
                i.ready = false;
                i.speed = 0;                
                i.user_id = "";
               
            }
            Game.activePlayers = 0;
            Game.HostID = "";
            Game.WinnerList.Clear();
            Game.GameStarted = false;
            Game.myhorseIndex = 0;

            Game.PlayerCamera.gameObject.SetActive(false);
            

            
        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
    }
}
