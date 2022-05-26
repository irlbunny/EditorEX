using EditorEX.AffinityPatches;
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

            // Managers
            Container.BindInterfacesAndSelfTo<CustomBeatmapObjectsToolbarViewManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<CustomEditorAudioFeedbackManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentGameObjectGroupManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpectrogramManager>().AsSingle();
        }
    }
}
