using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Gameplay.Level
{
    public class BurstSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        public SignalAssetEventPair[] signalAssetEventPairs;
        
        [Serializable]
        public class SignalAssetEventPair
        {
            public SignalAsset signalAsset;
            public ParameterizedEvent events;
 
            [Serializable]
            public class ParameterizedEvent : UnityEvent<BurstDefinition> { }
        }
 
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is BurstSignalEmitter burstEmitter)
            {
                var matches = signalAssetEventPairs.Where(x => ReferenceEquals(x.signalAsset, burstEmitter.asset));
                foreach (var m in matches)
                {
                    m.events.Invoke(burstEmitter.parameter);
                }
            }
        }
    }
}