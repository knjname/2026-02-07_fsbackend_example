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
dotnet test tests/FsApi.Domain.Tests/FsApi.Domain.Tests.fsproj
dotnet test tests/FsApi.UseCase.Tests/FsApi.UseCase.Tests.fsproj

# 特定テスト実行 (Expectoのフィルター)
dotnet test tests/FsApi.Domain.Tests/FsApi.Domain.Tests.fsproj -- --filter "テスト名"

# PostgreSQL起動 (API実行前に必要)
docker compose -f devenv/compose.yml up

# API実行
dotnet run --project src/FsApi.Api/FsApi.Api.fsproj

# バッチCLI実行
dotnet run --project src/FsApi.Batch/FsApi.Batch.fsproj -- list
dotnet run --project src/FsApi.Batch/FsApi.Batch.fsproj -- complete-all
```

## アーキテクチャ

クリーンアーキテクチャ（ヘキサゴナルアーキテクチャ）を採用。依存方向は外側→内側。

```
FsApi.Api / FsApi.Batch  (プレゼンテーション層)
    ↓
FsApi.UseCase             (アプリケーション層 - ビジネスロジック)
    ↓
FsApi.Domain              (ドメイン層 - エンティティ・バリデーション)
    ↑
FsApi.Infra               (インフラ層 - DB実装) ※UseCase層のPortを実装
```

### 各層の役割

- **FsApi.Domain**: `Todo`レコード型、`DomainError`判別共用体（NotFound / ValidationError）、バリデーション関数
- **FsApi.UseCase**: `ITodoRepository`ポート定義（Ports.fs）、CRUD用ユースケース関数（TodoUseCases.fs）。すべて`Result<'T, DomainError>`で関数的エラーハンドリング
- **FsApi.Infra**: PostgreSQLへのリポジトリ実装（Npgsql.FSharp使用）、DBマイグレーション（Database.fs）
- **FsApi.Api**: OxpeckerによるHTTPハンドラー、DTO定義、OpenAPI/Scalar UI対応
- **FsApi.Batch**: CLIバッチ処理（list / complete-all コマンド）

### 設計パターン

- DIコンテナ不使用。関数パラメータで依存性を渡す関数型スタイル
- ポート/アダプタパターン: `ITodoRepository`がポート、Infra層が実装
- 非同期I/Oは`Task`ベース
- テストフレームワークはExpecto

## DB接続情報（ローカル開発）

PostgreSQL 16（Docker）。接続先: `Host=localhost;Port=5432;Database=fsapi;Username=fsapi;Password=fsapi`
