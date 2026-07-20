title: PresentationPointer
---
## 概要

Windowsでのプレゼン時に、矢印や吹き出しを表示してマウスで操作できるポインタツール。

## 主な機能

### 矢印モード

![矢印モード](./screenshots/arrow-mode.png)

プレゼン中に注目したいポイントを指し示す矢印を表示します。

- ドラッグで自由に移動・配置
- 常に最前面表示（PowerPoint等の上でも使用可）
- サイズ3段階（128x128 / 256x256 / 512x512）
- 回転4方向（右上/右下/左下/左上）

### 吹き出しモード

![吹き出しモード](./screenshots/speech-mode.png)

テキスト入力可能な吹き出しを表示します。

- ダブルクリックでテキスト編集モードに
- 最大8文字×6行のテキスト表示
- サイズと回転の調整が可能

### 操作メニュー

![右クリックメニュー](./screenshots/context-menu.png)

右クリックで操作メニューを表示し、モード切替やサイズ・方向の変更が可能です。

- **モード**: 矢印 / 吹き出し
- **サイズ**: 128x128 / 256x256 / 512x512
- **方向**: 右上 / 右下 / 左下 / 左上
- **バージョン情報**: Build情報、著作権、GitHubリンク

対応OS: Windows（arm / x64）

## 動作要件

- `_standalone` 付き（x64_standalone.zip / arm_standalone.zip）: .NETランタイム同梱、インストール不要
- 上記以外（x64.zip / arm.zip）: 事前に.NETランタイムのインストールが必要

## ソースコード

GitHubで公開中: https://github.com/EARCE9000/ToolBox

