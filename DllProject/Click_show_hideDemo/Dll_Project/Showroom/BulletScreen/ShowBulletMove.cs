using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.Showroom.BulletScreen
{
    public class ShowBulletMove : DllGenerateBase
    {
        private Text showText;
        public override void Init()
        {
            showText = BaseMono.ExtralDatas[0].Target.GetComponent<Text>();
        }
        public override void Awake()
        {
        }

        public override void Start()
        {
            showText.text =BaseMono.OtherData;

            LayoutRebuilder.ForceRebuildLayoutImmediate(showText.gameObject.GetComponent<RectTransform>());
            if (!mStaticThings.I.isVRApp)
            {
                BaseMono.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(showText.gameObject.GetComponent<RectTransform>().sizeDelta.x + 30, 40);
            }
            else 
            {
                BaseMono.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(showText.gameObject.GetComponent<RectTransform>().sizeDelta.x + 30, 40);
                BaseMono.transform.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-(showText.gameObject.GetComponent<RectTransform>().sizeDelta.x + 30) / 2, BaseMono.transform.GetComponent<RectTransform>().anchoredPosition3D.y, 0);
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
            if (!mStaticThings.I.isVRApp)
            {
                if (mStaticThings.I.ismobile)
                {
                    Tweener tweener = BaseMono.transform.DOLocalMoveX(-Screen.width / 2 - 250, 15).SetSpeedBased().OnComplete(() =>
                    {
                        GameObject.Destroy(BaseMono.gameObject);
                    });
                    tweener.SetAutoKill(true);
                    tweener.Play();
                }
                else
                {
                    Tweener tweener = BaseMono.transform.DOLocalMoveX(-Screen.width / 2 - 250, 40).SetSpeedBased().OnComplete(() =>
                    {
                        GameObject.Destroy(BaseMono.gameObject);
                    });
                    tweener.SetAutoKill(true);
                    tweener.Play();
                }
            }
            else 
            {
                Tweener tweener = BaseMono.transform.DOLocalMoveX(-1246 / 2+(showText.gameObject.GetComponent<RectTransform>().sizeDelta.x + 30)/2, 40).SetSpeedBased().OnComplete(() =>
                {
                    GameObject.Destroy(BaseMono.gameObject);
                });
                tweener.SetAutoKill(true);
                tweener.Play();
            }
        }
    }
}
