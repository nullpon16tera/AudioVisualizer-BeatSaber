﻿using IPA;
using IPA.Config;
using IPA.Config.Stores;
using AudioVisualizer.Installers;
using UnityEngine;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace AudioVisualizer
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, Config conf, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Log.Info("AudioVisualizer initialized.");
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
            zenjector.Install<AudioVisualizerGameInstaller>(Location.Player);
            zenjector.Install<AudioVisualizerMenuInstaller>(Location.Menu);
        }

        [OnStart]
        public void OnApplicationStart() => Log.Debug("OnApplicationStart");
        
        [OnExit]
        public void OnApplicationQuit() => Log.Debug("OnApplicationQuit");
    }
}
