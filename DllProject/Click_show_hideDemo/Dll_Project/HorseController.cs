using com.ootii.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

using DG.Tweening;

public class HorseInfo
{
    public string user_id { get; set; }
    public bool selcted, ready= false;    
    public int index;    
    public float speed = 0;
    
}

public class NewUserInfo
{
    public List<HorseInfo> __horseinfo;
    public bool ButtonPressed, _gameStarted;
    public string _hostID = "";
    public int activeUsers;
}

namespace Dll_Project
{
    public class HorseController : DllGenerateBase 
    {
        public static HorseController _i;

        // Objects From Scene
        public Canvas MainCanvas;
        public Transform table;        
        public Transform PlayerCamera;
        Transform _firstPanel;
        Transform _secondPanel;

        public List<Transform> Horses = new List<Transform>();        // List Of Horse Objects
        
        public List<HorseInfo> _horsesInfo = new List<HorseInfo>();   // Store Horse Information
        public List<Transform> Doors = new List<Transform>();

        public int activePlayers = 0;
        public int Rank = 1;
        public int myhorseIndex;
        public int touchCount = 0;

        public bool GameStarted = false;
        public bool Finished = false;
        public bool counting = false;
        public bool horseselected = false;

        public string HostID;
        public string user_id;


        public Text hostid;
        // WIN POSX -417
        public override void Init()
        {
            _i = this;
            table = BaseMono.ExtralDatas[0].Target.transform;
            MainCanvas = BaseMono.ExtralDatas[1].Target.GetComponent<Canvas>();
            PlayerCamera = BaseMono.ExtralDatas[2].Target;
            _firstPanel = MainCanvas.transform.GetChild(0);
            _secondPanel = MainCanvas.transform.GetChild(1);
            hostid = MainCanvas.transform.Find("HostId").GetComponent<Text>();
            Debug.Log("HorseController Init !");
        }

        public override void Awake()
        {
            Debug.Log("HorseController Awake !");            
                       
        }      
      
        public override void Start()
        {
            RoomConnect();

            // INITIALIZE HORSES AND HORSE STATUS IN A LIST
            InitializeLists();

            Button SpeedUpBtn = _secondPanel.Find("SpeedBtn").GetComponent<Button>();
            SpeedUpBtn.gameObject.SetActive(true);
            SpeedUpBtn.onClick.AddListener(AddSpeed);

            if (mStaticThings.I != null && !mStaticThings.I.isVRApp)
            {
                user_id = mStaticThings.I.mAvatarID;
            }

           
            _firstPanel.Find("JoinGame").GetComponent<Button>().onClick.AddListener(JoinGame);
        }
        

        public override void LateUpdate()
        {
           
            if (GameStarted)
            {
                Text SpeedText = _secondPanel.Find("Speed").GetComponent<Text>();
                SpeedText.text = "Speed: " + (_horsesInfo[myhorseIndex].speed * 100);                
                
                PlayerCamera.localPosition =new Vector3(Horses[myhorseIndex].localPosition.x + 2, 161.953f ,PlayerCamera.localPosition.z); 
                Debug.Log(HostID);
                
                Accelerate(); 
                
                
                if (Horses[myhorseIndex].localPosition.x < -417 && !Finished)
                {
                    _secondPanel.Find("GameOver").GetChild(0).GetComponent<Text>().text =
                           "           " + Rank;
                    Rank++;
                    WsCChangeInfo ii = new WsCChangeInfo()
                    {
                        a = "Finished",
                        b = myhorseIndex.ToString(),
                        c = mStaticThings.I.mAvatarID,
                        d = Rank.ToString()
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ii);
                    Transform GameOverImage = _secondPanel.Find("GameOver");
                    GameOverImage.DOScaleX(1, 0.2f);
                    
                    Horses[myhorseIndex].GetComponent<Animator>().SetInteger("Speed", 1);
                    

                    
                    Finished = true;
                    
                }

            }
            if(activePlayers > 2)
            {
                _secondPanel.Find("Timer").GetComponent<Text>().text = currCountdownValue.ToString();
            }
            
        }
        public override void OnEnable() => Debug.Log("HorseController OnEnable !");

        public override void OnDisable() => Debug.Log("HorseController OnDisable !");

