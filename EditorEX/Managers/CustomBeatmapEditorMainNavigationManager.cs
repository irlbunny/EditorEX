using BeatmapEditor3D;
using EditorEX.UI;
using EditorEX.Utilities;
using HarmonyLib;
using IPA.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.Managers
{
    internal class CustomBeatmapEditorMainNavigationManager : IInitializable, IDisposable
    {
        private static Action<BeatmapEditorFlowCoordinator, BeatmapEditorViewController> _replaceTopViewControllerAccessor =
            MethodAccessor<BeatmapEditorFlowCoordinator, Action<BeatmapEditorFlowCoordinator, BeatmapEditorViewController>>.GetDelegate("ReplaceTopViewController");
        private static Action<BeatmapEditorMainNavigationViewController, BeatmapEditorMainNavigationViewController.EditorControlsButtonType> _setActiveNavButtonsAccessor =
            MethodAccessor<BeatmapEditorMainNavigationViewController, Action<BeatmapEditorMainNavigationViewController, BeatmapEditorMainNavigationViewController.EditorControlsButtonType>>.GetDelegate("SetActiveNavButtons");

        private static AccessTools.FieldRef<BeatmapEditorMainNavigationViewController, Button> _beatmapsListButtonAccessor =
            AccessTools.FieldRefAccess<BeatmapEditorMainNavigationViewController, Button>("_beatmapsListButton");

        private readonly DiContainer _container;
        private readonly MainBeatmapEditorFlowCoordinator _mainBeatmapEditorFlowCoordinator;
        private readonly BeatmapEditorMainNavigationViewController _beatmapEditorMainNavigationViewController;
        private readonly GameObject _projectButtonsGameObject;

        private TributesCreditsViewController _tributesCreditsViewController;
        private Button _tributesCreditsButton;

        public CustomBeatmapEditorMainNavigationManager(
            DiContainer container,
            MainBeatmapEditorFlowCoordinator mainBeatmapEditorFlowCoordinator,
            BeatmapEditorMainNavigationViewController beatmapEditorMainNavigationViewController)
        {
            _container = container;
            _mainBeatmapEditorFlowCoordinator = mainBeatmapEditorFlowCoordinator;
            _beatmapEditorMainNavigationViewController = beatmapEditorMainNavigationViewController;
            _projectButtonsGameObject = beatmapEditorMainNavigationViewController.transform.Find("ProjectButtons").gameObject;
        }

        public void Initialize()
        {
            _beatmapEditorMainNavigationViewController.didActivateEvent += BeatmapEditorMainNavigationViewController_didActivateEvent;
            _beatmapEditorMainNavigationViewController.buttonWasPressed += BeatmapEditorMainNavigationViewController_buttonWasPressed;
        }

        public void Dispose()
        {
            if (_tributesCreditsButton != null)
                UnityEngine.Object.Destroy(_tributesCreditsButton.gameObject);
        }

        private void BeatmapEditorMainNavigationViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (_tributesCreditsViewController == null)
                _tributesCreditsViewController = UIUtil.CreateViewController<TributesCreditsViewController>();

            if (_tributesCreditsButton == null)
            {
                var heartSprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("EditorEX.Resources.heart.png");
                _tributesCreditsButton = UIUtil.CreateNavbarButton(_container, heartSprite, _projectButtonsGameObject.transform, HandleTributesCreditsButtonPressed);
            }
        }

        private void BeatmapEditorMainNavigationViewController_buttonWasPressed(BeatmapEditorMainNavigationViewController.EditorControlsButtonType buttonType)
        {
            _tributesCreditsButton.enabled = true;
        }

        private void HandleTributesCreditsButtonPressed()
        {
            _tributesCreditsButton.enabled = false;
            _replaceTopViewControllerAccessor(_mainBeatmapEditorFlowCoordinator, _tributesCreditsViewController);
            _setActiveNavButtonsAccessor(_beatmapEditorMainNavigationViewController, BeatmapEditorMainNavigationViewController.EditorControlsButtonType.BeatmapList);
            _beatmapsListButtonAccessor(_beatmapEditorMainNavigationViewController).enabled = true;
        }
    }
}
