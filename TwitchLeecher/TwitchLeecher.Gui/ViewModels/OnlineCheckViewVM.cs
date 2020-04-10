using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.ViewModels
{
    public class OnlineCheckViewVM : ViewModelBase, INavigationState
    {
        #region Fields

        private readonly ITwitchService _twitchService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationsService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IPreferencesService _preferencesService;
        private readonly IOnlineCheckService _onlineCheckService;
        private readonly IFilenameService _filenameService;

        private readonly object _commandLockObject;

        private ICommand _openDownloadFolderCommand;
        private ICommand _updateOnlineStreamListCommand;
        private ICommand _downloadThisOnlineCommand;
        private ICommand _stopOnlineCheckCommand;
        private ICommand _startOnlineCheckCommand;

        private OnlineCheckState _onlineStreamState = OnlineCheckState.CheckOff;
        private string _onlineStreamsFullStatus;
        private string _channelsForCheck;
        
        #endregion Fields

        #region Constructors

        public OnlineCheckViewVM(
            ITwitchService twitchService,
            IDialogService dialogService,
            INavigationService navigationService,
            INotificationService notificationService,
            IEventAggregator eventAggregator,
            IPreferencesService preferencesService,
            IOnlineCheckService onlineCheckService,
            IFilenameService filenameService)
        {
            _twitchService = twitchService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _notificationsService = notificationService;
            _eventAggregator = eventAggregator;
            _preferencesService = preferencesService;
            _onlineCheckService = onlineCheckService;
            _filenameService = filenameService;

            _twitchService.PropertyChanged += TwitchService_PropertyChanged;
            
            _eventAggregator.GetEvent<PreferencesSavedEvent>().Subscribe(PreferencesSaved);
            _eventAggregator.GetEvent<OnlineCheckStatusChangedEvent>().Subscribe(OnlineCheckStatusChanged);
            _eventAggregator.GetEvent<DownloadsCountChangedEvent>().Subscribe(DownloadsCountChanged);
            
            PreferencesSaved();

            UpdateOnlineStreamStatus();

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public double ScrollPosition { get; set; }

        public string OnlineStreamsFullStatus
        {
            get
            {
                return _onlineStreamsFullStatus;
            }
            private set
            {
                SetProperty(ref _onlineStreamsFullStatus, value, nameof(OnlineStreamsFullStatus));
            }
        }

        public string ChannelsForCheck
        {
            get
            {
                return _channelsForCheck;
            }
            set
            {
                SetProperty(ref _channelsForCheck, value, nameof(ChannelsForCheck));
            }
        }
        
        public ObservableCollection<TwitchVideo> OnlineStreams
        {
            get
            {
                return _twitchService.OnlineStreams;
            }
        }

        public ObservableCollection<string> CurrentDownloadsIds
        {
            get
            {
                var onlineDownloading = _twitchService.Downloads.Where(x => x.DownloadParams.StreamingNow == true && x.IsDownloadingOrWill);
                return new ObservableCollection<string>(onlineDownloading.Select(x => x.DownloadParams.Video.Id).Distinct());
            }
        }

        public string CurrentDownloadOnlineStreamsChannels
        {
            get
            {
                var onlineDownloading = _twitchService.Downloads.Where(x => x.DownloadParams.StreamingNow == true && x.IsDownloadingOrWill);
                if (onlineDownloading.Any())
                {
                    return $"'{string.Join("', '", onlineDownloading.Select(x => x.DownloadParams.Video.Channel).Distinct())}'";
                }
                else
                    return "";
            }
        }

        public ICommand OpenDownloadFolderCommand
        {
            get
            {
                if (_openDownloadFolderCommand == null)
                {
                    _openDownloadFolderCommand = new DelegateCommand(OpenDownloadFolder);
                }

                return _openDownloadFolderCommand;
            }
        }

        public ICommand UpdateOnlineStreamListCommand
        {
            get
            {
                if (_updateOnlineStreamListCommand == null)
                {
                    _updateOnlineStreamListCommand = new DelegateCommand(UpdateOnlineStreamList);
                }

                return _updateOnlineStreamListCommand;
            }
        }

        public ICommand DownloadThisOnlineCommand
        {
            get
            {
                if (_downloadThisOnlineCommand == null)
                {
                    _downloadThisOnlineCommand = new DelegateCommand<string>(DownloadThisOnline);
                }

                return _downloadThisOnlineCommand;
            }
        }

        public ICommand StopOnlineCheckCommand
        {
            get
            {
                if (_stopOnlineCheckCommand == null)
                {
                    _stopOnlineCheckCommand = new DelegateCommand(StopOnlineCheck);
                }

                return _stopOnlineCheckCommand;
            }
        }

        public ICommand StartOnlineCheckCommand
        {
            get
            {
                if (_startOnlineCheckCommand == null)
                {
                    _startOnlineCheckCommand = new DelegateCommand(StartOnlineCheck);
                }

                return _startOnlineCheckCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void OpenDownloadFolder()
        {
            try
            {
                lock (_commandLockObject)
                {
                    string folder = _preferencesService.CurrentPreferences.OnlineCheckDownloadFolder;

                    if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
                    {
                        Process.Start(folder);
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void UpdateOnlineStreamList()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _onlineCheckService.PerformUpdateOnlineStreams();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void DownloadThisOnline(string id)
        {
            try
            {
                lock (_commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        TwitchVideo video = OnlineStreams.FirstOrDefault(v => v.Id == id);

                        if (video != null)
                        {
                            _onlineCheckService.StartDownloadOnlineStream(video.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void StopOnlineCheck()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _onlineCheckService.StopCheckOnlineStreams();
                    RebuildMenu();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        public void StartOnlineCheck()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _onlineCheckService.StartCheckOnlineStreams();
                    RebuildMenu();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void OnlineCheckStatusChanged(OnlineCheckState checkState)
        {
            if ((checkState & OnlineCheckState.CheckOnOff) != (_onlineStreamState & OnlineCheckState.CheckOnOff))
            {
                _onlineStreamState = checkState;
                RebuildMenu();
            }
            else
            {
                _onlineStreamState = checkState;
            }
            UpdateOnlineStreamStatus();
        }

        private void DownloadsCountChanged(int downloadCount)
        {
            FirePropertyChanged(nameof(CurrentDownloadsIds));
            FirePropertyChanged(nameof(CurrentDownloadOnlineStreamsChannels));
        }

        private void UpdateOnlineStreamStatus()
        {
            string curCheckState = "";
            switch (_onlineStreamState & OnlineCheckState.CheckCurState)
            {
                case OnlineCheckState.CheckNow:
                    curCheckState = "Check"; break;
                case OnlineCheckState.CheckEnd:
                    curCheckState = "Check"; break;
                case OnlineCheckState.Download:
                    curCheckState = "Download"; break;
                case OnlineCheckState.Wait:
                    curCheckState = "Wait"; break;
                default:
                    curCheckState = ""; break;
            }

            if ((_onlineStreamState & OnlineCheckState.CheckOnOff) == OnlineCheckState.CheckOn)
            {
                OnlineStreamsFullStatus = $"Online check is On, it is {curCheckState}ing now. Found {OnlineStreams.Count} online streams.";
            }
            else if (curCheckState.Length > 1)
            {
                OnlineStreamsFullStatus = $"Online check is Off, but it is {curCheckState}ing now. Found {OnlineStreams.Count} online streams.";
            }
            else
            { 
                OnlineStreamsFullStatus = $"Online check is Off. Found {OnlineStreams.Count} online streams.";
            }
        }

        private void PreferencesSaved()
        {
            try
            {
                ChannelsForCheck = _preferencesService.CurrentPreferences.OnlineCheckChannels.Count == 0
                    ? "No channels"
                    : $"'{string.Join("', '", _preferencesService.CurrentPreferences.OnlineCheckChannels)}'";
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        protected override List<MenuCommand> BuildMenu()
        {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if (menuCommands == null)
            {
                menuCommands = new List<MenuCommand>();
            }

            if ((_onlineCheckService.OnlineCheckState & OnlineCheckState.CheckOn) == OnlineCheckState.CheckOn)
            {
                menuCommands.Add(new MenuCommand(StopOnlineCheckCommand, "Stop check", "Download", 230));
            }
            else
            {
                menuCommands.Add(new MenuCommand(StartOnlineCheckCommand, "Start check", "Download", 230));
                menuCommands.Add(new MenuCommand(UpdateOnlineStreamListCommand, "Update streams", "Search", 230));
            }
            menuCommands.Add(new MenuCommand(OpenDownloadFolderCommand, "Open Download Folder", "FolderOpen", 230));

            return menuCommands;
        }

        #endregion Methods

        #region EventHandlers

        private void TwitchService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;

            FirePropertyChanged(propertyName);

            if (propertyName.Equals(nameof(OnlineStreams)))
            {
                ScrollPosition = 0;

                UpdateOnlineStreamStatus();
            }
        }

        #endregion EventHandlers
    }
}