using TwitchLeecher.Shared.Events;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Core.Events
{
    public class OnlineCheckStatusChangedEvent : PubSubEvent<OnlineCheckState>
    {
    }
}