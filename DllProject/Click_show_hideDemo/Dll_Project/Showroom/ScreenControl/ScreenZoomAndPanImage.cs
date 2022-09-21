using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dll_Project.Showroom.ScreenControl
{
    public class ScreenZoomAndPanImage : DllGenerateBase
    {
        public Transform ImageTarget;
        private GameObject bigScreenPanel;

        public float dist;

        private float panSpeedX;
        private float panSpeedY;
        private float zoomSpeed;

        public float PanxSpeedModifier = 0.2f;
        public float PanySpeedModifier = 0.2f;
        public float zoomSpeedModifier = 3;

        public float yclampmin = -250;
        public float yclampmax = 250;
        public float xclampmin = -480;
        public float xclampmax = 480;

        public float zoomclampmin = 1f;
        public float zoomclampmax = 2f;
        public override void Init()
        {
            ImageTarget = BaseMono.ExtralDatas[0].Target;
            bigScreenPanel = BaseMono.ExtralDatas[1].Target.gameObject;
        }
        public override void Awake()
        {
        }

        public override void Start()
        {
            if (mStaticThings.I.ismobile) 
            {
                yclampmin = -196;
                yclampmax = 196;
                xclampmin = -370;
                xclampmax = 370;
            }
            dist = ImageTarget.localScale.x;
        }
        public override void OnEnable()
        {
            IT_Gesture.onDraggingE += OnDragging;
            IT_Gesture.onPinchE += OnPinch;
        }

        public override void OnDisable()
        {
            IT_Gesture.onDraggingE -= OnDragging;
            IT_Gesture.onPinchE -= OnPinch;
        }

        public override void Update()
        {
            if (!bigScreenPanel.activeSelf)
            {
                if (dist != 1)
                    dist = 1;
                return;
            }
            if (!ImageTarget.gameObject.activeInHierarchy) return;

            //ImageTarget.localScale = new Vector3(dist, dist, dist);
            if (mStaticThings.I!=null) 
            {
                if (mStaticThings.I.BigscreenRoot.Find("ScreenRoot/Canvas_Picture/Canvas_PIC/Panel/RawImage/whiteboard") != null)
                {
                    if (mStaticThings.I.BigscreenRoot.Find("ScreenRoot/Canvas_Picture/Canvas_PIC/Panel/RawImage/whiteboard").gameObject.activeSelf)
                    {
                        ImageTarget.localScale = new Vector3(dist, -dist, dist);
                    }
                    else
                    {
                        ImageTarget.localScale = new Vector3(dist, dist, dist);
                    }
                }
            }

            if (dist != 1) 
            {
                float x = ImageTarget.GetComponent<RectTransform>().anchoredPosition.x;
                float y = ImageTarget.GetComponent<RectTransform>().anchoredPosition.y;
                x = Mathf.Clamp(x + panSpeedX, xclampmin * ((dist - 1f) / 1f), xclampmax * ((dist - 1f) / 1f));
                y = Mathf.Clamp(y + panSpeedY, yclampmin * ((dist - 1f) / 1f), yclampmax * ((dist - 1f) / 1f));
                ImageTarget.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                panSpeedX = 0;
                panSpeedY = 0;
            }

            dist += Time.deltaTime * zoomSpeed * 0.022f;
            dist = Mathf.Clamp(dist, zoomclampmin, zoomclampmax);
            zoomSpeed = 0;

            zoomSpeed += Input.GetAxis("Mouse ScrollWheel") * 500 * zoomSpeedModifier;
            RestorePos();
        }
        void OnDragging(DragInfo dragInfo)
        {
            if (!ImageTarget.gameObject.activeInHierarchy) return;
            dragInfo.delta /= IT_Gesture.GetDPIFactor();
            panSpeedY = dragInfo.delta.y * PanxSpeedModifier;
            panSpeedX = dragInfo.delta.x * PanySpeedModifier;
        }

        void OnPinch(PinchInfo pinfo)
        {
            if (!ImageTarget.gameObject.activeInHierarchy) return;
            zoomSpeed -= pinfo.magnitude * zoomSpeedModifier / IT_Gesture.GetDPIFactor();
        }
        float tempDist;
        private void RestorePos()
        {
            float x = 0;
            float y = 0;

            if (dist == 1)
            {
                ImageTarget.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                return;
            }
            if (dist == 2 || tempDist <= dist)
            {
                return;
            }
            if (ImageTarget.GetComponent<RectTransform>().anchoredPosition.x != 0)
            {
                x = ImageTarget.GetComponent<RectTransform>().anchoredPosition.x * ((dist - 1f) / 1f);
            }

            if (ImageTarget.GetComponent<RectTransform>().anchoredPosition.y != 0)
            {
                y = ImageTarget.GetComponent<RectTransform>().anchoredPosition.y * ((dist - 1f) / 1f);
            }

            ImageTarget.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            tempDist = dist;
        }
    }
}
