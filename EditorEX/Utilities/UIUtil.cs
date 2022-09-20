using BeatmapEditor3D;
using HMUI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.Utilities
{
    internal static class UIUtil
    {
        /// <summary>
        /// Agnostic class for image labels and text labels for UI elements.
        /// </summary>
        public class Label
        {
            public enum LabelType
            {
                Image,
                Text
            }

            public LabelType Type;

            private Sprite _sprite;
            private string _text;

            public Sprite GetSprite()
            {
                if (_sprite == null)
                    throw new InvalidOperationException($"Trying to get sprite on label of type: {Type}");

                return _sprite;
            }

            public string GetText()
            {
                if (_text == null)
                    throw new InvalidOperationException($"Trying to get text on label of type: {Type}");

                return _text;
            }

            public Label(string text)
            {
                Type = LabelType.Text;

                _text = text;
            }

            public Label(Sprite sprite)
            {
                Type = LabelType.Image;

                _sprite = sprite;
            }
        }

        public static TMP_InputField CreateInputField(Label label, Transform parent, Vector2 localPosition, Vector2 sizeDelta, UnityAction<string> action)
        {
            var templateObject = GameObject.Find("Wrapper/ViewControllers/EditBeatmapViewController/BeatmapInfoContainer/SongInfo/SongNameInput");
            if (templateObject == null)
            {
                templateObject = GameObject.Find("Wrapper/ScreenSystem/ScreenContainer/MainScreen/EditBeatmapViewController/BeatmapInfoContainer/SongInfo/SongNameInput");
                if (templateObject == null)
                    throw new NullReferenceException("Failed to get InputField template!");
            }

            var inputFieldGameObject = UnityEngine.Object.Instantiate(templateObject, parent, false);
            inputFieldGameObject.transform.localPosition = localPosition;
            inputFieldGameObject.GetComponent<RectTransform>().sizeDelta = sizeDelta;

            var labelGameObject = inputFieldGameObject.transform.Find("Label").gameObject;
            if (label.Type == Label.LabelType.Text)
                labelGameObject.GetComponent<TextMeshProUGUI>().text = label.GetText();
            else
            {
                UnityEngine.Object.DestroyImmediate(labelGameObject.GetComponent<CurvedTextMeshPro>());

                var labelImage = labelGameObject.AddComponent<Image>();
                labelImage.preserveAspect = true;
                labelImage.sprite = label.GetSprite();

                labelGameObject.AddComponent<ContentSizeFitter>();
            }

            var inputField = inputFieldGameObject.transform.Find("InputField").GetComponent<TMP_InputField>();
            inputField.text = string.Empty;
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(action);

            UnityEngine.Object.Destroy(inputField.GetComponent<StringInputFieldValidator>());
            UnityEngine.Object.Destroy(inputFieldGameObject.transform.Find("ModifiedHint").gameObject);

            return inputField;
        }

        public static Button CreateNavbarButton(DiContainer container, Sprite icon, Transform parent, UnityAction action)
        {
            var templateObject = GameObject.Find("Wrapper/ScreenSystem/ScreenContainer/Navbar/NavbarScreen/EditorControlsViewController/ProjectButtons/BeatmapsListButton");
            if (templateObject == null)
                throw new NullReferenceException("Failed to get Button template!");

            var navbarButtonGameObject = container.InstantiatePrefab(templateObject);
            navbarButtonGameObject.transform.SetParent(parent, false);

            if (icon != null)
                navbarButtonGameObject.transform.Find("Icon").gameObject.GetComponent<ImageView>().sprite = icon;

            var navbarButton = navbarButtonGameObject.GetComponent<NoTransitionsButton>();
            navbarButton.onClick.RemoveAllListeners();
            navbarButton.onClick.AddListener(action);
            navbarButton.enabled = true;

            return navbarButton;
        }
    }
}
