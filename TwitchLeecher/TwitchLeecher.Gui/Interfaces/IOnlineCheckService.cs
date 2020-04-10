using TwitchLeecher.Core.Models;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Interfaces
{
    public interface IOnlineCheckService
    {
        OnlineCheckState OnlineCheckState { get; }

        void DaytimeTimersStart();

        void DaytimeTimersStop();

        void StartCheckOnlineStreams();

        void StopCheckOnlineStreams();

        void PerformUpdateOnlineStreams();

        bool StartDownloadOnlineStream(string id);
    }
}