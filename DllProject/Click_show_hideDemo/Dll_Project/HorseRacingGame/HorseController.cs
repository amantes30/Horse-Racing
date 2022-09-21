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
    public bool selcted, ready = false;
    public int index;
    public float speed = 0;
}

public class MainCanvas 
{
    public Canvas Main;
    public WaitUI _waitUI;
    public RulesUI _rulesUI;
    public Transform GamePlayPanel, StartPanel;

}

public class RulesUI
{
    public Transform panel;
    public Button joinGameBtn;
    public Button closeBtn;
}
public class WaitUI
{
    public Transform panel;
    public Text numberOfPlayers;
    public Text waitTime;
    public Button closeBtn;
    public Button OkBtn;
}

namespace Dll_Project.HorseRacingGame
{
    public class HorseController : DllGenerateBase
    {
        public static HorseController _i;
        
        // Objects From Scene
        public MainCanvas _mainCanvas;
        public Transform table;
        public Transform PlayerCamera;
        
        

        public List<Transform> Horses = new List<Transform>();       // List Of Horse Objects
        public List<HorseInfo> _horsesInfo = new List<HorseInfo>();   // Store Horse Information
        public List<Transform> Doors = new List<Transform>();

        public Text hostid;


        public int activePlayers = 0;
        public int Rank = 1;
        public int myhorseIndex;
        public int touchCount = 0;
        public float currCountdownValue;

        public bool GameStarted = false;
        public bool Finished = false;
        public bool counting = false;
        public bool horseselected = false;
        public bool canQuit = false;

        public string HostID;
        public string user_id;


        
        
        public override void Init()
        {
            _i = this;
            table = BaseMono.ExtralDatas[0].Target.transform;
            
            PlayerCamera = BaseMono.ExtralDatas[2].Target;
            
            
            Debug.Log("NEWHorseController Init !");
        }

        public override void Awake()
        {
            Debug.Log("HorseController Awake !");
            InitializeAllCanvas();
        }

