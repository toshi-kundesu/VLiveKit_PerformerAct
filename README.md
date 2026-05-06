## 概要

VLiveKitの一部として開発している、  
キャラクターの挙動・表現を制御するためのパッケージです。

呼吸・視線・表情など、ライブ用途でキャラクターを自然に見せるための  
各種制御機能をまとめています。

ライブ中でもキャラクターが常に自然に動き続ける状態を作ることを目的としています。

---

## 主な機能

### キャラクター表現

- 呼吸モーション
- まばたき
- モーションの揺らぎ
- BlendShapeのゆらぎ制御
- 視線制御（アイダート）

キャラクターが静止状態でも自然に見えるような挙動を付加します。

---

### 入力・制御

- JoyPadによる表情操作
- 外部信号による制御（OSC）

---

### リップシンク / 信号処理

- リップシンク情報をOSC信号として送受信
- データの記録（REC）および再生

---

### VRM / シェーダー連携

- VRMインポート時のシェーダー自動変換（LiveToon連携）
- キャラクターセットアップの補助

---

## 含まれるライブラリ

本パッケージには以下のライブラリが含まれています：

- OSCJack  
  https://github.com/keijiro/OscJack  
  License: Unlicense

- EVMC4U  
  https://github.com/gpsnmeajp/EVMC4U  
  License: MIT

- WindForVRM  
  https://github.com/malaybaku/WindForVRM  
  License: Apache License 2.0

※ 各ライブラリにはそれぞれ個別のライセンスが適用されます。

---

## 開発状況

本パッケージはライブ制作での使用を前提に、  
継続的に調整・改善を行っています。

---

## インストール

`Packages/manifest.json` の `dependencies` に以下を追加してください。

```json
{
  "dependencies": {
    "com.toshi.vlivekit.performeract": "https://github.com/toshi-kundesu/VLiveKit_PerformerAct.git?path=/Assets/toshi.VLiveKit/PerformerAct#main"
  }
}
