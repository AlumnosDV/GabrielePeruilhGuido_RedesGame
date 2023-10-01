using System.Collections.Generic;

namespace RedesGame.Managers
{
    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<string, System.Action<object[]>> eventDictionary;

        protected override void Awake()
        {
            base.Awake();
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<string, System.Action<object[]>>();
            }
        }

        public static void StartListening(string eventName, System.Action<object[]> listener)
        {
            System.Action<object[]> thisEvent;
            if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent += listener;
                Instance.eventDictionary[eventName] = thisEvent;
            }
            else
            {
                thisEvent += listener;
                Instance.eventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(string eventName, System.Action<object[]> listener)
        {
            if (Instance == null) return;
            System.Action<object[]> thisEvent;
            if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent -= listener;
                Instance.eventDictionary[eventName] = thisEvent;
            }
        }

        public static void TriggerEvent(string eventName, params object[] data)
        {
            System.Action<object[]> thisEvent = null;
            if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent?.Invoke(data);
            }
        }

        private void OnDestroy()
        {
            eventDictionary?.Clear();
        }
    }
}
