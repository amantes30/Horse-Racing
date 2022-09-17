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

        private string HostID, user_id;        
        
        private bool game_started = false;

        
        public override void Init()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            canvas = BaseMono.ExtralDatas[0].Target.GetComponent<Canvas>();
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
            user_id = HorseController._i.user_id;
            
            Debug.Log("Reciever Is On");
        }
        
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.SendCChangeObj.ToString(), RecieveCChangeObj);           
                      
                       
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.SendCChangeObj.ToString(), RecieveCChangeObj);            
                   
        }
      
        void RecieveCChangeObj(IMessage m)
        {
            WsCChangeInfo ms = m.Data as WsCChangeInfo;
            
            switch (ms.a)
            { 
                case "RoomConnected":
                    
                    NewUserInfo _info = new NewUserInfo();
                    if (HostID == HorseController._i.user_id)
                    {
                        _info = new NewUserInfo()
                        {
                            //__horseinfo = HorseController._i._horsesInfo,
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

                        if (!_info._gameStarted)
                        {
                            _firstPanel.Find("JoinGame").gameObject.SetActive(true);
                        }
                        
                        //for (int i = 0; i < _info.activeUsers; i++) 
                        //{
                        //    Transform _tt = HorseController._i.Horses[i];
                            
                        //    SendPosition(_tt, "s");
                        //}
                        /*foreach (HorseInfo i in _info.__horseinfo)
                        {
                            if (i.selcted)
                            {
                                //Transform _tt = HorseController._i.Horses[i.index];
                                //SendPosition(_tt, "s");
                            }
                        }*/
                    }


                    break;
                case "ForNewUser":

                    if (mStaticThings.I.mAvatarID == ms.c)
                    {
                        List<Transform> newUserHorses = HorseController._i.Horses;
                        NewUserInfo p = JsonMapper.ToObject<NewUserInfo>(ms.b);                        
                        HorseController._i.GameStarted = p._gameStarted;
                        HorseController._i.HostID = p._hostID;
                        HorseController._i.activePlayers = p.activeUsers;
                        for (int i = 0; i < p.activeUsers; i++)
                        {
                            HorseInfo _tt = new HorseInfo()
                            {
                                selcted = true,
                                index = i,
                            };
                            HorseController._i._horsesInfo[i] = _tt;
                            Animator _DoorAnimator = HorseController._i.Doors[i].GetComponent<Animator>();
                            _DoorAnimator.SetTrigger("isOpen");

                            if (!p._gameStarted)
                            {
                                _firstPanel.Find("JoinGame").gameObject.SetActive(true);
                                Animator _animator = HorseController._i.Horses[i].GetComponent<Animator>();
                                _animator.SetInteger("Speed", 1);

                                mStaticThings.I.StartCoroutine(PrepareHorse(4, HorseController._i.Horses[i], _tt));

                            }


                            else if (p._gameStarted)
                            {
                                _firstPanel.Find("Rules").DOScaleX(0, 0.2f);
                                _firstPanel.Find("wait").DOScaleX(1, 0.3f);
                                // gamestarted UI wait
                                HorseController._i.ActivateWaitUI(p.activeUsers, 4);
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
                    }
                    break;
                    
                case "SelectHorse":
                    int _index = int.Parse(ms.b);
                    
                    HorseInfo selectedInfo = new HorseInfo()
                    {
                        selcted = true,
                        user_id = ms.c,
                        index = _index,                        
                    };
                     
                    HostID = HostID == "" ? ms.c : ms.d;
                    HorseController._i.HostID = HostID;
                    HorseController._i.hostid.text = "Host ID - " + HostID;
                    HorseController._i._horsesInfo[_index] = selectedInfo;
                    HorseController._i.activePlayers = int.Parse(ms.e);
                    
                    

                    Animator DoorAnimator = HorseController._i.Doors[_index].GetComponent<Animator>();
                    DoorAnimator.SetTrigger("isOpen");

                    
                    if (HorseController._i.counting)
                    {
                        HorseController._i.currCountdownValue = 15;
                    }

                    Animator h_animator = HorseController._i.Horses[_index].GetComponent<Animator>();
                    h_animator.SetInteger("Speed", 1);
                           
                    mStaticThings.I.StartCoroutine(PrepareHorse(4, HorseController._i.Horses[_index], selectedInfo));                   
                    Debug.Log("select Message");
                    break;
               
                case "StartGame":
                    HorseController._i.activePlayers = int.Parse(ms.b);
                    StartGame();

                    break;
                case "AddSpeed":                 
                    
                        int indexxx = int.Parse(ms.b);
                        Transform _t = HorseController._i.Horses[indexxx];
                        HorseInfo _i = HorseController._i._horsesInfo[indexxx];
                        _i.speed += 0.5f;
                        SendPosition(_t, "s");
                   
                    break;
                case "Finished":
                    if (!HorseController._i.GameStarted) return;

                    HorseController._i.Rank = int.Parse(ms.d);

                    HorseController._i.Horses[int.Parse(ms.b)].GetComponent<Animator>().SetInteger("Speed", 1);
                    HorseController._i._horsesInfo[int.Parse(ms.b)].speed = 0;
                    Debug.Log("RANKR = " + HorseController._i.Rank);
                    Debug.Log("act  = " + HorseController._i.activePlayers);

                    if (HorseController._i.Rank-1 == HorseController._i.activePlayers && HorseController._i.GameStarted)
                    {
                        HorseController._i.GameStarted = false;
                        Debug.Log("COMPLETEEEE");
                        Transform cam = HorseController._i.PlayerCamera;
                        cam.localPosition = new Vector3(-3.9f, cam.localPosition.y, cam.localPosition.z);
                        Button ResetBtn = canvas.transform.GetChild(1).Find("ResetButton").GetComponent<Button>();
                        ResetBtn.gameObject.SetActive(true);
                        ResetBtn.transform.DOScaleX(1, 0.2f);
                        ResetBtn.onClick.AddListener(() =>
                        {
                            mStaticThings.I.StartCoroutine(ResetGame());
                        });

                    }

                    break;

            }
        }
        IEnumerator PrepareHorse(float waittime, Transform _t , HorseInfo h_inf)
        {
            yield return new WaitForSeconds(waittime);
            _t.GetComponent<Animator>().SetInteger("Speed", 0);
            h_inf.ready = true;
        }
        void StartGame()
        {
            HorseController._i.GameStarted = true;
            

            Transform _horseObj;
            foreach (HorseInfo i in HorseController._i._horsesInfo)
            {
                _horseObj = HorseController._i.Horses[i.index];
                if (!i.selcted)
                {
                    _horseObj.gameObject.SetActive(false);

                }
                else
                {
                    i.speed += 1f;
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

            HorseController._i.SwitchPanel("Rules");
            
            foreach (HorseInfo i in HorseController._i._horsesInfo)
            {
                HorseController._i.Doors[i.index].GetComponent<Animator>().Rebind();
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
            HostID = "";
            HorseController._i.Rank = 1;
            
            HorseController._i.counting = false;
            HorseController._i.horseselected = false;
            HorseController._i.myhorseIndex = 0;
            HorseController._i.Finished = false;

            Transform _c = HorseController._i.PlayerCamera;
            _c.localPosition = new Vector3(-3, _c.localPosition.y, _c.localPosition.z);
            WsCChangeInfo wsCChangeInfo = new WsCChangeInfo
            {
                a = "RoomConnected",
                b = mStaticThings.I.mAvatarID,
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), wsCChangeInfo);



        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
    }
}
