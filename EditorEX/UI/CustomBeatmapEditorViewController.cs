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

        private void Awake()
        {
            gameObject.SetActive(false);
            gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<GraphicRaycaster>();
            _viewRelationDataAccessor(this) = new();
        }

        protected GameObject CreateTextGameObject(string name, string text, Vector2 localPosition, Vector2 sizeDelta)
        {
            var textGO = new GameObject(name, typeof(RectTransform));
            textGO.transform.SetParent(transform, false);
            textGO.transform.localPosition = localPosition;
            StartCoroutine(DelayedSizeDelta(textGO.GetComponent<RectTransform>(), sizeDelta));
            var textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.font = BeatSaberMarkupLanguage.BeatSaberUI.MainTextFont;
            textTMP.text = text;
            return textGO;
        }

        protected GameObject CreateImageGameObject(string name, Sprite image, Vector2 localPosition, Vector2 sizeDelta)
        {
            var imageGO = new GameObject(name, typeof(RectTransform));
            imageGO.transform.SetParent(transform, false);
            imageGO.transform.localPosition = localPosition;
            StartCoroutine(DelayedSizeDelta(imageGO.GetComponent<RectTransform>(), sizeDelta));
            imageGO.AddComponent<Image>().sprite = image;
            return imageGO;
        }

        // Weird hack, thanks Unity, you stink!
        private IEnumerator DelayedSizeDelta(RectTransform rectTransform, Vector2 sizeDelta)
        {
            yield return new WaitForEndOfFrame();
            rectTransform.sizeDelta = sizeDelta;
        }
    }
}
