using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Runtime;
using System.Text.Json;
using System.Windows.Input;
using Wpf.Ui.Common.Interfaces;
using RPEFluentManager.Models;
using System.Diagnostics;

namespace RPEFluentManager.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private string _pathSetting = String.Empty;

        

        [ObservableProperty]
        private Wpf.Ui.Appearance.ThemeType _currentTheme = Wpf.Ui.Appearance.ThemeType.Unknown;

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom()
        {
        }


        


        private void InitializeViewModel()
        {
            CurrentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
            AppVersion = $"RPEFluentManager - {GetAssemblyVersion()}";

            var a = SettingsHandler.GetSettings();
            if (a != null)
            {
                RPEManagerSettings set = a;
                PathSetting = set.ResourcePath;
            }


            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? String.Empty;
        }


        [RelayCommand]
        private void OnSaveSettings()
        {
            var newSettings = new RPEManagerSettings(); // 自定义设置对象

            newSettings.ResourcePath = PathSetting;

            SettingsHandler.SaveSettings(newSettings);
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Light)
                        break;

                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Light;

                    break;

                default:
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Dark)
                        break;

                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Dark;

                    break;
            }
        }
    }
}
