namespace FsApi.Infra

open Npgsql.FSharp

module Database =

    let migrate (connectionString: string) =
        connectionString
        |> Sql.connect
        |> Sql.query
            """
            CREATE TABLE IF NOT EXISTS todos (
                id SERIAL PRIMARY KEY,
                title VARCHAR(200) NOT NULL,
                is_completed BOOLEAN NOT NULL DEFAULT FALSE,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            )
            """
        |> Sql.executeNonQuery
        |> ignore
