namespace FsApi.Infra

open System
open Npgsql.FSharp
open FsApi.Domain
open FsApi.UseCase

module TodoRepository =

    let private mapRow (read: RowReader) : Todo = {
        Id = read.int "id"
        Title = read.string "title"
        IsCompleted = read.bool "is_completed"
        CreatedAt = read.datetimeOffset "created_at"
        UpdatedAt = read.datetimeOffset "updated_at"
    }

    let create (connectionString: string) : ITodoRepository = {
        GetAll =
            fun () ->
                connectionString
                |> Sql.connect
                |> Sql.query "SELECT id, title, is_completed, created_at, updated_at FROM todos ORDER BY id"
                |> Sql.executeAsync mapRow

        GetById =
            fun id ->
                task {
                    let! rows =
                        connectionString
                        |> Sql.connect
                        |> Sql.query "SELECT id, title, is_completed, created_at, updated_at FROM todos WHERE id = @id"
                        |> Sql.parameters [ "id", Sql.int id ]
                        |> Sql.executeAsync mapRow

                    return rows |> List.tryHead
                }

        Create =
            fun title ->
                task {
                    let! rows =
                        connectionString
                        |> Sql.connect
                        |> Sql.query
                            "INSERT INTO todos (title) VALUES (@title) RETURNING id, title, is_completed, created_at, updated_at"
                        |> Sql.parameters [ "title", Sql.string title ]
                        |> Sql.executeAsync mapRow

                    return rows |> List.head
                }

        Update =
            fun id title isCompleted ->
                task {
                    let setClauses = ResizeArray<string>()
                    let parameters = ResizeArray<string * SqlValue>()

                    match title with
                    | Some t ->
                        setClauses.Add("title = @title")
                        parameters.Add("title", Sql.string t)
                    | None -> ()

                    match isCompleted with
                    | Some c ->
                        setClauses.Add("is_completed = @is_completed")
                        parameters.Add("is_completed", Sql.bool c)
                    | None -> ()

                    if setClauses.Count = 0 then
                        let! rows =
                            connectionString
                            |> Sql.connect
                            |> Sql.query
                                "SELECT id, title, is_completed, created_at, updated_at FROM todos WHERE id = @id"
                            |> Sql.parameters [ "id", Sql.int id ]
                            |> Sql.executeAsync mapRow

                        return rows |> List.tryHead
                    else
                        setClauses.Add("updated_at = NOW()")
                        parameters.Add("id", Sql.int id)

                        let setClause = String.Join(", ", setClauses)

                        let sql =
                            $"UPDATE todos SET {setClause} WHERE id = @id RETURNING id, title, is_completed, created_at, updated_at"

                        let! rows =
                            connectionString
                            |> Sql.connect
                            |> Sql.query sql
                            |> Sql.parameters (parameters |> Seq.toList)
                            |> Sql.executeAsync mapRow

                        return rows |> List.tryHead
                }

        Delete =
            fun id ->
                task {
                    let! affected =
                        connectionString
                        |> Sql.connect
                        |> Sql.query "DELETE FROM todos WHERE id = @id"
                        |> Sql.parameters [ "id", Sql.int id ]
                        |> Sql.executeNonQueryAsync

                    return affected > 0
                }
    }
