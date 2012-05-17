﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameTools.Events
{
    public class BasicEventManager : EventManager
    {
        private MultipriorityQueue<int, BaseGameEvent> eventQueue;
        private Dictionary<string, List<EventReaction>> subscriberDirectory;

        private int currentTime;

        public BasicEventManager()
        {
            eventQueue = new MultipriorityQueue<int, BaseGameEvent>();

            subscriberDirectory = new Dictionary<string, List<EventReaction>>();

            currentTime = 0;
        }

        public void Subscribe(EventReaction eventReaction, string eventType)
        {
            if(!subscriberDirectory.ContainsKey(eventType))
            {
                subscriberDirectory.Add(eventType, new List<EventReaction>());
            }

            subscriberDirectory[eventType].Add(eventReaction);
        }

        public void SendEvent(BaseGameEvent theEvent)
        {
            if(subscriberDirectory.ContainsKey(theEvent.EventType))
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
            foreach(EventReaction eventReaction in subscriberDirectory[theEvent.EventType])
            {
                if(eventReaction(theEvent))
                    break;
            }
        }
    }
}
