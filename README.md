# FsApi - F# バックエンドスタック検証プロジェクト

2026年現在、F# で Web API バックエンドを構成する場合にどのような技術スタックになるかを検証するためのプロジェクト。
題材として Todo API（CRUD + バッチ処理）を実装している。

## 技術スタック

| カテゴリ | 選定技術 | 備考 |
|---|---|---|
| 言語 | **F#** (.NET 10) | `net10.0` ターゲット |
| Web フレームワーク | **Oxpecker** 1.5 | F# 向け軽量 Web フレームワーク。Giraffe の後継的な位置づけ |
| API ドキュメント | **Scalar** + OpenAPI | `Oxpecker.OpenApi` でルート定義から自動生成、Scalar UI で閲覧可能 |
| DB アクセス | **Npgsql.FSharp** 8.0 | PostgreSQL 用の F# フレンドリーなクエリビルダー。ORM は使わない |
| DB | **PostgreSQL 16** | Docker (Alpine) で起動 |
| テスト | **Expecto** 10.x | F# ネイティブなテストフレームワーク。`dotnet test` 統合は YoloDev.Expecto.TestSdk 経由 |
| コードフォーマッタ | **Fantomas** 7.0 | dotnet tool として管理。CI でフォーマットチェックを実施 |
| CI | **GitHub Actions** | ビルド・フォーマットチェック・テストを自動実行 |

## アーキテクチャ

クリーンアーキテクチャ（ヘキサゴナルアーキテクチャ）を採用。依存方向は外側から内側への一方向。

```
FsApi.Api / FsApi.Batch   (プレゼンテーション層 - HTTP / CLI)
        ↓
FsApi.UseCase              (アプリケーション層 - ビジネスロジック)
        ↓
FsApi.Domain               (ドメイン層 - エンティティ・バリデーション)
        ↑
FsApi.Infra                (インフラ層 - DB実装。UseCase層のポートを実装)
```

### 設計上のポイント

- **DI コンテナ不使用** - F# のレコード型でポート（`ITodoRepository`）を定義し、関数パラメータとして渡す関数型スタイル
- **`Result<'T, DomainError>` によるエラーハンドリング** - 例外ではなく判別共用体でドメインエラーを表現
- **非同期 I/O は `Task` ベース** - `task { }` コンピュテーション式を使用

## ディレクトリ構造

```
fsapi/
├── src/
│   ├── FsApi.Domain/            # ドメイン層（依存なし）
│   │   └── Todo.fs              #   Todo レコード型、DomainError 判別共用体、バリデーション
│   │
│   ├── FsApi.UseCase/           # アプリケーション層（Domain に依存）
│   │   ├── Ports.fs             #   ITodoRepository ポート定義（レコード型のインターフェース）
│   │   └── TodoUseCases.fs      #   CRUD + completeAll ユースケース関数
│   │
│   ├── FsApi.Infra/             # インフラ層（Domain, UseCase に依存）
│   │   ├── Database.fs          #   マイグレーション（CREATE TABLE IF NOT EXISTS）
│   │   └── TodoRepository.fs    #   ITodoRepository の PostgreSQL 実装
│   │
│   ├── FsApi.Api/               # Web API（全層に依存）
│   │   ├── Dto.fs               #   リクエスト/レスポンス DTO 定義
│   │   ├── Handlers.fs          #   Oxpecker HTTPハンドラー
│   │   └── Program.fs           #   エントリポイント、ルーティング、OpenAPI 設定
│   │
│   └── FsApi.Batch/             # バッチ CLI（全層に依存）
│       └── Program.fs           #   list / complete-all コマンド
│
├── tests/
│   ├── FsApi.Domain.Tests/      # ドメイン層テスト
│   │   ├── TodoTests.fs         #   validateTitle のテスト（8ケース）
│   │   └── Program.fs           #   Expecto テストランナー
│   │
│   └── FsApi.UseCase.Tests/     # ユースケース層テスト
│       ├── TodoUseCaseTests.fs  #   モックリポジトリを使ったテスト（27ケース）
│       └── Program.fs           #   Expecto テストランナー
│
├── devenv/
│   └── compose.yml              # ローカル開発用 PostgreSQL（Docker Compose）
│
├── .github/workflows/
│   └── ci.yml                   # GitHub Actions CI
│
├── .config/
│   └── dotnet-tools.json        # dotnet tool 管理（Fantomas）
│
├── FsApi.sln                    # ソリューションファイル
└── CLAUDE.md                    # Claude Code 向けプロジェクト説明
```

### 各層にどのモジュールを配置するか

| 層 | 配置するもの | 配置しないもの |
|---|---|---|
| **Domain** | エンティティのレコード型、判別共用体（エラー型など）、バリデーション関数 | DB アクセス、HTTP、外部ライブラリ依存 |
| **UseCase** | ポート定義（リポジトリ等のインターフェース）、ビジネスロジック関数 | 具体的な DB 実装、フレームワーク依存 |
| **Infra** | ポートの具体実装（DB リポジトリ）、マイグレーション | HTTP ハンドラー、ルーティング |
| **Api** | DTO、HTTP ハンドラー、ルーティング、OpenAPI 設定 | ビジネスロジック |
| **Batch** | CLI コマンド実装、引数パース | HTTP関連の処理 |

## セットアップ

### 前提条件

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/)（PostgreSQL 用）

### ローカル開発

```bash
# PostgreSQL を起動
docker compose -f devenv/compose.yml up -d

# ビルド
dotnet build FsApi.sln

# テスト実行
dotnet test FsApi.sln

# API サーバー起動（http://localhost:5000）
dotnet run --project src/FsApi.Api/FsApi.Api.fsproj

# API ドキュメント（Scalar UI）
# http://localhost:5000/scalar/v1 で閲覧可能
```

### バッチ CLI

```bash
# Todo 一覧表示
dotnet run --project src/FsApi.Batch/FsApi.Batch.fsproj -- list

# 全 Todo を完了にする
dotnet run --project src/FsApi.Batch/FsApi.Batch.fsproj -- complete-all
```

## API エンドポイント

| メソッド | パス | 説明 |
|---|---|---|
| GET | `/todos` | Todo 一覧取得 |
| GET | `/todos/{id}` | Todo 取得 |
| POST | `/todos` | Todo 作成 |
| PUT | `/todos/{id}` | Todo 更新 |
| DELETE | `/todos/{id}` | Todo 削除 |

## CI

GitHub Actions で以下を自動実行:

1. **Fantomas フォーマットチェック** - `dotnet fantomas --check .`
2. **ビルド** - `dotnet build`
3. **テスト** - `dotnet test`（CI 環境では PostgreSQL サービスコンテナを使用）
