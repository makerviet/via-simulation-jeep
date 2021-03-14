using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Koi.UI
{
    public class CanvasScalerSetup : MonoBehaviour
    {
        [SerializeField] CanvasScaler _canvasScaler;
        CanvasScaler canvasScaler
        {
            get
            {
                if (_canvasScaler == null)
                {
                    _canvasScaler = GetComponent<CanvasScaler>();
                }
                return _canvasScaler;
            }
        }

        [SerializeField] Vector2 sizeMin = new Vector2(1280, 720);
        [SerializeField] Vector2 sizeMax = new Vector2(1280, 840);

        private void Awake()
        {
            SetupCanvasScaler();
        }

        void Start()
        {
            
        }

        float lastTime = 0;
        private void Update()
        {
            if (Time.realtimeSinceStartup - lastTime >= 1)
            {
                lastTime = Time.realtimeSinceStartup;
                CheckListCanvasGroupAndFix();
            }
        }

        CanvasGroup[] listCanvasGroups;
        void CheckListCanvasGroupAndFix()
        {
            listCanvasGroups = transform.GetComponentsInChildren<CanvasGroup>(true);
            foreach (CanvasGroup canvasGroup in listCanvasGroups)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }


        public void SetupCanvasScaler()
        {
            Vector2 curSize = new Vector2(Screen.width, Screen.height);
            float deviceRatio = curSize.x / curSize.y;
            if (deviceRatio > sizeMin.x / sizeMin.y)
            {
                // long device -> fix height
                curSize.y = sizeMin.y;
                curSize.x = curSize.y * deviceRatio;
            }
            else
            {
                // short device -> fix width
                curSize.x = sizeMin.x;
                curSize.y = curSize.x / deviceRatio;

                if (curSize.y > sizeMax.y)
                {
                    curSize.y = sizeMax.y;
                    curSize.x = curSize.y * deviceRatio;
                }
            }

            canvasScaler.referenceResolution = curSize;
            //Debug.LogError("Setup to " + curSize);
        }
    }
}
