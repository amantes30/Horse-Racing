using com.ootii.Messages;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project
{
    public class Reciever : DllGenerateBase
    {
        private Canvas canvas;
        
        
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
                    HorseController._i.HostID = ms.b == "" ? "" : ms.b;                               
                    break;
                case "Select Horse":
                    int index = int.Parse(ms.b);                    
                    if (mStaticThings.I.mAvatarID == ms.c) { return; }
                    Transform selectedhorse = HorseController._i.Horses[index];
                    HorseInfo selectedInfo = HorseController._i._horsesInfo[index];

                    // set button Off
                    selectedhorse.GetChild(3).GetChild(0).GetChild(1).gameObject.SetActive(false);
                    selectedhorse.GetChild(3).GetChild(0).GetChild(3).gameObject.SetActive(true);
                    selectedhorse.GetChild(3).GetChild(0).GetChild(3).GetComponent<Text>().text = "备好了"; //ready

                    // update horse information
                    selectedInfo.selcted = true;
                    selectedInfo.user_id = ms.c;
                    selectedInfo.NoOfUsers += 1;  
                   
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
                case "AddSpeed":                   
                    if (mStaticThings.I.mAvatarID == HorseController._i.HostID)
                    {
                        int ind = int.Parse(ms.b);                        
                        HorseController._i._horsesInfo[ind].speed +=0.5f;
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
                _inf.NoOfUsers = 0;
                _inf.req_inpt = "";
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
