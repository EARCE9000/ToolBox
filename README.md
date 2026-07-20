# ToolBox
自作の便利ツールや実験用アプリケーションをまとめるリポジトリ

## プロジェクト一覧

### PresentationPointer

Windowsでのプレゼン時に、矢印や吹き出しを表示してマウスで操作できるポインタツール。

#### 主な機能

**矢印モード**
- プレゼン中に注目したいポイントを指し示す矢印を表示
- ドラッグで自由に移動・配置
- 常に最前面表示（PowerPoint等の上でも使用可）
- サイズ3段階（128x128 / 256x256 / 512x512）
- 回転4方向（右上/右下/左下/左上）

**吹き出しモード**
- テキスト入力可能な吹き出しを表示
- ダブルクリックでテキスト編集モード
- 最大8文字×6行のテキスト表示
- サイズと回転の調整が可能

**操作メニュー**
- 右クリックでメニュー表示
- モード切替（矢印/吹き出し）
- サイズ・方向の変更
- バージョン情報表示（Build情報、著作権、GitHubリンク）

#### 技術情報

- **対応OS**: Windows（arm64 / x64）
- **フレームワーク**: .NET 10
- **UI**: Windows Forms
- **機能**: 透過ウィンドウ、TopMost表示、ドラッグ移動

#### ビルド方法

```powershell
# プロジェクトのビルド
cd PresentationPointer
dotnet build

# Releaseビルド
dotnet build -c Release
```

#### 配布パッケージの作成

プロジェクトには複数のPublishプロファイルが用意されています：

```powershell
# x64版（ランタイム必要）
dotnet publish -p:PublishProfile=Win-x64

# x64版（スタンドアロン）
dotnet publish -p:PublishProfile=Win-x64-Standalone

# ARM64版（ランタイム必要）
dotnet publish -p:PublishProfile=Win-Arm64

# ARM64版（スタンドアロン）
dotnet publish -p:PublishProfile=Win-Arm64-Standalone
```

詳細は [PresentationPointer/publish.md](PresentationPointer/publish.md) を参照してください。

#### 動作要件

- **Standalone版**: .NETランタイム不要（単体で動作）
- **通常版**: .NET 10 Runtimeのインストールが必要

## ライセンス

各プロジェクトのライセンス情報については、個別のプロジェクトフォルダを参照してください。

