using com.ootii.Messages;
using Dll_Project.Showroom;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project
{
    public class EmoticonsController : DllGenerateBase
    {
        private Dictionary<string, GameObject> FaceParticles = new Dictionary<string, GameObject>();//表情特效和表情名字
        private Dictionary<string, Dictionary<string, GameObject>> AvatorParticles = new Dictionary<string, Dictionary<string, GameObject>>();//表情特效和人员ID
        private Transform particlesPool;//特效父类
        private GameObject particlesObject;//特效
        private string particlesName;
        private bool isStartParticles;
        private float particlesSyncTime = 5f;
        private Vector3 sendedPos;
        private Vector3 nowPos;

        private ExtralData[] extralDataBtn;

        private GameObject AvatorParent;
        public override void Init()
        {
            particlesPool = BaseMono.ExtralDatas[0].Target;
            ExtralDataObj[] extralDataObj = BaseMono.ExtralDataObjs;
            for (int i = 0; i < extralDataObj.Length; i++)
            {
                FaceParticles.Add(extralDataObj[i].OtherData, extralDataObj[i].Target as GameObject);
            }

            extralDataBtn = BaseMono.ExtralDatas[1].Info;
            AvatorParent = GameObject.Find("_WsAvatarsRoot");
        }
        #region
        public override void Awake()
        {
        }

        public override void Start()
        {
            for (int i = 0; i < extralDataBtn.Length; i++)
            {
                var temp = extralDataBtn[i].Target;
                temp.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SendParticlesInfo(temp.name);
                });
            }
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.AddListener(WsMessageType.RecieveMovingObj.ToString(), RecieveMovingObj);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveMovingObj.ToString(), RecieveMovingObj);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Update()
        {
            if (isStartParticles) 
            {
                particlesSyncTime -= Time.deltaTime;
                if (particlesSyncTime > 0)
                {
                    if (Time.frameCount % 20 == 0)
                    {
                        nowPos = mStaticThings.I.Maincamera.transform.position;
                        float dis = (nowPos - sendedPos).magnitude;
                        if (Math.Abs(dis) > 1)
                        {
                            //首先将自己的粒子同步到自己头上
                            if (AvatorParticles.ContainsKey(mStaticThings.I.mAvatarID))
                            {
                                //GameObject go = AvatorParticles[mStaticThings.I.mAvatarID][particlesName];
                                if (particlesObject != null)
                                {
                                    particlesObject.transform.position = mStaticThings.I.Maincamera.transform.position;
                                }
                            }
                            //然后将自己粒子的位置通知给别人
                            WsMovingObj sendMovingObj = new WsMovingObj()
                            {
                                id = mStaticThings.I.mAvatarID,
                                name = particlesName,
                                islocal = false,
                                position = new Vector3(mStaticThings.I.MainVRROOT.position.x, mStaticThings.I.MainVRROOT.position.y+1.2f, mStaticThings.I.MainVRROOT.position.z)
                            };
                            MessageDispatcher.SendMessage(this, WsMessageType.SendMovingObj.ToString(), sendMovingObj, 0);
                            sendedPos = nowPos;
                        }
                    }
                }
                else
                {
                    isStartParticles = false;//播放完就置为false
                    //particlesObject.SetActive(false);
                    //particlesObject.transform.GetComponentInChildren<ParticleSystem>().Stop();
                    //particlesObject.transform.GetComponent<AudioSource>().Stop();
                }
            }
        }
        #endregion

        private void SendParticlesInfo(string pariclesName)
        {
            SaveInfo.instance.SaveActionData(pariclesName, 10);
            Vector3 pos = mStaticThings.I.Maincamera.transform.position;
            Vector3 posRoot = mStaticThings.I.MainVRROOT.transform.position;
            WsCChangeInfo wsinfo1 = new WsCChangeInfo()
            {
                a = "ShowEmoticons",
                b = mStaticThings.I.mAvatarID,
                c = pariclesName,
                d = pos.x.ToString(),
                e = pos.y.ToString(),
                f = pos.z.ToString(),
                g = posRoot.x + " " + posRoot.y + " " + posRoot.z
            };
            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0.1f);

            particlesSyncTime = 5f;
            particlesName = pariclesName;
            isStartParticles = true;
            mStaticData.IsOpenIconPanel = true;
        }

        public void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (info.a == "ShowEmoticons")
            {
                IsLoadParticles(info);
            }
        }
        //判断是否加载特效
        private void IsLoadParticles(WsCChangeInfo info)
        {
            if (AvatorParticles.ContainsKey(info.b)) //判断人员是否有
            {
                var ParticlesDir = AvatorParticles[info.b];
                if (ParticlesDir.ContainsKey(info.c)) //判断特效是否有
                {
                    particlesObject = ParticlesDir[info.c];
                    particlesObject.SetActive(true);
                }
                else
                {
                    particlesObject = GameObject.Instantiate(FaceParticles[info.c], particlesPool);
                    particlesObject.name = info.b;
                    AvatorParticles[particlesObject.name].Add(info.c, particlesObject);
                }
            }
            else 
            {
                particlesObject = GameObject.Instantiate(FaceParticles[info.c], particlesPool);
                particlesObject.name = info.b;
                var dirTemp = new Dictionary<string, GameObject>();
                dirTemp.Add(info.c, particlesObject);
                AvatorParticles.Add(particlesObject.name, dirTemp);
            }

            //位置todo
            if (info.b == mStaticThings.I.mAvatarID)
            {
                float a = 0;
                float.TryParse(info.d, out a);
                float b = 0;
                float.TryParse(info.e, out b);
                float c = 0;
                float.TryParse(info.f, out c);

                particlesObject.transform.position = new Vector3(a, b, c);
            }
            else
            {
                if (AvatorParent!= null) 
                {
                    var go = AvatorParent.transform.Find(info.b);
                    float a = go.transform.Find("NamePanel").position.y;
                    var temp = info.g.Split(' ');
                    particlesObject.transform.position = new Vector3(float.Parse(temp[0]), a, float.Parse(temp[2]));
                }
                
            }
            //particlesObject.SetActive(true);
            particlesObject.transform.GetComponentInChildren<ParticleSystem>().Play();
            particlesObject.transform.GetComponent<AudioSource>().Play();
        }

        private void RecieveMovingObj(IMessage msg)//位置同步
        {
            if (msg == null|| msg.Data==null)
                return;
            WsMovingObj newMovingObj = msg.Data as WsMovingObj;
            if (mStaticThings.I == null) { return; }
            if (newMovingObj.id != mStaticThings.I.mAvatarID)
            {
                if (AvatorParticles.Count == 0)
                    return;
                if (AvatorParticles.ContainsKey(newMovingObj.id))
                {
                    if (AvatorParticles[newMovingObj.id].Count == 0)
                        return;
                    if (AvatorParticles[newMovingObj.id].ContainsKey(newMovingObj.name)) 
                    {
                        GameObject go = AvatorParticles[newMovingObj.id][newMovingObj.name];
                        if (go != null)
                        {
                            if (AvatorParent != null)
                            {
                                var goo = AvatorParent.transform.Find(newMovingObj.id);
                                float a = goo.transform.Find("NamePanel").position.y;
                                go.transform.position = new Vector3(newMovingObj.position.x, a, newMovingObj.position.z);
                            }
                                //go.transform.position = newMovingObj.position;
                        }
                    }
                }
            }
        }
    }
}
