using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

using DG.Tweening;

class NewUserInfo
{
    public bool _gameStarted = false;
    public string _hostID = "";
    public int activeUsers = 0;
}
namespace Dll_Project.HorseRacingGame
{
    public class HrsGameReciever : DllGenerateBase
    {
        MainCanvas _canvas;        
        private string HostID;

        public override void Init()
        {
           
                        
            Debug.Log("Reciever Is ON");
        }
        public override void Awake()
        {
            _canvas = HorseController._i._mainCanvas;
            
            Debug.Log("Reciever Is On");
        }
        public override void Start() => Debug.Log("Reciever Is On");

        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        }
        public override void OnTriggerEnter(Collider other) => base.OnTriggerEnter(other);
       
        IEnumerator PrepareHorse(float waittime, Transform _t, HorseInfo h_inf)
        {
            yield return new WaitForSeconds(waittime);
            _t.GetComponent<Animator>().SetInteger("Speed", 0);
            h_inf.ready = true;
        }
        void ResetGame()
        {
            //HorseController._i.Horses[y.index].GetComponent<Animator>().SetInteger("Speed", 1);
            
            _canvas.GamePlayPanel.gameObject.SetActive(false);
            _canvas.GamePlayPanel.Find("GameOver").DOScaleX(0, 0.2f);
            _canvas.GamePlayPanel.Find("ResetButton").DOScaleX(0, 0.2f);

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
            HorseController._i.touchCount = 0;

            Transform _c = HorseController._i.PlayerCamera;
            _c.localPosition = new Vector3(-3, _c.localPosition.y, _c.localPosition.z);
            WsCChangeInfo wsCChangeInfo = new WsCChangeInfo
            {
                a = "RoomConnected",
                b = mStaticThings.I.mAvatarID,
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), wsCChangeInfo);



        }
        void RecieveCChangeObj(IMessage m)
        {
            WsCChangeInfo ms = m.Data as WsCChangeInfo;

            switch (ms.a)
            {
                case "RoomConnected":
                    Debug.Log("Room Connect");
                    Debug.Log("OKKKKK I recived " + _canvas.Main.name);
                    _canvas.Main.transform.GetChild(2).GetChild(0).DOScaleX(0, 0.2f);
                    _canvas.Main.transform.GetChild(3).GetComponent<Text>().text = "Active Users: " + HorseController._i.activePlayers;
                    _canvas.Main.transform.GetChild(4).GetComponent<Text>().text = "Host: " + HostID;
                    if (HostID!=null && HostID == HorseController._i.user_id)
                    {
                        

                        //NewUserInfo _info = new NewUserInfo();
                        NewUserInfo _info = new NewUserInfo()
                        {
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
                            _canvas._rulesUI.joinGameBtn.gameObject.SetActive(true);
                        }
                    }


                    break;
                case "ForNewUser":

                    if (mStaticThings.I.mAvatarID == ms.c)
                    {

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
                                _canvas._rulesUI.joinGameBtn.gameObject.SetActive(true);
                                Animator _animator = HorseController._i.Horses[i].GetComponent<Animator>();
                                _animator.SetInteger("Speed", 1);

                                mStaticThings.I.StartCoroutine(PrepareHorse(4, HorseController._i.Horses[i], _tt));

                            }


                            else if (p._gameStarted)
                            {
                                _canvas._rulesUI.panel.DOScaleX(0, 0.2f);
                                _canvas._waitUI.panel.DOScaleX(1, 0.3f);
                                // gamestarted UI wait
                                HorseController._i.ActivateWaitUI(p.activeUsers, 4);
                                foreach (HorseInfo o in HorseController._i._horsesInfo)
                                {
                                    bool flag = o.selcted ? true : false;
                                    HorseController._i.Horses[o.index].gameObject.SetActive(flag);

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

                    HostID =  ms.d;
                    HorseController._i.HostID = HostID;
                   // HorseController._i.hostid.text = "Host ID - " + HostID;
                    HorseController._i._horsesInfo[_index] = selectedInfo;
                    HorseController._i.activePlayers = int.Parse(ms.e);


                    _canvas.Main.transform.GetChild(3).GetComponent<Text>().text = "Active Users: " + HorseController._i.activePlayers;
                    _canvas.Main.transform.GetChild(4).GetComponent<Text>().text = "Host: " + HorseController._i.HostID;
                    Animator DoorAnimator = HorseController._i.Doors[_index].GetComponent<Animator>();
                    DoorAnimator.SetTrigger("isOpen");
                    

                    if (HorseController._i.counting && ms.d == HostID) {

                        mStaticThings.I.StopAllCoroutines();
                        HorseController._i.currCountdownValue = 15;
                        mStaticThings.I.StartCoroutine( HorseController._i.StartCountdown(15));
                    }

                    Animator h_animator = HorseController._i.Horses[_index].GetComponent<Animator>();
                    h_animator.SetInteger("Speed", 1);

                    mStaticThings.I.StartCoroutine(PrepareHorse(4, HorseController._i.Horses[_index], selectedInfo));
                    Debug.Log("select Message");
                    break;
                case "Timer":
                    if (HorseController._i.counting)
                    {
                        _canvas.GamePlayPanel.Find("Timer").GetComponent<Text>().text = ms.b;
                    }
                    break;
                case "StartGame":
                    HorseController._i.activePlayers = int.Parse(ms.b);
                    _canvas.Main.transform.GetChild(2).GetChild(0).DOScaleX(0, 0.2f);
                    StartGame();

                    break;
                case "AddSpeed":
                    int indexxx = int.Parse(ms.b);
                    Transform _t = HorseController._i.Horses[indexxx];
                    HorseInfo _i = HorseController._i._horsesInfo[indexxx];
                    _i.speed += 0.5f;
                    

                    break;
                case "Finished":
                    if (ms.b == ms.d)
                    {
                        ResetGame();
                    }
                    if (!HorseController._i.GameStarted) return;

                    HorseController._i.Rank = int.Parse(ms.d);

                    HorseController._i.Horses[int.Parse(ms.b)].GetComponent<Animator>().SetInteger("Speed", 1);
                    HorseController._i._horsesInfo[int.Parse(ms.b)].speed = 0;
                    Debug.Log("RANKR = " + HorseController._i.Rank);
                    Debug.Log("act  = " + HorseController._i.activePlayers);
                    Button ResetBtn = _canvas.GamePlayPanel.Find("ResetButton").GetComponent<Button>();
                    
                    ResetBtn.onClick.AddListener(() =>
                    {
                        ResetGame();
                    });
                    if (HorseController._i.Rank - 1 == HorseController._i.activePlayers && HorseController._i.GameStarted)
                    {
                        HorseController._i.GameStarted = false;
                        Debug.Log("COMPLETEEEE");
                        Transform cam = HorseController._i.PlayerCamera;
                        cam.localPosition = new Vector3(-3.9f, cam.localPosition.y, cam.localPosition.z);
                        ResetBtn.gameObject.SetActive(true);
                        ResetBtn.transform.DOScaleX(1, 0.2f);
                    }
                    
                    break;
            }
        }
        void StartGame()
        {
            HorseController._i.GameStarted = true;
            Transform _horseObj;
            foreach (HorseInfo i in HorseController._i._horsesInfo)
            {
                _horseObj = HorseController._i.Horses[i.index];

                if (!i.selcted) { _horseObj.gameObject.SetActive(false); }
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
            if (HostID == mStaticThings.I.mAvatarID)
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
        }
        
        
    }
}
