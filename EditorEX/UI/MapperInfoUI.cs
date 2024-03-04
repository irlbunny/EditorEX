using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatSaberMarkupLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Zenject;

namespace EditorEX.UI
{
	internal class MapperInfoUI : IInitializable, IDisposable, ITickable
	{
		private RectTransform _textRoot;
		private TextMeshProUGUI _ratioText;
		private TextMeshProUGUI _timePercentageText;
		private TextMeshProUGUI _percentageMappedText;

		private BeatmapEditorAudioTimeSyncController _beatmapEditorAudioTimeSyncController;
		private BeatmapLevelDataModel _beatmapLevelDataModel;
		private BeatmapDataModel _beatmapDataModel;

		private MapperInfoUI(BeatmapEditorAudioTimeSyncController beatmapEditorAudioTimeSyncController, BeatmapLevelDataModel beatmapLevelDataModel, BeatmapDataModel beatmapDataModel)
		{
			_beatmapEditorAudioTimeSyncController = beatmapEditorAudioTimeSyncController;
			_beatmapLevelDataModel = beatmapLevelDataModel;
			_beatmapDataModel = beatmapDataModel;
		}

		public void Dispose()
		{
			GameObject.Destroy(_textRoot.gameObject);
		}

		public void Initialize()
		{
			var root = GameObject.Find("Wrapper/ScreenSystem/ScreenContainer").transform;
			_textRoot = new GameObject("Info").gameObject.AddComponent<RectTransform>();
			_textRoot.SetParent(root, false);
			_textRoot.anchorMin = new Vector2(0.7f, 0.9f);
			_textRoot.anchorMax = new Vector2(1f, 1f);
			_textRoot.SetAsFirstSibling();

			_ratioText = BeatSaberUI.CreateText(_textRoot, "Ratio L/R: 1", Vector2.zero);
			_timePercentageText = BeatSaberUI.CreateText(_textRoot, "Time %: 0%", Vector2.zero);
			_percentageMappedText = BeatSaberUI.CreateText(_textRoot, "% Mapped: 0%", Vector2.zero);

			_ratioText.fontSize = 30f;
			_timePercentageText.fontSize = 30f;
			_timePercentageText.rectTransform.anchoredPosition = new Vector2(0f, -30f);
			_percentageMappedText.fontSize = 30f;
			_percentageMappedText.rectTransform.anchoredPosition = new Vector2(0f, -60f);
		}

		private int _lastFrameFrameCount = 0;
		public void Tick()
		{
			if (_timePercentageText != null && _beatmapEditorAudioTimeSyncController != null)
			{
				var time = _beatmapEditorAudioTimeSyncController.songTime / _beatmapEditorAudioTimeSyncController.songLength;
				_timePercentageText.text = "Time %: " + (time * 100f).ToString("0.#") + "%";
			}

			var allObjectsCount = _beatmapLevelDataModel.beatmapObjectsFrames.SelectMany(x => x.frameData).Count(); // Need to find an event to subscribe to on place
			if (_lastFrameFrameCount != allObjectsCount)
			{
				if (_percentageMappedText != null)
				{
					float percentMapped = CalculatePercentMapped() * 100f;
					_percentageMappedText.text = $"% Mapped: {percentMapped.ToString("0.##")}%";
				}

				if (_ratioText != null)
				{
					float ratio = CalculateRatio();
					_ratioText.text = $"Ratio L/R: {ratio.ToString("0.##")}";
				}
			}
			_lastFrameFrameCount = allObjectsCount;
		}

		private float CalculatePercentMapped()
		{
			try
			{
				//WIP
				List<(float, float)> mappedSections = new();
				float lastFrameBeat = 0f;
				float lastNewFrameBeat = 0f;
				foreach (var framePair in _beatmapLevelDataModel._beatmapObjectsSortedCollection._beatFramesMap)
				{
					Plugin.Log.Info(framePair.Key.ToString());
					float beat = framePair.Key;
					var frame = framePair.Value;

					if (Mathf.Abs(beat - lastFrameBeat) > 3.0f)
					{
						if (lastFrameBeat != 0f)
						{
							mappedSections.Add((lastNewFrameBeat, lastFrameBeat));
						}
						lastNewFrameBeat = beat;
					}
					lastFrameBeat = beat;
				}

				float totalBeatsCounted = 0f;
				foreach (var pair in mappedSections)
				{
					Plugin.Log.Info(pair.Item1.ToString() + " - " + pair.Item2.ToString());
					totalBeatsCounted += Math.Abs(pair.Item1 - pair.Item2);
				}
				Plugin.Log.Info(totalBeatsCounted.ToString());
				Plugin.Log.Info(_beatmapDataModel.bpmData.totalBeats.ToString());
				return totalBeatsCounted / _beatmapDataModel.bpmData.totalBeats;
			}
			catch (Exception ex)
			{
				return -1f;
			}
		}

		private float CalculateRatio()
		{
			try
			{
				float leftNotes = 0;
				float rightNotes = 0;
				foreach (var frame in _beatmapLevelDataModel._beatmapObjectsSortedCollection._beatFramesMap)
				{
					foreach (var frameObject in frame.Value.frameData)
					{
						if (frameObject is NoteEditorData noteObject)
						{
							if (noteObject.type == ColorType.ColorA) leftNotes++;
							if (noteObject.type == ColorType.ColorB) rightNotes++;
						}

						if (frameObject is SliderEditorData sliderObject)
						{
							if (sliderObject.colorType == ColorType.ColorA) leftNotes++;
							if (sliderObject.colorType == ColorType.ColorB) rightNotes++;
						}
					}
				}
				return leftNotes / rightNotes;
			}
			catch (Exception ex)
			{
				return -1f;
			}
		}
	}
}
