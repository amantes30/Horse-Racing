using com.ootii.Messages;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using DG.Tweening;
namespace Dll_Project
{
    public class Reciever : DllGenerateBase
    {
        private Canvas canvas;

        private string HostID;

        
        private bool game_started = false;
        public override void Init()
        {
           
            Debug.Log("Reciever Is ON"); 
            
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
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);           
                      
                       
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
                case "RoomConnected":
                    
                    if (mStaticThings.I.mAvatarID == HostID)
                    {
                        NewUserInfo _info = JsonMapper.ToObject<NewUserInfo>(ms.c);
                        _info = new NewUserInfo() {
                            __horseinfo = HorseController._i._horsesInfo,
                            _gameStarted = HorseController._i.GameStarted,
                            _hostID = HorseController._i.HostID,
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
                        if (game_started)
                        {
                            // gamestarted UI wait
                            foreach (HorseInfo o in HorseController._i._horsesInfo)
                            {
                                if (o.selcted)
                                {
                                    HorseController._i.Horses[o.index].gameObject.SetActive(true);
                                }
                            }
                        }
                    }
                    break;
                case "CheckGameStatus":
                    bool already_started, full = false;
                    
                    if (mStaticThings.I.mAvatarID == ms.b)
                    {
                        int indexx = int.Parse(ms.c);
                        if(int.Parse(ms.c) > 10 || HorseController._i.GameStarted)
                        {
                            // FULL

                        }
                        else
                        {
                            WsCChangeInfo I = new WsCChangeInfo()
                            {
                                a = "Select Horse",
                                b = indexx.ToString(),
                                c = mStaticThings.I.mAvatarID,
                                
                            };MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), I);
                        }
                    }
                    break;
                case "Select Horse":
                    if (mStaticThings.I.mAvatarID == ms.c)
                    {
                        int index = int.Parse(ms.b);

                        Transform selectedhorse = HorseController._i.Horses[index];
                        HorseInfo selectedInfo = HorseController._i._horsesInfo[index];

                        // set button Off
                        canvas.transform.GetChild(0).Find("StartGame").gameObject.SetActive(false);
                        selectedhorse.GetChild(3).GetChild(0).GetChild(3).gameObject.SetActive(true);


                        // update horse information
                        selectedInfo.selcted = true;
                        selectedInfo.user_id = ms.c;
                        HostID = mStaticThings.I.mAvatarID;

                    }
                    Debug.Log("select Message");
                    break;
                case "StartGame":
                    /// turn OFF startGame btn
                    canvas.transform.GetChild(0).Find("StartGame").gameObject.SetActive(false);
                    // set hostID
                    HorseController._i.HostID = HorseController._i.HostID == "" ? mStaticThings.I.mAvatarID : ms.b;
                    Debug.Log(ms.b);
                    StartGame();
                    break;
                case "SpeedControl":                   
                    if (mStaticThings.I.mAvatarID == HorseController._i.HostID)
                    {
                        int ind = int.Parse(ms.b);
                        switch (ms.d)
                        {
                            case "+":                                
                                HorseController._i._horsesInfo[ind].speed += 0.5f;
                                break;
                            case "-":
                                HorseController._i._horsesInfo[ind].speed -= 0.5f;
                                break;
                        }
                        
                    }
                   
                    break;
                case "GameOver":
                    mStaticThings.I.StartCoroutine(ResetGame(int.Parse(ms.b)));
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
                else
                {
                    Transform selectedhorse = HorseController._i.Horses[i.index];
                    HorseInfo _info = HorseController._i._horsesInfo[i.index];
                    i.speed = 0.1f;
                    selectedhorse.GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = "当前播放";//playing
                    HorseController._i.GenerateRandomLetter(selectedhorse.gameObject, _info);
                }
                HorseController._i.GameStarted = true;
                game_started = true;

            }
        }

        IEnumerator ResetGame(int winner_index)
        {
            foreach(Transform _t in HorseController._i.Horses)
            {
                _t.GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = "Lost";
            }
            HorseController._i.Horses[winner_index].GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = "赢家"; //winner
            HorseController._i.GameStarted = false;
            canvas.transform.GetChild(0).Find("Speed").GetComponent<Text>().text = "速度: 0";
            yield return new WaitForSeconds(2);
            canvas.transform.GetChild(0).Find("Selected Car").GetComponent<Text>().text = "选定的马：";
            canvas.transform.GetChild(0).Find("Req_Input").GetComponent<Text>().text = "";

            canvas.transform.GetChild(0).Find("Speed").gameObject.SetActive(false);
            if (mStaticThings.I.mAvatarID == HorseController._i.HostID)
            {
                foreach (Transform _t in HorseController._i.Horses)
                {
                    _t.localPosition = new Vector3(_t.localPosition.x, _t.localPosition.y, 0);
                    
                    _t.gameObject.SetActive(true);
                    _t.GetChild(3).GetChild(0).GetChild(1).gameObject.SetActive(true);
                    _t.GetChild(3).GetChild(0).GetChild(3).gameObject.SetActive(false);
                    WsMovingObj m = new WsMovingObj
                    {
                        id = "",
                        name = _t.name,
                        islocal = true,
                        mark = "i",
                        position = _t.localPosition,
                        rotation = _t.localRotation,
                        scale = _t.localScale
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), m);
                }

            }
            HorseController._i.HostID = "";
            
            foreach (HorseInfo _inf in HorseController._i._horsesInfo)
            {
                if (_inf.selcted)
                {
                    HorseController._i.Horses[_inf.index].Find("HorseObj").GetChild(0).GetComponent<Animator>().SetBool("Eat_b", true);
                    HorseController._i.Horses[_inf.index].Find("HorseObj").GetChild(0).GetComponent<Animator>().SetFloat("Speed_f", 0f);
                }
                
                _inf.selcted = false;
                _inf.speed = 0;
                _inf.user_id = "";
            }
        }
        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
    }
}
