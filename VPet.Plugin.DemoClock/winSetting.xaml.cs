﻿using LinePutScript;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Panuon.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VPet.Plugin.DemoClock
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        private bool AllowChange = false;
        DemoClock Master;
        Setting Set;
        bool Initial = false;
        LpsDocument lps;
        List<(string Name, int AdCode, int CityCode)> _data;
        List<string> _schBoxSource;
        public winSetting(DemoClock master)
        {
            InitializeComponent();
            Resources = Application.Current.Resources;

            Master = master;
            Set = Master.Set;
            Switch24h.IsChecked = Set.Hour24;
            PlaceSilder.Value = Set.PlaceTop;
            OpacitySilder.Value = Set.Opacity * 100;
            SwitchAutoLayer.IsChecked = Set.PlaceAutoBack;
            NumTimeDiff.Value = Set.TimeShifting;
            NumDefCountDown.Value = Set.DefaultCountDown;
            NumTomatoWork.Value = Set.Tomato_WorkTime;
            NumTomatoRest.Value = Set.Tomato_RestTime;
            NumTomatoRestLong.Value = Set.Tomato_RestTimeLong;
            var TempText = "";
            Tools.TryGetInputTypeAndValue(Set.CountDownVoice, out TempText);
            TextCountDown.Text = TempText;
            Tools.TryGetInputTypeAndValue(Set.Tomato_WorkVoice, out TempText);
            TextTomatoWork.Text = TempText;
            Tools.TryGetInputTypeAndValue(Set.Tomato_RestVoice, out TempText);
            TextTomatoRest.Text = TempText;
            Tools.TryGetInputTypeAndValue(Set.Tomato_EndVoice, out TempText);
            TextTomatoEnd.Text = TempText;
            WeatherPositionSet.SelectedIndex = Set.WeatherPosition ? 0 : 1;
            DefaultWeather.IsChecked = Set.DefaultWeather;

            if (Master.mode != DemoClock.Mode.None)
            {
                NumTomatoWork.IsEnabled = false;
                NumTomatoRest.IsEnabled = false;
                NumTomatoRestLong.IsEnabled = false;
                AllowChange = false;
            }
            else
            {
                NumTomatoWork.IsEnabled = true;
                NumTomatoRest.IsEnabled = true;
                NumTomatoRestLong.IsEnabled = true;
                AllowChange = true;
            }
            _ = LoadDataAsync();
        }

        private void Switch24h_Checked(object sender, RoutedEventArgs e)
        {
            if (!AllowChange)
                return;
            Set.Hour24 = Switch24h.IsChecked.Value;
        }

        private void PlaceSilder_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AllowChange || !Initial)
                return;
            Set.PlaceTop = PlaceSilder.Value;
            Master.WPFTimeClock.Margin = new Thickness(0, Set.PlaceTop, 0, 0);
        }

        private void OpacitySilder_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AllowChange || !Initial)
                return;
            Set.Opacity = OpacitySilder.Value / 100;
            if (Master.WPFTimeClock.Opacity != 0.95)
                Master.WPFTimeClock.Opacity = Set.Opacity;
        }

        private void SwitchAutoLayer_Checked(object sender, RoutedEventArgs e)
        {
            if (!Initial)
                return;
            Set.PlaceAutoBack = SwitchAutoLayer.IsChecked.Value;
            if (Master.Set.PlaceAutoBack && Master.MW.Main.UIGrid_Back.Children.Contains(Master.WPFTimeClock))
            {
                Master.MW.Main.UIGrid_Back.Children.Remove(Master.WPFTimeClock);
                Master.MW.Main.UIGrid.Children.Insert(0, Master.WPFTimeClock);
            }
            else if (Master.Set.PlaceAutoBack == false && Master.MW.Main.UIGrid_Back.Children.Contains(Master.WPFTimeClock))
            {
                Master.MW.Main.UIGrid_Back.Children.Remove(Master.WPFTimeClock);
                Master.MW.Main.UIGrid.Children.Insert(0, Master.WPFTimeClock);
            }
        }

        private void NumTimeDiff_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange || !Initial)
                return;
            Set.TimeShifting = NumTimeDiff.Value.Value;
        }

        private void NumDefCountDown_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange || !Initial)
                return;
            Set.DefaultCountDown = NumDefCountDown.Value.Value;
        }

        private void NumTomatoWork_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange || !Initial)
                return;
            Set.Tomato_WorkTime = NumTomatoWork.Value.Value;
        }

        private void NumTomatoRest_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange || !Initial)
                return;
            Set.Tomato_RestTime = NumTomatoRest.Value.Value;
        }

        private void NumTomatoRestLong_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange || !Initial)
                return;
            Set.Tomato_RestTimeLong = NumTomatoRestLong.Value.Value;
        }

        private void btn_path_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "音频文件|*.wav;*.mp3";

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            Button button = sender as Button;
            switch (button.Tag)
            {
                case "CountDown":
                    TextCountDown.Text = openFileDialog.FileName;
                    Set.CountDownVoice = "file:" + TextCountDown.Text;
                    break;
                case "TomatoWork":
                    TextTomatoWork.Text = openFileDialog.FileName;
                    Set.Tomato_WorkVoice = "file:" + TextTomatoWork.Text;
                    break;
                case "TomatoRest":
                    TextTomatoRest.Text = openFileDialog.FileName;
                    Set.Tomato_RestVoice = "file:" + TextTomatoRest.Text;
                    break;
                case "TomatoEnd":
                    TextTomatoEnd.Text = openFileDialog.FileName;
                    Set.Tomato_EndVoice = "file:" + TextTomatoEnd.Text;
                    break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Master.winSetting = null;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Master.MW.ShowSetting(5);
        }

        private void TextTomatoWork_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (File.Exists(TextTomatoWork.Text))
            {
                Set.Tomato_WorkVoice = "file:" + TextTomatoWork.Text;
            }
            else
            {
                Set.Tomato_WorkVoice = "text:" + TextTomatoWork.Text;
            }
        }

        private void TextTomatoRest_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (File.Exists(TextTomatoRest.Text))
            {
                Set.Tomato_RestVoice = "file:" + TextTomatoRest.Text;
            }
            else
            {
                Set.Tomato_RestVoice = "text:" + TextTomatoRest.Text;
            }
        }

        private void TextTomatoEnd_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (File.Exists(TextTomatoEnd.Text))
            {
                Set.Tomato_EndVoice = "file:" + TextTomatoEnd.Text;
            }
            else
            {
                Set.Tomato_EndVoice = "text:" + TextTomatoEnd.Text;
            }
        }

        private void TextCountDown_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (File.Exists(TextCountDown.Text))
            {
                Set.CountDownVoice = "file:" + TextCountDown.Text;
            }
            else
            {
                Set.CountDownVoice = "text:" + TextCountDown.Text;
            }
        }

        private void Position_DataContextChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Initial)
                return;
            if (WeatherPositionSet.SelectedIndex == 0)
            {
                Set.WeatherPosition = true;
                Master.WPFTimeClock.UpdateWeatherState();
            }
            else
            {
                Set.WeatherPosition = false;
                Master.WPFTimeClock.UpdateWeatherState();
            }
        }

        private async void DefaultWeather_Checked(object sender, RoutedEventArgs e)
        {
            Set.DefaultWeather = DefaultWeather.IsChecked.Value;
            Master.WPFTimeClock.UpdateWeatherState();
            if(Set.DefaultWeather)
            {
                if (Master.weather == null)
                {
                    await Master.HandleWeatherAsync();
                }
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // 异步加载LPS文件数据，避免阻塞UI线程
                lps = await Task.Run(() => new LpsDocument(Properties.Resources.V3CityADCODE_lps));

                // 使用后台线程处理数据
                _data = await Task.Run(() => lps.Select(x => (x.Name, x.InfoToInt, x.TextToInt)).ToList());
                _data.Insert(0, ("自动选择", 0, 0));
                // 获取城市名并设置给下拉框数据源
                _schBoxSource = _data.Select(x => x.Name).ToList();
                // 在UI线程中更新ItemsSource
                Dispatcher.Invoke(() =>
                {
                    RegionBox.ItemsSource = _schBoxSource.Take(50);
                    RegionBox.Text = _data.FirstOrDefault(x => x.AdCode == Set.AdCode).Name;
                });
            }
            catch (Exception ex)
            {
                // 错误处理，显示异常信息
                MessageBoxX.Show($"加载城市数据失败: {ex.Message}");
            }
            Dispatcher.Invoke(() => DataContext = this);
            Initial = true;
        }

        private void SchBox_SearchTextChanged(object sender, SearchTextChangedRoutedEventArgs e)
        {
            if (!Initial)
                return;
            var searchText = e.Text?.Trim()?.ToLower();

            // 异步更新搜索结果，避免阻塞UI线程
            Task.Run(() =>
            {
                var filteredData = string.IsNullOrEmpty(searchText)
                    ? _schBoxSource.Take(50).ToList()
                    : _schBoxSource.Where(x => x.Contains(searchText, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();

                // 在UI线程中更新ItemsSource
                Dispatcher.Invoke(() =>
                {
                    RegionBox.ItemsSource = filteredData;
                });
            });
        }

        private async void SchBox_ItemClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!Initial)
                return;
            // 点击项目后的处理
            var searchBox = sender as SearchBox;
            var selectedText = searchBox.Text?.ToString();

            // 更新 AdCode
            var selectedItem = _data.FirstOrDefault(x => x.Name == selectedText);
            if (selectedItem != default)
            {
                Set.AdCode = selectedItem.AdCode;
            }
            await Master.HandleWeatherAsync();
            Master.WPFTimeClock.UpdateWeatherState();
        }
    }
    public enum WeatherPosition
    {
        [Description("左侧")]
        Left,
        [Description("右侧")]
        Right
    }
}
