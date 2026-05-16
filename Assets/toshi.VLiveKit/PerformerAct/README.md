# VLive Performer Act

Performer control package for virtual live production. It bundles the VLiveKit-specific performer logic and keeps common transport libraries as Unity Package Manager dependencies where possible.

## Package

- Package name: `com.toshi.vlivekit.performeract`
- Version: `0.1.6`
- Unity: 6000.3
- Repository: https://github.com/toshi-kundesu/VLiveKit_PerformerAct
- Package root: `Assets/toshi.VLiveKit/PerformerAct`

## Main Contents

- Breathing, blinking, gaze, expression, and BlendShape control helpers.
- JoyPad / OSC performer input receivers.
- VRM import and character setup helpers.

## Auto Animation Setup

For the normal character-root workflow, add `AutoAnimationSetup` to the avatar root first.

`AutoAnimationSetup` finds the Humanoid `Animator`, VRM `BlendShapeProxy`, face `SkinnedMeshRenderer`, and the active camera under the character setup. It then adds and wires the existing performer components:

- `AutoBlink` for automatic blinking.
- `AutoEyeDirt` for eye dart and camera-look motion.
- `BreathingAnimation` with a small default Humanoid breathing preset.
- `BlendShapeFollower` for subtle facial BlendShape jitter driven by blink shapes.
- `MultiBoneOffsetController` with a subtle head/neck/shoulder motion-jitter preset.
- `VRMWind` for light wind variation on VRM SpringBones.
- `JoyStickReceiver` and `ExpressionController` for controller/OSC facial expression control.
- `FacialReceiver` and `FacialBlendShapeController` for uLipSync-style OSC lip sync.
- `VRMFacialManager` so the shared Animator and BlendShapeProxy references stay connected.

If the avatar is not Humanoid, turn off `Require Humanoid` before running the context menu `Setup Auto Animation`. Use `Use Default Motion Preset` when you want to restore only the generated breathing and motion-jitter presets without changing the facial mappings.

Use `Resetup Auto Animation` when existing scene components were created by an older setup pass. It re-detects the Animator, camera, VRM BlendShapeProxy, and face renderer, then rebuilds generated breathing, motion-jitter, and facial-jitter defaults, reloads `VRMWind`, and keeps the component set in place.

## UPM Dependencies

These packages are resolved through Unity Package Manager and should not be vendored into this package:

- OSCJack `jp.keijiro.osc-jack` 2.0.0: https://github.com/keijiro/OscJack
- uOSC `com.hecomi.uosc` 1.0.1: https://github.com/hecomi/uOSC

Add the npm scoped registry to the project manifest before installing:

```json
{
  "scopedRegistries": [
    {
      "name": "npmjs",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "jp.keijiro",
        "com.hecomi"
      ]
    }
  ]
}
```

## Bundled Third-Party Code

- EVMC4U: https://github.com/gpsnmeajp/EVMC4U
- UniVRM / VRMShaders 0.120.0, kept bundled for now because the currently visible registry packages do not match this embedded version.
- WindForVRM runtime `VRMWind`: https://github.com/malaybaku/WindForVRM, Apache-2.0. See `ThirdPartyNotices/WindForVRM.md`.

## Install

Add this package to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.toshi.vlivekit.performeract": "https://github.com/toshi-kundesu/VLiveKit_PerformerAct.git?path=/Assets/toshi.VLiveKit/PerformerAct#main"
  }
}
```

In the VLiveKit sandbox this package is placed at `Packages/VLiveKit_PerformerAct` as a submodule and loaded through a local `file:` dependency.

## OSC Test Signal Window

Open `toshi > VLiveKit > Performer Act > OSC Test Signal` to send local test signals without preparing uLipSync or an external controller.

- Lip sync sends `/ulipsync/vowel/a`, `/i`, `/u`, `/e`, and `/o` to `FacialReceiver` on port `39580` by default.
- Expression controls send `JoyStickReceiver` addresses such as `/b1`-`/b6`, `/p1_X`, `/p1_Y`, `/slider1`, `/slider2`, and axis values on port `39581` by default.
- Use `Aeiou Loop` or `Talk Pulse` while tuning mouth mappings, then switch to `Manual` for exact values.
- Enable `Send zero values on stop` when you want the avatar face to return to rest after a test.

## Notes

uLipSync is used as an OSC sender convention (`/ulipsync/...`) but this package does not reference the uLipSync runtime API directly.

## License

This package's own code and assets follow the repository `LICENSE`. Bundled or dependent third-party packages keep their own licenses and notices.
