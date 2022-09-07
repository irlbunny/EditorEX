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
            Container.BindInterfacesTo<NoteTypeHelperPatch>().AsSingle();

            // Audio Spectrogram
#pragma warning disable IDE0059
            Type colorDataType = null;
#pragma warning restore IDE0059
            switch (config.SpectrogramColor)
            {
                case Config.SpectrogramColorConfig.Inferno:
                    colorDataType = typeof(InfernoColorData);
                    break;

                case Config.SpectrogramColorConfig.Poison:
                    colorDataType = typeof(PoisonColorData);
                    break;

                default:
                    throw new Exception("Unknown spectrogram color when attempting to bind IColorData type.");
            }

            Container.Bind<IColorData>().To(colorDataType).AsSingle();
            Container.BindInterfacesAndSelfTo<SpectrogramManager>().AsSingle();

            // Managers
            Container.BindInterfacesAndSelfTo<CustomBeatmapObjectsToolbarViewManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentGameObjectGroupManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentGameObjectManager>().AsSingle();
        }
    }
}
