#pragma once

#ifdef _WIN32
#define SPECTRAL_IMPRINT_EXPORT extern "C" __declspec(dllexport)
#else
#define SPECTRAL_IMPRINT_EXPORT extern "C" __attribute__((visibility("default")))
#endif

SPECTRAL_IMPRINT_EXPORT int SpectralImprintNativeDsp_GetVersion();

SPECTRAL_IMPRINT_EXPORT void SpectralImprintNativeDsp_ProcessDegradationBuffer(
    float* data,
    int sampleCount,
    int channels,
    int processingChannels,
    float gain,
    float bitLevels,
    int sampleHold,
    float* heldSamples,
    int heldSampleCapacity,
    int* holdCounter);
