# VLiveKit PerformerAct UPM Notes

The scoped-registry build excludes the VRM10 URP MToon shader asset:

- `EVMC4U/VRMShaders/VRM10/MToon10/Resources/VRM10/vrmc_materials_mtoon_urp.shader`

VLiveKit's public UPM package is intended for HDRP-oriented virtual live projects. Keeping the URP shader in the package makes Unity compile it even in projects without `com.unity.render-pipelines.universal`, which produces shader include errors.

The rest of the VRM/VRMShaders runtime files remain in the package. If URP support is needed later, publish a separate URP-compatible package or add URP as an explicit dependency.
