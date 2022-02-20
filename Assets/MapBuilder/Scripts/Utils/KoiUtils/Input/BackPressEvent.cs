using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Koi.DeviceInput
{
    public class BackPressEvent : MonoBehaviour
    {
        static BackPressEvent Instance;

        List<Action> onSwallowBackPressListener = new List<Action>();
        List<Action> onAnyBackPressAnyListener = new List<Action>();

        [SerializeField] float delayDuration = 0.5f;
        float delayRemain = 0;

        public int swallowLength;
        public int anyLength;

        void Awake()
        {
            if (Instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            Instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            delayRemain = 0;
        }

        public static void AddSwallowBackPressListener(Action pListener)
        {
            if (Instance != null)
            {
                Instance.DoAddSwallowBackPressListener(pListener);
            }
        }

        void DoAddSwallowBackPressListener(Action pListener)
        {
            onSwallowBackPressListener.Add(pListener);
        }

        public static void RemoveSwallowBackPressListener(Action pListener)
        {
            if (Instance != null)
            {
                Instance.DoRemoveSwallowBackPressListener(pListener);
            }
        }

        void DoRemoveSwallowBackPressListener(Action pListener)
        {
            Debug.LogError("BackPressEvent: swallow remove ");
            onSwallowBackPressListener.Remove(pListener);
        }


        public static void AddAnyBackPressListener(Action pListener)
        {
            if (Instance != null)
            {
                Instance.DoAddAnyBackPressListener(pListener);
            }
        }

        void DoAddAnyBackPressListener(Action pListener)
        {
            onAnyBackPressAnyListener.Add(pListener);
        }


        public static void RemoveAnyBackPressListener(Action pListener)
        {
            if (Instance != null)
            {
                Instance.DoRemoveAnyBackPressListener(pListener);
            }
        }

        void DoRemoveAnyBackPressListener(Action pListener)
        {
            Debug.LogError("BackPressEvent: any remove ");
            onAnyBackPressAnyListener.Remove(pListener);
        }

        void Update()
        {
            if (delayRemain > 0)
            {
                delayRemain -= Time.unscaledDeltaTime;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    OnBackPress();
                    delayRemain = delayDuration;
                }
            }

            swallowLength = onSwallowBackPressListener.Count;
            anyLength = onAnyBackPressAnyListener.Count;
        }

        void OnBackPress()
        {
            CallbackSwallow();

            CallbackAny();

            CleanListener();
        }

        void CallbackSwallow()
        {
            for (int i = onSwallowBackPressListener.Count - 1; i >= 0; i--)
            {
                if (onSwallowBackPressListener[i] != null)
                {
                    onSwallowBackPressListener[i]();
                    return;
                }
            }
        }

        void CallbackAny()
        {
            foreach (Action action in onAnyBackPressAnyListener)
            {
                if (action != null)
                {
                    action();
                }
            }
        }

        void CleanListener()
        {
            Debug.LogError("BackPressEvent: Do Clean");
            for (int i = onSwallowBackPressListener.Count - 1; i >= 0; i--)
            {
                Debug.LogError("BackPressEvent: swallow i = " + i);
                if (!(onSwallowBackPressListener[i] != null))
                {
                    Debug.LogError("BackPressEvent: clean swallow at " + i);
                    onSwallowBackPressListener.RemoveAt(i);
                }
                else
                {
                    Debug.LogError("BackPressEvent: swallow i = " + i + " keep");
                }
            }

            for (int i = onAnyBackPressAnyListener.Count - 1; i >= 0; i--)
            {
                Debug.LogError("BackPressEvent: any i = " + i);
                if (!(onAnyBackPressAnyListener[i] != null))
                {
                    Debug.LogError("BackPressEvent: clean any at " + i);
                    onAnyBackPressAnyListener.RemoveAt(i);
                }
                else
                {
                    Debug.LogError("BackPressEvent: any i = " + i + " keep");
                }
            }
        }
    }
}
