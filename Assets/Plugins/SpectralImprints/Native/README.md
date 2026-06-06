# Spectral Imprints Native DSP

This native C++ module is intentionally small: Unity still handles gameplay state,
audio source routing, and filter/delay setup in C#, while this library performs
one math-heavy per-sample degradation pass for the Spectral Imprints prototype.

The exported function processes Unity audio buffers in-place and applies:

- sample-hold reduction
- bit-depth quantization
- output gain
- final clipping

On macOS, rebuild the plugin from the project root with:

```sh
clang++ -std=c++17 -O3 -fvisibility=hidden -dynamiclib \
  -arch arm64 \
  Assets/Plugins/SpectralImprints/Native/SpectralImprintNativeDsp.cpp \
  -o Assets/Plugins/macOS/libSpectralImprintNativeDsp.dylib
```

The C# DSP filter falls back to its managed implementation if the native plugin
is missing or cannot be loaded.
