using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace Dll_Project.Showroom.BulletScreen
{
    public class ShowBulletScreen : DllGenerateBase
    {
        private Transform bulletScreenPanel;
        private InputField sendInputField;
        private Button sendButton;
        private Transform recievePanel;

        private Toggle BulletScreenToggle;
        private Transform ToggleGroud;
        public override void Init()
        {
            bulletScreenPanel = BaseMono.ExtralDatas[0].Target;
            sendInputField = BaseMono.ExtralDatas[1].Target.GetComponent<InputField>();
            sendButton = BaseMono.ExtralDatas[2].Target.GetComponent<Button>();

            recievePanel = BaseMono.ExtralDatas[3].Target;

            BulletScreenToggle = BaseMono.ExtralDatas[4].Target.GetComponent<Toggle>();
            ToggleGroud = BaseMono.ExtralDatas[5].Target;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            sendButton.onClick.AddListener(SendBulletScreen);
            BulletScreenToggle.onValueChanged.AddListener(ToggleClick);
            sendInputField.onValueChanged.AddListener(sendInput);
        }

        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            //MessageDispatcher.AddListener(VrDispMessageType.SetAdmin.ToString(), OnAdminChangedevent);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            //MessageDispatcher.RemoveListener(VrDispMessageType.SetAdmin.ToString(), OnAdminChangedevent);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Update()
        {
            if ((mStaticThings.I.isAdmin || mStaticThings.I.sadmin))
            {
                if (!mStaticThings.I.isVRApp) 
                {
                    if (!BulletScreenToggle.transform.parent.gameObject.activeSelf)
                    {
                        BulletScreenToggle.transform.parent.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                if (BulletScreenToggle.transform.parent.gameObject.activeSelf)
                {
                    BulletScreenToggle.transform.parent.gameObject.SetActive(false);
                    BulletScreenToggle.isOn = false;
                }
            }
        }
        #endregion

        private void SendBulletScreen()
        {
            if (mStaticThings.I != null)
            {
                if (!string.IsNullOrEmpty(sendInputField.text))
                {
                    WsCChangeInfo wsinfo = new WsCChangeInfo()
                    {
                        a = mStaticThings.I.nowRoomStartChID + "SendBulletScreen",
                        b = mStaticData.AvatorData.name + ":"+sendInputField.text
                    };
                    MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
                }
                sendInputField.text = null;
            }
        }

        private void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (info.a == mStaticThings.I.nowRoomStartChID + "SendBulletScreen") 
            {
                var tempClone = GameObject.Instantiate(recievePanel, bulletScreenPanel.parent);
                tempClone.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(200, UnityEngine.Random.Range(-120,-200), 0);
                tempClone.GetComponent<GeneralDllBehavior>().OtherData = info.b;
                tempClone.gameObject.SetActive(true);
            }
        }
        private void OnPointClickEvent(IMessage msg)
        {
            GameObject go = msg.Data as GameObject;
            if (!EventSystem.current.IsPointerOverGameObject()) 
            {
                if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                {
                    if (bulletScreenPanel.gameObject.activeSelf)
                    {
                        if (BulletScreenToggle.isOn == true)
                            BulletScreenToggle.isOn = false;
                    }
                }
            }
        }
        private void ToggleClick(bool isOn) 
        {
            if (isOn)
            {
                var x = ToggleGroud.GetComponent<RectTransform>().rect.width;
                bulletScreenPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-x, -60);
                SaveInfo.instance.SaveActionData("BulletScreen", 10);
                bulletScreenPanel.gameObject.SetActive(isOn);
                mStaticData.IsOpenPointClick = false;
            }
            else 
            {
                bulletScreenPanel.gameObject.SetActive(isOn);
                mStaticData.IsOpenPointClick = true;
            }
            sendInputField.text = null; 
        }

        //控制弹幕文字字数
        private void sendInput(string info) 
        {
            if (info.Length > 30)
            {
                sendInputField.text = info.Remove(30);
                return;
            }
        }
    }
}
