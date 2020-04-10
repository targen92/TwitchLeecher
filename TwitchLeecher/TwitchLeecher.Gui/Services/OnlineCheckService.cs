using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.Services
{
    internal class OnlineCheckService : IOnlineCheckService
    {

        #region Constants

        private readonly TimeSpan CHECK_TIMER_INTERVALL = TimeSpan.FromMinutes(5);

        #endregion Constants

        #region Fields

        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService _dialogService;
        private readonly ITwitchService _twitchService;
        private readonly INavigationService _navigationService;
        private readonly IPreferencesService _preferencesService;
        private readonly IFilenameService _filenameService;
        private readonly INotificationService _notificationService;
        
        private Timer startCheckTimer;
        private Timer endCheckTimer;
        private Timer checkOnlineTimer;

        private readonly object _checkOnlineListLockObject;
        private readonly object _onlineStateLockObject;

        private OnlineCheckState _onlineCheckState = OnlineCheckState.CheckOff;

        private int addedStreamCounter = -1;

        #endregion Fields

        #region Constructors

        public OnlineCheckService(
            IEventAggregator eventAggregator,
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            IPreferencesService preferencesService,
            IFilenameService filenameService,
            INotificationService notificationService)
        {
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _twitchService = twitchService;
            _navigationService = navigationService;
            _preferencesService = preferencesService;
            _filenameService = filenameService;
            _notificationService = notificationService;

            _eventAggregator.GetEvent<PreferencesSavedEvent>().Subscribe(PreferencesSaved);
            _eventAggregator.GetEvent<DownloadsCountChangedEvent>().Subscribe(DownloadsCountChanged);

            _checkOnlineListLockObject = new object();
            _onlineStateLockObject = new object();

            startCheckTimer = new Timer(StartCheckTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            endCheckTimer = new Timer(EndCheckTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            checkOnlineTimer = new Timer(CheckOnlineTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion Constructors

        #region Properties

        public OnlineCheckState OnlineCheckState
        {
            get
            {
                return _onlineCheckState;
            }
            private set
            {
                if (_onlineCheckState != value)
                {
                    _onlineCheckState = value;
                    _eventAggregator.GetEvent<OnlineCheckStatusChangedEvent>().Publish(_onlineCheckState);
                }
            }
        }
        #endregion Properties

        #region Methods

        public void DaytimeTimersStart()
        {
            var nowTime = DateTime.Now - DateTime.Today;
            var startTime = _preferencesService.CurrentPreferences.OnlineCheckStartDaytime;
            var endTime = _preferencesService.CurrentPreferences.OnlineCheckEndDaytime;

            if (startTime < nowTime)
            {
                startTime = startTime.Add(new TimeSpan(1, 0, 0, 0));
            }
            if (endTime < nowTime)
            {
                endTime = endTime.Add(new TimeSpan(1, 0, 0, 0));
            }

            startCheckTimer.Change(startTime - nowTime, Timeout.InfiniteTimeSpan);
            endCheckTimer.Change(endTime - nowTime, Timeout.InfiniteTimeSpan);
        }

        public void DaytimeTimersStop()
        {
            startCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
            endCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void StartCheckOnlineStreams()
        {
            SetOnlineCheckState(true);
            if (_preferencesService.CurrentPreferences.OnlineCheckStopAfterStreams > 0)
            {
                addedStreamCounter = _preferencesService.CurrentPreferences.OnlineCheckStopAfterStreams;
            }
            else
            {
                addedStreamCounter = -1;
            }
            checkOnlineTimer.Change(TimeSpan.Zero, CHECK_TIMER_INTERVALL);
        }

        public void StopCheckOnlineStreams()
        {
            if ((OnlineCheckState & OnlineCheckState.CheckCurState) == OnlineCheckState.Wait)
            {
                SetOnlineCheckState(OnlineCheckState.CheckNoState, false);
            }
            else
            {
                SetOnlineCheckState(false);
            }
            checkOnlineTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void PerformUpdateOnlineStreams()
        {
            PerformUpdateOnlineStreams(true, true);
        }

        public bool StartDownloadOnlineStream(string id)
        {
            var onlineStream = _twitchService.OnlineStreams.FirstOrDefault(x => x.Id == id);
            if (onlineStream == null)
            {
                return false;
            }

            var downloadParams = GetDownloadParametersFromVideo(onlineStream);
            if (downloadParams == null)
            {
                return false;
            }

            if (downloadParams.AutoSplit && downloadParams.AutoSplitTime.TotalSeconds > Preferences.MIN_SPLIT_LENGTH)
            {
                string baseFolder = Path.GetDirectoryName(downloadParams.FullPath);
                string baseFilename = Path.GetFileName(downloadParams.FullPath);

                var splitTimes = TwitchVideo.GetListOfSplitTimes(downloadParams.Video.Length, null, null, downloadParams.AutoSplitTime, downloadParams.AutoSplitOverlap);
                foreach (var splitPair in splitTimes)
                {
                    string tempFilename = splitPair.Item2.HasValue 
                        ? _filenameService.SubstituteWildcards(baseFilename, baseFolder, _twitchService.IsFileNameUsed, downloadParams.Video, downloadParams.Quality, splitPair.Item1, splitPair.Item2)
                        : baseFilename;
                    DownloadParameters tempParams = new DownloadParameters(downloadParams.Video, downloadParams.VodAuthInfo, downloadParams.Quality, baseFolder, tempFilename, downloadParams.DisableConversion, false, new TimeSpan(), 0);
                    tempParams.StreamingNow = false;
                    tempParams.AutoSplit = false;
                    tempParams.CropStart = splitPair.Item1.HasValue;
                    tempParams.CropStartTime = splitPair.Item1 ?? new TimeSpan();
                    tempParams.CropEnd = splitPair.Item2.HasValue;
                    tempParams.CropEndTime = splitPair.Item2 ?? downloadParams.Video.Length;
                    if (downloadParams.StreamingNow && !tempParams.CropEnd)
                    {
                        tempParams.StreamingNow = true;
                        tempParams.AutoSplit = true;
                        tempParams.AutoSplitOverlap = downloadParams.AutoSplitOverlap;
                        tempParams.AutoSplitTime = downloadParams.AutoSplitTime;
                    }
                    Application.Current.Dispatcher.Invoke(() => _twitchService.Enqueue(tempParams));
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => _twitchService.Enqueue(downloadParams));
            }
            return true;
        }

        private void PerformUpdateOnlineStreams(bool onlyCheckWithoutDownload, bool showLoadingScreen)
        {
            SetOnlineCheckState(OnlineCheckState.CheckNow);

            if (showLoadingScreen)
            {
                _navigationService.ShowLoading();
            }

            Task searchTask = new Task(() => _twitchService.UpdateOnlineChannels());

            searchTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _dialogService.ShowAndLogException(task.Exception);
                }

                if (showLoadingScreen && _preferencesService.CurrentPreferences.OnlineCheckUse)
                {
                    if (_navigationService.IsNowLoading)
                    {
                        _navigationService.ShowOnlineCheck();
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => _notificationService.ShowNotification("Online streams updated"));
                    }
                }

                if ((OnlineCheckState & OnlineCheckState.CheckOnOff) == OnlineCheckState.CheckOff)
                {
                    SetOnlineCheckState(OnlineCheckState.CheckNoState);
                }
                else
                {
                    SetOnlineCheckState(OnlineCheckState.CheckEnd);

                    if (!onlyCheckWithoutDownload && !task.IsFaulted)
                    {
                        StartDownloadAfterCheck();
                    }
                }

            });

            searchTask.Start();
        }

        private void StartDownloadAfterCheck()
        {
            lock (_checkOnlineListLockObject)
            {
                if (!_preferencesService.CurrentPreferences.OnlineCheckUse)
                {
                    return;
                }

                var onlineStreams = _twitchService.OnlineStreams.ToList();
                if (_preferencesService.CurrentPreferences.OnlineCheckDownloadOnlyNew)
                {
                    onlineStreams.RemoveAll(x => x.Length.TotalMinutes > 10);
                }

                var onlineStreamDownloads = _twitchService.Downloads.Where(x => x.IsDownloadingOrWill && x.DownloadParams.StreamingNow);

                onlineStreams.RemoveAll(x => onlineStreamDownloads.Any(y => x.Id == y.DownloadParams.Video.Id));

                if (onlineStreams.Count > 0)
                {
                    SetOnlineCheckState(OnlineCheckState.Download);
                    foreach (var onlineStream in onlineStreams)
                    {
                        StartDownloadOnlineStream(onlineStream.Id);

                        if (addedStreamCounter > 0)
                        {
                            addedStreamCounter--;
                            if (addedStreamCounter == 0)
                            {
                                StopCheckOnlineStreams();
                            }
                        }
                    }
                    return;
                }
                else
                {
                    SetOnlineCheckState(OnlineCheckState.Wait);
                    return;
                }
            }
        }

        private void PreferencesSaved()
        {
            try
            {
                if (!_preferencesService.CurrentPreferences.OnlineCheckUse)
                {
                    DaytimeTimersStop();
                    SetOnlineCheckState(OnlineCheckState.CheckNoState, false);
                    return;
                }
                if (_preferencesService.CurrentPreferences.OnlineCheckAutoStartEnd)
                {
                    DaytimeTimersStart();
                }
                else
                {
                    DaytimeTimersStop();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void DownloadsCountChanged(int downloadCount)
        {
            try
            {
                if (!_twitchService.Downloads.Any(x=>x.IsDownloadingOrWill && x.DownloadParams.StreamingNow))
                {
                    if ((OnlineCheckState & OnlineCheckState.CheckCurState) == OnlineCheckState.Download)
                    {
                        if ((OnlineCheckState & OnlineCheckState.CheckOnOff) == OnlineCheckState.CheckOff)
                        {
                            SetOnlineCheckState(OnlineCheckState.CheckNoState);
                        }
                        else
                        {
                            SetOnlineCheckState(OnlineCheckState.Wait);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }
        
        private void CheckOnlineTimerCallback(object state)
        {
            PerformUpdateOnlineStreams(false, false);
        }

        private void StartCheckTimerCallback(object state)
        {
            StartCheckOnlineStreams();
            Application.Current.Dispatcher.Invoke(() => _notificationService.ShowNotification("Online stream check started"));
        }

        private void EndCheckTimerCallback(object state)
        {
            StopCheckOnlineStreams();
            Application.Current.Dispatcher.Invoke(() => _notificationService.ShowNotification("Online stream check stopped"));
        }

        private DownloadParameters GetDownloadParametersFromVideo(TwitchVideo video)
        {
            if (video == null)
            {
                return null;
            }

            VodAuthInfo vodAuthInfo = _twitchService.RetrieveVodAuthInfo(video.Id);

            if (!vodAuthInfo.Privileged && vodAuthInfo.SubOnly)
            {
                if (!_twitchService.IsAuthorized)
                {
                    _dialogService.ShowMessageBox("This video is sub-only! Please authorize your Twitch account by clicking the Twitch button in the menu.", "SUB HYPE!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    _dialogService.ShowMessageBox("This video is sub-only but you are not subscribed to '" + video.Channel + "'!", "SUB HYPE!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                return null;
            }

            Preferences currentPrefs = _preferencesService.CurrentPreferences.Clone();

            string folder = currentPrefs.OnlineCheckDownloadFolder;

            string filename = currentPrefs.OnlineCheckDownloadFileName;
            filename = _filenameService.EnsureExtension(filename, currentPrefs.DownloadDisableConversion);

            if (currentPrefs.DownloadDisableConversion)
                currentPrefs.OnlineCheckUseAutoSplit = false;

            if (currentPrefs.OnlineCheckUseAutoSplit)
            {//Keep UNIQNUMBER wildcard in name to split file (only in conversation mode)
                string tempUniqWildcard = FilenameWildcards.UNIQNUMBER.Insert(FilenameWildcards.UNIQNUMBER.Length - 1, "_TEMP");
                filename = filename.Replace(FilenameWildcards.UNIQNUMBER, tempUniqWildcard);
                filename = _filenameService.SubstituteWildcards(filename, folder, _twitchService.IsFileNameUsed, video);
                filename = filename.Replace(tempUniqWildcard, FilenameWildcards.UNIQNUMBER);
            }
            else
                filename = _filenameService.SubstituteWildcards(filename, folder, _twitchService.IsFileNameUsed, video);

            TwitchVideoQuality shouldQualityOrNull = TwitchVideoQuality.TryFindQuality(video.Qualities, currentPrefs.OnlineCheckDownloadQuality);

            DownloadParameters downloadParams = new DownloadParameters(video, vodAuthInfo, shouldQualityOrNull, folder, filename, currentPrefs.DownloadDisableConversion,
                currentPrefs.OnlineCheckUseAutoSplit, currentPrefs.OnlineCheckSplitTime, currentPrefs.OnlineCheckSplitOverlapSeconds);

            downloadParams.StreamingNow = true;
            if (currentPrefs.OnlineCheckUseAutoSplit)
            {
                downloadParams.AutoSplit = true;
            }

            return downloadParams;
        }

        private void SetOnlineCheckState(OnlineCheckState stateWithoutOnOff, bool isCheckOn)
        {
            OnlineCheckState willState = (stateWithoutOnOff & OnlineCheckState.CheckCurState) | (isCheckOn ? OnlineCheckState.CheckOn : OnlineCheckState.CheckOff);
            if (OnlineCheckState != willState)
            {
                lock (_onlineStateLockObject)
                {
                    OnlineCheckState = willState;
                }
            }
        }

        private void SetOnlineCheckState(OnlineCheckState stateWithoutOnOff)
        {
            OnlineCheckState willState = (stateWithoutOnOff & OnlineCheckState.CheckCurState) | (OnlineCheckState & OnlineCheckState.CheckOnOff);
            if (OnlineCheckState != willState)
            {
                lock (_onlineStateLockObject)
                {
                    OnlineCheckState = willState;
                }
            }
        }

        private void SetOnlineCheckState(bool isCheckOn)
        {
            OnlineCheckState willState = (OnlineCheckState & OnlineCheckState.CheckCurState) | (isCheckOn ? OnlineCheckState.CheckOn : OnlineCheckState.CheckOff);
            if (OnlineCheckState != willState)
            {
                lock (_onlineStateLockObject)
                {
                    OnlineCheckState = willState;
                }
            }
        }
        #endregion Methods
    }
}