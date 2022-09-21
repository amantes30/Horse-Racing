using com.ootii.Messages;
using Dll_Project.Showroom;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project
{
    public class BigScreenCtrl : DllGenerateBase
    {
        private Transform bigScreenPanel;
        private RawImage BigScreenRI;

        private Button CloseButton;

        private Toggle bigScreenToggle;

        private Transform bgTemp;
        private Transform bgMask;

        public override void Init()
        {
            bigScreenPanel = BaseMono.ExtralDatas[0].Target;
            BigScreenRI = BaseMono.ExtralDatas[1].Target.GetComponent<RawImage>();
            CloseButton = BaseMono.ExtralDatas[2].Target.GetComponent<Button>();
            bigScreenToggle = BaseMono.ExtralDatas[3].Target.GetComponent<Toggle>();
            bgTemp = bigScreenPanel.Find("ShowBigScreenTemp");
            bgMask = bigScreenPanel.Find("ShowBigScreenTemp/BGMask");
        }
        public override void Awake()
        {
            base.Awake();
            System.GC.Collect(); 
        }
        public override void Start()
        {
            CloseButton.onClick.AddListener(() =>
            {
                bigScreenToggle.isOn = false;
            });

            bigScreenToggle.onValueChanged.AddListener(ToggleClick);

            if (mStaticThings.I != null) 
            {
                if (mStaticThings.I.ismobile)
                {
                    bgTemp.GetComponent<RectTransform>().sizeDelta = new Vector2(798, 448);
                    bgMask.GetComponent<RectTransform>().offsetMin = new Vector2(30, 28);
                    bgMask.GetComponent<RectTransform>().offsetMax = new Vector2(-29, -28);
                    //CloseButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(21, -21, 0);
                    //CloseButton.GetComponent<RectTransform>().sizeDelta = new Vector2(42, 42);
                }
                else 
                {
                    bgTemp.GetComponent<RectTransform>().sizeDelta = new Vector2(1010, 555);
                    bgMask.GetComponent<RectTransform>().offsetMin = new Vector2(30, 26);
                    bgMask.GetComponent<RectTransform>().offsetMax = new Vector2(-29, -29);
                    //CloseButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(28, -28, 0);
                    //CloseButton.GetComponent<RectTransform>().sizeDelta = new Vector2(56, 56);
                }
            }
        }

        private void ToggleClick(bool isOn) 
        {
            if (isOn)
            {
                bigScreenPanel.gameObject.SetActive(isOn);
                bigScreenPanel.parent.GetComponent<Canvas>().sortingOrder = 11;
                mStaticData.IsOpenPointClick = false;
                SaveInfo.instance.SaveActionData("OpenBigScreen", 10);
            }
            else 
            {
                bigScreenPanel.gameObject.SetActive(isOn);
                bigScreenPanel.parent.GetComponent<Canvas>().sortingOrder = 1;
                mStaticData.IsOpenPointClick = true;
                BigScreenRI.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                BigScreenRI.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                BigScreenRI.GetComponent<RectTransform>().localScale = Vector3.one;
                
            }

        }

        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.VRGetCacheProgress.ToString(), RecieveProgress);
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.VRGetCacheProgress.ToString(), RecieveProgress);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Update()
        {
            if (mStaticThings.I != null && bigScreenPanel.gameObject.activeSelf == true)
            {
                if (mStaticThings.I.BigscreenRoot != null)
                {
                    for (int i = 0; i < mStaticThings.I.BigscreenRoot.GetComponentsInChildren<RawImage>().Length; i++)
                    {
                        if (mStaticThings.I.BigscreenRoot.GetComponentsInChildren<RawImage>()[i].gameObject.activeSelf != false)
                        {
                            BigScreenRI.texture = mStaticThings.I.BigscreenRoot.GetComponentsInChildren<RawImage>()[i].texture;
                        }
                    }
                    //if (mStaticThings.I.BigscreenRoot.Find("ScreenRoot/Canvas_Picture/Canvas_PIC/Panel/RawImage/whiteboard") != null)
                    //{
                    //    if (mStaticThings.I.BigscreenRoot.Find("ScreenRoot/Canvas_Picture/Canvas_PIC/Panel/RawImage/whiteboard").gameObject.activeSelf)
                    //    {
                    //        BigScreenRI.GetComponent<RectTransform>().localScale = mStaticThings.I.BigscreenRoot.Find("ScreenRoot/Canvas_Picture/Canvas_PIC/Panel/RawImage/whiteboard").GetComponent<RectTransform>().localScale;
                    //    }
                    //    else
                    //    {
                    //        BigScreenRI.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    //    }
                    //}
                    
                }
            }
        }

        //进度展示
        void RecieveProgress(IMessage msg)
        {
            VRProgressInfo vrpinfo = msg.Data as VRProgressInfo;
            if (vrpinfo.progress!=1) 
            {
                MDebug("正在下载:" + vrpinfo.name + " 进度：" + (vrpinfo.progress * 100).ToString("F2") + "%");
            }
        }
        public void MDebug(string msg, int level = 0)
        {
            if (level == 0)
            {
                if (mStaticThings.I == null)
                {
                    return;
                }
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = msg,
                    b = InfoColor.black.ToString(),
                    c = "1",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
            }
        }
    }
}