        public override void OnTriggerEnter(Collider other) => Debug.LogWarning(other);
        void Accelerate()
        {
           
            if (mStaticThings.I.mAvatarID == HostID)
            {
                Debug.Log("ACCCCCCC");
                foreach (HorseInfo i in _horsesInfo)
                {
                    if (i.selcted && i.ready)
                    {
                        Horses[i.index].Translate(Vector3.forward * i.speed*Time.deltaTime);
                        WsMovingObj _mov = new WsMovingObj()
                        {
                            id = i.index.ToString(),
                            islocal = true,
                            mark = "i",
                            name = Horses[i.index].name,
                            position = Horses[i.index].localPosition,
                            rotation = Horses[i.index].localRotation,
                            scale = Horses[i.index].localScale,
                        };
                        MessageDispatcher.SendMessageData(WsMessageType.SendMovingObj.ToString(), _mov);

                    }
                }
            }
        }
        void AddSpeed()
        {
            touchCount++;
            if (touchCount > 10 && GameStarted)
            {
                WsCChangeInfo p = new WsCChangeInfo()
                {
                    a = "AddSpeed",
                    b = myhorseIndex.ToString(),
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), p);
                touchCount = 0;
            }
        }
        void RoomConnect()
        {
            MainCanvas.transform.GetChild(0).DOScaleX(1, 0.2f);            
            WsCChangeInfo wsCChangeInfo = new WsCChangeInfo
            {
                a = "RoomConnected",
                b = user_id,
                
               
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), wsCChangeInfo);
        }
        public float currCountdownValue;
        public IEnumerator StartCountdown(float countdownValue)
        {
            counting = true;
            currCountdownValue = countdownValue;
            PlayerCamera.gameObject.SetActive(true);

            while (currCountdownValue >= 0)
            {
                _secondPanel.Find("Timer").GetComponent<Text>().text = currCountdownValue.ToString();
                Debug.LogError("rec user " + user_id);

                Debug.Log("Countdown: " + currCountdownValue);
                yield return new WaitForSeconds(1.0f);
                /*WsCChangeInfo _in = new WsCChangeInfo()
                {
                    a = "Count",
                    b = currCountdownValue.ToString(),
                    c = activePlayers.ToString(),
                }; MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), _in);
                */
                currCountdownValue--;

            }
            if (activePlayers == 1)
            {
                mStaticThings.I.StartCoroutine(StartCountdown(15));
            }
            Debug.Log(activePlayers);
            Debug.Log("DONEEE");
            if (activePlayers > 1)
            {
                _secondPanel.gameObject.SetActive(true);
                _secondPanel.Find("Timer").DOScaleX(0, 0.5f);
                _firstPanel.Find("CloseBtn").gameObject.SetActive(false);
                WsCChangeInfo ms = new WsCChangeInfo
                {
                    a = "StartGame",
                    b = activePlayers.ToString(),
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ms);

            }
        }

        // Join Game Button
        void JoinGame()
        {
            Debug.Log("JoinGame Clicked");
            _firstPanel.Find("JoinGame").gameObject.SetActive(false);
            if (activePlayers == 0) { HostID = user_id; }
            
            if (activePlayers <= 10)
            {
                _firstPanel.Find("Rules").DOScaleX(0, 0.5f);
                _firstPanel.Find("JoinGame").DOScaleX(0, 0.5f);
                _secondPanel.Find("Timer").DOScaleX(1, 0.2f);
                _firstPanel.Find("Back").gameObject.SetActive(false);

                _firstPanel.Find("RawImage").DOScaleX(1, 0.2f);
                Debug.Log(mStaticThings.I.mAvatarID + " h_index: " + activePlayers);

                myhorseIndex = activePlayers;
                PositionCamera(myhorseIndex);
                horseselected = true;
                
                WsCChangeInfo _info = new WsCChangeInfo()
                {
                    a = "SelectHorse",
                    b = myhorseIndex.ToString(),
                    c = user_id,
                    d = HostID,
                    e = activePlayers.ToString()
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), _info);
                mStaticThings.I.StartCoroutine(StartCountdown(15));
            }
            else if (activePlayers > 10)
            {
                GameStarted = false;
                Debug.Log("WAITTTT");
                _firstPanel.Find("JoinGame").gameObject.SetActive(true);
                _firstPanel.Find("JoinGame").GetComponent<Button>().interactable = false;
                // FULL,  WAIT FEW MINUTES UI
            }

        }
        // Get HorseObjects, gates, and save their status
        void InitializeLists()
        {
            for (int i = 0; i < table.childCount - 1; i++)
            {
                Horses.Add(table.GetChild(i));
                HorseInfo h_inf = new HorseInfo
                {
                    index = i,
                    selcted = false,
                    user_id = "",
                    speed = 0,
                };
                _horsesInfo.Add(h_inf);
                Doors.Add(BaseMono.ExtralDatas[3].Info[i].Target);
            }
            _firstPanel.Find("Rules").gameObject.SetActive(true);
            _firstPanel.Find("JoinGame").gameObject.SetActive(true);
            _firstPanel.Find("CloseBtn").gameObject.SetActive(true);
            _firstPanel.Find("Back").gameObject.SetActive(true);
        }
        public void PositionCamera(int _index)
        {
            PlayerCamera.gameObject.SetActive(true);
            float zPos = Horses[_index].localPosition.z;
            
            PlayerCamera.localPosition = new Vector3 (PlayerCamera.transform.localPosition.x , 162.5f, zPos);
           
        }
        
    }
}