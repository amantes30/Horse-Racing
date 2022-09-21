using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

namespace Dll_Project.Showroom
{
    public class UIControl : DllGenerateBase
    {
        public static UIControl Instance;
        private Transform cameraTemp;
        public GameObject UICanvas;
        private GameObject UITemp;

        private ExtralData[] ToggleList;
        private ExtralData[] PanelList;
        public override void Init()
        {
            cameraTemp = BaseMono.ExtralDatas[0].Target;
            UICanvas = BaseMono.ExtralDatas[1].Target.gameObject;
            ToggleList = BaseMono.ExtralDatas[2].Info;
            PanelList = BaseMono.ExtralDatas[3].Info;
        }
        public override void Awake()
        {
            Instance = this;
        }

        public override void Start()
        {
            UITemp = GameObject.Find("LoginUICanvas");
            
        }
        public static bool commandson = false;
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), PointClick);

            //分别展示手机端和PC端UI大小
            if (mStaticThings.I != null)
            {
                if (!mStaticThings.I.isVRApp)
                {
                    if (mStaticThings.I.ismobile)
                    {
                        UICanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1246, 712);
                        UICanvas.GetComponent<CanvasScaler>().screenMatchMode = ScreenMatchMode.Shrink;
                    }
                    else
                    {
                        UICanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1400, 800);
                        UICanvas.GetComponent<CanvasScaler>().screenMatchMode = ScreenMatchMode.Expand;
                    }
                }
                else 
                {
                    UICanvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                    UICanvas.GetComponent<Canvas>().worldCamera = mStaticThings.I.Maincamera.GetComponent<Camera>();
                    UICanvas.transform.localScale = new Vector3(0.0009f, 0.0009f, 0.0009f);
                    UICanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1246, 712);
                    for (int i = 0; i < ToggleList.Length; i++)
                    {
                        ToggleList[i].Target.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    }
                    for (int i = 0; i < PanelList.Length; i++)
                    {
                        PanelList[i].Target.GetComponent<Image>().enabled = false;
                    }
                }
            }
            
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), PointClick);
        }

        public override void Update()
        {
            if (mStaticThings.I != null)
            {
                cameraTemp.position = mStaticThings.I.Maincamera.position;
                cameraTemp.rotation = mStaticThings.I.Maincamera.rotation;
            }
            LimitCamera();

            if (Input.GetKeyUp(KeyCode.F12))
            {
                commandson = !commandson;
            }
            if (!commandson)
            {
                ShowOrHideUI();
                return;
            }

            if (Input.GetKeyUp(KeyCode.C))
            {
                UICanvas.GetComponent<Canvas>().enabled = !UICanvas.GetComponent<Canvas>().enabled;
            }
        }
        private void PointClick(IMessage msg)
        {
            GameObject info = msg.Data as GameObject;
            if (info.name.Equals("Spirit")) 
            {
                if (mStaticThings.I.isVRApp) 
                {
                    UICanvas.gameObject.SetActive(true);
                    IsVR(info.transform.parent.Find("GameObject"));
                }
            }
        }

        //VR端UI切换成全局UI
        public void IsVR(Transform tf)
        {
            UICanvas.transform.SetParent(tf);
            UICanvas.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            UICanvas.GetComponent<RectTransform>().localRotation = Quaternion.Euler(15,180,0);
        }
        //控制场景包中UI的显隐
        private void ShowOrHideUI()
        {
            if (!mStaticThings.I.isVRApp)
            {
                if (UITemp != null && UITemp.GetComponent<Canvas>().enabled == true)
                {
                    if (UITemp.transform.Find("UnderMenuPanel").gameObject.activeSelf == false && UICanvas.GetComponent<Canvas>().enabled == false && cameraTemp.childCount == 0)
                    {
                        UICanvas.GetComponent<Canvas>().enabled = true;
                    }
                    else if (UITemp.transform.Find("UnderMenuPanel").gameObject.activeSelf == true && UICanvas.GetComponent<Canvas>().enabled == true)
                    {
                        UICanvas.GetComponent<Canvas>().enabled = false;
                    }
                }
            }
            //else 
            //{
            //    if (UITemp.activeSelf || UITemp.transform.Find("UnderMenuPanel").gameObject.activeSelf)
            //    {
            //        UICanvas.SetActive(false);
            //    }
            //    else if (UITemp.activeSelf && UITemp.transform.Find("UnderMenuPanel").gameObject.activeSelf)
            //    {
            //        UICanvas.SetActive(false);
            //    }
            //    else
            //    {
            //        UICanvas.SetActive(true);
            //    }
            //}

            if (UICanvas.GetComponent<Canvas>().enabled == false) 
            {
                for (int i = 0; i < UICanvas.transform.Find("ToggleGroud").childCount; i++)
                {
                    if (UICanvas.transform.Find("ToggleGroud").GetChild(i).Find("Toggle").GetComponent<Toggle>().isOn == true)
                    {
                        UICanvas.transform.Find("ToggleGroud").GetChild(i).Find("Toggle").GetComponent<Toggle>().isOn = false;
                    }
                }
            }
        }
        /// <summary>
        /// 限制摄像机翻转视角
        /// </summary>
        private void LimitCamera() 
        {
            if (mStaticThings.I != null)
            {
                if (!mStaticThings.I.isVRApp) 
                {
                    if (mStaticThings.I.Maincamera.localRotation.x < -0.5373)
                    {
                        mStaticThings.I.Maincamera.localEulerAngles = new Vector3(-65, 0, 0);
                    }
                    else if (mStaticThings.I.Maincamera.localRotation.x > 0.5373)
                    {
                        mStaticThings.I.Maincamera.localEulerAngles = new Vector3(65, 0, 0);
                    }
                }
            }
        }
    }
}
