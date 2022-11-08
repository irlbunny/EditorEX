using EditorEX.Managers;
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
