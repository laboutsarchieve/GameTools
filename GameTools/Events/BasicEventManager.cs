using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameTools.Events
{
    public class BasicEventManager : EventManager
    {
        private Dictionary<object, List<KeyValuePair<string, EventReaction>>> subscriberDirectory;
        private MultipriorityQueue<int, BaseGameEvent> eventQueue;
        private Dictionary<string, List<EventReaction>> reactionDirectory;

        private int currentTime;

        public BasicEventManager()
        {
            eventQueue = new MultipriorityQueue<int, BaseGameEvent>();
            reactionDirectory = new Dictionary<string, List<EventReaction>>();
            subscriberDirectory = new Dictionary<object, List<KeyValuePair<string, EventReaction>>>();

            currentTime = 0;
        }

        public void Subscribe(object subscriber, EventReaction eventReaction, string eventType)
        {
            if(!reactionDirectory.ContainsKey(eventType))
            {
                reactionDirectory.Add(eventType, new List<EventReaction>());
            }

            if(!subscriberDirectory.ContainsKey(subscriber))
            {
                subscriberDirectory.Add(subscriber, new List<KeyValuePair<string, EventReaction>>());
                subscriberDirectory[subscriber] = new List<KeyValuePair<string, EventReaction>>();
            }

            subscriberDirectory[subscriber].Add(new KeyValuePair<string, EventReaction>(eventType, eventReaction));

            reactionDirectory[eventType].Add(eventReaction);
        }
        public void Unsubscribe(object subscriber, EventReaction eventReaction, string eventType)
        {
            foreach(EventReaction reaction in reactionDirectory[eventType])
            {
                if(eventReaction == reaction)
                {                    
                    reactionDirectory[eventType].Remove(eventReaction);
                    subscriberDirectory[subscriber].Remove(new KeyValuePair<string,EventReaction>(eventType, eventReaction));
                    if(subscriberDirectory[subscriber].Count == 0)
                    { 
                        subscriberDirectory.Remove(subscriber);
                    }
                    
                    break;
                }
            }            
        }
        public void UpsubscribeFromAll(object subscriber)
        {
            while(subscriberDirectory.ContainsKey(subscriber))
            {
                KeyValuePair<string, EventReaction> reaction = subscriberDirectory[subscriber][0];
                Unsubscribe(subscriber, reaction.Value, reaction.Key);
            }
        }
        public void SendEvent(BaseGameEvent theEvent)
        {
            if(reactionDirectory.ContainsKey(theEvent.EventType))
            {
                if(theEvent.Delay == 0)
                    FireEvent(theEvent);
                else
                    eventQueue.Add(currentTime + theEvent.Delay, theEvent);
            }
        }
        public void Update(GameTime gameTime)
        {
            SendDelayedEvents(gameTime.ElapsedGameTime.Milliseconds);
        }
        public void SendDelayedEvents(int currentTime)
        {
            List<BaseGameEvent> eventsToFire;

            if(eventQueue.Count > 0)
            {
                eventsToFire = eventQueue.GetAndRemove(currentTime);

                foreach(BaseGameEvent theEvent in eventsToFire)
                {
                    FireEvent(theEvent);
                }
            }
        }
        private void FireEvent(BaseGameEvent theEvent)
        {
            foreach(EventReaction eventReaction in reactionDirectory[theEvent.EventType])
            {
                if(eventReaction(theEvent))
                    break;
            }
        }
    }
}
