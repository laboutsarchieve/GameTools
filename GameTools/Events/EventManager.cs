using System;
using GameTools;

namespace GameTools.Events
{
    public delegate bool EventReaction(BaseGameEvent theEvent);
    public interface EventManager
    {
        void Subscribe(EventReaction eventReaction, string eventType);
        void SendEvent(BaseGameEvent theEvent);
        void SendDelayedEvents(int currentTime);
    }
}
