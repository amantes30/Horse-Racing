using com.ootii.Messages;
using Dll_Project.Showroom;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dll_Project
{
    public class EmoticonsUIPanel : DllGenerateBase
    {
        private GameObject iconPanel;
        private GameObject CountDownOb;

        private Toggle emoticonToggle;
        public override void Init()
        {
            emoticonToggle = BaseMono.ExtralDatas[0].Target.GetComponent<Toggle>();
            iconPanel = BaseMono.ExtralDatas[1].Target.gameObject;

            CountDownOb = BaseMono.ExtralDatas[0].Info[0].Target.gameObject;

            

        }
        #region
        public override void Awake()
        {
        }

        public override void Start()
        {
            emoticonToggle.onValueChanged.AddListener(ToggleClick);
        }
        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }

        private void ToggleClick(bool isOn) 
        {
            if (isOn)
            {
                HidePanelClick();
                mStaticData.IsOpenPointClick = false;
                SaveInfo.instance.SaveActionData("Emoticon", 10);
            }
            else 
            {
                ShowPanelClick();
                mStaticData.IsOpenPointClick = true;
            }
        }
        float time = 5f;
        public override void Update()
        {
            if (mStaticData.IsOpenIconPanel) 
            {
                if (iconPanel.activeSelf ==true) 
                {
                    CountDownOb.SetActive(true);
                    emoticonToggle.interactable = false;
                    emoticonToggle.isOn = false;
                    iconPanel.SetActive(false);
                }
                time -= Time.deltaTime;
                CountDownOb.GetComponent<Image>().fillAmount = time / 5;
                CountDownOb.transform.Find("Text").GetComponent<Text>().text = Math.Ceiling(time).ToString();
                if (time < 0) 
                {
                    mStaticData.IsOpenIconPanel = false;
                    emoticonToggle.interactable = true;
                    CountDownOb.SetActive(false);
                    CountDownOb.GetComponent<Image>().fillAmount = 1;
                    CountDownOb.transform.Find("Text").GetComponent<Text>().text = "5";
                    time = 5;
                }
            }
        }
        #endregion

        //展示表情图标
        private void ShowPanelClick() 
        {
            iconPanel.SetActive(false);
        }
        private void HidePanelClick()
        {
            iconPanel.SetActive(true);
        }
    }
}
