using com.ootii.Messages;
using Dll_Project.Plaza.AvatarAllot;
using Dll_Project.Showroom;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Plaza.Teleport
{
    public class TeleportClick : DllGenerateBase
    {
        private Channel channel;
        private GameObject uiPanel;
        private Text text;
        private Text numText;
        private GameObject icon;
        public override void Init()
        {
            uiPanel = BaseMono.ExtralDatas[0].Target.gameObject;
            text = BaseMono.ExtralDatas[1].Target.GetComponent<Text>();
            numText = BaseMono.ExtralDatas[2].Target.GetComponent<Text>();
            icon = BaseMono.ExtralDatas[3].Target.gameObject;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            BaseMono.GetComponent<Button>().onClick.AddListener(Click);
            BaseMono.StartCoroutine(GetServer());
        }
        public override void OnEnable()
        {
            channel = JsonMapper.ToObject<Channel>(BaseMono.OtherData);
            text.text = channel.Name;
            numText.text = "...";
            isOpen = false;
        }

        public override void OnDisable()
        {
        }
        bool isOpen = true;
        float time=3;
        public override void Update()
        {
            if (uiPanel.activeSelf) 
            {
                time += Time.deltaTime;
                if (time > 2)
                {
                    if (!string.IsNullOrEmpty(channel.RoomURL))
                    {
                        BaseMono.StartCoroutine(GetAvatarNum(channel.RoomURL));
                    }
                    time = 0;
                }
            }
        }
        #endregion

        #region 获取roomURL列表
        private IEnumerator GetServer()
        {
            if (!string.IsNullOrEmpty(BaseMono.OtherData))
            {
                string url = mStaticThings.serverhttp + mStaticThings.I.now_ServerURL + "/" + mStaticThings.apiversion + "/getsingleroomserver?roomid=" + channel.RootRoomID + "&apikey=" + mStaticThings.apikey + "&apitoken=" + mStaticThings.apitoken + "&userid=" + mStaticThings.I.mAvatarID;
                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                {
                    request.Dispose();
                    BaseMono.StartCoroutine(GetServer()); 
                }
                else
                {
                    channel.RoomURL = ServerUrl(request.downloadHandler.text);
                    if (!string.IsNullOrEmpty(channel.RoomURL))
                    {
                        BaseMono.StartCoroutine(GetAvatarNum(channel.RoomURL));
                    }
                }
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

        #region 人数获取

        /// <summary>
        /// 获取人数
        /// </summary>
        private IEnumerator GetAvatarNum(string roomURL)
        {
            if (!string.IsNullOrEmpty(roomURL))
            {
                var url = roomURL + $"&apikey={mStaticThings.apikey}&apitoken={mStaticThings.apitoken}";
                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                {
                    request.Dispose();
                    //BaseMono.StartCoroutine(GetAvatarNum(roomURL));
                }
                else
                {
                    int playerNum = GetCountFromJson(request.downloadHandler.text);
                    if (playerNum >= channel.AreaMaxCount)
                    {
                        isOpen = false;
                        icon.SetActive(true);
                    }
                    else 
                    {
                        isOpen = true;
                        icon.SetActive(false);
                        text.text = channel.Name;
                        numText.text = playerNum + "/" + channel.AreaMaxCount;
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
        #endregion

        #region 频道跳转
        private void Click() 
        {
            if (isOpen) 
            {
                SaveInfo.instance.CollectPlatform(12);
                ChangeRoom(channel.RootRoomID, channel.RootVoiceID);
            }
        }
        /// <summary>传送</summary>
        private void ChangeRoom(string RootRoomID, string RootVoiceID)
        {
            if (string.IsNullOrEmpty(RootRoomID) || string.IsNullOrEmpty(RootVoiceID))
                return;
            VRRootChanelRoom ch = new VRRootChanelRoom
            {
                roomid = RootRoomID,
                voiceid = RootVoiceID,
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.ConnectToNewChanel.ToString(), ch, 0);
        }
        #endregion
    }
}
