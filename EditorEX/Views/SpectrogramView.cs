using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using UnityEngine;
using DSPLib;
using EditorEX.Views.SpectrogramColors;
using Unity.Collections;
using System.Reflection;

namespace EditorEX.Views
{
    public class SpectrogramView
    {
        private AudioClip audioClip;
        private Texture2D[] bandColors;

        private GameObject[] spectrogramChunks;

        private bool visible = true;

        public readonly float secondsPerChunk = 5;
        private readonly uint sampleCount = 512u;

        public SpectrogramView(AudioClip audioClip)
        {
            // NOTE: This code is heavily based upon Chromapper's spectrogram generation
            // Refrenced implementation: https://github.com/Caeden117/ChroMapper/blob/98ce36c6471c56cf252214f0c9825f23c3f5265c/Assets/__Scripts/MapEditor/Audio/AudioManager.cs#L81
            this.audioClip = audioClip;

            // Average all channels together into one set of samples
            float[] multiChannelSamples = new float[audioClip.samples * audioClip.channels];
            this.audioClip.GetData(multiChannelSamples, 0);
            double[] processedSamples = new double[audioClip.samples];
            for (var i = 0; i < multiChannelSamples.Length; i++)
                processedSamples[i / audioClip.channels] = multiChannelSamples[i] / this.audioClip.channels;

            // Basic AudioClip information
            var numChannels = audioClip.channels;
            var numTotalSamples = audioClip.samples;
            var clipLength = audioClip.length;
            var sampleRate = audioClip.frequency;

            // Number of "chunks" or texture segments produced
            var waveformChunks = (int)Math.Ceiling(clipLength / this.secondsPerChunk);

            var samples = (int)sampleCount / 2;
            var samplesPerChunk = sampleRate * secondsPerChunk;
            var columnsPerChunk = (int)samplesPerChunk / samples;
            var sampleOffset = samplesPerChunk / columnsPerChunk;

            // Initialize texture data
            var waveformBandVolumes = new float[columnsPerChunk * waveformChunks][];
            var waveformBandColors = new Texture2D[waveformChunks];
            var waveformBandCData = new NativeArray<Color32>[waveformChunks];
            for (var i = 0; i < waveformChunks; i++)
            {
                waveformBandColors[i] = new Texture2D(columnsPerChunk, samples + 1);
                waveformBandCData[i] = waveformBandColors[i].GetRawTextureData<Color32>();
            }

            // PreProcessing
            var windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.BH92, sampleCount);
            var scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

            var hzStep = sampleRate / (float)sampleCount;

            // Perform the Fast Fourier Transform
            var fftSize = sampleCount;
            var fft = new FFT();
            fft.Initialize(fftSize);
            var sampleChunk = new double[fftSize];

            var bins = fftSize / 2;
            var compFactors = new double[bins];

            var scalingConstant = 8d / fftSize;
            for (var y = 0; y < bins; y++) compFactors[y] = Math.Sqrt((y + 0.25) * scalingConstant);

            for (var chunkId = 0; chunkId < waveformChunks; chunkId++)
            {
                var bandColors = new Color[columnsPerChunk][];
                for (var k = 0; k < columnsPerChunk; k++)
                {
                    var i = (chunkId * columnsPerChunk) + k;

                    var curSampleSize = (int)fftSize;
                    if ((i * sampleOffset) + fftSize > processedSamples.Length)
                    {
                        waveformBandVolumes[i] = new float[bins + 1];

                        // There are no more samples, generate a blank texture
                        var r = Inferno.data[0, 0];
                        var g = Inferno.data[0, 1];
                        var b = Inferno.data[0, 2];
                        bandColors[k] = Enumerable.Repeat(new Color(r, g, b), (int)bins + 1)
                            .ToArray();
                        continue;
                    }

                    // Load the samples into a chunk for FFT
                    Buffer.BlockCopy(processedSamples, (int)(i * sampleOffset) * sizeof(double), sampleChunk, 0, curSampleSize * sizeof(double));

                    var scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);

                    // Perform FFT magnitude
                    var fftSpectrum = fft.Execute(scaledSpectrumChunk);
                    var scaledFFTSpectrum = DSP.ConvertComplex.ToMagnitude(fftSpectrum);
                    scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

                    // Compensate for frequency bin
                    for (var y = 0; y < bins; y++) scaledFFTSpectrum[y] *= compFactors[y];

                    var gradientFactor = 25;
                    waveformBandVolumes[i] = scaledFFTSpectrum.Select(it =>
                    {
                        if (it >= Math.Pow(Math.E, -255d / gradientFactor))
                            return (float)((Math.Log(it) + (255d / gradientFactor)) * gradientFactor) / 128f;
                        return 0f;
                    }).ToArray();
                    bandColors[k] = waveformBandVolumes[i].Select(it =>
                    {
                        var lerp = Mathf.InverseLerp(0, 2, it);
                        var r = Inferno.data[(int)Math.Round(255f * lerp), 0];
                        var g = Inferno.data[(int)Math.Round(255f * lerp), 1];
                        var b = Inferno.data[(int)Math.Round(255f * lerp), 2];
                        return new Color(r, g, b, 1.0f);
                    }).ToArray();
                }

