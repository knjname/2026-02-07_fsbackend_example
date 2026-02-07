# 開発環境セットアップガイド（macOS + VS Code + OSS）

このガイドでは、macOS 上で VS Code とオープンソースツールを使って FsApi プロジェクトの開発環境を構築する手順を説明する。

## 1. 前提条件のインストール

### Homebrew

未導入の場合はインストールする。

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

### .NET 10 SDK

```bash
brew install dotnet
```

インストール確認:

```bash
dotnet --version
# 10.0.x が表示されること
```

> Homebrew の `dotnet` パッケージが .NET 10 に対応していない場合は、[公式サイト](https://dotnet.microsoft.com/download/dotnet/10.0)からインストーラーをダウンロードする。

### Docker Desktop

```bash
brew install --cask docker
```

インストール後、Docker Desktop アプリを起動して初期設定を完了させる。

## 2. VS Code と拡張機能

### VS Code のインストール

```bash
brew install --cask visual-studio-code
```

### 推奨拡張機能

以下の拡張機能をインストールする。すべて OSS（MIT ライセンス）または無料で利用可能。

| 拡張機能 | ID | 用途 |
|---|---|---|
| Ionide for F# | `ionide.ionide-fsharp` | F# 言語サポート（IntelliSense、型情報、定義ジャンプ、プロジェクト管理） |
| EditorConfig | `editorconfig.editorconfig` | `.editorconfig` によるエディタ設定の統一 |
| Docker | `ms-azuretools.vscode-docker` | Docker Compose の操作、コンテナ管理 |
| REST Client | `humao.rest-client` | `.http` ファイルによる API リクエストテスト |

コマンドラインから一括インストール:

```bash
code --install-extension ionide.ionide-fsharp
code --install-extension editorconfig.editorconfig
code --install-extension ms-azuretools.vscode-docker
code --install-extension humao.rest-client
```

> **C# Dev Kit について**: `ms-dotnettools.csdevkit` はプロプライエタリライセンスのため、このガイドでは推奨しない。F# 開発には Ionide 単体で IntelliSense・型チェック・プロジェクト管理がすべて揃う。

## 3. プロジェクトの初期セットアップ

```bash
# リポジトリをクローン
git clone <repository-url>
cd fsapi

# dotnet ローカルツール（Fantomas コードフォーマッタ）をインストール
dotnet tool restore

# PostgreSQL を起動
docker compose -f devenv/compose.yml up -d

# ビルド
dotnet build FsApi.sln

# テスト実行（全テストが通ることを確認）
dotnet test FsApi.sln

# API サーバー起動
dotnet run --project src/FsApi.Api/FsApi.Api.fsproj
# http://localhost:5000 で起動
# http://localhost:5000/scalar/v1 で API ドキュメント（Scalar UI）を閲覧可能
```

## 4. VS Code 推奨設定

プロジェクトルートに `.vscode/settings.json` を作成し、以下の内容を記載する。

```json
{
  "editor.formatOnSave": true,
  "[fsharp]": {
    "editor.defaultFormatter": "ionide.ionide-fsharp"
  },
  "FSharp.fantomas.mode": "local",
  "files.watcherExclude": {
    "**/bin/**": true,
    "**/obj/**": true
  },
  "files.exclude": {
    "**/bin": true,
    "**/obj": true
  }
}
```

**設定内容の説明**:

- `editor.formatOnSave` — 保存時に Fantomas で自動フォーマット
- `FSharp.fantomas.mode: "local"` — プロジェクトの dotnet ローカルツールの Fantomas を使用（グローバルインストール版ではなく `.config/dotnet-tools.json` で管理されたバージョン）
- `files.watcherExclude` / `files.exclude` — ビルド成果物を監視・表示から除外してパフォーマンスを改善

## 5. .editorconfig

プロジェクトルートに `.editorconfig` を作成し、コーディングスタイルを統一する。

```ini
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true

[*.fs]
indent_style = space
indent_size = 4

[*.fsproj]
indent_style = space
indent_size = 2

[*.{yml,yaml,json}]
indent_style = space
indent_size = 2
```

> `indent_size = 4` は Fantomas のデフォルト設定に合わせている。

## 6. デバッグ設定

### launch.json

`.vscode/launch.json` を作成すると、F5 キーでデバッグ実行できる。

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "API 起動",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/src/FsApi.Api/bin/Debug/net10.0/FsApi.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/FsApi.Api",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "preLaunchTask": "build"
    },
    {
      "name": "Batch - list",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/src/FsApi.Batch/bin/Debug/net10.0/FsApi.Batch.dll",
      "args": ["list"],
      "cwd": "${workspaceFolder}/src/FsApi.Batch",
      "preLaunchTask": "build"
    },
    {
      "name": "Batch - complete-all",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/src/FsApi.Batch/bin/Debug/net10.0/FsApi.Batch.dll",
      "args": ["complete-all"],
      "cwd": "${workspaceFolder}/src/FsApi.Batch",
      "preLaunchTask": "build"
    }
  ]
}
```

> `coreclr` デバッグには C# 拡張機能 (`ms-dotnettools.csharp`) が必要。この拡張機能は MIT ライセンスで提供されている（C# Dev Kit とは別物）。必要に応じて `code --install-extension ms-dotnettools.csharp` でインストールする。

### tasks.json

`.vscode/tasks.json` を作成してビルド・テスト等のタスクを定義する。

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": ["build", "${workspaceFolder}/FsApi.sln"],
      "problemMatcher": "$msCompile",
      "group": {
        "kind": "build",
        "isDefault": true
      }
    },
    {
      "label": "test",
      "command": "dotnet",
      "type": "process",
      "args": ["test", "${workspaceFolder}/FsApi.sln"],
      "problemMatcher": "$msCompile",
      "group": "test"
    },
    {
      "label": "watch",
      "command": "dotnet",
      "type": "process",
      "args": [
        "watch",
        "run",
        "--project",
        "${workspaceFolder}/src/FsApi.Api/FsApi.Api.fsproj"
      ],
      "problemMatcher": "$msCompile",
      "isBackground": true
    },
    {
      "label": "format",
      "command": "dotnet",
      "type": "process",
      "args": ["fantomas", "."],
      "problemMatcher": []
    },
    {
      "label": "format-check",
      "command": "dotnet",
      "type": "process",
      "args": ["fantomas", "--check", "."],
      "problemMatcher": []
    }
  ]
}
```

