using System;
using GameTools;

namespace GameTools.Events
{
    public delegate bool EventReaction(BaseGameEvent theEvent);
    public interface EventManager
    {
        void Subscribe(object subscriber, EventReaction eventReaction, string eventType);
        void Unsubscribe(object subscriber, EventReaction eventReaction, string eventType);
        void UpsubscribeFromAll(object subscriber);                
        void SendEvent(BaseGameEvent theEvent);
        void SendDelayedEvents(int currentTime);
    }
}
