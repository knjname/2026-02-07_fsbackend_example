# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

F#で実装されたTodo APIアプリケーション。.NET 10、Oxpecker (Webフレームワーク)、PostgreSQL (Npgsql.FSharp)を使用。

## ビルド・テスト・実行コマンド

```bash
# ビルド
dotnet build FsApi.sln

# テスト全実行
dotnet test FsApi.sln

# 単一テストプロジェクト実行
dotnet test tests/FsApi.Todo.Tests/FsApi.Todo.Tests.fsproj

# 特定テスト実行 (Expectoのフィルター)
dotnet test tests/FsApi.Todo.Tests/FsApi.Todo.Tests.fsproj -- --filter "テスト名"

# PostgreSQL起動 (API実行前に必要)
docker compose -f devenv/compose.yml up

# API実行
dotnet run --project src/FsApi.Api/FsApi.Api.fsproj

# バッチCLI実行
dotnet run --project src/FsApi.Batch/FsApi.Batch.fsproj -- list
dotnet run --project src/FsApi.Batch/FsApi.Batch.fsproj -- complete-all
```

## アーキテクチャ

業務領域コロケーション（ドメイン単位のパッケージ構造）を採用。Todoドメインに関連するコード（型定義、ユースケース、ポート、リポジトリ実装）を `FsApi.Todo` プロジェクトにまとめている。

```
FsApi.SharedKernel          (共有カーネル - DomainError等の共通型)
    ↑
FsApi.Todo                  (Todoドメイン - 型・ポート・ユースケース・インフラ実装)
    ↑
FsApi.Infra                 (共有インフラ - DBマイグレーション実行のみ)
    ↑
FsApi.Api / FsApi.Batch     (プレゼンテーション層 - HTTP / CLI)
```

### 各プロジェクトの役割

- **FsApi.SharedKernel**: `DomainError`判別共用体（NotFound / ValidationError）など、ドメイン横断の共通型
- **FsApi.Todo**: Todoドメインのすべて。`Todo`レコード型・バリデーション（Domain/Todo.fs）、`ITodoRepository`ポート定義（Ports.fs）、CRUD用ユースケース関数（UseCases.fs）、FluentMigratorマイグレーション（Infra/Migrations/）、PostgreSQLリポジトリ実装（Infra/TodoRepository.fs）
- **FsApi.Infra**: FluentMigrator.Runnerを使ったDBマイグレーション実行（Database.fs）。ドメインへの依存なし
- **FsApi.Api**: OxpeckerによるHTTPハンドラー、DTO定義、OpenAPI/Scalar UI対応
- **FsApi.Batch**: CLIバッチ処理（list / complete-all コマンド）

### 設計パターン

- DIコンテナ不使用。関数パラメータで依存性を渡す関数型スタイル
- ポート/アダプタパターン: `ITodoRepository`がポート、`FsApi.Todo.Infra`が実装
- 非同期I/Oは`Task`ベース
- テストフレームワークはExpecto

## DB接続情報（ローカル開発）

PostgreSQL 16（Docker）。接続先: `Host=localhost;Port=5432;Database=fsapi;Username=fsapi;Password=fsapi`
