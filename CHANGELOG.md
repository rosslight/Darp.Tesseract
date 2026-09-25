# Changelog

## [0.5.0+tesseract.0.35.0](https://github.com/rosslight/Darp.Tesseract/compare/v0.4.0+tesseract.0.35.0...v0.5.0+tesseract.0.35.0) (2026-09-25)


### Features

* expose collision diagnostics ([#12](https://github.com/rosslight/Darp.Tesseract/issues/12)) ([2e67552](https://github.com/rosslight/Darp.Tesseract/commit/2e675529dade2452d3f7bbd5a9b7632877916889))
* expose configurable planner profiles ([#11](https://github.com/rosslight/Darp.Tesseract/issues/11)) ([d3a1aac](https://github.com/rosslight/Darp.Tesseract/commit/d3a1aacab03c102546aec976a2410fd3f3c4bf82))
* expose planning primitives ([#9](https://github.com/rosslight/Darp.Tesseract/issues/9)) ([43826bf](https://github.com/rosslight/Darp.Tesseract/commit/43826bfe8d329805b7fab4bb0955ab8b71d91324))
* expose stock planner profiles ([#10](https://github.com/rosslight/Darp.Tesseract/issues/10)) ([3fe80db](https://github.com/rosslight/Darp.Tesseract/commit/3fe80db0e3d9f4daaf75b96b28b7e72ae68738b4))
* expose Tesseract planning bindings ([#6](https://github.com/rosslight/Darp.Tesseract/issues/6)) ([60b936f](https://github.com/rosslight/Darp.Tesseract/commit/60b936ff63f1f954abe95e2ce64a31a1b7d12a59))


### Build

* remove embedded geometry project ([#8](https://github.com/rosslight/Darp.Tesseract/issues/8)) ([0f10561](https://github.com/rosslight/Darp.Tesseract/commit/0f1056169d3610df48cd25b0ab14a67c357db59d))

## [0.4.0+tesseract.0.35.0](https://github.com/rosslight/Darp.Tesseract/compare/v0.3.0+tesseract.0.35.0...v0.4.0+tesseract.0.35.0) (2026-09-23)


### Features

* Migrate Darp.Tesseract.Native to Aardvark.Base LA lib ([b70f8a5](https://github.com/rosslight/Darp.Tesseract/commit/b70f8a5fc862abb11711aa9a5eb4c6da6613abb8))


### Bug Fixes

* Pass geometry and integration tests ([405af58](https://github.com/rosslight/Darp.Tesseract/commit/405af583d0c3f722730e73e889409de2814c9452))
* Undo make classes Disposable and ensure span access is safe instead ([67445af](https://github.com/rosslight/Darp.Tesseract/commit/67445af45e7c4001f5da4b96d4c2a2a2916bc3de))

## [0.3.0+tesseract.0.35.0](https://github.com/rosslight/Darp.Tesseract/compare/v0.2.0+tesseract.0.35.0...v0.3.0+tesseract.0.35.0) (2026-09-22)


### Features

* Add a basic first version of an LA lib ([0e78290](https://github.com/rosslight/Darp.Tesseract/commit/0e782905701d696dded56030232da57105c0088d))
* Add environment commands generation ([#3](https://github.com/rosslight/Darp.Tesseract/issues/3)) ([3b90622](https://github.com/rosslight/Darp.Tesseract/commit/3b9062277fe849265680603b80cc9aff6e8f766d))

## [0.2.0+tesseract.0.35.0](https://github.com/rosslight/Darp.Tesseract/compare/v0.1.3+tesseract.0.35.0...v0.2.0+tesseract.0.35.0) (2026-09-16)


### Features

* Add basic kinematics ([7824e4b](https://github.com/rosslight/Darp.Tesseract/commit/7824e4b2de51b588ecccd5dfa0796cecd31dbde3))
* bootstrap Tesseract native bindings ([6fa90a9](https://github.com/rosslight/Darp.Tesseract/commit/6fa90a9df88503dbc290c60b556c0f2fa89071ac))
* Generate scene, geometry, environment, kinematics ([16da437](https://github.com/rosslight/Darp.Tesseract/commit/16da4377da6eba29d352456b1f0a29673c09b5c3))
* More deterministic builds ([986afa9](https://github.com/rosslight/Darp.Tesseract/commit/986afa924d9d0afa86d48bdde69fb48520675a88))


### Bug Fixes

* clean portable native runtimes ([3ec873b](https://github.com/rosslight/Darp.Tesseract/commit/3ec873bfbaff53f2c11ab7431dc6f649d1e2d80d))
* ignore otool filename header ([84ee95a](https://github.com/rosslight/Darp.Tesseract/commit/84ee95ab568ab9e25078d0caa2f7d77d4c34a6f4))
* make macOS wrapper relocatable ([c807c10](https://github.com/rosslight/Darp.Tesseract/commit/c807c10fc0af450e140e44327cfe3a992fe367e7))
* make native matrix portable ([d604dd2](https://github.com/rosslight/Darp.Tesseract/commit/d604dd2cdc3cba33a0dcbbf2d95593ae81e0e611))
* Make nuget package independent of pixi environment ([24d473c](https://github.com/rosslight/Darp.Tesseract/commit/24d473c76cb0eddaf7b6a4c83c1e99fc017a0313))
* normalize macOS wrapper after collection ([0b97ffe](https://github.com/rosslight/Darp.Tesseract/commit/0b97ffe2a8ae8b495d099c2e25c2e83aab6a5f35))
* normalize packaged macOS metadata ([4dc433e](https://github.com/rosslight/Darp.Tesseract/commit/4dc433e1309a3d3b68c2e7ae1819ef93287eac59))
* pin the macOS compiler generation ([bbd309d](https://github.com/rosslight/Darp.Tesseract/commit/bbd309dead125d416b4b717331127e5080f5e897))
* preserve Eigen index type in bindings ([04d1ae5](https://github.com/rosslight/Darp.Tesseract/commit/04d1ae514322df0cb804ac378484f2e62db08cc4))
* restore local native test assets ([8a1f006](https://github.com/rosslight/Darp.Tesseract/commit/8a1f00657fb8b58a93ad2322ebfdd8335061846c))
* skip the OPW sample executable ([a75b741](https://github.com/rosslight/Darp.Tesseract/commit/a75b741694860f81eb72035469976a875e21d868))
* target the Linux host C++ ABI ([1f9add8](https://github.com/rosslight/Darp.Tesseract/commit/1f9add857219208610a850a64d3d2a678eb88d38))


### Build

* prepare NuGet trusted publishing and native attribution ([#1](https://github.com/rosslight/Darp.Tesseract/issues/1)) ([35c5506](https://github.com/rosslight/Darp.Tesseract/commit/35c55063a0c4d4f2b072644fb243c208b122e952))
