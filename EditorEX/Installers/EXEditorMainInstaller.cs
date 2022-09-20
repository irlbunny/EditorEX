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
            var beatmapEditorScreen = Container.Resolve<BeatmapEditorScreenSystem>().mainScreen;

            Container.BindInterfacesAndSelfTo<TributesCreditsViewController>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("TributesCreditsViewController")
                .UnderTransform(beatmapEditorScreen.transform)
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<CustomNavigationViewManager>().AsSingle();
        }
    }
}
