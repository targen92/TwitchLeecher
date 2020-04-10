using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class PreferencesViewVM : ViewModelBase
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly IPreferencesService _preferencesService;

        private Preferences _currentPreferences;

        private ICommand _addFavouriteChannelCommand;
        private ICommand _removeFavouriteChannelCommand;
        private ICommand _chooseDownloadTempFolderCommand;
        private ICommand _chooseDownloadFolderCommand;
        private ICommand _useOnlineCheckChangeCommand;
        private ICommand _addOnlineCheckChannelCommand;
        private ICommand _removeOnlineCheckChannelCommand;
        private ICommand _chooseOnlineCheckDownloadFolderCommand;
        private ICommand _chooseExternalPlayerCommand;
        private ICommand _clearExternalPlayerCommand;
        private ICommand _saveCommand;
        private ICommand _undoCommand;
        private ICommand _defaultsCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public PreferencesViewVM(
            IDialogService dialogService,
            INotificationService notificationService,
            IPreferencesService preferencesService)
        {
            _dialogService = dialogService;
            _notificationService = notificationService;
            _preferencesService = preferencesService;

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public Preferences CurrentPreferences
        {
            get
            {
                if (_currentPreferences == null)
                {
                    _currentPreferences = _preferencesService.CurrentPreferences.Clone();
                }

                return _currentPreferences;
            }

            private set
            {
                SetProperty(ref _currentPreferences, value);
            }
        }

        public ICommand AddFavouriteChannelCommand
        {
            get
            {
                if (_addFavouriteChannelCommand == null)
                {
                    _addFavouriteChannelCommand = new DelegateCommand(AddFavouriteChannel);
                }

                return _addFavouriteChannelCommand;
            }
        }

        public ICommand RemoveFavouriteChannelCommand
        {
            get
            {
                if (_removeFavouriteChannelCommand == null)
                {
                    _removeFavouriteChannelCommand = new DelegateCommand(RemoveFavouriteChannel);
                }

                return _removeFavouriteChannelCommand;
            }
        }

        public ICommand ChooseDownloadTempFolderCommand
        {
            get
            {
                if (_chooseDownloadTempFolderCommand == null)
                {
                    _chooseDownloadTempFolderCommand = new DelegateCommand(ChooseDownloadTempFolder);
                }

                return _chooseDownloadTempFolderCommand;
            }
        }

        public ICommand ChooseDownloadFolderCommand
        {
            get
            {
                if (_chooseDownloadFolderCommand == null)
                {
                    _chooseDownloadFolderCommand = new DelegateCommand(ChooseDownloadFolder);
                }

                return _chooseDownloadFolderCommand;
            }
        }

        public ICommand UseOnlineCheckChangeCommand
        {
            get
            {
                if (_useOnlineCheckChangeCommand == null)
                {
                    _useOnlineCheckChangeCommand = new DelegateCommand(UseOnlineCheckChange);
                }

                return _useOnlineCheckChangeCommand;
            }
        }

        public ICommand AddOnlineCheckChannelCommand
        {
            get
            {
                if (_addOnlineCheckChannelCommand == null)
                {
                    _addOnlineCheckChannelCommand = new DelegateCommand(AddCheckOnlineChannel);
                }

                return _addOnlineCheckChannelCommand;
            }
        }

        public ICommand RemoveOnlineCheckChannelCommand
        {
            get
            {
                if (_removeOnlineCheckChannelCommand == null)
                {
                    _removeOnlineCheckChannelCommand = new DelegateCommand(RemoveCheckOnlineChannel);
                }

                return _removeOnlineCheckChannelCommand;
            }
        }

        public ICommand ChooseOnlineCheckDownloadFolderCommand
        {
            get
            {
                if (_chooseOnlineCheckDownloadFolderCommand == null)
                {
                    _chooseOnlineCheckDownloadFolderCommand = new DelegateCommand(ChooseOnlineCheckDownloadFolder);
                }

                return _chooseOnlineCheckDownloadFolderCommand;
            }
        }

        public ICommand ChooseExternalPlayerCommand
        {
            get
            {
                if (_chooseExternalPlayerCommand == null)
                {
                    _chooseExternalPlayerCommand = new DelegateCommand(ChooseExternalPlayer);
                }

                return _chooseExternalPlayerCommand;
            }
        }

        public ICommand ClearExternalPlayerCommand
        {
            get
            {
                if (_clearExternalPlayerCommand == null)
                {
                    _clearExternalPlayerCommand = new DelegateCommand(ClearExternalPlayer);
                }

                return _clearExternalPlayerCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new DelegateCommand(Save);
                }

                return _saveCommand;
            }
        }

        public ICommand UndoCommand
        {
            get
            {
                if (_undoCommand == null)
                {
                    _undoCommand = new DelegateCommand(Undo);
                }

                return _undoCommand;
            }
        }

        public ICommand DefaultsCommand
        {
            get
            {
                if (_defaultsCommand == null)
                {
                    _defaultsCommand = new DelegateCommand(Defaults);
                }

                return _defaultsCommand;
            }
        }

        public int DownloadSplitTimeHours
        {
            get
            {
                return (int) CurrentPreferences.DownloadSplitTime.TotalHours;
            }
            set
            {
                TimeSpan current = CurrentPreferences.DownloadSplitTime;
                CurrentPreferences.DownloadSplitTime = new TimeSpan(value, current.Minutes, current.Seconds);

                FirePropertyChanged(nameof(DownloadSplitTimeHours));
                FirePropertyChanged(nameof(DownloadSplitTimeMinutes));
                FirePropertyChanged(nameof(DownloadSplitTimeSeconds));
            }
        }

        public int DownloadSplitTimeMinutes
        {
            get
            {
                return CurrentPreferences.DownloadSplitTime.Minutes;
            }
            set
            {
                TimeSpan current = CurrentPreferences.DownloadSplitTime;
                CurrentPreferences.DownloadSplitTime = new TimeSpan((int) CurrentPreferences.DownloadSplitTime.TotalHours, value, current.Seconds);

                FirePropertyChanged(nameof(DownloadSplitTimeHours));
                FirePropertyChanged(nameof(DownloadSplitTimeMinutes));
                FirePropertyChanged(nameof(DownloadSplitTimeSeconds));
            }
        }

        public int DownloadSplitTimeSeconds
        {
            get
            {
                return CurrentPreferences.DownloadSplitTime.Seconds;
            }
            set
            {
                TimeSpan current = CurrentPreferences.DownloadSplitTime;
                CurrentPreferences.DownloadSplitTime = new TimeSpan((int) CurrentPreferences.DownloadSplitTime.TotalHours, current.Minutes, value);

                FirePropertyChanged(nameof(DownloadSplitTimeHours));
                FirePropertyChanged(nameof(DownloadSplitTimeMinutes));
                FirePropertyChanged(nameof(DownloadSplitTimeSeconds));
            }
        }

        public int OnlineCheckSplitTimeHours
        {
            get
            {
                return (int)CurrentPreferences.OnlineCheckSplitTime.TotalHours;
            }
            set
            {
                TimeSpan current = CurrentPreferences.OnlineCheckSplitTime;
                CurrentPreferences.OnlineCheckSplitTime = new TimeSpan(value, current.Minutes, current.Seconds);

                FirePropertyChanged(nameof(OnlineCheckSplitTimeHours));
                FirePropertyChanged(nameof(OnlineCheckSplitTimeMinutes));
                FirePropertyChanged(nameof(OnlineCheckSplitTimeSeconds));
            }
        }

        public int OnlineCheckSplitTimeMinutes
        {
            get
            {
                return CurrentPreferences.OnlineCheckSplitTime.Minutes;
            }
            set
            {
                TimeSpan current = CurrentPreferences.OnlineCheckSplitTime;
                CurrentPreferences.OnlineCheckSplitTime = new TimeSpan((int)CurrentPreferences.OnlineCheckSplitTime.TotalHours, value, current.Seconds);

                FirePropertyChanged(nameof(OnlineCheckSplitTimeHours));
                FirePropertyChanged(nameof(OnlineCheckSplitTimeMinutes));
                FirePropertyChanged(nameof(OnlineCheckSplitTimeSeconds));
            }
        }

        public int OnlineCheckSplitTimeSeconds
        {
            get
            {
                return CurrentPreferences.OnlineCheckSplitTime.Seconds;
            }
            set
            {
                TimeSpan current = CurrentPreferences.OnlineCheckSplitTime;
                CurrentPreferences.OnlineCheckSplitTime = new TimeSpan((int)CurrentPreferences.OnlineCheckSplitTime.TotalHours, current.Minutes, value);

                FirePropertyChanged(nameof(OnlineCheckSplitTimeHours));
                FirePropertyChanged(nameof(OnlineCheckSplitTimeMinutes));
                FirePropertyChanged(nameof(OnlineCheckSplitTimeSeconds));
            }
        }

        public int OnlineCheckStartDaytimeHours
        {
            get
            {
                return (int)CurrentPreferences.OnlineCheckStartDaytime.TotalHours;
            }
            set
            {
                TimeSpan current = CurrentPreferences.OnlineCheckStartDaytime;
                CurrentPreferences.OnlineCheckStartDaytime = new TimeSpan(value, current.Minutes, current.Seconds);

                FirePropertyChanged(nameof(OnlineCheckStartDaytimeHours));
                FirePropertyChanged(nameof(OnlineCheckStartDaytimeMinutes));
                FirePropertyChanged(nameof(OnlineCheckStartDaytimeSeconds));
            }
        }

        public int OnlineCheckStartDaytimeMinutes
        {
            get
            {
                return CurrentPreferences.OnlineCheckStartDaytime.Minutes;
            }
            set
            {
                TimeSpan current = CurrentPreferences.OnlineCheckStartDaytime;
                CurrentPreferences.OnlineCheckStartDaytime = new TimeSpan((int)CurrentPreferences.OnlineCheckStartDaytime.TotalHours, value, current.Seconds);

                FirePropertyChanged(nameof(OnlineCheckStartDaytimeHours));
                FirePropertyChanged(nameof(OnlineCheckStartDaytimeMinutes));
                FirePropertyChanged(nameof(OnlineCheckStartDaytimeSeconds));
            }
        }

        public int OnlineCheckStartDaytimeSeconds
        {
            get
            {
                return CurrentPreferences.OnlineCheckStartDaytime.Seconds;
            }
            set
            {
                TimeSpan current = CurrentPreferences.OnlineCheckStartDaytime;
                CurrentPreferences.OnlineCheckStartDaytime = new TimeSpan((int)CurrentPreferences.OnlineCheckStartDaytime.TotalHours, current.Minutes, value);

                FirePropertyChanged(nameof(OnlineCheckStartDaytimeHours));
                FirePropertyChanged(nameof(OnlineCheckStartDaytimeMinutes));
                FirePropertyChanged(nameof(OnlineCheckStartDaytimeSeconds));
            }
        }

        public int OnlineCheckEndDaytimeHours
        {
            get
            {
                return (int)CurrentPreferences.OnlineCheckEndDaytime.TotalHours;
            }
            set
            {
                TimeSpan current = CurrentPreferences.OnlineCheckEndDaytime;
                CurrentPreferences.OnlineCheckEndDaytime = new TimeSpan(value, current.Minutes, current.Seconds);

                FirePropertyChanged(nameof(OnlineCheckEndDaytimeHours));
                FirePropertyChanged(nameof(OnlineCheckEndDaytimeMinutes));
                FirePropertyChanged(nameof(OnlineCheckEndDaytimeSeconds));
            }
        }

        public int OnlineCheckEndDaytimeMinutes
        {
            get
            {
                return CurrentPreferences.OnlineCheckEndDaytime.Minutes;
            }
            set
            {
                TimeSpan current = CurrentPreferences.OnlineCheckEndDaytime;
                CurrentPreferences.OnlineCheckEndDaytime = new TimeSpan((int)CurrentPreferences.OnlineCheckEndDaytime.TotalHours, value, current.Seconds);

                FirePropertyChanged(nameof(OnlineCheckEndDaytimeHours));
                FirePropertyChanged(nameof(OnlineCheckEndDaytimeMinutes));
                FirePropertyChanged(nameof(OnlineCheckEndDaytimeSeconds));
            }
        }

        public int OnlineCheckEndDaytimeSeconds
        {
            get
            {
                return CurrentPreferences.OnlineCheckEndDaytime.Seconds;
            }
            set
            {
                TimeSpan current = CurrentPreferences.OnlineCheckEndDaytime;
                CurrentPreferences.OnlineCheckEndDaytime = new TimeSpan((int)CurrentPreferences.OnlineCheckEndDaytime.TotalHours, current.Minutes, value);

                FirePropertyChanged(nameof(OnlineCheckEndDaytimeHours));
                FirePropertyChanged(nameof(OnlineCheckEndDaytimeMinutes));
                FirePropertyChanged(nameof(OnlineCheckEndDaytimeSeconds));
            }
        }

        #endregion Properties

        #region Methods

        private void AddFavouriteChannel()
        {
            try
            {
                lock (_commandLockObject)
                {
                    string currentChannel = CurrentPreferences.SearchChannelName;

                    if (!string.IsNullOrWhiteSpace(currentChannel))
                    {
                        string existingEntry = CurrentPreferences.SearchFavouriteChannels.FirstOrDefault(channel => channel.Equals(currentChannel, StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrWhiteSpace(existingEntry))
                        {
                            CurrentPreferences.SearchChannelName = existingEntry;
                        }
                        else
                        {
                            CurrentPreferences.SearchFavouriteChannels.Add(currentChannel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void RemoveFavouriteChannel()
        {
            try
            {
                lock (_commandLockObject)
                {
                    string currentChannel = CurrentPreferences.SearchChannelName;

                    if (!string.IsNullOrWhiteSpace(currentChannel))
                    {
                        string existingEntry = CurrentPreferences.SearchFavouriteChannels.FirstOrDefault(channel => channel.Equals(currentChannel, StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrWhiteSpace(existingEntry))
                        {
                            CurrentPreferences.SearchFavouriteChannels.Remove(existingEntry);
                            CurrentPreferences.SearchChannelName = CurrentPreferences.SearchFavouriteChannels.FirstOrDefault();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadTempFolder()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _dialogService.ShowFolderBrowserDialog(CurrentPreferences.DownloadTempFolder, ChooseDownloadTempFolderCallback);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadTempFolderCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    CurrentPreferences.DownloadTempFolder = folder;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadFolder()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _dialogService.ShowFolderBrowserDialog(CurrentPreferences.DownloadFolder, ChooseDownloadFolderCallback);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadFolderCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    CurrentPreferences.DownloadFolder = folder;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void UseOnlineCheckChange()
        {
            try
            {
                lock (_commandLockObject)
                {
                    bool currentOnlineCheckUse = CurrentPreferences.OnlineCheckUse;

                    if (!currentOnlineCheckUse)
                    {
                        var mesResult = _dialogService.ShowMessageBox(@"This feature use a lot of internet: using it TL can repeatedly check and download streams without any warning or actions from your side.
Recommend to use it with unlimited internet. Are you sure to use it?", "WARNING!", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (mesResult == MessageBoxResult.Yes)
                        {
                            CurrentPreferences.OnlineCheckUse = !currentOnlineCheckUse;
                        }
                    }
                    else
                    {
                        CurrentPreferences.OnlineCheckUse = !currentOnlineCheckUse;
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void AddCheckOnlineChannel()
        {
            try
            {
                lock (_commandLockObject)
                {
                    string currentChannel = CurrentPreferences.OnlineCheckChannelName;

                    if (!string.IsNullOrWhiteSpace(currentChannel))
                    {
                        string existingEntry = CurrentPreferences.OnlineCheckChannels.FirstOrDefault(channel => channel.Equals(currentChannel, StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrWhiteSpace(existingEntry))
                        {
                            CurrentPreferences.OnlineCheckChannelName = existingEntry;
                        }
                        else
                        {
                            CurrentPreferences.OnlineCheckChannels.Add(currentChannel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void RemoveCheckOnlineChannel()
        {
            try
            {
                lock (_commandLockObject)
                {
                    string currentChannel = CurrentPreferences.OnlineCheckChannelName;

                    if (!string.IsNullOrWhiteSpace(currentChannel))
                    {
                        string existingEntry = CurrentPreferences.OnlineCheckChannels.FirstOrDefault(channel => channel.Equals(currentChannel, StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrWhiteSpace(existingEntry))
                        {
                            CurrentPreferences.OnlineCheckChannels.Remove(existingEntry);
                            CurrentPreferences.OnlineCheckChannelName = CurrentPreferences.OnlineCheckChannels.FirstOrDefault();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseOnlineCheckDownloadFolder()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _dialogService.ShowFolderBrowserDialog(CurrentPreferences.OnlineCheckDownloadFolder, ChooseOnlineCheckDownloadFolderCallback);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseOnlineCheckDownloadFolderCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    CurrentPreferences.OnlineCheckDownloadFolder = folder;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseExternalPlayer()
        {
            try
            {
                lock (_commandLockObject)
                {
                    var filter = new CommonFileDialogFilter("Executables", "*.exe");
                    _dialogService.ShowFileBrowserDialog(filter, CurrentPreferences.MiscExternalPlayer, ChooseExternalPlayerCallback);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ClearExternalPlayer()
        {
            try
            {
                lock (_commandLockObject)
                {
                    CurrentPreferences.MiscExternalPlayer = null;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseExternalPlayerCallback(bool cancelled, string file)
        {
            try
            {
                if (!cancelled)
                {
                    CurrentPreferences.MiscExternalPlayer = file;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Save()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _dialogService.SetBusy();
                    Validate();

                    if (!HasErrors)
                    {
                        _preferencesService.Save(_currentPreferences);
                        CurrentPreferences = null;
                        _notificationService.ShowNotification("Preferences saved");
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Undo()
        {
            try
            {
                lock (_commandLockObject)
                {
                    MessageBoxResult result = _dialogService.ShowMessageBox("Undo current changes and reload last saved preferences?", "Undo", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _dialogService.SetBusy();
                        CurrentPreferences = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Defaults()
        {
            try
            {
                lock (_commandLockObject)
                {
                    MessageBoxResult result = _dialogService.ShowMessageBox("Load default preferences?", "Defaults", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _dialogService.SetBusy();
                        _preferencesService.Save(_preferencesService.CreateDefault());
                        CurrentPreferences = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(CurrentPreferences);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                CurrentPreferences?.Validate();

                if (CurrentPreferences.HasErrors)
                {
                    AddError(currentProperty, "Invalid Preferences!");

                    var needErrorList = CurrentPreferences.GetErrors(nameof(CurrentPreferences.DownloadSplitTime)) as List<string>;

                    if (needErrorList != null && needErrorList.Count > 0)
                    {
                        string firstError = needErrorList.First();
                        AddError(nameof(DownloadSplitTimeHours), firstError);
                        AddError(nameof(DownloadSplitTimeMinutes), firstError);
                        AddError(nameof(DownloadSplitTimeSeconds), firstError);
                    }

                    needErrorList = CurrentPreferences.GetErrors(nameof(CurrentPreferences.OnlineCheckSplitTime)) as List<string>;

                    if (needErrorList != null && needErrorList.Count > 0)
                    {
                        string firstError = needErrorList.First();
                        AddError(nameof(OnlineCheckSplitTimeHours), firstError);
                        AddError(nameof(OnlineCheckSplitTimeMinutes), firstError);
                        AddError(nameof(OnlineCheckSplitTimeSeconds), firstError);
                    }

                    needErrorList = CurrentPreferences.GetErrors(nameof(CurrentPreferences.OnlineCheckStartDaytime)) as List<string>;

                    if (needErrorList != null && needErrorList.Count > 0)
                    {
                        string firstError = needErrorList.First();
                        AddError(nameof(OnlineCheckStartDaytimeHours), firstError);
                        AddError(nameof(OnlineCheckStartDaytimeMinutes), firstError);
                        AddError(nameof(OnlineCheckStartDaytimeSeconds), firstError);
                    }

                    needErrorList = CurrentPreferences.GetErrors(nameof(CurrentPreferences.OnlineCheckEndDaytime)) as List<string>;

                    if (needErrorList != null && needErrorList.Count > 0)
                    {
                        string firstError = needErrorList.First();
                        AddError(nameof(OnlineCheckEndDaytimeHours), firstError);
                        AddError(nameof(OnlineCheckEndDaytimeMinutes), firstError);
                        AddError(nameof(OnlineCheckEndDaytimeSeconds), firstError);
                    }
                }
            }
        }

        public override void OnBeforeHidden()
        {
            try
            {
                CurrentPreferences = null;
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

            menuCommands.Add(new MenuCommand(SaveCommand, "Save", "Save"));
            menuCommands.Add(new MenuCommand(UndoCommand, "Undo", "Undo"));
            menuCommands.Add(new MenuCommand(DefaultsCommand, "Default", "Wrench"));

            return menuCommands;
        }

        #endregion Methods
    }
}