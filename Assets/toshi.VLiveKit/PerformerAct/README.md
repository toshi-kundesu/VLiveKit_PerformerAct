# VLive Performer Act

キャラクターの呼吸、瞬き、視線、表情、入力連携をまとめた performer control package です。

## Package

- Package name: `com.toshi.vlivekit.performeract`
- Version: `0.1.3`
- Unity: 6000.3
- Repository: https://github.com/toshi-kundesu/VLiveKit_PerformerAct
- Package root: `Assets/toshi.VLiveKit/PerformerAct`

## 主な内容

- 呼吸、瞬き、motion 揺らぎ、BlendShape の補助制御
- JoyPad / OSC 入力による performer 操作
- VRM import 後の shader / character setup 補助

## 依存・同梱 asset

- OSCJack: https://github.com/keijiro/OscJack
- EVMC4U: https://github.com/gpsnmeajp/EVMC4U
- WindForVRM: https://github.com/malaybaku/WindForVRM

## インストール

Unity の `Packages/manifest.json` の `dependencies` に追加します。

```json
{
  "dependencies": {
    "com.toshi.vlivekit.performeract": "https://github.com/toshi-kundesu/VLiveKit_PerformerAct.git?path=/Assets/toshi.VLiveKit/PerformerAct#main"
  }
}
```

VLiveKit sandbox では submodule として `Packages/VLiveKit_PerformerAct` に配置し、`file:` 参照で読み込んでいます。

## 注意

- 同梱 third-party asset にはそれぞれの license が適用されます。

## License

この package 独自のコードと asset は repository の `LICENSE` に従います。third-party asset を含む場合は、それぞれの license / README を確認してください。
