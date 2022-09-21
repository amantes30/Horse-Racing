using com.ootii.Messages;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Showroom.ChangeCard
{
    public class SaveGetCardInfo : DllGenerateBase
    {
        private Transform messagePanel;
        private Toggle messageToggle;
        private Transform contentParent;
        private GameObject infoParfe;
        private Button closeBtn;

        private GameObject messageImg;//显示是否有消息
        public override void Init()
        {
            messagePanel = BaseMono.ExtralDatas[0].Target;
            messageToggle = BaseMono.ExtralDatas[1].Target.GetComponent<Toggle>();
            contentParent = BaseMono.ExtralDatas[2].Target;
            infoParfe = BaseMono.ExtralDatas[3].Target.gameObject;
            closeBtn = BaseMono.ExtralDatas[4].Target.GetComponent<Button>();

            messageImg = BaseMono.ExtralDatas[5].Target.gameObject;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            messageToggle.onValueChanged.AddListener(ToggleClick);
            closeBtn.onClick.AddListener(()=> 
            {
                messageToggle.isOn = false;
                for (int i = 1; i < contentParent.childCount; i++)
                {
                    GameObject.Destroy(contentParent.GetChild(i).gameObject);
                }
            });
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Update()
        {
            if (saveDic.Count != 0)
            {
                messageImg.SetActive(true);
            }
            else if(messageImg.activeSelf)
            {
                messageImg.SetActive(false);
            }
        }
        #endregion
        /// <summary>
        /// 控制消息面板显示隐藏
        /// </summary>
        /// <param name="isOn"></param>
        private void ToggleClick(bool isOn)
        {
            if (isOn)
            {
                messagePanel.gameObject.SetActive(isOn);
                messagePanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 10;
                mStaticData.IsOpenPointClick = false;
                ShowPersonInfoClick();
                SaveInfo.instance.SaveActionData("Message", 10);
            }
            else 
            {
                messagePanel.gameObject.SetActive(isOn);
                messagePanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 1;
                mStaticData.IsOpenPointClick = true;
            }
        }

        #region 保存想要获取本人名片的人信息

        public static Dictionary<string, string> saveDic = new Dictionary<string, string>();
        /// <summary>
        /// 接受所有人员发送消息
        /// </summary>
        /// <param name="msg"></param>
        private void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (info.a == mStaticThings.I.nowRoomStartChID + "GetCardInfo")
            {
                if (info.b == mStaticThings.I.mAvatarID)
                {
                    if (saveDic.Count != 0)
                    {
                        if (!saveDic.ContainsKey(info.c))
                        {
                            saveDic.Add(info.c, info.d + "  " + info.e);
                        }
                    }
                    else
                    {
                        saveDic.Add(info.c, info.d + "  " + info.e);
                    }
                    if (messagePanel.gameObject.activeSelf) 
                    {
                        ShowPersonInfoClick();
                    }
                }
            }
        }
        #endregion

        #region 展示想要获取本人名片信息
        private void ShowPersonInfoClick()
        {
            for (int i = 1; i < contentParent.childCount; i++)
            {
                GameObject.Destroy(contentParent.GetChild(i).gameObject);
            }
            messagePanel.gameObject.SetActive(true);
            contentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, saveDic.Count * 70);
            var iter = saveDic.GetEnumerator();
            while (iter.MoveNext())
            {
                var go = GameObject.Instantiate(infoParfe, contentParent);
                go.SetActive(true);
                go.transform.GetComponent<RectTransform>().localScale = Vector3.one;
                go.name = iter.Current.Key;
                go.transform.Find("NameText").GetComponent<Text>().text = iter.Current.Value;
            }
            iter.Dispose();
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
