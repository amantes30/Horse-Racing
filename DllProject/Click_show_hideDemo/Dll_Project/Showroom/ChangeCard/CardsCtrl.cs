using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using LitJson;
using System.Collections;
using UnityEngine.Networking;

namespace Dll_Project.Showroom
{
    public class CardInfo
    {
        public GameObject CardGO;
        public Vector3 OldLocalPos;
    }
    //oss配置名片结构
    public class Card_Info
    {
        public string exhibition_id;
        public string company_name;
        public string name;
        public string position;
        public string to_info;
        public string weixin;
        public string email;
        public string namepinyin;
    }
    //交换名片结构
    public class card_info 
    {
        public string name;
        public string company;
        public string contact;
        public string job;
        public string exhibition_id;
    }
    public class self_info 
    {
        public string name;
        public string company;
        public string contact;
        public string job;
        public string exhibition_id;
    }

    public class CardsCtrl : DllGenerateBase
    {
        private Transform cardParent;//BaseMono
        private List<Transform> cardList = new List<Transform>();
        public override void Init()
        {
            cardParent = BaseMono.ExtralDatas[0].Target;
        }
        public override void Start()
        {
            for (int i = 0; i < cardParent.childCount; i++)
            {
                cardParent.GetChild(i).gameObject.SetActive(true);
                cardList.Add(cardParent.GetChild(i));
            }

            BaseMono.StartCoroutine(LoadCardInfoFile(2));
        }
        public override void OnEnable()
        {
        }
        public override void OnDisable()
        {
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        #region json
       public IEnumerator LoadCardInfoFile(float delayTime = 0)
        {
            yield return new WaitForSeconds(delayTime);
            if (mStaticData.BoothAsset.cardInfo== null)
            {
                BaseMono.StartCoroutine(LoadCardInfoFile(2));
            }
            else 
            {
                FuZhiCard(mStaticData.BoothAsset.cardInfo);
            }
        }
        public void FuZhiCard(List<Card_Info> ci) 
        {
            for (int i = 0; i < ci.Count; i++)
            {
                if (i < 8)
                {
                    cardList[i].Find("tip_pos1/Card/Name").GetComponent<Text>().text = ci[i].name;
                    cardList[i].Find("tip_pos1/Card/ZhiWei").GetComponent<Text>().text = ci[i].position;
                    cardList[i].Find("tip_pos1/Card/NamePingYin").GetComponent<Text>().text = ci[i].namepinyin;
                    cardList[i].Find("tip_pos1/Card/GongSi").GetComponent<Text>().text = ci[i].company_name;
                    cardList[i].Find("tip_pos1/Card/Phone").GetComponent<Text>().text = ci[i].to_info;
                    cardList[i].Find("tip_pos1/Card/WeiXin").GetComponent<Text>().text = ci[i].weixin;
                    cardList[i].Find("tip_pos1/Card/email").GetComponent<Text>().text = ci[i].email;
                }
            }
        }
        #endregion
    }
}
