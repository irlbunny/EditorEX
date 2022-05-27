using EditorEX.AffinityPatches;
using EditorEX.AudioSpectrogram.Colors;
using EditorEX.AudioSpectrogram.Managers;
using EditorEX.Managers;
using Zenject;

namespace EditorEX.Installers
{
    internal class EXLevelEditorInstaller : Installer
    {
        public override void InstallBindings()
        {
            // Patches
            Container.BindInterfacesTo<NoteTypeHelperPatch>().AsSingle();

            // Audio Spectrogram
            Container.Bind<IColorData>().To<InfernoColorData>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpectrogramManager>().AsSingle();

            // Managers
            Container.BindInterfacesAndSelfTo<CustomBeatmapObjectsToolbarViewManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentGameObjectGroupManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentGameObjectManager>().AsSingle();
        }
    }
}
