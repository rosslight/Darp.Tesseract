# Third-party notices

Darp.Tesseract.Native's binding and packaging code is licensed under Apache-2.0.
Included third-party code retains its own licenses; the package's Apache-2.0
metadata does not relicense these components.

## Incorporated upstream projects

- [Tesseract Robotics](https://github.com/tesseract-robotics/tesseract), pinned by
  the repository submodule, contains Apache-2.0, BSD-2-Clause and BSD-3-Clause
  code. Copyrights include Southwest Research Institute and the Georgia
  Institute of Technology's UR kinematics implementation.
- [Boost Plugin Loader](https://github.com/tesseract-robotics/boost_plugin_loader)
  and [OPW Kinematics](https://github.com/Jmeyer1292/opw_kinematics) retain their
  pinned upstream license texts and copyrights.

Their license texts are in `licenses/upstream/`. Concrete upstream copyright
notices are preserved in each runtime's `licenses/<rid>/COPYRIGHTS.txt`.
Tesseract's build-only patches are tracked under `patches/` in the source
repository. PCL/VTK and visualization are disabled in this package.

## Native dependency materials

Each `licenses/<rid>/dependencies.json` records the actual pinned packages,
versions, build identifiers, licenses, package URLs and runtime files selected
for that platform. The accompanying component directories contain the original
package license texts, attributions and source recipes, including upstream
source URLs, hashes and any packaging patches. Header/static inputs such as
Eigen, Cereal, Bullet and Boost are included even when there is no adjacent DLL.

- Eigen is MPL-2.0. Its corresponding source is available from the exact URL and
  hash in its included `source-recipe/meta.yaml`; the included recipe patches
  describe changes made by the dependency packager.
- Orocos KDL is LGPL-2.1-or-later and is dynamically linked. You may replace its
  adjacent runtime library with an interface-compatible modified version.
  Debugging and reverse engineering for modifications to this LGPL component
  are permitted under its license. Its complete upstream 1.5.3 source archive
  is included in `licenses/sources/`, together with the Windows shared-library
  build patch. On other platforms, the included pinned dependency source
  recipes describe the upstream archive and any dependency packaging patches.
  Windows uses CMake's automatic DLL symbol exports instead of upstream's
  static-library choice; no KDL algorithms are modified.
- Windows packages include Microsoft Visual C++ runtime files. Their original
  Microsoft license materials are included in the corresponding runtime
  component directory, including the authoritative RTF and convenience TXT
  when supplied by the pinned dependency package. They are not Apache-licensed.

## Rebuilding

The [source repository](https://github.com/rosslight/Darp.Tesseract) contains the
pinned submodule revisions, `pixi.lock`, CMake build files and patches. Run
`git submodule update --init --recursive` and `pixi run build-native` on the
matching host to rebuild. Binding generation is a separate explicit operation.

Third-party notices and source recipes are distributed for attribution and
source availability; they do not grant additional rights beyond each
component's own license.
