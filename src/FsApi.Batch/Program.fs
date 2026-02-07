open System.IO
open Microsoft.Extensions.Configuration
open FsApi.Infra
open FsApi.UseCase

let getConnectionString () =
    let config =
        ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional = true)
            .Build()

    let cs = config.GetConnectionString("DefaultConnection")

    if isNull cs then
        "Host=localhost;Port=5432;Database=fsapi;Username=fsapi;Password=fsapi"
    else
        cs

let runList (repo: ITodoRepository) =
    task {
        let! result = TodoUseCases.getAll repo ()

        match result with
        | Ok todos ->
            if todos.IsEmpty then
                printfn "No todos found."
            else
                for todo in todos do
                    let status = if todo.IsCompleted then "x" else " "
                    printfn $"[{status}] #{todo.Id}: {todo.Title}"
        | Error e -> eprintfn $"Error: %A{e}"
    }

let runCompleteAll (repo: ITodoRepository) =
    task {
        let! count = TodoUseCases.completeAll repo ()
        printfn $"Completed {count} todo(s)."
    }

[<EntryPoint>]
let main args =
    let connectionString = getConnectionString ()
    Database.migrate connectionString
    let repo = TodoRepository.create connectionString

    match args with
    | [| "list" |] -> (runList repo).GetAwaiter().GetResult()
    | [| "complete-all" |] -> (runCompleteAll repo).GetAwaiter().GetResult()
    | _ ->
        eprintfn "Usage: FsApi.Batch <command>"
        eprintfn "Commands:"
        eprintfn "  list          List all todos"
        eprintfn "  complete-all  Mark all todos as completed"
        exit 1

    0
