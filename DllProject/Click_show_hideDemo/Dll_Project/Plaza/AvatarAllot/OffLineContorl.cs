using com.ootii.Messages;
using Dll_Project.Plaza.Fly;
using Dll_Project.Showroom;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Plaza.AvatarAllot
{
    #region 获取频道人员数结构
    public class ChannelScene
    {
        public string id { get; set; }
        public string scene { get; set; }
        public bool isremote { get; set; }
    }
    public class Wp
    {
        public double y { get; set; }
        public double z { get; set; }
    }
    public class Wr
    {
        public double y { get; set; }
        public double w { get; set; }
    }

    public class Ws
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }
    public class Cp
    {
        public Hp hp { get; set; }
        public Hr hr { get; set; }
        public Hlp hlp { get; set; }
        public Hlr hlr { get; set; }
        public Hrp hrp { get; set; }
        public Hrr hrr { get; set; }
    }
    public class Hp
    {
        public double y { get; set; }
    }

    public class Hr
    {
        public double w { get; set; }
    }

    public class Hlp
    {
    }

    public class Hlr
    {
        public double w { get; set; }
    }

    public class Hrp
    {
    }

    public class Hrr
    {
        public double w { get; set; }
    }
    public class AlistItem
    {
        public string name { get; set; }
        public int sex { get; set; }
        public string wsid { get; set; }
        public ChannelScene scene { get; set; }
        public bool ae { get; set; }
        public string id { get; set; }
        public string aid { get; set; }
        public int m { get; set; }
        public Wp wp { get; set; }
        public double vol { get; set; }
        public Wr wr { get; set; }
        public Ws ws { get; set; }
        public Cp cp { get; set; }
        public string cl { get; set; }
    }
    public class PersonRoot
    {
        public List<AlistItem> alist { get; set; }
    }
    #endregion
    public class OffLineContorl : DllGenerateBase
    {
        public static OffLineContorl Instance;
        private Transform groupParent;

        private GameObject uiPlane;
        private Text titleText;
        private Button cancelBtn;
        private Button sureBtn;
        private ExtralData[] extralDatas;

        private GameObject AllPeoplePanel;

        private GameObject PingTai;
        public override void Init()
        {
            groupParent = BaseMono.ExtralDatas[0].Target;
            uiPlane = BaseMono.ExtralDatas[1].Target.gameObject;
            titleText = BaseMono.ExtralDatas[2].Target.GetComponent<Text>();
            cancelBtn = BaseMono.ExtralDatas[3].Target.GetComponent<Button>();
            sureBtn = BaseMono.ExtralDatas[4].Target.GetComponent<Button>();
            extralDatas = BaseMono.ExtralDatas[5].Info;
            AllPeoplePanel = BaseMono.ExtralDatas[6].Target.gameObject;
            PingTai = BaseMono.ExtralDatas[7].Target.gameObject;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            BaseMono.StartCoroutine(ChangePos());
            cancelBtn.onClick.AddListener(() => { uiPlane.SetActive(false); });
            sureBtn.onClick.AddListener(()=> { 
                uiPlane.SetActive(false);
                if (mStaticThings.I.isVRApp) 
                {
                    uiPlane.transform.parent.parent.gameObject.SetActive(false);
                }
                SaveInfo.instance.CollectPlatform(12);
                PingTai.SetActive(false);
                ChangeChanne();
            });
        }
        public override void OnEnable()
        {
            Instance = this;
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void Update()
        {
        }
        #endregion
        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("area_one"))
            {
                uiPlane.SetActive(true);
                titleText.text = "是否传至下方";
                if (mStaticThings.I.isVRApp)
                {
                    uiPlane.transform.parent.parent.gameObject.SetActive(true);
                }
            }
            else
            {
                uiPlane.SetActive(false);
            }
        }

        #region 人数获取
        int count = 1;
        public void ChangeChanne()
        {
            BaseMono.StartCoroutine(GetServer(count));
        }

        /// <summary>
        /// 获取人数
        /// </summary>
        private IEnumerator GetAvatarNum(int index, string roomURL, Action action)
        {
            if (!string.IsNullOrEmpty(roomURL))
            {
                var url = roomURL + $"&apikey={mStaticThings.apikey}&apitoken={mStaticThings.apitoken}";
                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError )
                {
                    request.Dispose();
                    BaseMono.StartCoroutine(GetAvatarNum(index, roomURL, action));
                }
                else
                {
                    int playerNum = GetCountFromJson(request.downloadHandler.text);
                    if (mStaticData.Identity.IsConfig == true && mStaticData.Identity.VIP.Contains(mStaticThings.I.mAvatarID))
                    {
                        ChangeRoom(mStaticData.CannelInformatica.OffLineCannels[1].RootRoomID, mStaticData.CannelInformatica.OffLineCannels[1].RootVoiceID);
                        JumpPos(groupParent);
                    }
                    else if (playerNum >= mStaticData.CannelInformatica.OffLineCannels[index].AreaMaxCount)
                    {
                        count++;
                        if (count < mStaticData.CannelInformatica.OffLineCannels.Count)
                        {
                            action();
                        }
                        else 
                        {
                            AllPeoplePanel.SetActive(true);
                        }
                    }
                    else
                    {
                        ChangeRoom(mStaticData.CannelInformatica.OffLineCannels[index].RootRoomID, mStaticData.CannelInformatica.OffLineCannels[index].RootVoiceID);
                        JumpPos(groupParent);
                    }

                    request.Dispose();
                }
            }
        }
        /// <summary> Json文件实体类 </summary>
        private int GetCountFromJson(string PersonList)
        {
            if (!string.IsNullOrEmpty(PersonList))
            {
                PersonRoot personRoot = JsonMapper.ToObject<PersonRoot>(PersonList);
                return personRoot.alist.Count;
            }
            else
            {
                return 0;
            }
        }

        private void Load()
        {
            BaseMono.StartCoroutine(GetServer(count));
        }
        #endregion

        #region 获取roomURL列表
        private IEnumerator GetServer(int index)
        {
            if (mStaticData.CannelInformatica.OffLineCannels.Count != 0)
            {
                string url = mStaticThings.serverhttp + mStaticThings.I.now_ServerURL + "/" + mStaticThings.apiversion + "/getsingleroomserver?roomid=" + mStaticData.CannelInformatica.OffLineCannels[index].RootRoomID + "&apikey=" + mStaticThings.apikey + "&apitoken=" + mStaticThings.apitoken + "&userid=" + mStaticThings.I.mAvatarID;
                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                {
                    request.Dispose();
                    BaseMono.StartCoroutine(GetServer(index));
                }
                else
                {
                    mStaticData.CannelInformatica.OffLineCannels[index].RoomURL = ServerUrl(request.downloadHandler.text);
                    if (!string.IsNullOrEmpty(mStaticData.CannelInformatica.OffLineCannels[index].RoomURL))
                    {
                        BaseMono.StartCoroutine(GetAvatarNum(count, mStaticData.CannelInformatica.OffLineCannels[count].RoomURL, Load));
                        
                    }
                    else
                    {
                        request.Dispose();
                        BaseMono.StartCoroutine(GetServer(index));
                    }
                }
            }
            else
            {
                yield return new WaitForSeconds(1);
                BaseMono.StartCoroutine(GetServer(index));
            }
        }

        private string ServerUrl(string str)
        {
            JsonData jd = JsonMapper.ToObject(str);
            JsonData jsonData = jd["data"];
            if (jd["info"].ToString() == "sucess")
            {
                var temp = "http://" + jsonData["server"].ToString() + "/getavatarlist?room=" + jsonData["room"].ToString();
                return temp;
            }
            return null;
        }
        #endregion

        #region 频道跳转
        private void JumpPos(Transform group)
        {
            int sort = 0;
            int max = group.GetComponent<VRPlayceGroup>()._VRPlayceDots.Count - 1;
            if (mStaticThings.AllStaticAvatarsDic.Count > 1)
            {
                sort = mStaticThings.I.GetSortNumber(mStaticThings.I.mAvatarID);
            }
            int playcenum = Mathf.Clamp(sort, 0, max);

            CharacterController control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
            if (control != null)
                control.enabled = false;

            mStaticThings.I.MainVRROOT.position = group.GetComponent<VRPlayceGroup>()._VRPlayceDots[playcenum].transform.position;
            if (control != null)
                control.enabled = true;

            for (int i = 0; i < extralDatas.Length; i++)
            {
                extralDatas[i].Target.localScale = Vector3.one;
            }
            SaveInfo.instance.CollectPlatform(11);
        }
        /// <summary>传送</summary>
        private void ChangeRoom(string RootRoomID, string RootVoiceID)
        {
            VRRootChanelRoom ch = new VRRootChanelRoom
            {
                roomid = RootRoomID,
                voiceid = RootVoiceID,
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.ConnectToNewChanel.ToString(), ch, 0);
        }
        #endregion

        /// <summary>
        /// 不是离线频道时跳转人员位置
        /// </summary>
        /// <returns></returns>
        private IEnumerator ChangePos() 
        {
            yield return new WaitForSeconds(0.1f);
            if (mStaticData.CannelInformatica.OffLineCannels.Count == 0)
            {
                BaseMono.StartCoroutine(ChangePos());
            }
            else 
            {
                if (mStaticData.CannelInformatica.OffLineCannels[0].RootRoomID != mStaticThings.I.nowRoomID) 
                {
                    JumpPos(groupParent);
                }
            }
        }
    }
}
