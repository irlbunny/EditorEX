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
    internal class CustomNavigationViewManager : IInitializable, IDisposable
    {
        private static Action<BeatmapEditorFlowCoordinator, BeatmapEditorViewController> _replaceTopViewControllerAccessor =
            MethodAccessor<BeatmapEditorFlowCoordinator, Action<BeatmapEditorFlowCoordinator, BeatmapEditorViewController>>.GetDelegate("ReplaceTopViewController");

        private static Action<BeatmapEditorMainNavigationViewController, BeatmapEditorMainNavigationViewController.EditorControlsButtonType> _setActiveNavButtonsAccessor =
            MethodAccessor<BeatmapEditorMainNavigationViewController, Action<BeatmapEditorMainNavigationViewController, BeatmapEditorMainNavigationViewController.EditorControlsButtonType>>.GetDelegate("SetActiveNavButtons");
        private static AccessTools.FieldRef<BeatmapEditorMainNavigationViewController, Button> _beatmapsListButtonAccessor =
            AccessTools.FieldRefAccess<BeatmapEditorMainNavigationViewController, Button>("_beatmapsListButton");

        private readonly DiContainer _container;
        private readonly TributesCreditsViewController _tributesCreditsViewController;
        private readonly MainBeatmapEditorFlowCoordinator _mainBeatmapEditorFlowCoordinator;
        private readonly BeatmapEditorMainNavigationViewController _beatmapEditorMainNavigationViewController;
        private readonly GameObject _projectButtonsGameObject;

        private Button _tributesCreditsButton;

        public CustomNavigationViewManager(DiContainer container, TributesCreditsViewController tributesCreditsViewController,
            MainBeatmapEditorFlowCoordinator mainBeatmapEditorFlowCoordinator, BeatmapEditorMainNavigationViewController beatmapEditorMainNavigationViewController)
        {
            _container = container;
            _tributesCreditsViewController = tributesCreditsViewController;
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
            if (_tributesCreditsButton == null)
            {
                var tributesCreditsImage = BeatSaberMarkupLanguage.Utilities.GetResource(Assembly.GetExecutingAssembly(), "EditorEX.Resources.heart.png");
                var tributesCreditsIcon = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(tributesCreditsImage);
                _tributesCreditsButton = UIUtil.CreateNavbarButton(_container, tributesCreditsIcon, _projectButtonsGameObject.transform, HandleTributesCreditsButtonPressed);
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
