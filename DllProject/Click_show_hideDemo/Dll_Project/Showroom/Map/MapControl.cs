using com.ootii.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.Showroom.Map
{
    public class MapControl : DllGenerateBase
    {
        public static MapControl instances;

        private GameObject mapPanel;
        private Toggle mapToggle;
        private Transform AvatarPoint;
        private Transform PointParent;

        private Transform contentParent;//父类
        private GameObject infoTogglePrabel;//预制体

        private Button closeBtn;
        private Transform buttonList;

        private Transform HintPanel;

        private Vector3 modelSize;
        private Transform mapRightPoint;//小地图真实位置基准点
        private Transform uimapRightPoint;//小地图UI位置基准点

        private Transform Content;//人员列表滑动条
        public override void Init()
        {
            mapPanel = BaseMono.ExtralDatas[0].Target.gameObject;
            mapRightPoint = BaseMono.ExtralDatas[0].Info[0].Target;
            uimapRightPoint = BaseMono.ExtralDatas[0].Info[1].Target;

            mapToggle = BaseMono.ExtralDatas[1].Target.GetComponent<Toggle>();
            AvatarPoint = BaseMono.ExtralDatas[2].Target;
            PointParent = BaseMono.ExtralDatas[3].Target;

            contentParent = BaseMono.ExtralDatas[4].Target;
            infoTogglePrabel = BaseMono.ExtralDatas[5].Target.gameObject;

            closeBtn = BaseMono.ExtralDatas[6].Target.GetComponent<Button>();
            buttonList = BaseMono.ExtralDatas[7].Target;

            HintPanel = BaseMono.ExtralDatas[8].Target;

            Content = BaseMono.ExtralDatas[9].Target;
        }
        #region 初始
        public override void Awake()
        {
            instances = this;
        }

        public override void Start()
        {
            modelSize = new Vector3(449.28607f, 0, 332.30561f);
            for (int i = 0; i < PointParent.childCount; i++)
            {
                int a = i;
                PointParent.GetChild(a).GetComponent<Button>().onClick.AddListener(() => 
                {
                    //MulticastMoveClick(PointParent.GetChild(a));
                    ShowUI(true, PointParent.GetChild(a));
                });
            }
            HintPanel.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => { ShowUI(false, null); });
            HintPanel.Find("SureButton").GetComponent<Button>().onClick.AddListener(SureClick);

            mapToggle.onValueChanged.AddListener(ToggleClick);
            closeBtn.onClick.AddListener(() => { mapToggle.isOn = false; });

            //BaseMono.StartCoroutine(ShowTeleportButton(1.1f));
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), PointClick);

            MessageDispatcher.AddListener(CommonVREventType.VR_RightStickAxis.ToString(), GetInput);
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointEnter.ToString(), PointEnter);
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointExit.ToString(), PointExit);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), PointClick);

            MessageDispatcher.RemoveListener(CommonVREventType.VR_RightStickAxis.ToString(), GetInput);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointEnter.ToString(), PointEnter);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointExit.ToString(), PointExit);
        }
        float time;
        public override void Update()
        {
            if (mapPanel.activeSelf) 
            {
                ShowMyAvatorPos();

                time += Time.deltaTime;
                if (time >= 2) 
                {
                    GetmAvatorList();
                    time = 0;
                }
            }
        }
        #endregion
        bool m_Open;
        void GetInput(IMessage msg)
        {
            if (m_Open == true)
            {
                Vector2 v2 = (Vector2)msg.Data;
                if (v2.y < 0)
                {
                    Content.GetComponent<Scrollbar>().value += 0.01f;
                }
                else if (v2.y > 0)
                {
                    Content.GetComponent<Scrollbar>().value -= 0.01f;
                }
            }
        }
        
        void PointEnter(IMessage msg)
        {
            GameObject temp = msg.Data as GameObject;
            if (temp.name == "Scroll View")
            {
                m_Open = true;
            }
        }
        void PointExit(IMessage msg)
        {
            GameObject temp = msg.Data as GameObject;
            if (temp.name == "Scroll View")
            {
                m_Open = false;
            }
        }
        private void PointClick(IMessage msg)
        {
            GameObject info = msg.Data as GameObject;
            if (info.name == "导览图") 
            {
                mapToggle.isOn = true;
            }
        }

        private void ToggleClick(bool isOn)
        {
            if (isOn)
            {
                mapPanel.SetActive(isOn);
                mapPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 10;
                GetmAvatorList();
                mStaticData.IsOpenPointClick = false;
            }
            else
            {
                mapPanel.SetActive(isOn);
                mapPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 1;
                mStaticData.IsOpenPointClick = true;
            }
        }

        /// <summary>
        /// 在地图上展示自己的位置
        /// </summary>
        private void ShowMyAvatorPos() 
        {
            AvatarPoint.GetComponent<RectTransform>().anchoredPosition = WordToScreenPos(mStaticThings.I.MainVRROOT.position);
        }


        /// <summary>
        /// 世界位置转换成地图上平面位置
        /// </summary>
        /// <param name="wordV3"></param>
        /// <returns></returns>
        public Vector2 WordToScreenPos(Vector3 wordV3) 
        {
            Vector2 temp;
            var withRate = mapPanel.transform.Find("MapPanel").GetComponent<RectTransform>().rect.width / modelSize.x;
            var hightRate = mapPanel.transform.Find("MapPanel").GetComponent<RectTransform>().rect.height / modelSize.z;

            var xlearp = withRate * (wordV3.x - mapRightPoint.position.x);
            var ylearp = hightRate * (wordV3.z - mapRightPoint.position.z);

            temp.x = uimapRightPoint.GetComponent<RectTransform>().anchoredPosition.x+ xlearp;
            temp.y = ylearp + uimapRightPoint.GetComponent<RectTransform>().anchoredPosition.y;

            return temp;
        }

        /// <summary>
        /// 小地图位置转换成世界坐标
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        public Vector3 ScreenToWordPos(Vector2 screenPos) 
        {
            Vector3 temp = new Vector3();
            var withRate = modelSize.x/mapPanel.transform.Find("MapPanel").GetComponent<RectTransform>().rect.width;
            var hightRate = modelSize.z / mapPanel.transform.Find("MapPanel").GetComponent<RectTransform>().rect.height;

            var xlearp = withRate * (uimapRightPoint.GetComponent<RectTransform>().anchoredPosition.x - screenPos.x);
            var zlearp = hightRate * (uimapRightPoint.GetComponent<RectTransform>().anchoredPosition.y- screenPos.y);

            temp.x = mapRightPoint.position.x - xlearp;
            temp.z = mapRightPoint.position.z - zlearp
;            temp.y = 0.5f;
            return temp;
        }


        #region 获取人员列表
        Dictionary<string, Transform> tempDir = new Dictionary<string, Transform>();
        private void GetmAvatorList()
        {
            tempDir.Clear();
            for (int i = 1; i < contentParent.childCount; i++)
            {
                if (mStaticThings.I.GetAllStaticAvatarList().Contains(contentParent.GetChild(i).GetComponent<GeneralDllBehavior>().OtherData))
                {
                    contentParent.GetChild(i).gameObject.SetActive(true);
                    tempDir.Add(contentParent.GetChild(i).GetComponent<GeneralDllBehavior>().OtherData, contentParent.GetChild(i));
                }
                else
                {
                    contentParent.GetChild(i).gameObject.SetActive(false);
                    tempDir.Add(contentParent.GetChild(i).GetComponent<GeneralDllBehavior>().OtherData, contentParent.GetChild(i));
                }
            }

            for (int i = 0; i < mStaticThings.I.GetAllStaticAvatarList().Count; i++)
            {
                if (mStaticThings.I.GetAllStaticAvatarList()[i]!=mStaticThings.I.mAvatarID) 
                {
                    if (!tempDir.ContainsKey(mStaticThings.I.GetAllActiveAvatarList()[i]))
                    {
                        var go = GameObject.Instantiate(infoTogglePrabel, contentParent);
                        go.SetActive(true);
                        go.transform.GetComponent<RectTransform>().localScale = Vector3.one;
                        go.GetComponent<GeneralDllBehavior>().OtherData = mStaticThings.I.GetAllStaticAvatarList()[i];
                        go.transform.Find("Label").GetComponent<Text>().text = mStaticThings.AllStaticAvatarsDic[mStaticThings.I.GetAllStaticAvatarList()[i]].name;
                    }
                    else
                    {
                        tempDir[mStaticThings.I.GetAllActiveAvatarList()[i]].gameObject.SetActive(true);
                        tempDir[mStaticThings.I.GetAllActiveAvatarList()[i]].Find("Label").GetComponent<Text>().text = mStaticThings.AllStaticAvatarsDic[mStaticThings.I.GetAllStaticAvatarList()[i]].name;
                    }
                }
            }
        }

        #endregion

        #region 玻璃房传送光柱位置传送按钮显隐
        private IEnumerator ShowTeleportButton(float time) 
        {
            yield return new WaitForSeconds(time);
            if (mStaticData.CompanyAsset.posTeleports.Count == 0)
            {
                BaseMono.StartCoroutine(ShowTeleportButton(1.1f));
            }
            else 
            {
                GetButtonList();
            }
        }

        private void GetButtonList() 
        {
            for (int i = 0; i < mStaticData.CompanyAsset.posTeleports.Count; i++)
            {
                var a = i;
                if (mStaticData.CompanyAsset.posTeleports[a].HaveTeleport != "0")
                {
                    buttonList.Find(a.ToString()).gameObject.SetActive(true);
                    buttonList.Find(a.ToString()).GetChild(1).GetComponent<Text>().text = mStaticData.CompanyAsset.posTeleports[a].Name;
                    buttonList.Find(a.ToString()).GetComponent<Button>().onClick.AddListener(()=> { Click(a); });
                }
                else 
                {
                    buttonList.Find(a.ToString()).gameObject.SetActive(false);
                }
            }
        }
        private void Click(int index) 
        {
            switch (index)
            {
                case 0:
                    MoveClick(new Vector3(16.741f, 0.09f, 46.623f));
                    break;
                case 1:
                    MoveClick(new Vector3(15.82f, 0.09f, 43.72f));
                    break;
                case 2:
                    MoveClick(new Vector3(14.16f, 0.09f, 41.188f));
                    break;
                case 3:
                    MoveClick(new Vector3(12.053f, 0.09f, 39.142f));
                    break;
            }
            mapToggle.isOn = false;
        }
        private void MoveClick(Vector3 point)
        {
            CharacterController control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
            if (control != null)
                control.enabled = false;

            mStaticThings.I.MainVRROOT.localPosition = point;
            if (control != null)
                control.enabled = true;
        }
        #endregion

        #region 位置定点传送
        private Transform boonPoint;
        private void ShowUI(bool isOpen,Transform tf) 
        {
            if (isOpen)
            {
                HintPanel.gameObject.SetActive(true);
                HintPanel.Find("SureButton").GetChild(0).GetComponent<Text>().text = "去" + tf.Find("Image/Text").GetComponent<Text>().text;
                HintPanel.Find("Text").GetComponent<Text>().text = "您确定要去<color=red>" + tf.Find("Image/Text").GetComponent<Text>().text + "</color>吗？";
                boonPoint = tf;
            }
            else 
            {
                HintPanel.gameObject.SetActive(false);
            }
        }

        private void SureClick() 
        {
            HintPanel.gameObject.SetActive(false);
            mapToggle.isOn = false;
            MulticastMoveClick(boonPoint);
        }
        private void MulticastMoveClick(Transform screenPos)
        {
            CharacterController control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
            if (control != null)
                control.enabled = false;

            mStaticThings.I.MainVRROOT.position = ScreenToWordPos(screenPos.GetComponent<RectTransform>().anchoredPosition);
            mStaticThings.I.MainVRROOT.rotation = screenPos.Find("Rotate").GetComponent<RectTransform>().rotation;
            if (control != null)
                control.enabled = true;
            if (mStaticThings.I.isVRApp)
            {
                mStaticThings.I.MainVRROOT.rotation = new Quaternion(0, 0, 0, 0);
                mapPanel.transform.parent.parent.gameObject.SetActive(false);
            }
        }
        #endregion
    }
}
