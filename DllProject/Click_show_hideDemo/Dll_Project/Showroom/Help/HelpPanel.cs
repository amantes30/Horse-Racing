using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.Showroom
{
    public class HelpPanel : DllGenerateBase
    {
        private GameObject helpPanel;
        private Toggle helpToggle;
        private Button cancelBtn;
        private GameObject clickPanel;
        private Button leftBtn;
        private Button rightBtn;
        private Text pageText;

        private Transform FuntionPanel;
        private Transform AllImgPanel;
        public override void Init()
        {
            helpPanel = BaseMono.ExtralDatas[0].Target.gameObject;
            helpToggle = BaseMono.ExtralDatas[1].Target.GetComponent<Toggle>();
            cancelBtn = BaseMono.ExtralDatas[2].Target.GetComponent<Button>();
            clickPanel = BaseMono.ExtralDatas[3].Target.gameObject;
            leftBtn = BaseMono.ExtralDatas[3].Info[0].Target.GetComponent<Button>();
            rightBtn = BaseMono.ExtralDatas[3].Info[1].Target.GetComponent<Button>();
            pageText = BaseMono.ExtralDatas[3].Info[2].Target.GetComponent<Text>();
            FuntionPanel = BaseMono.ExtralDatas[4].Target;
            AllImgPanel = BaseMono.ExtralDatas[5].Target;
        }
        #region 初始化
        public override void Awake()
        {
        }

        public override void Start()
        {
            helpToggle.onValueChanged.AddListener(ToggleClick);
            cancelBtn.onClick.AddListener(() => { BaseMono.StartCoroutine(CancelClick()); });
            leftBtn.onClick.AddListener(LeftCick);
            rightBtn.onClick.AddListener(RightClick);

            FuntionPanel.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { ShowClick(AllImgPanel.GetChild(0).gameObject,0); });
            FuntionPanel.GetChild(1).GetComponent<Button>().onClick.AddListener(() => { ShowClick(AllImgPanel.GetChild(1).gameObject,1); });
            FuntionPanel.GetChild(2).GetComponent<Button>().onClick.AddListener(() => { ShowClick(AllImgPanel.GetChild(2).gameObject,2); });
            FuntionPanel.GetChild(3).GetComponent<Button>().onClick.AddListener(() => { ShowClick(AllImgPanel.GetChild(3).gameObject,3); });

            if (mStaticThings.I!=null) 
            {
                if (!mStaticThings.I.isVRApp)
                {
                    GameObject.Destroy(AllImgPanel.GetChild(1).GetChild(1).gameObject);
                    if (mStaticThings.I.ismobile)
                    {
                        GameObject.Destroy(AllImgPanel.GetChild(0).GetChild(0).gameObject);
                        GameObject.Destroy(AllImgPanel.GetChild(0).GetChild(2).gameObject);
                    }
                    else
                    {
                        GameObject.Destroy(AllImgPanel.GetChild(0).GetChild(1).gameObject);
                        GameObject.Destroy(AllImgPanel.GetChild(0).GetChild(2).gameObject);
                    }
                }
                else 
                {
                    GameObject.Destroy(AllImgPanel.GetChild(1).GetChild(0).gameObject);
                    GameObject.Destroy(AllImgPanel.GetChild(0).GetChild(0).gameObject);
                    GameObject.Destroy(AllImgPanel.GetChild(0).GetChild(1).gameObject);
                    FuntionPanel.GetChild(0).GetChild(0).GetComponent<Text>().text = "手柄说明";
                }
            }
        }
        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }

        public override void Update()
        {
            if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
            {
                if (FuntionPanel.GetChild(3).GetComponent<Button>().interactable == false)
                {
                    FuntionPanel.GetChild(3).GetComponent<Button>().interactable = true;
                    FuntionPanel.GetChild(3).Find("Text").GetComponent<Text>().color = new Color(50 / 255f, 50 / 255f, 50 / 255f, 1);
                }
            }
            else 
            {
                FuntionPanel.GetChild(3).GetComponent<Button>().interactable = false;
                FuntionPanel.GetChild(3).Find("Text").GetComponent<Text>().color = new Color(180 / 255f, 180 / 255f, 180 / 255f, 1);
            }
        }
        #endregion

        /// <summary>
        /// 控制指南面板显隐
        /// </summary>
        /// <param name="isOn"></param>
        private void ToggleClick(bool isOn)
        {
            if (isOn)
            {
                helpPanel.SetActive(isOn);
                helpPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 10;
                mStaticData.IsOpenPointClick = false;
                ShowClick(AllImgPanel.GetChild(0).gameObject, 0);
                SaveInfo.instance.SaveActionData("Fingerpost", 10);
            }
            else
            {
                helpPanel.SetActive(isOn);
                helpPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 1;
                mStaticData.IsOpenPointClick = true;
            }
        }
        private IEnumerator CancelClick() 
        {
            yield return new WaitForSeconds(0.1f);
            helpToggle.isOn = false;
        }

        #region 指南不同部分展示和翻页
        List<GameObject> imgList = new List<GameObject>();
        int page;
        /// <summary>
        /// 展示对应模块的指南功能
        /// </summary>
        private void ShowClick(GameObject go,int index)
        {
            for (int i = 0; i < FuntionPanel.childCount; i++)
            {
                if (i == index)
                {
                    FuntionPanel.GetChild(i).GetComponent<Image>().color = new Color(1, 101 / 255f, 38 / 255f, 100 / 255f);
                }
                else 
                {
                    FuntionPanel.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);
                }
            }
            for (int i = 0; i < AllImgPanel.childCount; i++)
            {
                if (AllImgPanel.GetChild(i).name == go.name)
                {
                    AllImgPanel.GetChild(i).gameObject.SetActive(true);
                }
                else 
                {
                    AllImgPanel.GetChild(i).gameObject.SetActive(false);
                }
            }
            clickPanel.gameObject.SetActive(false);
            if (go.transform.childCount == 0)
            {
                return;
            }
            imgList.Clear();
            for (int i = 0; i < go.transform.childCount; i++)
            {
                imgList.Add(go.transform.GetChild(i).gameObject);
                go.transform.GetChild(i).gameObject.SetActive(false);
            }
            go.transform.GetChild(0).gameObject.SetActive(true);

            if (go.transform.childCount > 1)
            {
                clickPanel.gameObject.SetActive(true);
                page = 0;
                pageText.text=page+1+"/"+go.transform.childCount;
            }


        }
        /// <summary>
        /// 上一页
        /// </summary>
        private void LeftCick()
        {
            for (int i = 0; i < imgList.Count; i++)
            {
                imgList[i].SetActive(false);
            }
            if (page >= 1)
            {
                page--;
            }
            pageText.text = page+1 + "/" + imgList.Count;
            imgList[page].SetActive(true);
        }
        /// <summary>
        /// 下一页
        /// </summary>
        private void RightClick()
        {
            for (int i = 0; i < imgList.Count; i++)
            {
                imgList[i].SetActive(false);
            }
            if (page < imgList.Count -1)
            {
                page++;
            }
            pageText.text = page+1 + "/" + imgList.Count;
            imgList[page].SetActive(true);
        }
        #endregion
    }
}
