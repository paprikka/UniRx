﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniRx
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        // Public Commands
        public static void Post(Action item)
        {
            lock (gate)
            {
                Instance.actionList.Add(item);
            }
        }

        new public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return Instance.StartCoroutine_Auto(routine);
        }


        static object gate = new object();
        List<Action> actionList = new List<Action>();

        static MainThreadDispatcher instance;
        static bool initialized;

        private MainThreadDispatcher()
        {

        }

        static MainThreadDispatcher Instance
        {
            get
            {
                Initialize();
                return instance;
            }
        }

        public static void Initialize()
        {
            if (!initialized)
            {
                if (!Application.isPlaying) return;
                initialized = true;
                instance = new GameObject("MainThreadDispatcher").AddComponent<MainThreadDispatcher>();
                DontDestroyOnLoad(instance);
            }
        }

        void Awake()
        {
            instance = this;
            initialized = true;
        }

        void Update()
        {
            Action[] actions;
            lock (gate)
            {
                if (actionList.Count == 0) return;

                actions = actionList.ToArray();
                actionList.Clear();
            }
            foreach (var action in actions)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex); // Is log can't handle...?
                }
            }
        }

        // for Lifecycle Management

        Subject<bool> onApplicationFocus;
        void OnApplicationFocus(bool focus)
        {
            if (onApplicationFocus != null) onApplicationFocus.OnNext(focus);
        }
        public static IObservable<bool> OnApplicationFocusAsObservable()
        {
            return Instance.onApplicationFocus ?? (Instance.onApplicationFocus = new Subject<bool>());
        }

        Subject<bool> onApplicationPause;
        void OnApplicationPause(bool pause)
        {
            if (onApplicationPause != null) onApplicationPause.OnNext(pause);
        }
        public static IObservable<bool> OnApplicationPauseAsObservable()
        {
            return Instance.onApplicationPause ?? (Instance.onApplicationPause = new Subject<bool>());
        }

        Subject<Unit> onApplicationQuit;
        void OnApplicationQuit()
        {
            if (onApplicationQuit != null) onApplicationQuit.OnNext(Unit.Default);
        }
        public static IObservable<Unit> OnApplicationQuitAsObservable()
        {
            return Instance.onApplicationQuit ?? (Instance.onApplicationQuit = new Subject<Unit>());
        }
    }
}