using com.ootii.Messages;
using DG.Tweening;
using Dll_Project.Plaza.AvatarAllot;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Showroom
{
    public class AllIdentity
    {
        public string id;
        public string name;
        public string level;
    }
    class MyInformationPanelControl : DllGenerateBase
    {
        private GameObject infoPanel;
        private InputField enterpriseInput;
        private InputField nameInput;
        private InputField postInput;
        private InputField contactInput;
        private Button sureButton;

        private Text titleText;
        private Text NamePlateText;

        private GameObject TeleportScene;

        private Toggle myToggle;

        private Dropdown dropdown;
        private Button CloseBtn;

        private GameObject firstHelpPanel;
        private Transform imgParent;
        private Transform buttonList;
        private Text showPage;
        private Button tiaoBtn;
        private Button nextBtn;
        private Button knowBtn;
        public override void Init()
        {
            infoPanel = BaseMono.ExtralDatas[0].Target.gameObject;
            enterpriseInput = BaseMono.ExtralDatas[1].Target.GetComponent<InputField>();
            nameInput = BaseMono.ExtralDatas[2].Target.GetComponent<InputField>();
            postInput = BaseMono.ExtralDatas[3].Target.GetComponent<InputField>();
            contactInput = BaseMono.ExtralDatas[4].Target.GetComponent<InputField>();
            sureButton = BaseMono.ExtralDatas[5].Target.GetComponent<Button>();
            titleText = BaseMono.ExtralDatas[6].Target.GetComponent<Text>();
            NamePlateText = BaseMono.ExtralDatas[7].Target.GetComponent<Text>();

            TeleportScene = BaseMono.ExtralDatas[8].Target.gameObject;
            myToggle = BaseMono.ExtralDatas[9].Target.GetComponent<Toggle>();

            dropdown = BaseMono.ExtralDatas[10].Target.GetComponent<Dropdown>();
            CloseBtn = BaseMono.ExtralDatas[11].Target.GetComponent<Button>();

            //首次展示指南UI
            firstHelpPanel = BaseMono.ExtralDatas[12].Target.gameObject;
            imgParent = BaseMono.ExtralDatas[12].Info[0].Target;
            buttonList = BaseMono.ExtralDatas[12].Info[1].Target;
            showPage = BaseMono.ExtralDatas[12].Info[2].Target.GetComponent<Text>();
            tiaoBtn = BaseMono.ExtralDatas[12].Info[3].Target.GetComponent<Button>();
            nextBtn = BaseMono.ExtralDatas[12].Info[4].Target.GetComponent<Button>();
            knowBtn = BaseMono.ExtralDatas[12].Info[5].Target.GetComponent<Button>();
        }
        #region 初始设置
        public override void Awake()
        {
        }

        public override void Start()
        {
            if (BaseMono.ExtralDatas[0].OtherData == "floor1")
            {
                //面板数据获取
                BaseMono.StartCoroutine(GetURL());
            }
            else
            {
                PlayerPrefs.SetString(mStaticThings.I.nowRoomChID+"SceneData", DateTime.Now + "," + mStaticThings.I.nowRoomID + "," + mStaticThings.I.nowRoomChID + "," + mStaticThings.I.nowSceneLoadName);
            }
            sureButton.onClick.AddListener(SaveInfoClick);

            TeleportScene.transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(CancelClick);
            TeleportScene.transform.Find("SureButton").GetComponent<Button>().onClick.AddListener(SureClick);

            myToggle.onValueChanged.AddListener(ToggleClick);

            enterpriseTrigger();
            nameTrigger();
            postTrigger();
            contactTrigger();


            nameInput.onValueChanged.AddListener(nameInputFieldChange);
            enterpriseInput.onValueChanged.AddListener(companyInputFieldChange);
            CloseBtn.onClick.AddListener(() => { BaseMono.StartCoroutine(CloseClick()); });

            //首次展示指南UI
            if (mStaticThings.I.ismobile)
            {
                GameObject.Destroy(imgParent.Find("PCPanel").gameObject);
            }
            else 
            {
                GameObject.Destroy(imgParent.Find("AndPanel").gameObject);
            }
            showPage.text = "1" + "/" + (imgParent.childCount-1);
            tiaoBtn.onClick.AddListener(TiaoClick);
            knowBtn.onClick.AddListener(TiaoClick);
            nextBtn.onClick.AddListener(NextClick);
        }
        public override void OnEnable()
        {
            
        }

        public override void OnDisable()
        {
            
        }
        
        float time = 3;
        public override void Update()
        {
            if (titleText.gameObject.activeSelf)
            {
                time -= Time.deltaTime;
                if (time < 0)
                {
                    titleText.gameObject.SetActive(false);
                    time = 3;
                }
            }

        }
        #endregion
        private IEnumerator GetURL()
        {
            yield return new WaitForSeconds(0.1f);
            if (string.IsNullOrEmpty(Plaza.mStaticData.AllURL.MyInfoURL))
            {
                BaseMono.StartCoroutine(GetURL());
            }
            else
            {
                URL = Plaza.mStaticData.AllURL.MyInfoURL;
                BaseMono.StartCoroutine(VerifyToken());
            }
        }

        #region Token验证 我的信息获取、保存，身份列表获取
        //private static string URL = "http://192.168.2.106:7011";
        private string URL /*= "http://121.37.129.57/middle"*/;
        private string GetTokenURL;//身份验证
        private string GetCardURL;//获取我的面板信息
        private string SaveCardURL;//保存我的面板信息
        private string GetIdentityURL;//获取身份列表

        private IEnumerator VerifyToken() 
        {
            GetTokenURL = URL + "/api/check";
            GetTokenURL = GetTokenURL + "?apikey=" + mStaticThings.apikey + "&apitoken=" + mStaticThings.apitoken;
            //GetTokenURL = GetTokenURL + "?apikey=" + "cfb974cf6da8ab50d956885ad4d8e0a0" + "&apitoken=" + "pyvePyUo8m83yVXp";
            UnityWebRequest uwr = UnityWebRequest.Get(GetTokenURL);
            yield return uwr.SendWebRequest();
            JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);
            if (jd["code"].ToString() == "200")
            {
                BaseMono.StartCoroutine(GetIdentityList());
                BaseMono.StartCoroutine(GetMyInfoFirst());
            }
            else if (jd["code"].ToString() == "401")
            {
                MDebug("身份认证失败", 0);
            }
        }

        private IEnumerator GetMyInfoFirst() //首次获取我的面板信息
        {
            GetCardURL = URL + "/api/get/usercard";
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("vs_id", mStaticThings.I.mAvatarID);
            wwwForm.AddField("room_id", /*"nlllgla"*/mStaticThings.I.nowRoomID);
            UnityWebRequest uwr = UnityWebRequest.Post(GetCardURL, wwwForm);
            uwr.SetRequestHeader("Authorization", mStaticThings.apitoken);
            yield return uwr.SendWebRequest();
            JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);
            if (jd["code"].ToString() == "200")
            {
                if (jd["data"].ToString() != null)
                {
                    mStaticData.uuid = jd["data"]["uuid"].ToString();
                    if (!JsonData.Equals(jd["data"]["name"], null) && !JsonData.Equals(jd["data"]["company"], null) &&
                        !JsonData.Equals(jd["data"]["job"], null) && !JsonData.Equals(jd["data"]["phone"], null))
                    {
                        nameInput.text = jd["data"]["name"].ToString();
                        enterpriseInput.text = jd["data"]["company"].ToString();
                        postInput.text = jd["data"]["job"].ToString();
                        contactInput.text = jd["data"]["phone"].ToString();
                        if (!JsonData.Equals(jd["data"]["identity"], null))
                        {
                            if (identityList != null)
                            {
                                for (int i = 0; i < identityList.Count; i++)
                                {
                                    if (identityList[i].id == jd["data"]["identity"].ToString())
                                    {
                                        dropdown.value = i;
                                    }
                                }
                            }
                        }

                        mStaticData.AvatorData.company_name = enterpriseInput.text;
                        mStaticData.AvatorData.name = nameInput.text;
                        mStaticData.AvatorData.position = postInput.text;
                        mStaticData.AvatorData.contact_info = contactInput.text;
                        if (!JsonData.Equals(jd["data"]["identity"], null))
                        {
                            mStaticData.AvatorData.identity = jd["data"]["identity"].ToString();
                        }
                        //mStaticData.uuid = jd["data"]["uuid"].ToString();

                        mStaticThings.I.mNickName = nameInput.text + "\n" + enterpriseInput.text;
                        MessageDispatcher.SendMessage(WsMessageType.SendChangeAvatar.ToString());

                        TiaoZhuanScene();
                    }
                    else
                    {
                        firstHelpPanel.SetActive(true);
                        infoPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 11;
                    }
                }
                
            }
        }
        private IEnumerator GetMyInfo() //获取我的面板信息
        {
            GetCardURL = URL + "/api/get/usercard";
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("vs_id", mStaticThings.I.mAvatarID);
            wwwForm.AddField("room_id", /*"nlllgla"*/mStaticThings.I.nowRoomID);
            UnityWebRequest uwr = UnityWebRequest.Post(GetCardURL, wwwForm);
            uwr.SetRequestHeader("Authorization", mStaticThings.apitoken);
            yield return uwr.SendWebRequest();
            JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);
            if (jd["code"].ToString() == "200")
            {
                if (jd["data"].ToString() != null)
                {
                    if (!JsonData.Equals(jd["data"]["name"], null) && !JsonData.Equals(jd["data"]["company"], null) &&
                        !JsonData.Equals(jd["data"]["job"], null) && !JsonData.Equals(jd["data"]["phone"], null))
                    {
                        nameInput.text = jd["data"]["name"].ToString();
                        enterpriseInput.text = jd["data"]["company"].ToString();
                        postInput.text = jd["data"]["job"].ToString();
                        contactInput.text = jd["data"]["phone"].ToString();
                        if (identityList != null)
                        {
                            for (int i = 0; i < identityList.Count; i++)
                            {
                                if (!JsonData.Equals(jd["data"]["identity"], null)) 
                                {
                                    if (identityList[i].id == jd["data"]["identity"].ToString())
                                    {
                                        dropdown.value = i;
                                    }
                                }
                            }
                        }

                        mStaticData.AvatorData.company_name = enterpriseInput.text;
                        mStaticData.AvatorData.name = nameInput.text;
                        mStaticData.AvatorData.position = postInput.text;
                        mStaticData.AvatorData.contact_info = contactInput.text;
                        if (!JsonData.Equals(jd["data"]["identity"], null))
                        {
                            mStaticData.AvatorData.identity = jd["data"]["identity"].ToString();
                        }
                        mStaticData.uuid = jd["data"]["uuid"].ToString();

                        mStaticThings.I.mNickName = nameInput.text + "\n" + enterpriseInput.text;
                        MessageDispatcher.SendMessage(WsMessageType.SendChangeAvatar.ToString());

                        ShowPanelClick();
                    }
                    else
                    {
                        ShowPanelClick();
                        CloseBtn.gameObject.SetActive(false);
                    }
                }

            }
        }
        private IEnumerator SaveMyInfo() //修改保存我的面板信息
        {
            WWWForm wwwForm = new WWWForm();
            if (string.IsNullOrEmpty(enterpriseInput.text) || string.IsNullOrEmpty(nameInput.text) || string.IsNullOrEmpty(postInput.text) || string.IsNullOrEmpty(contactInput.text))
            {
                titleText.gameObject.SetActive(true);
                titleText.text = "数据不能为空！";

            }
            else
            {
                if (!IsEmail(contactInput.text) || !IsPhoton(contactInput.text))
                {
                    SaveCardURL = URL + "/api/update/usercard";

                    wwwForm.AddField("company", enterpriseInput.text);
                    wwwForm.AddField("name", nameInput.text);
                    wwwForm.AddField("identity", identityList[dropdown.value].id);
                    wwwForm.AddField("job", postInput.text);
                    wwwForm.AddField("phone", contactInput.text);
                    wwwForm.AddField("uuid", mStaticData.uuid);
                    UnityWebRequest uwr = UnityWebRequest.Post(SaveCardURL, wwwForm);
                    uwr.SetRequestHeader("Authorization", mStaticThings.apitoken);
                    yield return uwr.SendWebRequest();

                    JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);

                    if (int.Parse(jd["code"].ToString()) == 200)
                    {
                        infoPanel.SetActive(false);
                        mStaticData.AvatorData.company_name = enterpriseInput.text;
                        mStaticData.AvatorData.name = nameInput.text;
                        mStaticData.AvatorData.position = postInput.text;
                        mStaticData.AvatorData.contact_info = contactInput.text;
                        mStaticThings.I.mNickName = mStaticData.AvatorData.name + "\n" + mStaticData.AvatorData.company_name;
                        MessageDispatcher.SendMessage(WsMessageType.SendChangeAvatar.ToString());

                        infoPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 1;
                        myToggle.interactable = true;
                        myToggle.isOn = false;

                        if (!CloseBtn.gameObject.activeSelf)
                        {
                            CloseBtn.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        titleText.gameObject.SetActive(true);
                        titleText.text = "数据保存失败。";
                    }
                }
                else
                {
                    titleText.gameObject.SetActive(true);
                    titleText.text = "电话或邮箱格式不对";
                    yield return null;
                }
            }
        }

        List<AllIdentity> identityList = new List<AllIdentity>();
        private IEnumerator GetIdentityList() //获取身份列表
        {
            GetIdentityURL = URL + "/api/identity/list";
            GetIdentityURL = GetIdentityURL + "?room_id=" + mStaticThings.I.nowRoomID;
            UnityWebRequest uwr = UnityWebRequest.Get(GetIdentityURL);
            uwr.SetRequestHeader("Authorization", mStaticThings.apitoken);
            yield return uwr.SendWebRequest();
            JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);
            if (jd["code"].ToString() == "200") 
            {
                if (!string.IsNullOrEmpty(jd["data"].ToString())) 
                {
                    identityList.Clear();
                    for (int i = 0; i < jd["data"].Count; i++)
                    {
                        AllIdentity allIdentity = new AllIdentity();
                        allIdentity.id = jd["data"][i]["id"].ToString();
                        allIdentity.name = jd["data"][i]["name"].ToString();
                        allIdentity.level = jd["data"][i]["level"].ToString();
                        identityList.Add(allIdentity);
                    }
                    if (identityList != null) 
                    {
                        SetDropdown(identityList);
                    }
                }
            }
        }

        private void SetDropdown(List<AllIdentity> identityList) 
        {
            for (int i = 0; i < identityList.Count; i++)
            {
                List<Dropdown.OptionData> listOptions = new List<Dropdown.OptionData>();
                listOptions.Add(new Dropdown.OptionData(identityList[i].name));
                dropdown.AddOptions(listOptions);
            }
        }
        #endregion

        #region 第一次进来展示指南
        int index;
        private void TiaoClick() 
        {
            firstHelpPanel.SetActive(false);
            ShowPanelClick();
            CloseBtn.gameObject.SetActive(false);
        }

        private void NextClick() 
        {
            index++;
            showPage.text = (index + 1) + "/" + imgParent.childCount;
            for (int i = 0; i < imgParent.childCount; i++)
            {
                if (i == index)
                {
                    imgParent.GetChild(i).gameObject.SetActive(true);
                }
                else 
                {
                    imgParent.GetChild(i).gameObject.SetActive(false);
                }
            }
            switch (index)
            {
                case 0:
                    buttonList.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, -105, 0);
                    break;
                case 1:
                    if (mStaticThings.I.ismobile)
                    {
                        buttonList.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(265, 100, 0);
                    }
                    else 
                    {
                        buttonList.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-4.5f, -105, 0);
                    }
                    break;
                case 2:
                    buttonList.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(235, 0, 0);
                    break;
                case 3:
                    buttonList.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(235, 0, 0);
                    break;
                case 4:
                    buttonList.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(280, 0, 0);
                    break;
            }
            if (imgParent.childCount - index == 1) 
            {
                tiaoBtn.gameObject.SetActive(false);
                nextBtn.gameObject.SetActive(false);
                knowBtn.gameObject.SetActive(true);
            }
        }
        #endregion

        #region 进来时跳转到上次离开时的场景
        string[] tempArray;
        private void TiaoZhuanScene()
        {
            if (mStaticThings.I.LastIDLinkChanelRoomList.Count == 1)
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(mStaticThings.I.nowRoomChID+"SceneData")))
                {
                    string temp = PlayerPrefs.GetString(mStaticThings.I.nowRoomChID+"SceneData");
                    tempArray = temp.Split(',');
                    if (tempArray[3] != mStaticThings.I.nowSceneLoadName)
                    {
                        TimeSpan date = DateTime.Now - DateTime.Parse(tempArray[0]);
                        if (date.TotalMinutes < 10)
                        {
                            TeleportScene.transform.Find("ContentText").GetComponent<Text>().text = "上次退出场景为" +"<color=orange>"+ tempArray[3] + "</color>"+"\n" + "是否再次进入该场景？";
                            TeleportScene.SetActive(true);
                        }
                        else
                        {
                            PlayerPrefs.SetString(mStaticThings.I.nowRoomChID + "SceneData", "");
                            ShowPanelClick();
                        }
                    }
                }
                else 
                {
                    ShowPanelClick();
                }
            }
            else 
            {
                if (mStaticThings.I.LastIDLinkChanelRoomList.Count == 0) 
                {
                    ShowPanelClick();
                }
                PlayerPrefs.SetString(mStaticThings.I.nowRoomChID + "SceneData", "");
            }
        }
        private void SureClick()
        {
            if (tempArray != null)
            {
                TeleportScene.SetActive(false);
                BaseMono.StartCoroutine(ChangeRoom(tempArray[1], tempArray[2]));

                PlayerPrefs.SetString(mStaticThings.I.nowRoomChID + "SceneData", "");
                Resources.UnloadUnusedAssets();
            }
        }
        private void CancelClick()
        {
            TeleportScene.SetActive(false);
            TeleportScene.transform.Find("ContentText").GetComponent<Text>().text = null;
        }
        private IEnumerator ChangeRoom(string RootRoomID, string RootVoiceID)
        {
            yield return new WaitForSeconds(1);
            if (string.IsNullOrEmpty(RootRoomID) || string.IsNullOrEmpty(RootVoiceID))
                yield return null;
            VRRootChanelRoom ch = new VRRootChanelRoom
            {
                roomid = RootRoomID,
                voiceid = RootVoiceID,
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.ConnectToNewChanel.ToString(), ch, 0);
        }
        #endregion

        #region 展示我的信息面板
        int infoIndex = 0;
        private void ToggleClick(bool isOn)
        {
            if (isOn)
            {
                IsShowPanel();
                mStaticData.IsOpenPointClick = false;
                if (infoIndex == 1)
                {
                    SaveInfo.instance.SaveActionData("MyInfo", 10);
                }
                else if (infoIndex == 0)
                {
                    infoIndex = 1;
                }
            }
            else
            {
                infoPanel.SetActive(false);
                mStaticData.IsOpenPointClick = true;
            }
        }
        private IEnumerator CloseClick()
        {
            yield return new WaitForSeconds(0.1f);
            myToggle.isOn = false;
            infoPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 1;
        }
        private void IsShowPanel()
        {
            BaseMono.StartCoroutine(GetMyInfo());
        }

        private void ShowPanelClick()
        {
            if (mStaticThings.I.isVRApp) 
            {
                infoPanel.transform.parent.parent.gameObject.SetActive(true);
            }
            infoPanel.SetActive(true);
            myToggle.isOn = true;
            infoPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 10;
        }

        private void SaveInfoClick()
        {
            BaseMono.StartCoroutine(SaveMyInfo());
        }
        #endregion

        #region 判断输入的是电话还是邮箱
        public bool IsEmail(string str)//判断邮箱
        {
            string expression = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex regex = new Regex(expression);
            if (regex.IsMatch(str))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 判断字符串是否是电话
        /// </summary>
        public bool IsPhoton(string str)
        {
            //string expression = "^((13[0-9])|(15[^4,\\D])|(18[0,5-9]))\\d{8}$";
            string expression = "^(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\\d{8}$";
            Regex regex = new Regex(expression);
            if (regex.IsMatch(str))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region 支持于vr端射线点击InputField显示小键盘
        private void enterpriseTrigger()
        {
            EventTrigger eventTrigger = enterpriseInput.GetComponent<EventTrigger>();
            eventTrigger.triggers = new List<EventTrigger.Entry>();
            EventTrigger.Entry enterpriseEntry = new EventTrigger.Entry
            {
                //事件类型
                eventID = EventTriggerType.PointerClick,
                callback = new EventTrigger.TriggerEvent()
                //创建回调函数
            };
            enterpriseEntry.callback.AddListener((data) =>
            {

                InputFieldClick(enterpriseInput);
            });
            eventTrigger.triggers.Add(enterpriseEntry);
        }
        private void nameTrigger()
        {
            EventTrigger eventTrigger = nameInput.GetComponent<EventTrigger>();
            eventTrigger.triggers = new List<EventTrigger.Entry>();
            EventTrigger.Entry nameEntry = new EventTrigger.Entry
            {
                //事件类型
                eventID = EventTriggerType.PointerClick,
                callback = new EventTrigger.TriggerEvent()
                //创建回调函数
            };
            nameEntry.callback.AddListener((data) =>
            {

                InputFieldClick(nameInput);
            });
            eventTrigger.triggers.Add(nameEntry);
        }

        private void postTrigger()
        {
            EventTrigger eventTrigger = postInput.GetComponent<EventTrigger>();
            eventTrigger.triggers = new List<EventTrigger.Entry>();
            EventTrigger.Entry postEntry = new EventTrigger.Entry
            {
                //事件类型
                eventID = EventTriggerType.PointerClick,
                callback = new EventTrigger.TriggerEvent()
                //创建回调函数
            };
            postEntry.callback.AddListener((data) =>
            {

                InputFieldClick(postInput);
            });
            eventTrigger.triggers.Add(postEntry);
        }

        private void contactTrigger()
        {
            EventTrigger eventTrigger = contactInput.GetComponent<EventTrigger>();
            eventTrigger.triggers = new List<EventTrigger.Entry>();
            EventTrigger.Entry contactEntry = new EventTrigger.Entry
            {
                //事件类型
                eventID = EventTriggerType.PointerClick,
                callback = new EventTrigger.TriggerEvent()
                //创建回调函数
            };
            contactEntry.callback.AddListener((data) =>
            {

                InputFieldClick(contactInput);
            });
            eventTrigger.triggers.Add(contactEntry);
        }
        private void InputFieldClick(InputField fd)
        {
            MessageDispatcher.SendMessage(fd, VrDispMessageType.InputFildClicked.ToString(), fd.text, 0);
        }
        #endregion

        #region InputField组件输入与显示输入文字同步
        private void nameInputFieldChange(string name)
        {
            
            if (name.Length > 6)
            {
                nameInput.text = name.Remove(6);
                return;
            }
            NamePlateText.text = $"{name}\n{enterpriseInput.text}";
        }
        private void companyInputFieldChange(string name)
        {
            if (name.Length > 12)
            {
                enterpriseInput.text = name.Remove(12);
                return;
            }
            NamePlateText.text = $"{nameInput.text}\n{name}";
        }
        #endregion

        public void MDebug(string msg, int level = 0)
        {
            if (level == 0)
            {
                if (mStaticThings.I == null)
                {
                    Debug.Log(msg);
                    return;
                }
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = msg,
                    b = InfoColor.black.ToString(),
                    c = "3",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
            }
        }

    }

}
