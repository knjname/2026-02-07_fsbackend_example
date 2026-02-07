namespace FsApi.Todo.Infra.Migrations

open FluentMigrator

[<Migration(20250101120000L)>]
type CreateTodosTable() =
    inherit Migration()

    override this.Up() =
        this.Create
            .Table("todos")
            .WithColumn("id")
            .AsInt32()
            .NotNullable()
            .PrimaryKey()
            .Identity()
            .WithColumn("title")
            .AsString(200)
            .NotNullable()
            .WithColumn("is_completed")
            .AsBoolean()
            .NotNullable()
            .WithDefaultValue(false)
            .WithColumn("created_at")
            .AsDateTimeOffset()
            .NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at")
            .AsDateTimeOffset()
            .NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime)
        |> ignore

    override this.Down() = this.Delete.Table("todos") |> ignore
