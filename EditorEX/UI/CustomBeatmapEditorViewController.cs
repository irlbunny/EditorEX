using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using HarmonyLib;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI
{
    internal class CustomBeatmapEditorViewController : BeatmapEditorViewController
    {
        private static AccessTools.FieldRef<BeatmapEditorViewController, BeatmapEditorViewRelationData> _viewRelationDataAccessor =
            AccessTools.FieldRefAccess<BeatmapEditorViewController, BeatmapEditorViewRelationData>("_viewRelationData");

        protected CustomBeatmapEditorViewController()
        {
            _viewRelationDataAccessor(this) = new();
        }

        protected GameObject CreateTextGameObject(string name, string text, Vector2 localPosition, Vector2 sizeDelta)
        {
            var textGameObject = new GameObject(name, typeof(RectTransform));
            textGameObject.transform.SetParent(transform, false);
            textGameObject.transform.localPosition = localPosition;
            StartCoroutine(DelayedSizeDelta(textGameObject.GetComponent<RectTransform>(), sizeDelta));
            var textTMP = textGameObject.AddComponent<TextMeshProUGUI>();
            textTMP.font = BeatSaberMarkupLanguage.BeatSaberUI.MainTextFont;
            textTMP.text = text;
            return textGameObject;
        }

        protected GameObject CreateImageGameObject(string name, Sprite image, Vector2 localPosition, Vector2 sizeDelta)
        {
            var imageGameObject = new GameObject(name, typeof(RectTransform));
            imageGameObject.transform.SetParent(transform, false);
            imageGameObject.transform.localPosition = localPosition;
            StartCoroutine(DelayedSizeDelta(imageGameObject.GetComponent<RectTransform>(), sizeDelta));
            imageGameObject.AddComponent<Image>().sprite = image;
            return imageGameObject;
        }

        // Weird hack, thanks Unity, you stink!
        private IEnumerator DelayedSizeDelta(RectTransform rectTransform, Vector2 sizeDelta)
        {
            yield return new WaitForEndOfFrame();
            rectTransform.sizeDelta = sizeDelta;
        }
    }
}
