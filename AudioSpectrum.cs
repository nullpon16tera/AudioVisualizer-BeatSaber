﻿// Audio spectrum component
// By Keijiro Takahashi, 2013
// https://github.com/keijiro/unity-audio-spectrum
using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class AudioSpectrum : MonoBehaviour
{
    #region Band type definition
    public enum BandType {
        FourBand,
        FourBandVisual,
        EightBand,
        TenBand,
        TwentySixBand,
        ThirtyOneBand,
        ThirtyEXOneBand
    };

    static float[][] middleFrequenciesForBands = {
        new float[]{ 125.0f, 500, 1000, 2000 },
        new float[]{ 250.0f, 400, 600, 800 },
        new float[]{ 63.0f, 125, 500, 1000, 2000, 4000, 6000, 8000 },
        new float[]{ 31.5f, 63, 125, 250, 500, 1000, 2000, 4000, 8000, 16000 },
        new float[]{ 25.0f, 31.5f, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000 },
        new float[]{ 20.0f, 25, 31.5f, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 10000, 12500, 16000, 20000 },
        new float[]{ 20.0f, 25, 31.5f, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 9000, 10000, 11000, 12000 },
    };
    static float[] bandwidthForBands = {
        1.414f, // 2^(1/2)
        1.260f, // 2^(1/3)
        1.414f, // 2^(1/2)
        1.414f, // 2^(1/2)
        1.122f, // 2^(1/6)
        1.122f, // 2^(1/6)
        1.414f
    };
    #endregion

    #region Public method
    public static BandType ConvertToBandtype(string bandTypeName)
    {
        return Enum.GetValues(typeof(AudioSpectrum.BandType)).OfType<AudioSpectrum.BandType>().FirstOrDefault(x => string.Equals(x.ToString(), bandTypeName, StringComparison.CurrentCultureIgnoreCase));
    }
    #endregion

    #region Public variables
    public int numberOfSamples = 1024;
    public BandType bandType = BandType.TenBand;
    public float fallSpeed = 0.08f;
    public float sensibility = 8.0f;
    public event Action<AudioSpectrum> UpdatedRawSpectrums;
    #endregion

    #region Private variables
    float[] rawSpectrum;
    float[] levels;
    float[] peakLevels;
    float[] meanLevels;
    #endregion

    #region Public property
    public float[] Levels {
        get { return levels; }
    }

    public float[] PeakLevels {
        get { return peakLevels; }
    }
    
    public float[] MeanLevels {
        get { return meanLevels; }
    }
    public BandType Band
    {
        get => this.bandType;

        set => this.SetBandType(ref this.bandType, value);
    }
    #endregion

    #region Private functions
    private bool SetBandType(ref BandType bt, BandType value)
    {
        if (bt == value) {
            return false;
        }
        bt = value;
        var bandCount = middleFrequenciesForBands[(int)bt].Length;
        if (this.levels.Length != bandCount) {
            this.levels = new float[bandCount];
            this.peakLevels = new float[bandCount];
            this.meanLevels = new float[bandCount];
        }
        return true;
    }
    void CheckBuffers ()
    {
        if (rawSpectrum == null || rawSpectrum.Length != numberOfSamples) {
            rawSpectrum = new float[numberOfSamples];
        }
        var bandCount = middleFrequenciesForBands [(int)bandType].Length;
        if (levels == null || levels.Length != bandCount) {
            levels = new float[bandCount];
            peakLevels = new float[bandCount];
            meanLevels = new float[bandCount];
        }
    }

    int FrequencyToSpectrumIndex (float f)
    {
        var i = Mathf.FloorToInt (f / AudioSettings.outputSampleRate * 2.0f * rawSpectrum.Length);
        return Mathf.Clamp (i, 0, rawSpectrum.Length - 1);
    }
    #endregion

    #region Monobehaviour functions
    void Awake ()
    {
        CheckBuffers ();
    }

    void Update ()
    {
        CheckBuffers ();

        AudioListener.GetSpectrumData (rawSpectrum, 0, FFTWindow.BlackmanHarris);

        float[] middlefrequencies = middleFrequenciesForBands [(int)bandType];
        var bandwidth = bandwidthForBands [(int)bandType];

        var falldown = fallSpeed * Time.deltaTime;
        var filter = Mathf.Exp (-sensibility * Time.deltaTime);

        for (var bi = 0; bi < levels.Length; bi++) {
            int imin = FrequencyToSpectrumIndex (middlefrequencies [bi] / bandwidth);
            int imax = FrequencyToSpectrumIndex (middlefrequencies [bi] * bandwidth);

            var bandMax = 0.0f;
            for (var fi = imin; fi <= imax; fi++) {
                bandMax = Mathf.Max (bandMax, rawSpectrum [fi]);
            }

            levels [bi] = bandMax;
            peakLevels [bi] = Mathf.Max (peakLevels [bi] - falldown, bandMax);
            meanLevels [bi] = bandMax - (bandMax - meanLevels [bi]) * filter;
        }

        this.UpdatedRawSpectrums?.Invoke(this);
    }
    #endregion
}