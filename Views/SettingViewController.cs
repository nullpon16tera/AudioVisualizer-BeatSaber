﻿using AudioVisualizer.Configuration;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Parser;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Zenject;

namespace AudioVisualizer.Views
{
    [HotReload]
    internal class SettingViewController : BSMLAutomaticViewController, IInitializable, INotifyPropertyChanged
    {

        public static SettingViewController Instance { get; }

        [UIParams]
        BSMLParserParams parserParams;

        public void updateUI()
        {
            parserParams.EmitEvent("cancel");
        }

        /// <summary>メニュー内で表示するかどうか を取得、設定</summary>
        private bool _showMenu;
        [UIValue("show-menu")]
        /// <summary>メニュー内で表示するかどうか を取得、設定</summary>
        public bool ShowMenu
        {
            get => this._showMenu;

            set => this.SetProperty(ref this._showMenu, value);
        }

        /// <summary>ゲーム中に表示するかどうか を取得、設定</summary>
        private bool _showGame;
        [UIValue("show-game")]
        /// <summary>ゲーム中に表示するかどうか を取得、設定</summary>
        public bool ShowGame
        {
            get => this._showGame;

            set => this.SetProperty(ref this._showGame, value);
        }

        private static readonly string s_resourceName = "AudioVisualizer.Views.SettingViewController";
        private static readonly string s_tabName = "AudioVisualizer";

        public event PropertyChangedEventHandler PropertyChanged;

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            // Code to run after BSML finishes
            this.ShowGame = PluginConfig.Instance.ShowGame;
            this.ShowMenu = PluginConfig.Instance.ShowMenu;
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string memberName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }
            field = value;
            this.OnPropertyChanged(new PropertyChangedEventArgs(memberName));
            return true;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(this.ShowGame)) {
                PluginConfig.Instance.ShowGame = this.ShowGame;
            }
            else if (args.PropertyName == nameof(this.ShowMenu)) {
                PluginConfig.Instance.ShowMenu = this.ShowMenu;
            }
            this.PropertyChanged?.Invoke(this, args);
        }

        public void TabSetup()
        {
            GameplaySetup.Instance?.RemoveTab(s_tabName);
            GameplaySetup.Instance?.AddTab(s_tabName, s_resourceName, this);
        }

        protected override void OnDestroy()
        {
            GameplaySetup.Instance?.RemoveTab(s_tabName);
            base.OnDestroy();
        }

        public void Initialize()
        {
            this.TabSetup();
        }

    }
}
