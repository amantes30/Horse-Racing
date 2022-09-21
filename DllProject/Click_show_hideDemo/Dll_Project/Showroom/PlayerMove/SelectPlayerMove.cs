using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using LitJson;
using UnityEngine.UI;

namespace Dll_Project.Showroom.PlayerMove
{
    public class SelectPlayerMove : DllGenerateBase
    {
        private GameObject selectPlayerPanel;//面板
        private Toggle selectPlayerToggle;
        private Transform contentParent;//父类
        private GameObject infoTogglePrabel;//预制体
        private Toggle allSelectToggle;//全选
        private Toggle openBtn;//是否来时移动人物

        private GameObject SpecialEffects;
        public override void Init()
        {
            selectPlayerPanel = BaseMono.ExtralDatas[0].Target.gameObject;
            selectPlayerToggle = BaseMono.ExtralDatas[1].Target.GetComponent<Toggle>();
            contentParent = BaseMono.ExtralDatas[2].Target;
            infoTogglePrabel = BaseMono.ExtralDatas[3].Target.gameObject;
            allSelectToggle = BaseMono.ExtralDatas[4].Target.GetComponent<Toggle>();
            openBtn = BaseMono.ExtralDatas[5].Target.GetComponent<Toggle>();

            SpecialEffects = BaseMono.ExtralDataObjs[0].Target as GameObject;
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            selectPlayerToggle.onValueChanged.AddListener(ToggleClick);
            allSelectToggle.onValueChanged.AddListener(AllSelectToggleClick);
            openBtn.onValueChanged.AddListener(OpenToggleClick);
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        }
        float time;
        public override void Update()
        {
            if ((mStaticThings.I.isAdmin || mStaticThings.I.sadmin))
            {
                if (!mStaticThings.I.isVRApp)
                {
                    if (!selectPlayerToggle.transform.parent.gameObject.activeSelf)
                    {
                        selectPlayerToggle.transform.parent.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                if (selectPlayerToggle.transform.parent.gameObject.activeSelf)
                {
                    selectPlayerToggle.transform.parent.gameObject.SetActive(false);
                    selectPlayerToggle.isOn = false;
                }
            }

            if (openBtn.isOn) 
            {
                if (mStaticThings.I != null)
                {
                    if (mStaticThings.I.isVRApp && !mStaticThings.I.isAdmin && !mStaticThings.I.sadmin)
                        return;
                    if (mStaticThings.I.ismobile)
                    {
                        if (Input.GetMouseButtonDown(0)&& !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                        {
                            Ray ray = mStaticThings.I.Maincamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out RaycastHit hitInfo))
                            {
                                if (hitInfo.transform.GetComponent<VRPlayceMeshMark>()) 
                                {
                                    SureMove(hitInfo.point);
                                    ShowSpecialEffect(hitInfo.point);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0)&& !EventSystem.current.IsPointerOverGameObject())
                        {
                            Ray ray = mStaticThings.I.Maincamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out RaycastHit hitInfo))
                            {
                                if (hitInfo.transform.GetComponent<VRPlayceMeshMark>())
                                {
                                    SureMove(hitInfo.point);
                                    ShowSpecialEffect(hitInfo.point);
                                }
                            }
                        }
                    }
                }
            }

            if (selectPlayerPanel.activeSelf) 
            {
                time += Time.deltaTime;
                if (time >= 2)
                {
                    GetmAvatorList();
                    time = 0;
                }
            }
        }
        #endregion

        private void ToggleClick(bool isOn)
        {
            if (isOn)
            {
                selectPlayerPanel.SetActive(isOn);
                selectPlayerPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 11;
                mStaticData.IsOpenPointClick = false;
                GetmAvatorList();
                SaveInfo.instance.SaveActionData("PlayerMove", 10);
            }
            else
            {
                selectPlayerPanel.SetActive(isOn);
                selectPlayerPanel.transform.parent.parent.GetComponent<Canvas>().sortingOrder = 1;
                mStaticData.IsOpenPointClick = true;
            }
        }

        private void OpenToggleClick(bool isOn) 
        {
            //selectPlayerToggle.isOn = false;
        }

        private GameObject PCTemp;
        private void ShowSpecialEffect(Vector3 point) 
        {
            if (PCTemp == null)
            {
                PCTemp = GameObject.Instantiate(SpecialEffects);
            }
            PCTemp.transform.position = point;
            var ps= PCTemp.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i].Play();
            }
        }

        /// <summary>
        /// 获取人员列表展示
        /// </summary>
        Dictionary<string, Transform> tempDir = new Dictionary<string, Transform>();
        private void GetmAvatorList()
        {
            tempDir.Clear();
            for (int i = 1; i < contentParent.childCount; i++)
            {
                if (mStaticThings.I.GetAllStaticAvatarList().Contains(contentParent.GetChild(i).GetComponent<GeneralDllBehavior>().OtherData))
                {
                    contentParent.GetChild(i).gameObject.SetActive(false);
                    tempDir.Add(contentParent.GetChild(i).GetComponent<GeneralDllBehavior>().OtherData, contentParent.GetChild(i));
                }
                else 
                {
                    contentParent.GetChild(i).gameObject.SetActive(false);
                    contentParent.GetChild(i).GetComponent<Toggle>().isOn = false;
                    tempDir.Add(contentParent.GetChild(i).GetComponent<GeneralDllBehavior>().OtherData, contentParent.GetChild(i));
                }
            }

            for (int i = 0; i < mStaticThings.I.GetAllStaticAvatarList().Count; i++)
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

        /// <summary>
        /// 人员列表全选和不选
        /// </summary>
        /// <param name="isOn"></param>
        private void AllSelectToggleClick(bool isOn)
        {
            if (isOn)
            {
                for (int i = 0; i < contentParent.childCount; i++)
                {
                    contentParent.GetChild(i).GetComponent<Toggle>().isOn = true;
                }

            }
            else
            {
                for (int i = 0; i < contentParent.childCount; i++)
                {
                    contentParent.GetChild(i).GetComponent<Toggle>().isOn = false;
                }

            }
        }

        /// <summary>
        /// 确定移动人
        /// </summary>
        /// <param name="point"></param>
        private void SureMove(Vector3 point) 
        {
            var temp = JsonMapper.ToJson(mStaticData.MovePlayerList);
            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = "MoveOtherPlayer",
                b = temp,
                c = point.x.ToString(),
                d = point.y.ToString(),
                e = point.z.ToString(),
            };
            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
        }

        private void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (info.a.Equals("MoveOtherPlayer"))
            {
                var pl = JsonMapper.ToObject<List<string>>(info.b);
                if (pl.Contains(mStaticThings.I.mAvatarID))
                {
                    MoveClick(new Vector3(float.Parse(info.c), float.Parse(info.d), float.Parse(info.e)));
                }
            }
        }
        /// <summary>
        /// 人物移动
        /// </summary>
        /// <param name="point"></param>
        private void MoveClick(Vector3 point)
        {
            CharacterController control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
            if (control != null)
                control.enabled = false;

            mStaticThings.I.MainVRROOT.position = new Vector3(UnityEngine.Random.Range(point.x - 0.5f, point.x + 0.5f), point.y, UnityEngine.Random.Range(point.z - 0.5f, point.z + 0.5f));
            if (control != null)
                control.enabled = true;
        }
        

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
