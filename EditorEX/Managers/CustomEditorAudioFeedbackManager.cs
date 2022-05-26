using BeatmapEditor3D.Controller;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace EditorEX.Managers
{
    internal class CustomEditorAudioFeedbackManager : IInitializable, IDisposable
    {
        private static AccessTools.FieldRef<EditorAudioFeedbackController, AudioClip> _notePassedFeedbackAccessor =
            AccessTools.FieldRefAccess<EditorAudioFeedbackController, AudioClip>("_notePassedFeedback");

        private readonly Config _config;
        private readonly EditorAudioFeedbackController _editorAudioFeedbackController;
        private readonly AudioClip _originalNotePassedFeedback;

        public CustomEditorAudioFeedbackManager(Config config, EditorAudioFeedbackController editorAudioFeedbackController)
        {
            _config = config;
            _editorAudioFeedbackController = editorAudioFeedbackController;
            _originalNotePassedFeedback = _notePassedFeedbackAccessor(editorAudioFeedbackController);
        }

        public void Initialize()
        {
            _config.Updated += Config_Updated;

            EnableCustomNotePassedFeedback(_config.HideEnvironmentSpectrograms);
        }

        public void Dispose()
        {
            _config.Updated -= Config_Updated;

            EnableCustomNotePassedFeedback(false); // Reset to default state.
        }

        private void Config_Updated(Config config)
        {
            EnableCustomNotePassedFeedback(config.UseCustomNotePassedFeedback);
        }

        public void EnableCustomNotePassedFeedback(bool enabled)
        {
            if (enabled)
            {
                var filePath = Path.Combine(Plugin.DataPath, "NotePassedFeedback.ogg");
                if (File.Exists(filePath))
                {
                    using (var www = UnityWebRequestMultimedia.GetAudioClip(FileHelpers.GetEscapedURLForFilePath(filePath), AudioType.UNKNOWN))
                    {
                        var request = www.SendWebRequest();
                        while (!request.isDone)
                        { }

                        if (!www.isNetworkError)
                        {
                            var audioClip = DownloadHandlerAudioClip.GetContent(www);
                            if (audioClip != null && audioClip.loadState == AudioDataLoadState.Loaded)
                                _notePassedFeedbackAccessor(_editorAudioFeedbackController) = audioClip;
                        }
                    }
                }
            }
            else
                _notePassedFeedbackAccessor(_editorAudioFeedbackController) = _originalNotePassedFeedback;
        }
    }
}