        public override void Start()
        {

            if (mStaticThings.I != null && !mStaticThings.I.isVRApp)
            {
                user_id = mStaticThings.I.mAvatarID;
            }
            // INITIALIZE HORSES AND HORSE STATUS IN A LIST
            InitializeLists();
            
            //hostid = _mainCanvas.Main.transform.Find("HostId").GetComponent<Text>();
            RoomConnect();
        }
        public override void LateUpdate()
        {
           
            if (canQuit && Input.GetKeyDown(KeyCode.Q))
            {
                mStaticThings.I.StopAllCoroutines();
                StartCountdown(15).Reset();
                currCountdownValue = 15;
                Debug.Log("ola");
                HostID = "";
                activePlayers = 0;
                counting = false;
                _mainCanvas.Main.transform.GetChild(2).GetChild(0).DOScaleX(0, 0.2f);
                WsCChangeInfo ii = new WsCChangeInfo()
                {
                    a = "Finished",
                    b = 0.ToString(),
                    c = mStaticThings.I.mAvatarID,
                    d = 0.ToString()
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ii);
                canQuit = false;
            }
            if (GameStarted && horseselected)
            {
               
                if (HostID == "" || HostID == null)
                {
                    
                    currCountdownValue = 15;                    
                    activePlayers = 0;
                    counting = false;
                    _mainCanvas.Main.transform.GetChild(2).GetChild(0).DOScaleX(0, 0.2f);
                    WsCChangeInfo ii = new WsCChangeInfo()
                    {
                        a = "Finished",
                        b = 0.ToString(),
                        c = mStaticThings.I.mAvatarID,
                        d = 0.ToString()
                    };
                    MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ii);
                    GameStarted = false;
                    horseselected = false;
                    canQuit = false;
                }
                
                Text SpeedText = _mainCanvas.GamePlayPanel.Find("Speed").GetComponent<Text>();
                SpeedText.text = "Speed: " + (_horsesInfo[myhorseIndex].speed * 100);

                PlayerCamera.localPosition = new Vector3(Horses[myhorseIndex].localPosition.x + 2, 161.953f, PlayerCamera.localPosition.z);


                Accelerate();

                UIhorsePosUpdate(Horses[myhorseIndex], false);
                if (Horses[myhorseIndex].localPosition.x < -417 && !Finished)
                {
                    _mainCanvas.GamePlayPanel.Find("GameOver").GetChild(0).GetComponent<Text>().text =
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
                    Transform GameOverImage = _mainCanvas.GamePlayPanel.Find("GameOver");
                    GameOverImage.DOScaleX(1, 0.2f);

                    Horses[myhorseIndex].GetComponent<Animator>().SetInteger("Speed", 1);

                    UIhorsePosUpdate(Horses[myhorseIndex], true);

                    Finished = true;

                }

            }
            if (activePlayers > 2)
            {
                _mainCanvas.GamePlayPanel.Find("Timer").GetComponent<Text>().text = currCountdownValue.ToString();
            }

        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), Click);
            Debug.Log("HorseController OnEnable !");
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), Click);
            Debug.Log("HorseController OnDisable !");
        }

        public override void OnTriggerEnter(Collider other) => Debug.LogWarning(other);
        
        // for the little UI on the top left corner to show the progress of the horse
        void UIhorsePosUpdate(Transform _tt, bool reset)
        {
            Transform Img = _mainCanvas.GamePlayPanel.Find("HorsePosUI").GetChild(3).GetChild(0);
            Img.gameObject.SetActive(true);
            Img.localPosition = new Vector3((_tt.localPosition.x * 460) / -417, 0, 0);
            if (reset)
            {
                Img.gameObject.SetActive(false);
                Img.localPosition = Vector3.zero;
            }
        }
        public void SwitchPanel(string _which)
        {
            switch (_which)
            {
                case "Rules":
                    _mainCanvas._waitUI.panel.DOScaleX(0, 0.2f);
                    _mainCanvas.GamePlayPanel.gameObject.SetActive(false);                    
                    _mainCanvas.StartPanel.gameObject.SetActive(true);
                    _mainCanvas.StartPanel.transform.DOScaleX(1, 0.2f);
                    _mainCanvas._rulesUI.panel.DOScaleX(1, 0.2f);
                    _mainCanvas._rulesUI.panel.gameObject.SetActive(true);
                    _mainCanvas._rulesUI.joinGameBtn.gameObject.SetActive(true);
                    _mainCanvas._rulesUI.joinGameBtn.interactable = true;
                    break;
                case "GameOver":
                    _mainCanvas.GamePlayPanel.gameObject.SetActive(true);
                    _mainCanvas.GamePlayPanel.DOScaleX(1, 0.2f);
                    break;
                case "Wait":
                    ActivateWaitUI(activePlayers, 4);
                    break;
                case "Timer":
                    _mainCanvas.StartPanel.gameObject.SetActive(false);
                    _mainCanvas.GamePlayPanel.Find("Timer").DOScaleX(1, 0.2f);
                    _mainCanvas.GamePlayPanel.gameObject.SetActive(true);
                    _mainCanvas.GamePlayPanel.Find("RawImage").DOScaleX(1, 0.2f);
                    break;
                case "InGame":
                    _mainCanvas.GamePlayPanel.Find("RawImage").DOScaleX(0, 0.2f);
                    _mainCanvas.GamePlayPanel.Find("SpeedBtn").gameObject.SetActive(true);
                    break;
            }
        }
        public void ActivateWaitUI(int NoOfPlayers, int _waittime)
        {
            _mainCanvas._waitUI.closeBtn.onClick.AddListener(() =>
            {
                RoomConnect();
                SwitchPanel("Rules");
                _mainCanvas._waitUI.panel.DOScaleX(0, 0.2f);
            });
            _mainCanvas._waitUI.OkBtn.onClick.AddListener(() =>
            {
                RoomConnect();
                SwitchPanel("Rules");
                _mainCanvas._waitUI.panel.gameObject.SetActive(false);
                _mainCanvas._waitUI.panel.DOScaleX(0, 0.2f);
            });
            _mainCanvas._rulesUI.panel.DOScaleX(0, 0.2f);
            _mainCanvas._waitUI.numberOfPlayers.text = NoOfPlayers.ToString();
            _mainCanvas._waitUI.waitTime.text = _waittime.ToString() + " 分钟";
            _mainCanvas._waitUI.panel.gameObject.SetActive(true);
            _mainCanvas._waitUI.panel.DOScaleX(1, 0.2f);

        }
        
        // Timer
        public IEnumerator StartCountdown(float countdownValue)
        {
            counting = true;
            currCountdownValue = countdownValue;
            PlayerCamera.gameObject.SetActive(true);

            while (currCountdownValue >= 0)
            {
                //_mainCanvas.GamePlayPanel.Find("Timer").GetComponent<Text>().text = currCountdownValue.ToString();
                WsCChangeInfo o = new WsCChangeInfo()
                {
                    a = "Timer",
                    b = currCountdownValue.ToString()
                }; MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), o);
                yield return new WaitForSeconds(1.0f);
                
                currCountdownValue--;

            }
            if (activePlayers == 1)
            {
                //StartCountdown(15).Reset();
                mStaticThings.I.StartCoroutine(StartCountdown(15));
                _mainCanvas.Main.transform.GetChild(2).GetChild(0).DOScaleX(1, 0.2f);
                canQuit = true;
                currCountdownValue = 15;
            }

            if (activePlayers > 1)
            {
                _mainCanvas.GamePlayPanel.gameObject.SetActive(true);
                _mainCanvas.GamePlayPanel.Find("Timer").DOScaleX(0, 0.5f);

                WsCChangeInfo ms = new WsCChangeInfo
                {
                    a = "StartGame",
                    b = activePlayers.ToString(),
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), ms);

            }
        }
        public void InitializeAllCanvas()
        {
            _mainCanvas = new MainCanvas();
           
            _mainCanvas.Main = BaseMono.ExtralDatas[1].Target.GetComponent<Canvas>();
            _mainCanvas.StartPanel = _mainCanvas.Main.transform.GetChild(0);
            _mainCanvas.GamePlayPanel = _mainCanvas.Main.transform.GetChild(1);

            _mainCanvas._rulesUI = new RulesUI();
            _mainCanvas._rulesUI.panel = _mainCanvas.StartPanel.Find("Rules");
            _mainCanvas._rulesUI.closeBtn = _mainCanvas._rulesUI.panel.Find("CloseBtn").GetComponent<Button>();
            _mainCanvas._rulesUI.joinGameBtn = _mainCanvas._rulesUI.panel.Find("JoinGame").GetComponent<Button>();

            _mainCanvas._waitUI = new WaitUI();
            _mainCanvas._waitUI.panel = _mainCanvas.StartPanel.Find("wait");
            _mainCanvas._waitUI.numberOfPlayers = _mainCanvas._waitUI.panel.Find("info").GetChild(0).GetComponent<Text>();
            _mainCanvas._waitUI.waitTime = _mainCanvas._waitUI.panel.Find("info").GetChild(1).GetComponent<Text>();
            _mainCanvas._waitUI.closeBtn = _mainCanvas._waitUI.panel.Find("closeBtn").GetComponent<Button>();
            _mainCanvas._waitUI.OkBtn = _mainCanvas._waitUI.panel.Find("OkBtn").GetComponent<Button>();

            // Rules
            _mainCanvas._rulesUI.joinGameBtn.onClick.AddListener(JoinGame);
            _mainCanvas._rulesUI.closeBtn.onClick.AddListener(() => { _mainCanvas.StartPanel.DOScaleX(0, 0.3f); });


            Button SpeedUpBtn = _mainCanvas.GamePlayPanel.Find("SpeedBtn").GetComponent<Button>();
            SpeedUpBtn.gameObject.SetActive(true);
            SpeedUpBtn.onClick.AddListener(AddSpeed);
            SwitchPanel("Rules");

            // horseprogressBar

            
            
        }       
        void Click(IMessage msg)
        {
            if ((msg.Data as GameObject).name == "HorsesParent" && !horseselected)
            {
                _mainCanvas.StartPanel.DOScaleX(1, 0.3f);
            }
        }
       
        void Accelerate()
        {

            if (mStaticThings.I.mAvatarID == HostID)
            {

                foreach (HorseInfo i in _horsesInfo)
                {
                    if (i.selcted && i.ready)
                    {
                        Horses[i.index].Translate(Vector3.forward * i.speed * Time.deltaTime);
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
            
            WsCChangeInfo wsCChangeInfo = new WsCChangeInfo
            {
                a = "RoomConnected",
                b = user_id,
            };
            MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), wsCChangeInfo);
        } 

        // Join Game Button
        void JoinGame()
        {
            Debug.Log("JoinGame Clicked");
            _mainCanvas._rulesUI.joinGameBtn.gameObject.SetActive(false);
            if (activePlayers == 0) { HostID = user_id; }
            
            if (activePlayers < 10)
            {

                SwitchPanel("Timer");
                Debug.Log(mStaticThings.I.mAvatarID + " h_index: " + activePlayers);

                myhorseIndex = activePlayers;
                PositionCamera(myhorseIndex);
                horseselected = true;
                counting = true;
                activePlayers++;
                WsCChangeInfo _info = new WsCChangeInfo()
                {
                    a = "SelectHorse",
                    b = myhorseIndex.ToString(),
                    c = user_id,
                    d = HostID,
                    e = activePlayers.ToString()
                };
                MessageDispatcher.SendMessageData(WsMessageType.SendCChangeObj.ToString(), _info);

                
            }
            else if (activePlayers >= 10 || GameStarted)
            {
                Debug.Log("WAITTTT");
                SwitchPanel("Wait");
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
            
        }
        void PositionCamera(int _index)
        {
            PlayerCamera.gameObject.SetActive(true);
            float zPos = Horses[_index].localPosition.z;
            PlayerCamera.localPosition = new Vector3(PlayerCamera.transform.localPosition.x, 162.5f, zPos);


        }

    }
}