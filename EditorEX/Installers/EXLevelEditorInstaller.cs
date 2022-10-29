using EditorEX.AffinityPatches;
using EditorEX.AudioSpectrogram.Colors;
using EditorEX.AudioSpectrogram.Managers;
using EditorEX.Managers;
using System;
using Zenject;

namespace EditorEX.Installers
{
    internal class EXLevelEditorInstaller : Installer
    {
        public override void InstallBindings()
        {
            var config = Container.Resolve<Config>();

            // Patches
            Container.BindInterfacesTo<BeatmapObjectViewColorHelperPatch>().AsSingle();
            Container.BindInterfacesTo<ColorEventMarkerObjectPatch>().AsSingle();
            Container.BindInterfacesTo<ColorTypeHelperPatch>().AsSingle();
            Container.BindInterfacesTo<EventObjectViewColorHelperPatch>().AsSingle();
            Container.BindInterfacesTo<NoteTypeHelperPatch>().AsSingle();

            // Audio Spectrogram
            var colorDataType = config.SpectrogramColor switch
            {
                Config.SpectrogramColorConfig.Inferno => typeof(InfernoColorData),
                Config.SpectrogramColorConfig.Poison => typeof(PoisonColorData),
                _ => throw new Exception("Unknown spectrogram color when attempting to bind IColorData type.")
            };
            Container.Bind<IColorData>().To(colorDataType).AsSingle();
            Container.BindInterfacesAndSelfTo<SpectrogramManager>().AsSingle();

            // Managers
            Container.BindInterfacesAndSelfTo<CustomBeatmapObjectsToolbarViewManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentGameObjectGroupManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentGameObjectManager>().AsSingle();
        }
    }
}
