using BeatmapEditor3D;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EditorEX.Utilities
{
    internal static class UIUtils
    {
        //Agnostic class for image labels and text labels for ui elements.
        internal class Label
        {
            internal enum Type
            {
                Image,
                Text,
            };
            private Sprite _sprite;
            private string _text;

            internal Type type;

            internal Sprite GetSprite()
            {
                if(_sprite == null)
                {
                    throw new InvalidOperationException("Trying to get sprite on Label of type: " + type.ToString());
                }
                return _sprite;
            }

            internal string GetText()
            {
                if (_text == null)
                {
                    throw new InvalidOperationException("Trying to get text on Label of type: " + type.ToString());
                }
                return _text;
            }

            internal Label(string text)
            {
                _text = text;
                type = Type.Text;
            }

            internal Label(Sprite sprite)
            {
                _sprite = sprite;
                type = Type.Image;
            }
        }
        public static TMP_InputField CreateInputField(Label label, Transform parent, string value, Vector2 localPosition, Vector2 sizeDelta, UnityAction<string> enterAction)
        {
            GameObject templateObject = GameObject.Find("Wrapper/ViewControllers/EditBeatmapViewController/BeatmapInfoContainer/SongInfo/SongNameInput");
            if (templateObject == null)
            {
                templateObject = GameObject.Find("Wrapper/ScreenSystem/ScreenContainer/MainScreen/EditBeatmapViewController/BeatmapInfoContainer/SongInfo/SongNameInput");
                if (templateObject == null)
                {
                    throw new NullReferenceException("Failed to get InputField template");
                }
            }

            GameObject newInputField = GameObject.Instantiate(templateObject, parent, false);
            newInputField.transform.localPosition = localPosition;
            newInputField.GetComponent<RectTransform>().sizeDelta = sizeDelta;

            GameObject labelObj = newInputField.transform.Find("Label").gameObject;

            if (label.type == Label.Type.Text)
            {
                labelObj.GetComponent<TextMeshProUGUI>().text = label.GetText();
            }
            else
            {
                GameObject.DestroyImmediate(labelObj.GetComponent<CurvedTextMeshPro>());
                Image img = labelObj.AddComponent<Image>();
                img.preserveAspect = true;
                img.sprite = label.GetSprite();
                labelObj.AddComponent<ContentSizeFitter>();

            }

            TMP_InputField inputField = newInputField.transform.Find("InputField").GetComponent<TMP_InputField>();
            inputField.text = value;
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(enterAction);
            GameObject.Destroy(inputField.GetComponent<StringInputFieldValidator>());

            GameObject.Destroy(newInputField.transform.Find("ModifiedHint").gameObject);

            return inputField;
        }
    }
}
