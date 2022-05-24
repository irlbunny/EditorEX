using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using DSPLib;
using EditorEX.AudioSpectrogram.Colors;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;

namespace EditorEX.AudioSpectrogram
{
    // NOTE: This code is heavily based upon ChroMapper's spectrogram generation.
    // Referenced implementation: https://github.com/Caeden117/ChroMapper/blob/98ce36c6471c56cf252214f0c9825f23c3f5265c/Assets/__Scripts/MapEditor/Audio/AudioManager.cs#L81
    internal class SpectrogramView : IDisposable
    {
        private static AccessTools.FieldRef<BeatmapObjectPlacementHelper, IBeatmapDataModel> _beatmapDataModelAccessor =
            AccessTools.FieldRefAccess<BeatmapObjectPlacementHelper, IBeatmapDataModel>("_beatmapDataModel");

        private const float SECONDS_PER_CHUNK = 5f;
        private const uint SAMPLE_COUNT = 512;

        private const int GRADIENT_FACTOR = 25;

        private AudioClip _audioClip;

        private IColorData _colorData;
        private Texture2D[] _bandColors;

        private bool _visible = true;

        private GameObject[] _spectrogramChunks;

        public GameObject Container { get; private set; }
        public static SpectrogramView Instance { get; set; }

        public SpectrogramView(AudioClip audioClip)
        {
            _audioClip = audioClip;
            _colorData = new InfernoColorData();

            // Average all channels together into one set of samples.
            var multiChannelSamples = new float[audioClip.samples * audioClip.channels];
            _audioClip.GetData(multiChannelSamples, 0);

            var processedSamples = new double[audioClip.samples];
            for (var i = 0; i < multiChannelSamples.Length; i++)
                processedSamples[i / audioClip.channels] = multiChannelSamples[i] / _audioClip.channels;

            // Basic AudioClip information.
            var numChannels = audioClip.channels;
            var numTotalSamples = audioClip.samples;
            var audioLength = audioClip.length;
            var sampleRate = audioClip.frequency;

            // Number of "chunks" or texture segments produced.
            var waveformChunks = (int) Math.Ceiling(audioLength / SECONDS_PER_CHUNK);

            var samples = (int) SAMPLE_COUNT / 2;
            var samplesPerChunk = sampleRate * SECONDS_PER_CHUNK;
            var columnsPerChunk = (int) samplesPerChunk / samples;
            var sampleOffset = samplesPerChunk / columnsPerChunk;

            // Initialize texture data.
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

            var hzStep = sampleRate / (float) SAMPLE_COUNT;

            // Perform the FFT.
            var fftSize = SAMPLE_COUNT;
            var fft = new FFT();
            fft.Initialize(fftSize);
            var sampleChunk = new double[fftSize];

            var bins = fftSize / 2;
            var compFactors = new double[bins];

            var scalingConstant = 8d / fftSize;
            for (var y = 0; y < bins; y++)
                compFactors[y] = Math.Sqrt((y + 0.25d) * scalingConstant);

            for (var chunkId = 0; chunkId < waveformChunks; chunkId++)
            {
                var bandColors = new Color[columnsPerChunk][];
                for (var k = 0; k < columnsPerChunk; k++)
                {
                    var i = (chunkId * columnsPerChunk) + k;

                    var curSampleSize = (int) fftSize;
                    if ((i * sampleOffset) + fftSize > processedSamples.Length)
                    {
                        waveformBandVolumes[i] = new float[bins + 1];

                        // There are no more samples, generate a blank texture.
                        bandColors[k] = Enumerable.Repeat(new Color(_colorData.Data[0, 0], _colorData.Data[0, 1], _colorData.Data[0, 2]), (int) bins + 1).ToArray();
                        continue;
                    }

                    // Load the samples into a chunk for FFT.
                    Buffer.BlockCopy(processedSamples, (int) (i * sampleOffset) * sizeof(double), sampleChunk, 0, curSampleSize * sizeof(double));

                    var scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);

                    // Perform FFT magnitude.
                    var fftSpectrum = fft.Execute(scaledSpectrumChunk);
                    var scaledFFTSpectrum = DSP.ConvertComplex.ToMagnitude(fftSpectrum);
                    scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

                    // Compensate for frequency bin.
                    for (var y = 0; y < bins; y++)
                        scaledFFTSpectrum[y] *= compFactors[y];

                    waveformBandVolumes[i] = scaledFFTSpectrum.Select(value =>
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
                var data = waveformBandColorData[chunkId];
                for (var y = 0; y < bandColors[0].Length; y++)
                {
                    for (var x = 0; x < bandColors.Length; x++)
                        data[index++] = bandColors[x][y];
                }

                // Render the spectrogram.
                var toRender = new float[columnsPerChunk][];
                Array.Copy(waveformBandVolumes, chunkId * columnsPerChunk, toRender, 0, columnsPerChunk);

                waveformBandColors[chunkId].Apply(true);
            }
            
            _bandColors = waveformBandColors;
        }

        public void RefreshView(float startTimeBeats, float endTimeBeats, BeatmapObjectPlacementHelper beatmapObjectPlacementHelper)
        {
            if (!_visible)
                return;

            // startTimeBeats is 5 beats behind the current beat.
            var startTime = _beatmapDataModelAccessor(beatmapObjectPlacementHelper).bpmData.BeatToTime(startTimeBeats + 5f);
            var endTime = _beatmapDataModelAccessor(beatmapObjectPlacementHelper).bpmData.BeatToTime(endTimeBeats);

            var firstSpectrogram = (int) Math.Floor(startTime / SECONDS_PER_CHUNK) - 1;
            var lastSpectrogram = (int) Math.Ceiling(endTime / SECONDS_PER_CHUNK);

            firstSpectrogram = Math.Max(0, firstSpectrogram);
            lastSpectrogram = Math.Min(lastSpectrogram, _bandColors.Length - 1);

            if (Container == null && _spectrogramChunks == null && _bandColors != null)
            {
                // Load AssetBundle for the Spectrogram material.
                var spectrogramBundle = AssetBundle.LoadFromMemory(BeatSaberMarkupLanguage.Utilities.GetResource(Assembly.GetExecutingAssembly(), "EditorEX.Resources.spectrogram.bundle"));
                var baseMaterial = spectrogramBundle.LoadAsset<Material>("spectrogram");
                spectrogramBundle.Unload(false);

                Container = new GameObject("SpectrogramContainer");
                Container.transform.localScale = new(Config.Instance.SpectrogramWidth, 1f, 1f);

                // Create a clone of BeatmapObjectsContainer beatline.
                var beatLine = GameObject.Find("Wrapper/BeatmapObjectsContainer/BeatGridContainer/CurrentBeatline");
                var newBeatLine = UnityEngine.Object.Instantiate(beatLine);
                newBeatLine.transform.parent = Container.transform;
                newBeatLine.transform.localPosition = new(0f, .02f, 0f);
                newBeatLine.transform.transform.localScale = new(5f, 1f, 1f);

                _spectrogramChunks = new GameObject[_bandColors.Length];

                for (var i = 0; i < _bandColors.Length; i++)
                {
                    var newMaterial = UnityEngine.Object.Instantiate(baseMaterial);
                    newMaterial.color = new(1f, 1f, 1f, 0f);
                    newMaterial.SetTexture("_Tex", _bandColors[i]);

                    var spectrogramChunk = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    spectrogramChunk.GetComponent<MeshRenderer>().material = newMaterial;
                    spectrogramChunk.transform.parent = Container.transform;
                    spectrogramChunk.transform.Rotate(new(0f, 90f, 0f));
                    spectrogramChunk.name = $"SpectrogramChunk{i}";

                    // Primitive Plane is 10x10, changes the size to (5 seconds of distance) x 5.
                    spectrogramChunk.transform.localScale = new(beatmapObjectPlacementHelper.timeToZDistanceScale * SECONDS_PER_CHUNK / 10f, 1f, -.5f);
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
                var z = beatmapObjectPlacementHelper.TimeToPosition(((float) (i * SECONDS_PER_CHUNK)) - startTime);
                _spectrogramChunks[i].transform.localPosition = new(0f, .01f, z + (beatmapObjectPlacementHelper.timeToZDistanceScale * SECONDS_PER_CHUNK / 2f));
                _spectrogramChunks[i].transform.localScale = new(beatmapObjectPlacementHelper.timeToZDistanceScale * SECONDS_PER_CHUNK / 10f, 1f, -.5f);
                _spectrogramChunks[i].SetActive(true);
            }
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (!visible)
            {
                for (var i = 0; i < _spectrogramChunks.Length; i++)
                    _spectrogramChunks[i].SetActive(false);
            }
        }

        public void Dispose()
        {
            // Destroy all chunks before creating a new SpectrogramView.
            for (var i = 0; i < _spectrogramChunks.Length; i++)
                UnityEngine.Object.Destroy(_spectrogramChunks[i]);
        }
    }
}
