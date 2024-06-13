#  *_綴りが間違ってるって？仕様だ！_*

# YMM4CustomEasengK

## 概要
* YMM4で座標とかをLuaで制御できるようにするやつ。
* Easengとありますがそれ以外にも使い道しかない
## 使い方
### 右の *_release_* からダウンロードできるよ
* アニメーション効果を付ける
* 何に対して使うかを`対象`から選ぶ
* `Func`にスクリプトを記述する
* エラーがある場合`error`に表示されると思います(臨時的に廃止)
### 例
対象 `X`<br>
Func `return _FRAME //こうすると1フレームに1px右に進む`
* 円運動 
  * 対象 `X` Func `return 300*math.sin(math.pi/128*_FRAME)`
  * 対象 `Y` Func `return 300*math.cos(math.pi/128*_FRAME)`
## 特殊な変数

* `_FRAME` オブジェクトの初めから見た現在のフレームが格納されています
* `_LENGTH` オブジェクトの長さが格納されています
* `_FPS` FPSだと思う
* `_LAYER` 存在しているレイヤー
* `_Y` オブジェクトのY座標
* `_X` オブジェクトのX座標
* `_Z` オブジェクトのZ座標
* `_R` オブジェクトのZ回転角
* `_RX` オブジェクトのX回転角
* `_RY` オブジェクトのY回転角
* `_ZO` オブジェクトの拡大率
* `_CX` オブジェクトの中心X座標
* `_CY` オブジェクトの中心Y座標
* ~~`ReT` プログラム終了時この変数に格納されている数値が対象に反映されます~~
* ~~`TEMP` ここに代入したものは次代入するまで持ち越せる(stringなので注意)テストしてない~~
## その他
* licenseはLuaJITに帰属
* バグやら意見やら見つけたら@kotolind2a1に頼む

理論上どんな動きも可能だぜ