                var data = waveformBandCData[chunkId];

                var index = 0;
                for (var y = 0; y < bandColors[0].Length; y++)
                {
                    for (var x = 0; x < bandColors.Length; x++)
                        data[index++] = bandColors[x][y];
                }

                // Render the spectrogram
                var toRender = new float[columnsPerChunk][];
                // get chunk
                Array.Copy(waveformBandVolumes, chunkId * columnsPerChunk, toRender, 0, columnsPerChunk);
                waveformBandColors[chunkId].Apply(true);
            }

            this.bandColors = waveformBandColors;
        }

        static AccessTools.FieldRef<BeatmapObjectPlacementHelper, IBeatmapDataModel> dataModelRef =
        AccessTools.FieldRefAccess<BeatmapObjectPlacementHelper, IBeatmapDataModel>("_beatmapDataModel");

        public void RefreshView(float startTimeBeats, float endTimeBeats, BeatmapObjectPlacementHelper helper)
        {
            if (!this.visible)
            {
                return;
            }
            // startTimeBeats is 5 beats behind the current beat
            float startTime = dataModelRef(helper).bpmData.BeatToTime(startTimeBeats + 5);
            float endTime = dataModelRef(helper).bpmData.BeatToTime(endTimeBeats);

            var firstSpectrogram = (int)Math.Floor((startTime) / this.secondsPerChunk) - 1;
            var lastSpectrogram = (int)Math.Ceiling(endTime / this.secondsPerChunk);

            firstSpectrogram = Math.Max(0, firstSpectrogram);
            lastSpectrogram = Math.Min(lastSpectrogram, this.bandColors.Length - 1);

            if (this.spectrogramChunks == null)
            {
                if (this.bandColors != null)
                {
                    // Load asset bundle for the spectrogram material
                    AssetBundle myAssetBundle;
                    var bundle = BeatSaberMarkupLanguage.Utilities.GetResource(Assembly.GetExecutingAssembly(), "EditorEX.Resources.spectrogram.bundle");
                    myAssetBundle = AssetBundle.LoadFromMemory(bundle);

                    var baseMaterial = myAssetBundle.LoadAsset<Material>("spectrogram");
                    myAssetBundle.Unload(false);
                    // End AssetBundle stuff

                    this.spectrogramChunks = new GameObject[this.bandColors.Length];
                    for (int i = 0; i < this.bandColors.Length; i++)
                    {
                        var newMat = Material.Instantiate(baseMaterial);
                        newMat.color = new Color(1, 1, 1, 0);
                        newMat.SetTexture("_Tex", this.bandColors[i]);

                        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        obj.GetComponent<MeshRenderer>().material = newMat;
                        obj.transform.Rotate(new Vector3(0, 90, 0));

                        // Primitive Plane is 10x10, changes the size to (5 seconds of distance)x5
                        obj.transform.localScale = new Vector3(helper.timeToZDistanceScale * ((float)this.secondsPerChunk) / 10f, 1f, 0.5f);

                        obj.SetActive(false);


                        this.spectrogramChunks[i] = obj;
                    }
                }
            }
            // Render the spectrograms
            for (int i = 0; i < firstSpectrogram; i++)
            {
                this.spectrogramChunks[i].SetActive(false);
            }
            for (int i = lastSpectrogram; i < spectrogramChunks.Length; i++)
            {
                this.spectrogramChunks[i].SetActive(false);
            }
            for (int i = firstSpectrogram; i < lastSpectrogram; i++)
            {
                var z = helper.TimeToPosition(((float)(i * this.secondsPerChunk)) - startTime);
                this.spectrogramChunks[i].transform.localPosition = new Vector3(-7f, 0.01f, z + (helper.timeToZDistanceScale * ((float)this.secondsPerChunk) / 2f));
                this.spectrogramChunks[i].transform.localScale = new Vector3(helper.timeToZDistanceScale * ((float)this.secondsPerChunk) / 10f, 1f, 0.5f);
                this.spectrogramChunks[i].SetActive(true);
            }
        }

        public void CleanUp()
        {
            // Destroy all chunks before creating a new SpectrogramView
            for (int i = 0; i < this.spectrogramChunks.Length; i++)
            {
                UnityEngine.Object.Destroy(this.spectrogramChunks[i]);
            }
        }

        public void SetVisible(bool visible)
        {
            this.visible = visible;
            if (!visible)
            {
                for (int i = 0; i < this.spectrogramChunks.Length; i++)
                {
                    this.spectrogramChunks[i].SetActive(false);
                }
            }
        }
    }
}
