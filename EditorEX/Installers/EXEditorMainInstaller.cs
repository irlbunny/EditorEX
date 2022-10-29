using BeatmapEditor3D;
using EditorEX.Managers;
using EditorEX.UI;
using Zenject;

namespace EditorEX.Installers
{
    internal class EXEditorMainInstaller : Installer
    {
        public override void InstallBindings()
        {
            // Managers
            Container.BindInterfacesAndSelfTo<CustomBeatmapEditorMainNavigationManager>().AsSingle();
        }
    }
}
