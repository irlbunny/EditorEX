using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using EditorEX.Installers;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using IPA.Utilities;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using System.IO;
using System.Reflection;
using IPAConfig = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace EditorEX
{
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        internal const string HARMONYID = "com.github.ItsKaitlyn03.EditorEX";
        internal static Harmony HarmonyInstance { get; private set; } = new(HARMONYID);

        internal static string DataPath { get; private set; } = Path.Combine(UnityGame.UserDataPath, "EditorEX");

        [Init]
        public Plugin(IPALogger logger, IPAConfig conf, PluginMetadata metadata, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);

            var config = conf.Generated<Config>();
            zenjector.Install(Location.App, container =>
            {
                container.BindInstance(config).AsSingle();
                container.BindInstance(new UBinder<Plugin, PluginMetadata>(metadata));
            });

            zenjector.Install<EXLevelEditorInstaller, BeatmapLevelEditorInstaller>();
        }

        [OnEnable]
        public void OnEnable()
        {
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnDisable]
        public void OnDisable()
        {
            HarmonyInstance.UnpatchSelf();
        }
    }
}
