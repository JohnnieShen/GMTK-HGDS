using System;
using System.Runtime.InteropServices;
using UnityEngine;

public struct SpectralImprintDspProfile
{
    public bool active;
    public float gain;
    public float lowPassCutoff;
    public float delayWet;
    public float delayFeedback;
    public float bitDepth;
    public int sampleHold;
    public float delayTimeSeconds;
    public float parameterSmoothTime;

    public static SpectralImprintDspProfile Clean(
        float cutoff,
        float bitDepth,
        float delayTimeSeconds,
        float parameterSmoothTime)
    {
        return new SpectralImprintDspProfile
        {
            active = false,
            gain = 1f,
            lowPassCutoff = cutoff,
            delayWet = 0f,
            delayFeedback = 0f,
            bitDepth = bitDepth,
            sampleHold = 1,
            delayTimeSeconds = delayTimeSeconds,
            parameterSmoothTime = parameterSmoothTime
        };
    }
}

public class SpectralImprintDspFilter : MonoBehaviour
{
    const int MaxDspChannels = 8;

    readonly OnePoleLowPass lowPass = new OnePoleLowPass();
    readonly SpectralDelayLine delayLine = new SpectralDelayLine();
    readonly SampleHoldReducer sampleHoldReducer = new SampleHoldReducer();
    readonly NativeDegradationState nativeDegradationState = new NativeDegradationState();
    readonly SmoothedDspProfile smoothedProfile = new SmoothedDspProfile();

    SpectralImprintDspProfile targetProfile;
    int dspSampleRate;
    bool nativeDspAvailable;

    void Awake()
    {
        InitializeDspState();
    }

    public void SetProfile(SpectralImprintDspProfile profile)
    {
        bool wasActive = targetProfile.active;
        targetProfile = Sanitized(profile);
        smoothedProfile.SetTarget(targetProfile);

        if (!wasActive && targetProfile.active)
            smoothedProfile.SnapToTarget();

        if (!targetProfile.active)
            ResetDspState();
    }

    public void ResetDspState()
    {
        lowPass.Reset();
        delayLine.Reset();
        sampleHoldReducer.Reset();
        nativeDegradationState.Reset();
    }

    void InitializeDspState()
    {
        dspSampleRate = Mathf.Max(1, AudioSettings.outputSampleRate);
        lowPass.Initialize(MaxDspChannels);
        delayLine.Initialize(dspSampleRate, MaxDspChannels, 0.5f);
        sampleHoldReducer.Initialize(MaxDspChannels);
        nativeDegradationState.Initialize(MaxDspChannels);
        nativeDspAvailable = SpectralImprintNativeDsp.IsAvailable;

        targetProfile = SpectralImprintDspProfile.Clean(16000f, 24f, 0.12f, 0.035f);
        smoothedProfile.SetTarget(targetProfile);
        smoothedProfile.SnapToTarget();
    }

