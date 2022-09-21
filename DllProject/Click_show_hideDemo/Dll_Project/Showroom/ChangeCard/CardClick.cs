using com.ootii.Messages;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Showroom.ChangeCard
{
    public class CardClick : DllGenerateBase
    {
        public string url /*= "http://121.37.129.57/middle/message/change"*/;

        private Button cancelBtn;
        private Button sureBtn;
        private Transform contentParent;
        private GameObject MessagePanel;
        public override void Init()
        {
            cancelBtn = BaseMono.ExtralDatas[0].Target.GetComponent<Button>();
            sureBtn = BaseMono.ExtralDatas[1].Target.GetComponent<Button>();
            MessagePanel = BaseMono.ExtralDatas[2].Target.gameObject;
            contentParent = BaseMono.transform.parent;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            cancelBtn.onClick.AddListener(CancelClick);
            sureBtn.onClick.AddListener(AddCardClick);
        }
        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }
        #endregion

        public override void Update()
        {
            if (MessagePanel.activeSelf) 
            {
                for (int i = 0; i < contentParent.childCount; i++)
                {
                    if (i == contentParent.childCount - 1)
                    {
                        contentParent.GetChild(i).Find("LineText").gameObject.SetActive(false);
                    }
                    else
                    {
                        contentParent.GetChild(i).Find("LineText").gameObject.SetActive(true);
                    }
                }
            }
            
        }

        /// <summary>
        /// 拒绝添加名片
        /// </summary>
        private void CancelClick()
        {
            if (SaveGetCardInfo.saveDic.ContainsKey(BaseMono.name))
            {
                SaveGetCardInfo.saveDic.Remove(BaseMono.name);
            }
            GameObject.DestroyImmediate(BaseMono.gameObject);
            contentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, SaveGetCardInfo.saveDic.Count * 70);
        }
        private void AddCardClick()
        {
            self_info self_Info = new self_info();
            self_Info.name = mStaticData.AvatorData.name;
            self_Info.company = mStaticData.AvatorData.company_name;
            self_Info.contact = mStaticData.AvatorData.contact_info;
            self_Info.job = mStaticData.AvatorData.position;
            self_Info.exhibition_id = "";

            BaseMono.StartCoroutine(GetOtherPlayerInfo(self_Info, BaseMono.gameObject.name));
        }
        
        private string GetCardURL/* ="http://121.37.129.57/middle/api/get/usercard"*/;//获取我的面板信息
        private IEnumerator GetOtherPlayerInfo(self_info si, string toID) //首次获取我的面板信息
        {
            if (string.IsNullOrEmpty(Plaza.mStaticData.AllURL.MyInfoURL))
            {
                yield break;
            }
            GetCardURL = Plaza.mStaticData.AllURL.MyInfoURL + "/api/get/usercard";
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("vs_id", toID);
            wwwForm.AddField("room_id", /*"nlllgla"*/mStaticThings.I.nowRoomID);
            UnityWebRequest uwr = UnityWebRequest.Post(GetCardURL, wwwForm);
            uwr.SetRequestHeader("Authorization", mStaticThings.apitoken);
            yield return uwr.SendWebRequest();
            JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);
            if (jd["code"].ToString() == "200")
            {
                card_info card_Info = new card_info();
                card_Info.name = jd["data"]["name"].ToString();
                card_Info.company = jd["data"]["company"].ToString();
                card_Info.contact = jd["data"]["phone"].ToString();
                card_Info.job = jd["data"]["job"].ToString();
                card_Info.exhibition_id = "";

                BaseMono.StartCoroutine(SendCardInfo(si, card_Info));
            }
        }

        private IEnumerator SendCardInfo(self_info si, card_info ci)
        {
            if (string.IsNullOrEmpty(Plaza.mStaticData.AllURL.ExchangeCard)) 
            {
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = "URL获取失败！",
                    b = InfoColor.green.ToString(),
                    c = "2",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
                yield break;
            }
            url = Plaza.mStaticData.AllURL.ExchangeCard + "/message/change";
            WWWForm wwwForm = new WWWForm();
            wwwForm.AddField("card_info", JsonMapper.ToJson(ci));
            wwwForm.AddField("self_info", JsonMapper.ToJson(si));

            UnityWebRequest uwr = UnityWebRequest.Post(url, wwwForm);
            uwr.SetRequestHeader("Authorization", mStaticThings.apitoken);
            yield return uwr.SendWebRequest();

            JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);
            if (int.Parse(jd["code"].ToString()) == 200)
            {
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = "信息发送成功！",
                    b = InfoColor.green.ToString(),
                    c = "2",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
                CancelClick();
            }
            else if (int.Parse(jd["code"].ToString()) == 41001)
            {
                //text.text = "联系方式不合法";
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = "对方联系方式不合法",
                    b = InfoColor.green.ToString(),
                    c = "2",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
            }
            else if (int.Parse(jd["code"].ToString()) == 41002)
            {
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = "联系方式不合法",
                    b = InfoColor.green.ToString(),
                    c = "2",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
            }
            else
            {
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = jd["message"].ToString(),
                    b = InfoColor.green.ToString(),
                    c = "2",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
            }
        }
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
    }
}
