# Third-party notices

The original C# binding is derived from [Mirsario/SteamAudio.NET](https://github.com/Mirsario/SteamAudio.NET), copyright 2024 Mirsario, under MIT. Its original copyright notice is retained in LICENSE.md. This repository substantially redesigns that binding; it is not Valve's or Mirsario's official release.

Steam Audio 4.8.1, its C declarations and translated API documentation are copyright Valve Corporation under Apache-2.0. LICENSE.Natives.md and Native/notices/Valve-THIRDPARTY.md contain the applicable license and upstream dependency notices, including HRTF data notices.

The vcpkg port patches include modified code from [NVGT](https://github.com/samtupy/nvgt), commit 734395594aa9d44804902b9517fed54d8023b6b2. Its zlib license is preserved in Native/notices/NVGT-LICENSE.md.

CI artifacts carry the copyright notices of all built dependencies, including PFFFT, libmysofa, zlib and FlatBuffers. Preserve those notices when redistributing native assets.