    // Unity calls this on the audio thread; keep it allocation-free and math-only.
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!targetProfile.active || data == null || data.Length == 0 || channels <= 0)
            return;

        int channelCount = Mathf.Clamp(channels, 1, MaxDspChannels);
        int sampleFrames = data.Length / channels;
        float smoothing = DspMath.GetSmoothingCoefficient(
            data.Length,
            channels,
            dspSampleRate,
            targetProfile.parameterSmoothTime);

        SpectralImprintDspProfile profile = smoothedProfile.Advance(smoothing);
        float lowPassAlpha = DspMath.GetLowPassAlpha(profile.lowPassCutoff, dspSampleRate);
        float bitLevels = DspMath.BitDepthToLevels(profile.bitDepth);
        int delaySamples = delayLine.GetDelaySampleCount(profile.delayTimeSeconds);
        bool reduceSampleRate = profile.sampleHold > 1;

        if (nativeDspAvailable)
        {
            for (int frame = 0; frame < sampleFrames; frame++)
            {
                int frameOffset = frame * channels;

                for (int ch = 0; ch < channelCount; ch++)
                {
                    int dataIndex = frameOffset + ch;
                    float input = data[dataIndex];

                    float filtered = lowPass.Process(ch, input, lowPassAlpha);
                    float delayed = delayLine.Read(ch, delaySamples);
                    delayLine.Write(ch, Mathf.Clamp(filtered + delayed * profile.delayFeedback, -1f, 1f));

                    data[dataIndex] = filtered + delayed * profile.delayWet;
                }

                delayLine.Advance();
            }

            if (SpectralImprintNativeDsp.TryProcessDegradation(
                data,
                data.Length,
                channels,
                channelCount,
                profile.gain,
                bitLevels,
                profile.sampleHold,
                nativeDegradationState))
            {
                return;
            }

            nativeDspAvailable = false;
            sampleHoldReducer.Reset();
            ProcessManagedDegradationOnly(
                data,
                channels,
                channelCount,
                profile.gain,
                bitLevels,
                profile.sampleHold);
            return;
        }

        for (int frame = 0; frame < sampleFrames; frame++)
        {
            bool captureHeldSample = !reduceSampleRate || sampleHoldReducer.ShouldCapture();
            int frameOffset = frame * channels;

            for (int ch = 0; ch < channelCount; ch++)
            {
                int dataIndex = frameOffset + ch;
                float input = data[dataIndex];

                float filtered = lowPass.Process(ch, input, lowPassAlpha);
                float delayed = delayLine.Read(ch, delaySamples);
                delayLine.Write(ch, Mathf.Clamp(filtered + delayed * profile.delayFeedback, -1f, 1f));

                float processed = filtered + delayed * profile.delayWet;
                processed = sampleHoldReducer.Process(ch, processed, captureHeldSample, reduceSampleRate);
                processed = BitDepthReducer.Process(processed, bitLevels);

                data[dataIndex] = Mathf.Clamp(processed * profile.gain, -1f, 1f);
            }

            delayLine.Advance();
            sampleHoldReducer.Advance(profile.sampleHold);
        }
    }

    void ProcessManagedDegradationOnly(
        float[] data,
        int channels,
        int channelCount,
        float gain,
        float bitLevels,
        int sampleHold)
    {
        int sampleFrames = data.Length / channels;
        bool reduceSampleRate = sampleHold > 1;

        for (int frame = 0; frame < sampleFrames; frame++)
        {
            bool captureHeldSample = !reduceSampleRate || sampleHoldReducer.ShouldCapture();
            int frameOffset = frame * channels;

            for (int ch = 0; ch < channelCount; ch++)
            {
                int dataIndex = frameOffset + ch;
                float processed = sampleHoldReducer.Process(ch, data[dataIndex], captureHeldSample, reduceSampleRate);
                processed = BitDepthReducer.Process(processed, bitLevels);
                data[dataIndex] = Mathf.Clamp(processed * gain, -1f, 1f);
            }

            sampleHoldReducer.Advance(sampleHold);
        }
    }

    SpectralImprintDspProfile Sanitized(SpectralImprintDspProfile profile)
    {
        profile.gain = Mathf.Max(0f, profile.gain);
        profile.lowPassCutoff = Mathf.Max(20f, profile.lowPassCutoff);
        profile.delayWet = Mathf.Clamp01(profile.delayWet);
        profile.delayFeedback = Mathf.Clamp(profile.delayFeedback, 0f, 0.95f);
        profile.bitDepth = Mathf.Clamp(profile.bitDepth, 2f, 24f);
        profile.sampleHold = Mathf.Max(1, profile.sampleHold);
        profile.delayTimeSeconds = Mathf.Max(0.001f, profile.delayTimeSeconds);
        profile.parameterSmoothTime = Mathf.Max(0.001f, profile.parameterSmoothTime);
        return profile;
    }
}

class SmoothedDspProfile
{
    SpectralImprintDspProfile target;
    SpectralImprintDspProfile current;

    public void SetTarget(SpectralImprintDspProfile profile)
    {
        target = profile;
        current.active = profile.active;
        current.delayTimeSeconds = profile.delayTimeSeconds;
        current.parameterSmoothTime = profile.parameterSmoothTime;
    }

    public void SnapToTarget()
    {
        current = target;
    }

    public SpectralImprintDspProfile Advance(float smoothing)
    {
        current.active = target.active;
        current.gain = DspMath.Smooth(current.gain, target.gain, smoothing);
        current.lowPassCutoff = DspMath.Smooth(current.lowPassCutoff, target.lowPassCutoff, smoothing);
        current.delayWet = DspMath.Smooth(current.delayWet, target.delayWet, smoothing);
        current.delayFeedback = DspMath.Smooth(current.delayFeedback, target.delayFeedback, smoothing);
        current.bitDepth = DspMath.Smooth(current.bitDepth, target.bitDepth, smoothing);
        current.sampleHold = target.sampleHold;
        current.delayTimeSeconds = target.delayTimeSeconds;
        current.parameterSmoothTime = target.parameterSmoothTime;
        return current;
    }
}

class OnePoleLowPass
{
    float[] state;

    public void Initialize(int channels)
    {
        state = new float[channels];
    }

    public void Reset()
    {
        if (state != null)
            System.Array.Clear(state, 0, state.Length);
    }

    public float Process(int channel, float input, float alpha)
    {
        state[channel] += alpha * (input - state[channel]);
        return state[channel];
    }
}

class SpectralDelayLine
{
    float[] buffer;
    int channelCount;
    int bufferLength;
    int sampleRate;
    int writeIndex;

    public void Initialize(int sampleRate, int channels, float maxDelaySeconds)
    {
        this.sampleRate = Mathf.Max(1, sampleRate);
        channelCount = channels;
        bufferLength = Mathf.Max(2, Mathf.CeilToInt(this.sampleRate * maxDelaySeconds));
        buffer = new float[bufferLength * channelCount];
    }

    public void Reset()
    {
        if (buffer != null)
            System.Array.Clear(buffer, 0, buffer.Length);

        writeIndex = 0;
    }

