using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.Helpers;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class Preferences : BindableBase
    {
        static public int MIN_SPLIT_LENGTH { get { return 120; } }//in seconds
        //At least 60 seconds
        static public int TIMER_STREAMINGNOW_INTERVAL_MIN { get { return 5; } }//in minutes
        static public int CHECK_NEW_PARTS_COUNT { get { return 2; } }

        #region Fields

        private Version _version;

        private bool _appCheckForUpdates;

        private bool _appShowDonationButton;

        private RangeObservableCollection<string> _searchFavouriteChannels;

        private string _searchChannelName;

        private VideoType _searchVideoType;

        private LoadLimitType _searchLoadLimitType;

        private int _searchLoadLastDays;

        private int _searchLoadLastVods;

        private bool _searchOnStartup;

        private string _downloadTempFolder;

        private string _downloadFolder;

        private string _downloadFileName;

        private VideoQuality _downloadQuality;

        private bool _downloadSubfoldersForFav;

        private bool _downloadRemoveCompleted;

        private bool _downloadDisableConversion;
        
        private bool _downloadSplitUse;

        private TimeSpan _downloadSplitTime;

        private int _splitOverlapSeconds;

        private bool _downloadAndConcatSimultaneously;

        private bool _onlineCheckUse;

        private string _onlineCheckChannelName;

        private RangeObservableCollection<string> _onlineCheckChannels;

        private bool _onlineCheckAutoStartEnd;

        private TimeSpan _onlineCheckStartDaytime;

        private TimeSpan _onlineCheckEndDaytime;

        private bool _onlineCheckStartOnStartup;

        private bool _onlineCheckDownloadOnlyNew;

        private VideoQuality _onlineCheckDownloadQuality;

        private string _onlineCheckDownloadFolder;

        private string _onlineCheckDownloadFileName;

        private bool _onlineCheckUseAutoSplit;

        private TimeSpan _onlineCheckSplitTime;

        private int _onlineCheckSplitOverlapSeconds;

        private bool _onlineCheckAddProgrammToAutoload;//TODO FEATURE

        private int _onlineCheckStopAfterStreams;

        private int _miscRetryOnErrorInstantCount;

        private int _miscRetryOnErrorPauseCount;

        private bool _miscUseExternalPlayer;

        private string _miscExternalPlayer;

        #endregion Fields

        #region Properties

        public Version Version
        {
            get
            {
                return _version;
            }
            set
            {
                SetProperty(ref _version, value);
            }
        }

        public bool AppCheckForUpdates
        {
            get
            {
                return _appCheckForUpdates;
            }
            set
            {
                SetProperty(ref _appCheckForUpdates, value);
            }
        }

        public bool AppShowDonationButton
        {
            get
            {
                return _appShowDonationButton;
            }
            set
            {
                SetProperty(ref _appShowDonationButton, value);
            }
        }
        
        public int MiscRetryOnErrorInstantCount
        {
            get
            {
                return _miscRetryOnErrorInstantCount;
            }
            set
            {
                SetProperty(ref _miscRetryOnErrorInstantCount, value);
            }
        }

        public int MiscRetryOnErrorPauseCount
        {
            get
            {
                return _miscRetryOnErrorPauseCount;
            }
            set
            {
                SetProperty(ref _miscRetryOnErrorPauseCount, value);
            }
        }

        public bool MiscUseExternalPlayer
        {
            get
            {
                return _miscUseExternalPlayer;
            }
            set
            {
                SetProperty(ref _miscUseExternalPlayer, value);
            }
        }

        public string MiscExternalPlayer
        {
            get
            {
                return _miscExternalPlayer;
            }

            set
            {
                SetProperty(ref _miscExternalPlayer, value);
            }
        }

        public RangeObservableCollection<string> SearchFavouriteChannels
        {
            get
            {
                if (_searchFavouriteChannels == null)
                {
                    _searchFavouriteChannels = new RangeObservableCollection<string>();
                }

                return _searchFavouriteChannels;
            }
        }

        public string SearchChannelName
        {
            get
            {
                return _searchChannelName;
            }
            set
            {
                SetProperty(ref _searchChannelName, value);
            }
        }

        public VideoType SearchVideoType
        {
            get
            {
                return _searchVideoType;
            }
            set
            {
                SetProperty(ref _searchVideoType, value);
            }
        }

        public LoadLimitType SearchLoadLimitType
        {
            get
            {
                return _searchLoadLimitType;
            }
            set
            {
                SetProperty(ref _searchLoadLimitType, value);
            }
        }

        public int SearchLoadLastDays
        {
            get
            {
                return _searchLoadLastDays;
            }
            set
            {
                SetProperty(ref _searchLoadLastDays, value);
            }
        }

        public int SearchLoadLastVods
        {
            get
            {
                return _searchLoadLastVods;
            }
            set
            {
                SetProperty(ref _searchLoadLastVods, value);
            }
        }

        public bool SearchOnStartup
        {
            get
            {
                return _searchOnStartup;
            }
            set
            {
                SetProperty(ref _searchOnStartup, value);
            }
        }

        public string DownloadTempFolder
        {
            get
            {
                return _downloadTempFolder;
            }
            set
            {
                SetProperty(ref _downloadTempFolder, value);
            }
        }

        public string DownloadFolder
        {
            get
            {
                return _downloadFolder;
            }
            set
            {
                SetProperty(ref _downloadFolder, value);
            }
        }

        public string DownloadFileName
        {
            get
            {
                return _downloadFileName;
            }
            set
            {
                SetProperty(ref _downloadFileName, value);
            }
        }

        public VideoQuality DownloadQuality
        {
            get
            {
                return _downloadQuality;
            }
            set
            {
                SetProperty(ref _downloadQuality, value);
            }
        }

        public bool DownloadSubfoldersForFav
        {
            get
            {
                return _downloadSubfoldersForFav;
            }
            set
            {
                SetProperty(ref _downloadSubfoldersForFav, value);
            }
        }

        public bool DownloadRemoveCompleted
        {
            get
            {
                return _downloadRemoveCompleted;
            }
            set
            {
                SetProperty(ref _downloadRemoveCompleted, value);
            }
        }

        public bool DownloadDisableConversion
        {
            get
            {
                return _downloadDisableConversion;
            }
            set
            {
                SetProperty(ref _downloadDisableConversion, value);
            }
        }

        public bool DownloadAndConcatSimultaneously
        {
            get
            {
                return _downloadAndConcatSimultaneously;
            }
            set
            {
                SetProperty(ref _downloadAndConcatSimultaneously, value);
            }
        }

        public bool DownloadSplitUse
        {
            get
            {
                return _downloadSplitUse;
            }
            set
            {
                SetProperty(ref _downloadSplitUse, value);
            }
        }

        public TimeSpan DownloadSplitTime
        {
            get
            {
                return _downloadSplitTime;
            }
            set
            {
                SetProperty(ref _downloadSplitTime, value);
            }
        }

        public int SplitOverlapSeconds
        {
            get
            {
                return _splitOverlapSeconds;
            }
            set
            {
                SetProperty(ref _splitOverlapSeconds, value);
            }
        }

        public bool OnlineCheckUse
        {
            get
            {
                return _onlineCheckUse;
            }
            set
            {
                SetProperty(ref _onlineCheckUse, value);
            }
        }

        public string OnlineCheckChannelName
        {
            get
            {
                return _onlineCheckChannelName;
            }
            set
            {
                SetProperty(ref _onlineCheckChannelName, value);
            }
        }

        public RangeObservableCollection<string> OnlineCheckChannels
        {
            get
            {
                if (_onlineCheckChannels == null)
                {
                    _onlineCheckChannels = new RangeObservableCollection<string>();
                }

                return _onlineCheckChannels;
            }
        }

        public bool OnlineCheckStartOnStartup
        {
            get
            {
                return _onlineCheckStartOnStartup;
            }
            set
            {
                SetProperty(ref _onlineCheckStartOnStartup, value);
            }
        }

        public bool OnlineCheckDownloadOnlyNew
        {
            get
            {
                return _onlineCheckDownloadOnlyNew;
            }
            set
            {
                SetProperty(ref _onlineCheckDownloadOnlyNew, value);
            }
        }

        public bool OnlineCheckAutoStartEnd
        {
            get
            {
                return _onlineCheckAutoStartEnd;
            }
            set
            {
                SetProperty(ref _onlineCheckAutoStartEnd, value);
            }
        }

        public TimeSpan OnlineCheckStartDaytime
        {
            get
            {
                return _onlineCheckStartDaytime;
            }
            set
            {
                SetProperty(ref _onlineCheckStartDaytime, value);
            }
        }

        public TimeSpan OnlineCheckEndDaytime
        {
            get
            {
                return _onlineCheckEndDaytime;
            }
            set
            {
                SetProperty(ref _onlineCheckEndDaytime, value);
            }
        }

        public VideoQuality OnlineCheckDownloadQuality
        {
            get
            {
                return _onlineCheckDownloadQuality;
            }
            set
            {
                SetProperty(ref _onlineCheckDownloadQuality, value);
            }
        }

        public string OnlineCheckDownloadFolder
        {
            get
            {
                return _onlineCheckDownloadFolder;
            }
            set
            {
                SetProperty(ref _onlineCheckDownloadFolder, value);
            }
        }

        public string OnlineCheckDownloadFileName
        {
            get
            {
                return _onlineCheckDownloadFileName;
            }
            set
            {
                SetProperty(ref _onlineCheckDownloadFileName, value);
            }
        }

        public bool OnlineCheckUseAutoSplit
        {
            get
            {
                return _onlineCheckUseAutoSplit;
            }
            set
            {
                SetProperty(ref _onlineCheckUseAutoSplit, value);
            }
        }

        public TimeSpan OnlineCheckSplitTime
        {
            get
            {
                return _onlineCheckSplitTime;
            }
            set
            {
                SetProperty(ref _onlineCheckSplitTime, value);
            }
        }

        public int OnlineCheckSplitOverlapSeconds
        {
            get
            {
                return _onlineCheckSplitOverlapSeconds;
            }
            set
            {
                SetProperty(ref _onlineCheckSplitOverlapSeconds, value);
            }
        }

        public bool OnlineCheckAddProgrammToAutoload
        {
            get
            {
                return _onlineCheckAddProgrammToAutoload;
            }
            set
            {
                SetProperty(ref _onlineCheckAddProgrammToAutoload, value);
            }
        }

        public int OnlineCheckStopAfterStreams
        {
            get
            {
                return _onlineCheckStopAfterStreams;
            }
            set
            {
                SetProperty(ref _onlineCheckStopAfterStreams, value);
            }
        }

        #endregion Properties

        #region Methods

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(MiscExternalPlayer);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (MiscUseExternalPlayer)
                {
                    if (string.IsNullOrWhiteSpace(_miscExternalPlayer))
                    {
                        AddError(currentProperty, "Please specify an external player!");
                    }
                    else if (!_miscExternalPlayer.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        AddError(currentProperty, "Filename must be an executable!");
                    }
                    else if (!File.Exists(_miscExternalPlayer))
                    {
                        AddError(currentProperty, "The specified file does not exist!");
                    }
                }
            }

            currentProperty = nameof(MiscRetryOnErrorInstantCount);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_miscRetryOnErrorInstantCount < 0 || _miscRetryOnErrorInstantCount > 99)
                {
                    AddError(currentProperty, "Value has to be between 0 and 99!");
                }
            }

            currentProperty = nameof(MiscRetryOnErrorPauseCount);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_miscRetryOnErrorPauseCount < 0 || _miscRetryOnErrorPauseCount > 99)
                {
                    AddError(currentProperty, "Value has to be between 0 and 99!");
                }
            }

            currentProperty = nameof(SearchChannelName);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_searchOnStartup && string.IsNullOrWhiteSpace(_searchChannelName))
                {
                    AddError(currentProperty, "If 'Search on Startup' is enabled, you need to specify a default channel name!");
                }
            }

            currentProperty = nameof(SearchLoadLastDays);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_searchLoadLimitType == LoadLimitType.Timespan && (_searchLoadLastDays < 1 || _searchLoadLastDays > 999))
                {
                    AddError(currentProperty, "Value has to be between 1 and 999!");
                }
            }

            currentProperty = nameof(SearchLoadLastVods);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_searchLoadLimitType == LoadLimitType.LastVods && (_searchLoadLastVods < 1 || _searchLoadLastVods > 999))
                {
                    AddError(currentProperty, "Value has to be between 1 and 999!");
                }
            }

            currentProperty = nameof(DownloadTempFolder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(DownloadTempFolder))
                {
                    AddError(currentProperty, "Please specify a temporary download folder!");
                }
            }

            currentProperty = nameof(DownloadFolder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_downloadFolder))
                {
                    AddError(currentProperty, "Please specify a default download folder!");
                }
            }

            currentProperty = nameof(DownloadFileName);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_downloadFileName))
                {
                    AddError(currentProperty, "Please specify a default download filename!");
                }
                else if (_downloadFileName.Contains(".") || FileSystem.FilenameContainsInvalidChars(_downloadFileName))
                {
                    string invalidChars = new string(Path.GetInvalidFileNameChars());

                    AddError(currentProperty, $"Filename contains invalid characters ({invalidChars}.)!");
                }
            }

            currentProperty = nameof(DownloadSplitUse);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_downloadSplitUse && !_downloadDisableConversion && !Regex.IsMatch(_downloadFileName, FilenameWildcards.UNIQNUMBER_REGEX))
                {
                    string errorMessage = $"With autosplit option enabled, download file name has to contain {FilenameWildcards.UNIQNUMBER_REGEX_EXAMPLE1} or {FilenameWildcards.UNIQNUMBER_REGEX_EXAMPLE2} for autonaming!";
                    AddError(currentProperty, errorMessage);
                    AddError(nameof(DownloadFileName), errorMessage);
                }
            }

            currentProperty = nameof(DownloadSplitTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_downloadSplitUse && !_downloadDisableConversion && _downloadSplitTime.TotalSeconds < Preferences.MIN_SPLIT_LENGTH)
                {
                    string errorMessage = $"Split time has to be equal or more {Preferences.MIN_SPLIT_LENGTH} seconds!";
                    AddError(currentProperty, errorMessage);
                    AddError(nameof(DownloadSplitUse), errorMessage);
                }
            }

            currentProperty = nameof(SplitOverlapSeconds);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_downloadSplitUse && !_downloadDisableConversion && (_splitOverlapSeconds >= Preferences.MIN_SPLIT_LENGTH / 2 || _splitOverlapSeconds < 0))
                {
                    string errorMessage = $"Overlap seconds has to be less than {Preferences.MIN_SPLIT_LENGTH / 2} seconds!";
                    AddError(currentProperty, errorMessage);
                    AddError(nameof(DownloadSplitUse), errorMessage);
                }
            }

            if (_onlineCheckUse)
            {
                currentProperty = nameof(OnlineCheckChannels);

                if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
                {
                    if (_onlineCheckStartOnStartup &&
                        (OnlineCheckChannels == null || OnlineCheckChannels.Count == 0 || OnlineCheckChannels.Any(x => string.IsNullOrWhiteSpace(x))))
                    {
                        string errorMessage = "If 'Check online on Startup' is enabled, you need to specify at least one channel name!";
                        AddError(currentProperty, errorMessage);
                        AddError(nameof(OnlineCheckStartOnStartup), errorMessage);
                    }
                }

                currentProperty = nameof(OnlineCheckAutoStartEnd);

                if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
                {
                    if (_onlineCheckAutoStartEnd)
                    {
                        if (_onlineCheckStartDaytime.TotalDays >= 1)
                        {
                            string errorMessage = "Start checktime has to be equal or less 23:59:59!";
                            AddError(currentProperty, errorMessage);
                            AddError(nameof(OnlineCheckStartDaytime), errorMessage);
                        }
                        if (_onlineCheckEndDaytime.TotalDays < 0)
                        {
                            string errorMessage = "Start checktime has to be greater than 0!";
                            AddError(currentProperty, errorMessage);
                            AddError(nameof(OnlineCheckStartDaytime), errorMessage);
                        }
                        if (_onlineCheckEndDaytime.TotalDays >= 1)
                        {
                            string errorMessage = "End checktime has to be equal or less 23:59:59!";
                            AddError(currentProperty, errorMessage);
                            AddError(nameof(OnlineCheckEndDaytime), errorMessage);
                        }
                        if (_onlineCheckEndDaytime.TotalDays < 0)
                        {
                            string errorMessage = "End checktime has to be greater than 0!";
                            AddError(currentProperty, errorMessage);
                            AddError(nameof(OnlineCheckEndDaytime), errorMessage);
                        }
                        if (_onlineCheckEndDaytime.Hours == _onlineCheckStartDaytime.Hours && _onlineCheckEndDaytime.Minutes == _onlineCheckStartDaytime.Minutes && _onlineCheckEndDaytime.Seconds == _onlineCheckStartDaytime.Seconds)
                        {
                            string errorMessage = "Start and End checktime has to be different!";
                            AddError(currentProperty, errorMessage);
                            AddError(nameof(OnlineCheckStartDaytime), errorMessage);
                            AddError(nameof(OnlineCheckEndDaytime), errorMessage);
                        }
                    }
                }

                currentProperty = nameof(OnlineCheckDownloadFolder);

                if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
                {
                    if (string.IsNullOrWhiteSpace(_onlineCheckDownloadFolder))
                    {
                        AddError(currentProperty, "Please specify a default download folder for online streams!");
                    }
                }

                currentProperty = nameof(OnlineCheckDownloadFileName);

                if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
                {
                    if (string.IsNullOrWhiteSpace(_onlineCheckDownloadFileName))
                    {
                        AddError(currentProperty, "Please specify a default download filename for online streams!");
                    }
                    else if (_onlineCheckDownloadFileName.Contains(".") || FileSystem.FilenameContainsInvalidChars(_onlineCheckDownloadFileName))
                    {
                        string invalidChars = new string(Path.GetInvalidFileNameChars());
                        AddError(currentProperty, $"Filename contains invalid characters ({invalidChars}.)!");
                    }
                    else if (!Regex.IsMatch(_onlineCheckDownloadFileName, FilenameWildcards.UNIQNUMBER_REGEX))
                    {
                        AddError(currentProperty, $"Download file name for online streams has to contain {FilenameWildcards.UNIQNUMBER_REGEX_EXAMPLE1} or {FilenameWildcards.UNIQNUMBER_REGEX_EXAMPLE2} for autonaming!");
                    }
                }

                currentProperty = nameof(OnlineCheckSplitTime);

                if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
                {
                    if (_onlineCheckUseAutoSplit && !_downloadDisableConversion && _onlineCheckSplitTime.TotalSeconds < Preferences.MIN_SPLIT_LENGTH)
                    {
                        string errorMessage = $"Split time has to be equal or more {Preferences.MIN_SPLIT_LENGTH} seconds!";
                        AddError(currentProperty, errorMessage);
                        AddError(nameof(OnlineCheckUseAutoSplit), errorMessage);
                    }
                }

                currentProperty = nameof(OnlineCheckSplitOverlapSeconds);

                if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
                {
                    if (_onlineCheckUseAutoSplit && !_downloadDisableConversion && (_onlineCheckSplitOverlapSeconds >= Preferences.MIN_SPLIT_LENGTH / 2 || _splitOverlapSeconds < 0))
                    {
                        string errorMessage = $"Overlap seconds has to be less than {Preferences.MIN_SPLIT_LENGTH / 2} seconds!";
                        AddError(currentProperty, errorMessage);
                        AddError(nameof(OnlineCheckUseAutoSplit), errorMessage);
                    }
                }

                currentProperty = nameof(OnlineCheckStopAfterStreams);

                if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
                {
                    if (_onlineCheckStopAfterStreams < 0)
                    {
                        string errorMessage = "Stop after strams count can't be less than 0!";
                        AddError(currentProperty, errorMessage);
                    }
                }
            }
        }

        public Preferences Clone()
        {
            Preferences clone = new Preferences()
            {
                Version = Version,
                AppCheckForUpdates = AppCheckForUpdates,
                AppShowDonationButton = AppShowDonationButton,
                MiscRetryOnErrorInstantCount = MiscRetryOnErrorInstantCount,
                MiscRetryOnErrorPauseCount = MiscRetryOnErrorPauseCount,
                MiscUseExternalPlayer = MiscUseExternalPlayer,
                MiscExternalPlayer = MiscExternalPlayer,
                SearchChannelName = SearchChannelName,
                SearchVideoType = SearchVideoType,
                SearchLoadLimitType = SearchLoadLimitType,
                SearchLoadLastDays = SearchLoadLastDays,
                SearchLoadLastVods = SearchLoadLastVods,
                SearchOnStartup = SearchOnStartup,
                DownloadTempFolder = DownloadTempFolder,
                DownloadFolder = DownloadFolder,
                DownloadFileName = DownloadFileName,
                DownloadQuality = DownloadQuality,
                DownloadSubfoldersForFav = DownloadSubfoldersForFav,
                DownloadRemoveCompleted = DownloadRemoveCompleted,
                DownloadDisableConversion = DownloadDisableConversion,
                DownloadAndConcatSimultaneously = DownloadAndConcatSimultaneously,
                DownloadSplitUse = DownloadSplitUse,
                DownloadSplitTime = DownloadSplitTime,
                SplitOverlapSeconds = SplitOverlapSeconds,
                OnlineCheckAddProgrammToAutoload = OnlineCheckAddProgrammToAutoload,
                OnlineCheckAutoStartEnd = OnlineCheckAutoStartEnd,
                OnlineCheckEndDaytime = OnlineCheckEndDaytime,
                OnlineCheckStartOnStartup = OnlineCheckStartOnStartup,
                OnlineCheckDownloadOnlyNew = OnlineCheckDownloadOnlyNew,
                OnlineCheckStartDaytime = OnlineCheckStartDaytime,
                OnlineCheckDownloadQuality = OnlineCheckDownloadQuality,
                OnlineCheckDownloadFolder = OnlineCheckDownloadFolder,
                OnlineCheckDownloadFileName = OnlineCheckDownloadFileName,
                OnlineCheckUse = OnlineCheckUse,
                OnlineCheckChannelName = OnlineCheckChannelName,
                OnlineCheckSplitTime = OnlineCheckSplitTime,
                OnlineCheckStopAfterStreams = OnlineCheckStopAfterStreams,
                OnlineCheckUseAutoSplit = OnlineCheckUseAutoSplit,
                OnlineCheckSplitOverlapSeconds = OnlineCheckSplitOverlapSeconds
            };

            clone.SearchFavouriteChannels.AddRange(SearchFavouriteChannels);
            clone.OnlineCheckChannels.AddRange(OnlineCheckChannels);

            return clone;
        }
        #endregion Methods
    }
}