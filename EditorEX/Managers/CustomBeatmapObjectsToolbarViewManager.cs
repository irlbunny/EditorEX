using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using HarmonyLib;
using HMUI;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.Managers
{
    internal class CustomBeatmapObjectsToolbarViewManager : IInitializable, IDisposable
    {
        private static AccessTools.FieldRef<EditBeatmapLevelNavigationViewController, BeatmapObjectsToolbarView> _beatmapObjectsToolbarViewAccessor =
            AccessTools.FieldRefAccess<EditBeatmapLevelNavigationViewController, BeatmapObjectsToolbarView>("_beatmapObjectsToolbarView");
        private static AccessTools.FieldRef<BeatmapObjectsToolbarView, Toggle> _noteAToggleAccessor =
            AccessTools.FieldRefAccess<BeatmapObjectsToolbarView, Toggle>("_noteAToggle");
        private static AccessTools.FieldRef<BeatmapObjectsToolbarView, Toggle> _noteBToggleAccessor =
            AccessTools.FieldRefAccess<BeatmapObjectsToolbarView, Toggle>("_noteBToggle");

        private readonly Config _config;
        private readonly ColorManager _colorManager;
        private readonly BeatmapObjectsToolbarView _beatmapObjectsToolbarView;
        private readonly ImageView _noteAImageView, _noteBImageView;
        private readonly Sprite _originalNoteASprite, _originalNoteBSprite;
        private readonly Sprite _customNoteSprite;

        public CustomBeatmapObjectsToolbarViewManager(
            Config config,
            ColorManager colorManager,
            EditBeatmapLevelNavigationViewController editBeatmapLevelNavigationViewController)
        {
            _config = config;
            _colorManager = colorManager;
            _beatmapObjectsToolbarView = _beatmapObjectsToolbarViewAccessor(editBeatmapLevelNavigationViewController);
            _noteAImageView = _noteAToggleAccessor(_beatmapObjectsToolbarView).transform.Find("Background/Icon").GetComponent<ImageView>();
            _noteBImageView = _noteBToggleAccessor(_beatmapObjectsToolbarView).transform.Find("Background/Icon").GetComponent<ImageView>();
            _originalNoteASprite = _noteAImageView.sprite;
            _originalNoteBSprite = _noteBImageView.sprite;
            _customNoteSprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("EditorEX.Resources.note.png");
        }

        public void Initialize()
        {
            _config.Updated += Config_Updated;

            EnableEnvironmentColors(_config.UseColorScheme);
        }

        public void Dispose()
        {
            EnableEnvironmentColors(false); // Reset to default state.
        }

        private void Config_Updated(Config config)
        {
            EnableEnvironmentColors(_config.UseColorScheme);
        }

        public void EnableEnvironmentColors(bool enabled)
        {
            if (enabled)
            {
                _noteAImageView.sprite = _customNoteSprite;
                _noteBImageView.sprite = _customNoteSprite;
                _noteAImageView.color = _colorManager.ColorForType(ColorType.ColorA);
                _noteBImageView.color = _colorManager.ColorForType(ColorType.ColorB);
            }
            else
            {
                _noteAImageView.sprite = _originalNoteASprite;
                _noteBImageView.sprite = _originalNoteBSprite;
                _noteAImageView.color = Color.white;
                _noteBImageView.color = Color.white;
            }
        }
    }
}
