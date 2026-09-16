# Third-party notices

Darp.Tesseract.Native's binding and packaging code is Apache-2.0. Bundled
third-party components retain their own licenses.

`licenses/upstream/` contains license texts from the pinned Tesseract Robotics,
Boost Plugin Loader and OPW Kinematics submodules. Tesseract includes Apache-2.0,
BSD-2-Clause and BSD-3-Clause code, including Southwest Research Institute and
Georgia Institute of Technology contributions. `licenses/<rid>/COPYRIGHTS.txt`
preserves upstream copyright notices.

`licenses/<rid>/` contains original licenses for distributed native dependencies
and incorporated headers, plus dependency source recipes with URLs, hashes and
packaging patches. This includes Eigen's MPL-2.0 source materials and Microsoft's
original Visual C++ runtime license documents where applicable.

Orocos KDL is LGPL-2.1-or-later and dynamically linked on every platform. You may
replace its adjacent library with an interface-compatible modified version;
debugging and reverse engineering for those modifications are permitted.
`licenses/win-x64/orocos-kdl/sources/` includes the hash-verified KDL 1.5.3 source
archive and the Windows DLL patch. Other platforms' source recipes describe their
packaging patches. No KDL algorithms are modified.

To rebuild, use the pinned submodules, `pixi.lock`, CMake files and patches in
[the source repository](https://github.com/rosslight/Darp.Tesseract), then run
`git submodule update --init --recursive` and `pixi run build-native` on the
matching host. Binding generation is a separate operation.
