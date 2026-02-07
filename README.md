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

業務領域コロケーション（ドメイン単位のパッケージ構造）を採用。ドメインに関連するコードを1つのプロジェクトにまとめ、凝集度を高めている。

```
FsApi.SharedKernel          (共有カーネル - DomainError等の共通型)
    ↑
FsApi.Todo                  (Todoドメイン - 型・ポート・ユースケース・インフラ実装)
    ↑
FsApi.Infra                 (共有インフラ - DBマイグレーション実行)
    ↑
FsApi.Api / FsApi.Batch     (プレゼンテーション層 - HTTP / CLI)
```

### 設計上のポイント

- **業務領域コロケーション** - ドメインの型定義、ポート、ユースケース、インフラ実装を1プロジェクトに集約
- **DI コンテナ不使用** - F# のレコード型でポート（`ITodoRepository`）を定義し、関数パラメータとして渡す関数型スタイル
- **`Result<'T, DomainError>` によるエラーハンドリング** - 例外ではなく判別共用体でドメインエラーを表現
- **非同期 I/O は `Task` ベース** - `task { }` コンピュテーション式を使用

## ディレクトリ構造

```
fsapi/
├── src/
│   ├── FsApi.SharedKernel/         # 共有カーネル（依存なし）
│   │   └── DomainError.fs          #   DomainError 判別共用体
│   │
│   ├── FsApi.Todo/                 # Todoドメイン（SharedKernel に依存）
│   │   ├── Domain/Todo.fs          #   Todo レコード型、バリデーション
│   │   ├── Ports.fs                #   ITodoRepository ポート定義
│   │   ├── UseCases.fs             #   CRUD + completeAll ユースケース関数
│   │   └── Infra/                  #   インフラ実装
│   │       ├── Migrations/         #     FluentMigrator マイグレーション
│   │       └── TodoRepository.fs   #     ITodoRepository の PostgreSQL 実装
│   │
│   ├── FsApi.Infra/                # 共有インフラ（ドメイン非依存）
│   │   └── Database.fs             #   FluentMigrator Runner によるマイグレーション実行
│   │
│   ├── FsApi.Api/                  # Web API（SharedKernel, Todo, Infra に依存）
│   │   ├── Todo/Dto.fs             #   リクエスト/レスポンス DTO 定義
│   │   ├── Todo/Handlers.fs        #   Oxpecker HTTPハンドラー
│   │   └── Program.fs              #   エントリポイント、ルーティング、OpenAPI 設定
│   │
│   └── FsApi.Batch/                # バッチ CLI（SharedKernel, Todo, Infra に依存）
│       └── Program.fs              #   list / complete-all コマンド
│
├── tests/
│   └── FsApi.Todo.Tests/           # Todoドメインテスト
│       ├── Domain/TodoTests.fs     #   validateTitle のテスト（7ケース）
│       ├── UseCases/               #   モックリポジトリを使ったテスト（14ケース）
│       │   └── TodoUseCaseTests.fs
│       └── Program.fs              #   Expecto テストランナー
│
├── devenv/
│   └── compose.yml                 # ローカル開発用 PostgreSQL（Docker Compose）
│
├── .github/workflows/
│   └── ci.yml                      # GitHub Actions CI
│
├── .config/
│   └── dotnet-tools.json           # dotnet tool 管理（Fantomas）
│
├── FsApi.sln                       # ソリューションファイル
└── CLAUDE.md                       # Claude Code 向けプロジェクト説明
```

### 新しいドメインの追加方法

新しいドメイン（例: `User`）を追加する場合:

1. `src/FsApi.User/` プロジェクトを作成し、Domain/型定義、Ports.fs、UseCases.fs、Infra/ をまとめる
2. `FsApi.SharedKernel` への参照を追加
3. `FsApi.Api` と必要なプレゼンテーション層から参照を追加
4. マイグレーションアセンブリを `Database.migrate` の引数に追加

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
