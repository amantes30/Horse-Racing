using com.ootii.Messages;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.Showroom.ChangeCard
{
    public class SendCard : DllGenerateBase
    {
        public string Checkurl/* = "http://121.37.129.57/middle/api/get/usercard"*/;

        private Transform sendCardPanel;
        private Text nameText;
        private Text identityText;
        private Text companyText;
        private Button sendButton;
        private Button cancelButton;

        private string toMavtorId;
        public override void Init()
        {
            sendCardPanel = BaseMono.ExtralDatas[0].Target;
            nameText = BaseMono.ExtralDatas[1].Target.GetComponent<Text>();
            identityText = BaseMono.ExtralDatas[2].Target.GetComponent<Text>();
            companyText = BaseMono.ExtralDatas[3].Target.GetComponent<Text>();
            sendButton = BaseMono.ExtralDatas[4].Target.GetComponent<Button>();
            cancelButton = BaseMono.ExtralDatas[5].Target.GetComponent<Button>();
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            sendButton.onClick.AddListener(SendCardClick);
            cancelButton.onClick.AddListener(CancelCardClick);
            
            
        }

        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
        }

        public override void Update()
        {
            if (mStaticThings.I!=null) 
            {
                if (!mStaticThings.I.isVRApp) 
                {
                    if (mStaticThings.I.ismobile)
                    {
                        if (Input.GetMouseButtonDown(0) && sendCardPanel.gameObject.activeSelf == false && mStaticData.IsOpenPointClick == true && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                        {
                            Ray ray = mStaticThings.I.Maincamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out RaycastHit hitInfo, 3))
                            {
                                GameObject go = hitInfo.transform.gameObject;
                                if (go.GetComponentInParent<LookAtNearController>())
                                {
                                    if (mStaticThings.AllStaticAvatarsDic.ContainsKey(go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name))
                                    {
                                        toMavtorId = go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name;
                                        BaseMono.StartCoroutine(GetPointCard(toMavtorId));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0) && sendCardPanel.gameObject.activeSelf == false && mStaticData.IsOpenPointClick == true && !EventSystem.current.IsPointerOverGameObject())
                        {
                            Ray ray = mStaticThings.I.Maincamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out RaycastHit hitInfo, 3))
                            {
                                GameObject go = hitInfo.transform.gameObject;
                                if (go.GetComponentInParent<LookAtNearController>())
                                {
                                    if (mStaticThings.AllStaticAvatarsDic.ContainsKey(go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name))
                                    {
                                        toMavtorId = go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name;
                                        BaseMono.StartCoroutine(GetPointCard(toMavtorId));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        private IEnumerator GetPointCard(string mAvatarID)
        {
            yield return new WaitForSeconds(0.1f);
            if (mStaticData.IsOpenPointClick == true)
            {
                if (string.IsNullOrEmpty(Plaza.mStaticData.AllURL.ExchangeCard))
                    yield break;
                Checkurl = Plaza.mStaticData.AllURL.ExchangeCard + "/api/get/usercard";
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("vs_id", mAvatarID);
                wwwForm.AddField("room_id",mStaticThings.I.nowRoomID);
                UnityWebRequest uwr = UnityWebRequest.Post(Checkurl, wwwForm);
                uwr.SetRequestHeader("Authorization", mStaticThings.apitoken);
                yield return uwr.SendWebRequest();
                JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);

                if (jd["code"].ToString() == "200")
                {
                    if (jd["data"].ToString() != null)
                    {
                        if (!string.IsNullOrEmpty(jd["data"]["company"].ToString()))
                            companyText.text = jd["data"]["company"].ToString();
                        if (!string.IsNullOrEmpty(jd["data"]["name"].ToString()))
                            nameText.text = jd["data"]["name"].ToString();
                    }
                    for (int i = 0; i < mStaticData.CompanyAsset.IdentityInfo.Count; i++)
                    {
                        if (mStaticData.CompanyAsset.IdentityInfo[i].mAvatorID == mAvatarID)
                        {
                            identityText.text = mStaticData.CompanyAsset.IdentityInfo[i].Sign;
                        }
                        else
                        {
                            identityText.text = null;
                        }
                    }
                    if (mStaticThings.I.isVRApp) 
                    {
                        sendCardPanel.transform.parent.gameObject.SetActive(true);
                    }
                    sendCardPanel.gameObject.SetActive(true);
                    sendCardPanel.transform.parent.GetComponent<Canvas>().sortingOrder = 11;
                }
                else
                {
                }
            }
        }

        void OnPointClickEvent(IMessage msg) 
        {
            GameObject go = (GameObject)msg.Data;
            if (mStaticThings.I.isVRApp) 
            {
                if (go.GetComponentInParent<LookAtNearController>())
                {
                    if (mStaticThings.AllStaticAvatarsDic.ContainsKey(go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name))
                    {
                        toMavtorId = go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name;
                        BaseMono.StartCoroutine(GetPointCard(toMavtorId));
                    }
                }
            }
        }
        private void SendCardClick() 
        {
            sendCardPanel.gameObject.SetActive(false);
            sendCardPanel.transform.parent.GetComponent<Canvas>().sortingOrder = 1;
            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = mStaticThings.I.nowRoomStartChID + "GetCardInfo",
                b = toMavtorId,
                c = mStaticThings.I.mAvatarID,
                d = mStaticData.AvatorData.name,
                e = mStaticData.AvatorData.company_name
            };
            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
        }
        private void CancelCardClick() 
        {
            sendCardPanel.gameObject.SetActive(false);
            sendCardPanel.transform.parent.GetComponent<Canvas>().sortingOrder = 1;
        }
    }
}
