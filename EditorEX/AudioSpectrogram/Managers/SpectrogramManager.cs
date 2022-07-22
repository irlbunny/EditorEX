using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Visuals;
using DSPLib;
using EditorEX.AudioSpectrogram.Colors;
using System;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using Zenject;

namespace EditorEX.AudioSpectrogram.Managers
{
    internal class SpectrogramManager : IInitializable, ITickable, IDisposable
    {
        private const float SECONDS_PER_CHUNK = 5f;
        private const uint SAMPLE_COUNT = 512;
        private const int GRADIENT_FACTOR = 25;

        private readonly Config _config;
        private readonly SignalBus _signalBus;
        private readonly BeatmapObjectPlacementHelper _beatmapObjectPlacementHelper;
        private readonly IBeatmapLevelState _beatmapLevelState;
        private readonly ILevelEditorState _levelEditorState;
        private readonly IBeatmapDataModel _beatmapDataModel;
        private readonly IColorData _colorData;

        private bool _visible = true;
        private GameObject _spectrogramContainer;
        private GameObject[] _spectrogramChunks;
        private Texture2D[] _bandColors;

        public SpectrogramManager(Config config, SignalBus signalBus,
            BeatmapObjectPlacementHelper beatmapObjectPlacementHelper, IBeatmapLevelState beatmapLevelState,
            ILevelEditorState levelEditorState, IBeatmapDataModel beatmapDataModel,
            IColorData colorData)
        {
            _config = config;
            _signalBus = signalBus;
            _beatmapObjectPlacementHelper = beatmapObjectPlacementHelper;
            _beatmapLevelState = beatmapLevelState;
            _levelEditorState = levelEditorState;
            _beatmapDataModel = beatmapDataModel;
            _colorData = colorData;
        }

        public void Initialize()
        {
            _config.Updated += Config_Updated;
            _signalBus.Subscribe<LevelEditorStateZenModeUpdatedSignal>(HandleLevelEditorStateZenModeUpdated);

            InitializeChunks(_beatmapDataModel.audioClip);
            SetVisible(!_levelEditorState.zenMode && _config.ShowSpectrogram);
        }

        private void InitializeChunks(AudioClip audioClip)
        {
            var fft = new FFT(); fft.Initialize(SAMPLE_COUNT);

            // Average all channels together into one set of samples.
            var multiChannelSamples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(multiChannelSamples, 0);

            var processedSamples = new double[audioClip.samples];
            for (var i = 0; i < multiChannelSamples.Length; i++)
                processedSamples[i / audioClip.channels] = multiChannelSamples[i] / audioClip.channels;

            var samples = (int) SAMPLE_COUNT / 2;
            var samplesPerChunk = audioClip.frequency * SECONDS_PER_CHUNK;
            var columnsPerChunk = (int) samplesPerChunk / samples;
            var sampleOffset = samplesPerChunk / columnsPerChunk;
            var sampleChunk = new double[SAMPLE_COUNT];

            // Initialize texture data.
            var waveformChunks = (int) Math.Ceiling(audioClip.length / SECONDS_PER_CHUNK); // Number of "chunks" or texture segments produced.
            var waveformBandVolumes = new float[columnsPerChunk * waveformChunks][];
            var waveformBandColors = new Texture2D[waveformChunks];
            var waveformBandColorData = new NativeArray<Color32>[waveformChunks];
            for (var i = 0; i < waveformChunks; i++)
            {
                waveformBandColors[i] = new Texture2D(columnsPerChunk, samples + 1);
                waveformBandColorData[i] = waveformBandColors[i].GetRawTextureData<Color32>();
            }

            // Pre-processing.
            var windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.BH92, SAMPLE_COUNT);
            var scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

            var compFactors = new double[SAMPLE_COUNT / 2];
            for (var y = 0; y < compFactors.Length; y++)
                compFactors[y] = Math.Sqrt((y + 0.25d) * (8d / SAMPLE_COUNT));

