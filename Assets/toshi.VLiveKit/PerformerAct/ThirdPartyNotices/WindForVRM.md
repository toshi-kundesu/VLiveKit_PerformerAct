# WindForVRM

Source: https://github.com/malaybaku/WindForVRM

Original author: baku_dreameater / malaybaku

Original copyright: Copyright 2019-2022 @baku_dreameater

License: Apache License, Version 2.0

License text: `Apache-2.0.txt`

Reference license URL: https://www.apache.org/licenses/LICENSE-2.0

Included files:

- `Assets/toshi.VLiveKit/PerformerAct/Runtime/WindForVRM/VRMWind.cs`
- `Assets/toshi.VLiveKit/PerformerAct/WindForVRM/Example/`

Local changes:

- Moved `VRMWind.cs` into the PerformerAct runtime assembly folder so `AutoAnimationSetup` can wire it with the rest of the character auto-animation setup.
- Added a null-safe load path and `ReloadVrm(Transform vrmRoot)` helper so setup can rebuild SpringBone wind references in place.

The upstream repository is published as an Apache-2.0 licensed sample for adding wind-like effects to VRM `VRMSpringBone` components. Keep this notice with redistributed PerformerAct packages that include the `VRMWind` runtime script.
