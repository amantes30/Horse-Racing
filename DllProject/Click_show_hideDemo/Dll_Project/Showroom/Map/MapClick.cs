using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.Showroom.Map
{
    public class MapClick : DllGenerateBase
    {
        private Button SkipBtn;
        private Toggle lookToggle;
        private Transform iconPanel;

        private Transform HintPanel;
        private Toggle mapToggle;

        private GameObject AvatarPanel;
        private GameObject uiCanvas;
        public override void Init()
        {
            SkipBtn = BaseMono.ExtralDatas[0].Target.GetComponent<Button>();
            lookToggle = BaseMono.ExtralDatas[1].Target.GetComponent<Toggle>();
            iconPanel = BaseMono.ExtralDatas[2].Target;
            HintPanel = BaseMono.ExtralDatas[3].Target;
            mapToggle = BaseMono.ExtralDatas[4].Target.GetComponent<Toggle>();
            uiCanvas = BaseMono.ExtralDatas[5].Target.gameObject;
            
            AvatarPanel = GameObject.Find("_WsAvatarsRoot");
        }
        #region 初始化
        public override void Awake()
        {
        }

        public override void Start()
        {
            SkipBtn.onClick.AddListener(SkipClick);
            lookToggle.onValueChanged.AddListener(ToggleClick);
        }

        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
            lookToggle.isOn = false;
        }
        //bool isOpen = false;
        public override void Update()
        {
            if (lookToggle.isOn)
            {
                GetOtherAvatarUIPos();
            }
        }
        #endregion
        Transform iconTemp;
        private void ToggleClick(bool isOn)
        {
            if (isOn)
            {
                iconTemp = GameObject.Instantiate(iconPanel, iconPanel.parent);
                iconTemp.gameObject.SetActive(false);
            }
            else
            {
                GameObject.Destroy(iconTemp.gameObject);
            }
        }
        /// <summary>
        /// 获取选择人员在Ui上的位置
        /// </summary>
        private void GetOtherAvatarUIPos()
        {
            var temp = AvatarPanel.transform.Find(BaseMono.OtherData);
            if (temp != null)
            {
                if (iconTemp != null)
                {
                    iconTemp.gameObject.SetActive(true);
                    iconTemp.GetComponent<RectTransform>().anchoredPosition = MapControl.instances.WordToScreenPos(temp.position);
                }
            }
        }

        private void SkipClick()
        {
            ShowUI(true);
            HintPanel.Find("CancelButton").GetComponent<Button>().onClick.RemoveAllListeners();
            HintPanel.Find("SureButton").GetComponent<Button>().onClick.RemoveAllListeners();
            HintPanel.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() => { ShowUI(false); });
            HintPanel.Find("SureButton").GetComponent<Button>().onClick.AddListener(SureClick);
        }
        private void MoveClick(Vector3 point)
        {
            CharacterController control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
            if (control != null)
                control.enabled = false;

            mStaticThings.I.MainVRROOT.position = new Vector3(UnityEngine.Random.Range(point.x - 0.5f, point.x + 0.5f), point.y, UnityEngine.Random.Range(point.z - 0.5f, point.z + 0.5f));
            if (control != null)
                control.enabled = true;
            if (mStaticThings.I.isVRApp) 
            {
                mStaticThings.I.MainVRROOT.rotation = new Quaternion(0, 0, 0, 0);
                uiCanvas.SetActive(false);
            }
        }

        #region 人员定点传送
        private void ShowUI(bool isOpen)
        {
            if (isOpen)
            {
                HintPanel.gameObject.SetActive(true);
                HintPanel.Find("SureButton").GetChild(0).GetComponent<Text>().text = "去TA身边";
                HintPanel.Find("Text").GetComponent<Text>().text = "您确定要去<color=red>" + BaseMono.transform.Find("Label").GetComponent<Text>().text.Split('\n')[0] + "</color>身边吗？";
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
            var temp = AvatarPanel.transform.Find(BaseMono.OtherData);
            if (temp != null)
            {
                if (temp.position.y < 0)
                {

                }
                else
                {
                    MoveClick(temp.position);
                }
            }
        }
        #endregion
    }
}
