#include "SpectralImprintNativeDsp.h"

#include <algorithm>
#include <cmath>

namespace
{
float Clamp01Audio(float value)
{
    return std::max(-1.0f, std::min(1.0f, value));
}

float Quantize(float sample, float levels)
{
    if (levels <= 2.0f)
        levels = 2.0f;

    return std::round(sample * levels) / levels;
}
}

int SpectralImprintNativeDsp_GetVersion()
{
    return 1;
}

void SpectralImprintNativeDsp_ProcessDegradationBuffer(
    float* data,
    int sampleCount,
    int channels,
    int processingChannels,
    float gain,
    float bitLevels,
    int sampleHold,
    float* heldSamples,
    int heldSampleCapacity,
    int* holdCounter)
{
    if (data == nullptr || sampleCount <= 0 || channels <= 0)
        return;

    const int channelCount = std::max(1, std::min(processingChannels, channels));
    const int heldCapacity = std::max(0, heldSampleCapacity);
    const int frames = sampleCount / channels;
    const int holdAmount = std::max(1, sampleHold);
    int counter = holdCounter != nullptr ? std::max(0, *holdCounter) : 0;
    const bool sampleHoldEnabled = holdAmount > 1 && heldSamples != nullptr && heldCapacity > 0;

    for (int frame = 0; frame < frames; ++frame)
    {
        const bool captureHeldSample = !sampleHoldEnabled || counter == 0;
        const int frameOffset = frame * channels;

        for (int ch = 0; ch < channelCount; ++ch)
        {
            const int dataIndex = frameOffset + ch;
            float processed = data[dataIndex];

            if (sampleHoldEnabled && ch < heldCapacity)
            {
                if (captureHeldSample)
                    heldSamples[ch] = processed;

                processed = heldSamples[ch];
            }

            processed = Quantize(processed, bitLevels);
            data[dataIndex] = Clamp01Audio(processed * gain);
        }

        if (sampleHoldEnabled)
        {
            ++counter;
            if (counter >= holdAmount)
                counter = 0;
        }
    }

    if (holdCounter != nullptr)
        *holdCounter = counter;
}
