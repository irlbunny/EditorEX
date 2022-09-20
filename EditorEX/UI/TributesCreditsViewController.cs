using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI
{
    internal class TributesCreditsViewController : BeatmapEditorViewController
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

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                var lilyImage = BeatSaberMarkupLanguage.Utilities.GetResource(Assembly.GetExecutingAssembly(), "EditorEX.Resources.lily.jpg");
                CreateTextObject("TributesHeaderText", "<size=150%>Tributes:", new(-780f, 480f), new(200f, 80f));
                CreateImageObject("LilyImage", BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(lilyImage), new(-750f, 300f), new(256f, 256f));
                CreateTextObject("LilyHeaderText", "<size=125%>Lily/Lone/Lonely/LonelyCen", new(-375f, 375f), new(450f, 50f));
                CreateTextObject("LilyTributeText",
                    "<size=60%>Lily was an amazing lighter, but not only that, an amazing friend as well. She was honestly one of the nicest people I had ever met, if I had the chance to meet her again, I would do so again and again.\n" +
                    "She was honestly the most creative person in the Beat Saber community, I hope that she will now be resting happily forever. I hope we meet again sometime, thank you for being my friend, Lily! -Kaitlyn\n\n" +
                    "Rest In Peace, Lily. I (and many others) will miss you. 2001-2022",
                    new(400f, 5f), new(2000f, 650f));
                CreateTextObject("CreditHeaderText", "<size=75%>Thank you, to everyone who has contributed to EditorEX!", new(-580f, 90f), new(600f, 80f));
            }
        }

        private GameObject CreateTextObject(string name, string text, Vector2 localPosition, Vector2 sizeDelta)
        {
            var textGO = new GameObject(name, typeof(RectTransform));
            textGO.transform.SetParent(transform, false);
            textGO.transform.localPosition = localPosition;
            StartCoroutine(SetSizeDeltaCoroutine(textGO.GetComponent<RectTransform>(), sizeDelta));
            var textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.font = BeatSaberMarkupLanguage.BeatSaberUI.MainTextFont;
            textTMP.text = text;
            return textGO;
        }

        private GameObject CreateImageObject(string name, Sprite image, Vector2 localPosition, Vector2 sizeDelta)
        {
            var imageGO = new GameObject(name, typeof(RectTransform));
            imageGO.transform.SetParent(transform, false);
            imageGO.transform.localPosition = localPosition;
            StartCoroutine(SetSizeDeltaCoroutine(imageGO.GetComponent<RectTransform>(), sizeDelta));
            imageGO.AddComponent<Image>().sprite = image;
            return imageGO;
        }

        // Weird hack, thanks Unity for nothing.
        private IEnumerator SetSizeDeltaCoroutine(RectTransform rectTransform, Vector2 sizeDelta)
        {
            yield return new WaitForEndOfFrame();
            rectTransform.sizeDelta = sizeDelta;
        }
    }
}