    public int GetDelaySampleCount(float delaySeconds)
    {
        return Mathf.Clamp(Mathf.RoundToInt(delaySeconds * sampleRate), 1, bufferLength - 1);
    }

    public float Read(int channel, int delaySamples)
    {
        int readIndex = writeIndex - delaySamples;
        if (readIndex < 0)
            readIndex += bufferLength;

        return buffer[GetBufferIndex(channel, readIndex)];
    }

    public void Write(int channel, float sample)
    {
        buffer[GetBufferIndex(channel, writeIndex)] = sample;
    }

    public void Advance()
    {
        writeIndex++;
        if (writeIndex >= bufferLength)
            writeIndex = 0;
    }

    int GetBufferIndex(int channel, int sampleIndex)
    {
        return channel * bufferLength + sampleIndex;
    }
}

class SampleHoldReducer
{
    float[] heldSamples;
    int holdCounter;

    public void Initialize(int channels)
    {
        heldSamples = new float[channels];
    }

    public void Reset()
    {
        if (heldSamples != null)
            System.Array.Clear(heldSamples, 0, heldSamples.Length);

        holdCounter = 0;
    }

    public bool ShouldCapture()
    {
        return holdCounter == 0;
    }

    public float Process(int channel, float input, bool capture, bool enabled)
    {
        if (!enabled)
            return input;

        if (capture)
            heldSamples[channel] = input;

        return heldSamples[channel];
    }

    public void Advance(int holdAmount)
    {
        if (holdAmount <= 1)
        {
            holdCounter = 0;
            return;
        }

        holdCounter++;
        if (holdCounter >= holdAmount)
            holdCounter = 0;
    }
}

static class BitDepthReducer
{
    public static float Process(float sample, float levels)
    {
        return Mathf.Round(sample * levels) / levels;
    }
}

static class DspMath
{
    public static float Smooth(float current, float target, float smoothing)
    {
        return current + (target - current) * smoothing;
    }

    public static float GetSmoothingCoefficient(
        int sampleCount,
        int channels,
        int sampleRate,
        float smoothTimeSeconds)
    {
        float frames = Mathf.Max(1f, sampleCount / Mathf.Max(1f, channels));
        float seconds = frames / Mathf.Max(1f, sampleRate);
        return 1f - Mathf.Exp(-seconds / Mathf.Max(0.001f, smoothTimeSeconds));
    }

    public static float GetLowPassAlpha(float cutoff, int sampleRate)
    {
        float clampedCutoff = Mathf.Clamp(cutoff, 20f, sampleRate * 0.45f);
        float rc = 1f / (2f * Mathf.PI * clampedCutoff);
        float dt = 1f / Mathf.Max(1f, sampleRate);
        return dt / (rc + dt);
    }

    public static float BitDepthToLevels(float bitDepth)
    {
        return Mathf.Max(2f, Mathf.Pow(2f, bitDepth));
    }
}

class NativeDegradationState
{
    public float[] heldSamples;
    public int holdCounter;

    public void Initialize(int channels)
    {
        heldSamples = new float[channels];
    }

    public void Reset()
    {
        if (heldSamples != null)
            Array.Clear(heldSamples, 0, heldSamples.Length);

        holdCounter = 0;
    }
}

static class SpectralImprintNativeDsp
{
    const string LibraryName = "SpectralImprintNativeDsp";

    public static bool IsAvailable { get; private set; }

    static SpectralImprintNativeDsp()
    {
        try
        {
            IsAvailable = GetVersion() >= 1;
        }
        catch (DllNotFoundException)
        {
            IsAvailable = false;
        }
        catch (EntryPointNotFoundException)
        {
            IsAvailable = false;
        }
        catch (BadImageFormatException)
        {
            IsAvailable = false;
        }
    }

    public static bool TryProcessDegradation(
        float[] data,
        int sampleCount,
        int channels,
        int processingChannels,
        float gain,
        float bitLevels,
        int sampleHold,
        NativeDegradationState state)
    {
        if (!IsAvailable || state?.heldSamples == null)
            return false;

        try
        {
            ProcessDegradationBuffer(
                data,
                sampleCount,
                channels,
                processingChannels,
                gain,
                bitLevels,
                sampleHold,
                state.heldSamples,
                state.heldSamples.Length,
                ref state.holdCounter);
            return true;
        }
        catch (Exception)
        {
            IsAvailable = false;
            return false;
        }
    }

    [DllImport(LibraryName, EntryPoint = "SpectralImprintNativeDsp_GetVersion")]
    static extern int GetVersion();

    [DllImport(LibraryName, EntryPoint = "SpectralImprintNativeDsp_ProcessDegradationBuffer")]
    static extern void ProcessDegradationBuffer(
        [In, Out] float[] data,
        int sampleCount,
        int channels,
        int processingChannels,
        float gain,
        float bitLevels,
        int sampleHold,
        [In, Out] float[] heldSamples,
        int heldSampleCapacity,
        ref int holdCounter);
}
