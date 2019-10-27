﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable]
    public class UnityStateEvent : UnityEvent<string>
    {}
    
    public interface IStateManager
    {
        int State { set; }
        string[] StateTitles { get; }
        void Next();
        void Rewind();
    }
    
    public class StateManager : MonoBehaviour, IStateManager
    {
        [SerializeField] private string csvFileName = "qlist";
        [SerializeField] private UnityStateEvent onStateChanged;
        [SerializeField] private int sceneSelection;

        void Start()
        { 
            var content = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/" + csvFileName + ".csv");
            var lines  = content.Split("\n"[0]);
            StateTitles = new string[lines.Length - 1];
            for (var i = 1; i < lines.Length; ++i)
            {
                var columns = (lines[i].Trim()).Split(","[0]);
                StateTitles[i-1] = columns[0];
            }
        }
        public string[] StateTitles { get; private set; }

        public int State
        {
            set
            {
                sceneSelection = value;
                onStateChanged.Invoke(StateTitles[sceneSelection]);
            }
        }
        
        public void Next()
        {
            if (sceneSelection >= StateTitles.Length - 1) return;
            sceneSelection++;
            onStateChanged.Invoke(StateTitles[sceneSelection]);
        }

        public void Rewind()
        {
            if (sceneSelection == 0) return;
            sceneSelection = 0;
            onStateChanged.Invoke(StateTitles[sceneSelection]);
        }
    }

}