            for (var chunkId = 0; chunkId < waveformChunks; chunkId++)
            {
                var bandColors = new Color[columnsPerChunk][];
                for (var k = 0; k < columnsPerChunk; k++)
                {
                    var i = (chunkId * columnsPerChunk) + k;
                    if ((i * sampleOffset) + SAMPLE_COUNT > processedSamples.Length)
                    {
                        waveformBandVolumes[i] = new float[compFactors.Length + 1];

                        // There are no more samples, generate a blank texture.
                        bandColors[k] = Enumerable.Repeat(new Color(_colorData.Data[0, 0], _colorData.Data[0, 1], _colorData.Data[0, 2]), compFactors.Length + 1).ToArray();
                        continue;
                    }

                    // Load the samples into a chunk and perform FFT magnitude.
                    Buffer.BlockCopy(processedSamples, (int) (i * sampleOffset) * sizeof(double), sampleChunk, 0, ((int) SAMPLE_COUNT) * sizeof(double));
                    var fftSpectrum = fft.Execute(DSP.Math.Multiply(sampleChunk, windowCoefs));
                    var scaledFftSpectrum = DSP.Math.Multiply(DSP.ConvertComplex.ToMagnitude(fftSpectrum), scaleFactor);

                    // Compensate for frequency bin.
                    for (var y = 0; y < compFactors.Length; y++)
                        scaledFftSpectrum[y] *= compFactors[y];

                    waveformBandVolumes[i] = scaledFftSpectrum.Select(value =>
                    {
                        if (value >= Math.Pow(Math.E, -255d / GRADIENT_FACTOR))
                            return (float) ((Math.Log(value) + (255d / GRADIENT_FACTOR)) * GRADIENT_FACTOR) / 128f;

                        return 0f;
                    }).ToArray();

                    bandColors[k] = waveformBandVolumes[i].Select(value =>
                    {
                        var lerp = Mathf.InverseLerp(0f, 2f, value);
                        var r = _colorData.Data[(int) Math.Round(255f * lerp), 0];
                        var g = _colorData.Data[(int) Math.Round(255f * lerp), 1];
                        var b = _colorData.Data[(int) Math.Round(255f * lerp), 2];

                        return new Color(r, g, b, 1f);
                    }).ToArray();
                }

                var index = 0;
                for (var y = 0; y < bandColors[0].Length; y++)
                {
                    for (var x = 0; x < bandColors.Length; x++)
                        waveformBandColorData[chunkId][index++] = bandColors[x][y];
                }

                waveformBandColors[chunkId].Apply(true);
            }

            _bandColors = waveformBandColors;
        }

