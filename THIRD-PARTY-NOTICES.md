# Third-party notices

The original C# binding is derived from [Mirsario/SteamAudio.NET](https://github.com/Mirsario/SteamAudio.NET), copyright 2024 Mirsario, under MIT. Its original copyright notice is retained in LICENSE.md. This repository substantially redesigns that binding; it is not Valve's or Mirsario's official release.

Steam Audio 4.8.1, its C declarations and translated API documentation are copyright Valve Corporation under Apache-2.0. LICENSE.Natives.md and Native/notices/Valve-THIRDPARTY.md contain the applicable license and upstream dependency notices, including HRTF data notices.

The vcpkg overlay/build integration and unaligned multiply-accumulate correction are adapted from [NVGT](https://github.com/samtupy/nvgt), inspected at commit 734395594aa9d44804902b9517fed54d8023b6b2. These are altered versions; the original NVGT zlib license is in Native/notices/NVGT-LICENSE.md. Unrelated NVGT simulation/performance patches are not included.

The static NativeAOT publishing arrangement was informed by [Refractor](https://github.com/trypsynth/refractor), inspected at commit 706b7c567837b50e03d8a70ff0f537c01d432e14. Its source is MIT licensed. Build targets here are written for Steam Audio and its dependencies.

CI artifacts carry the copyright notices of all built dependencies, including PFFFT, libmysofa, zlib and FlatBuffers. Preserve those notices when redistributing native assets.