**主なタスク**:

- **build** (`Ctrl+Shift+B`) — ソリューション全体をビルド。デバッグ起動前に自動実行される
- **test** — 全テストを実行
- **watch** — API サーバーをホットリロード付きで起動
- **format** — Fantomas でコード全体をフォーマット
- **format-check** — CI と同じフォーマットチェックをローカルで実行

## 7. データベース管理ツール（任意）

GUI でデータベースを操作したい場合は、OSS の DB クライアントを利用できる。

### DBeaver Community（Apache 2.0 ライセンス）

```bash
brew install --cask dbeaver-community
```

接続設定:

| 項目 | 値 |
|---|---|
| ホスト | `localhost` |
| ポート | `5432` |
| データベース | `fsapi` |
| ユーザー名 | `fsapi` |
| パスワード | `fsapi` |

## 8. 日常の開発ワークフロー

```bash
# 1. PostgreSQL の起動（初回 or コンテナ停止後）
docker compose -f devenv/compose.yml up -d

# 2. ビルド
dotnet build FsApi.sln

# 3. テスト実行
dotnet test FsApi.sln

# 4. API サーバー起動（ホットリロード付き）
dotnet watch run --project src/FsApi.Api/FsApi.Api.fsproj

# 5. コミット前にフォーマットチェック（CI と同じチェック）
dotnet fantomas --check .

# フォーマット違反がある場合は自動修正
dotnet fantomas .
```

### PostgreSQL の停止

```bash
# コンテナ停止（データは保持）
docker compose -f devenv/compose.yml stop

# コンテナ削除（データも削除）
docker compose -f devenv/compose.yml down -v
```

### よく使うコマンドまとめ

| 操作 | コマンド |
|---|---|
| DB 起動 | `docker compose -f devenv/compose.yml up -d` |
| ビルド | `dotnet build FsApi.sln` |
| テスト全実行 | `dotnet test FsApi.sln` |
| API 起動 | `dotnet run --project src/FsApi.Api/FsApi.Api.fsproj` |
| API 起動（ホットリロード） | `dotnet watch run --project src/FsApi.Api/FsApi.Api.fsproj` |
| Batch - 一覧 | `dotnet run --project src/FsApi.Batch/FsApi.Batch.fsproj -- list` |
| Batch - 全完了 | `dotnet run --project src/FsApi.Batch/FsApi.Batch.fsproj -- complete-all` |
| フォーマット | `dotnet fantomas .` |
| フォーマットチェック | `dotnet fantomas --check .` |
| API ドキュメント | http://localhost:5000/scalar/v1 |