        public void Tick()
        {
            if (!_visible)
                return;

            // startTimeBeats is 5 beats behind the current beat.
            var startTime = _beatmapDataModel.bpmData.BeatToTime(_beatmapLevelState.beat);
            var endTime = _beatmapDataModel.bpmData.BeatToTime(_beatmapLevelState.beat + 16f);

            var firstSpectrogram = (int) Math.Floor(startTime / SECONDS_PER_CHUNK) - 1;
            var lastSpectrogram = (int) Math.Ceiling(endTime / SECONDS_PER_CHUNK);

            firstSpectrogram = Math.Max(0, firstSpectrogram);
            lastSpectrogram = Math.Min(lastSpectrogram, _bandColors.Length - 1);

            if (_spectrogramContainer == null && _spectrogramChunks == null && _bandColors != null)
            {
                // Load AssetBundle for the Spectrogram material.
                var spectrogramBundle = AssetBundle.LoadFromMemory(BeatSaberMarkupLanguage.Utilities.GetResource(Assembly.GetExecutingAssembly(), "EditorEX.Resources.spectrogram.bundle"));
                var baseMaterial = spectrogramBundle.LoadAsset<Material>("spectrogram");
                spectrogramBundle.Unload(false);

                _spectrogramContainer = new GameObject("SpectrogramContainer");
                _spectrogramContainer.transform.position = new(-7f + _config.SpectrogramXOffset, 0f, 0f);
                _spectrogramContainer.transform.localScale = new(_config.SpectrogramWidth, 1f, 1f);

                // Create a clone of BeatmapObjectsContainer beatline.
                var beatline = Resources.FindObjectsOfTypeAll<BeatmapObjectBeatLine>().Where(x => x.name == "CurrentBeatline").First().gameObject;
                var newBeatline = UnityEngine.Object.Instantiate(beatline);
                newBeatline.transform.parent = _spectrogramContainer.transform;
                newBeatline.transform.localPosition = new(0f, .02f, 0f);
                newBeatline.transform.transform.localScale = new(5f, 1f, 1f);
                newBeatline.name = "CurrentBeatline";

                _spectrogramChunks = new GameObject[_bandColors.Length];

                for (var i = 0; i < _bandColors.Length; i++)
                {
                    var newMaterial = UnityEngine.Object.Instantiate(baseMaterial);
                    newMaterial.color = new(1f, 1f, 1f, 0f);
                    newMaterial.SetTexture("_Tex", _bandColors[i]);

                    var spectrogramChunk = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    spectrogramChunk.GetComponent<MeshRenderer>().material = newMaterial;
                    spectrogramChunk.transform.parent = _spectrogramContainer.transform;
                    spectrogramChunk.transform.Rotate(new(0f, 90f, 0f));
                    spectrogramChunk.name = $"SpectrogramChunk{i}";

                    // Primitive plane is 10x10, changes the size to (5 seconds of distance) x 5.
                    spectrogramChunk.transform.localScale = new(_beatmapObjectPlacementHelper.timeToZDistanceScale * SECONDS_PER_CHUNK / 10f, 1f, -.5f);
                    spectrogramChunk.SetActive(false);

                    _spectrogramChunks[i] = spectrogramChunk;
                }
            }

            // Render the spectrograms.
            for (var i = 0; i < firstSpectrogram; i++)
                _spectrogramChunks[i].SetActive(false);
            for (var i = lastSpectrogram; i < _spectrogramChunks.Length; i++)
                _spectrogramChunks[i].SetActive(false);
            for (var i = firstSpectrogram; i < lastSpectrogram; i++)
            {
                var z = _beatmapObjectPlacementHelper.TimeToPosition(((float) (i * SECONDS_PER_CHUNK)) - startTime);

                _spectrogramChunks[i].transform.localPosition = new(0f, .01f, z + (_beatmapObjectPlacementHelper.timeToZDistanceScale * SECONDS_PER_CHUNK / 2f));
                _spectrogramChunks[i].transform.localScale = new(_beatmapObjectPlacementHelper.timeToZDistanceScale * SECONDS_PER_CHUNK / 10f, 1f, -.5f);
                _spectrogramChunks[i].SetActive(true);
            }
        }

        public void Dispose()
        {
            _config.Updated -= Config_Updated;
            _signalBus.TryUnsubscribe<LevelEditorStateZenModeUpdatedSignal>(HandleLevelEditorStateZenModeUpdated);

            UnityEngine.Object.Destroy(_spectrogramContainer); // When we destroy the container, we're also destroying all chunks.

            foreach (var bandColor in _bandColors)
            {
                UnityEngine.Object.Destroy(bandColor);
            }
        }

        private void Config_Updated(Config config)
        {
            SetVisible(!_levelEditorState.zenMode && _config.ShowSpectrogram);

            if (_spectrogramContainer != null)
            {
                _spectrogramContainer.transform.position = new(-7f + _config.SpectrogramXOffset, 0f, 0f);
                _spectrogramContainer.transform.localScale = new(_config.SpectrogramWidth, 1f, 1f);
            }
        }

        private void HandleLevelEditorStateZenModeUpdated()
        {
            SetVisible(!_levelEditorState.zenMode && _config.ShowSpectrogram);
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;

            if (_spectrogramContainer != null)
                _spectrogramContainer.SetActive(visible);
        }
    }
}
