namespace FsApi.UseCase

open System.Threading.Tasks
open FsApi.Domain

module TodoUseCases =

    let getAll (repo: ITodoRepository) () : Task<Result<Todo list, DomainError>> =
        task {
            let! todos = repo.GetAll()
            return Ok todos
        }

    let getById (repo: ITodoRepository) (id: int) : Task<Result<Todo, DomainError>> =
        task {
            let! todo = repo.GetById id
            match todo with
            | Some t -> return Ok t
            | None -> return Error(NotFound id)
        }

    let create (repo: ITodoRepository) (title: string) : Task<Result<Todo, DomainError>> =
        task {
            match Todo.validateTitle title with
            | Error e -> return Error e
            | Ok validTitle ->
                let! todo = repo.Create validTitle
                return Ok todo
        }

    let update
        (repo: ITodoRepository)
        (id: int)
        (title: string option)
        (isCompleted: bool option)
        : Task<Result<Todo, DomainError>> =
        task {
            match title with
            | Some t ->
                match Todo.validateTitle t with
                | Error e -> return Error e
                | Ok validTitle ->
                    let! result = repo.Update id (Some validTitle) isCompleted
                    match result with
                    | Some todo -> return Ok todo
                    | None -> return Error(NotFound id)
            | None ->
                let! result = repo.Update id None isCompleted
                match result with
                | Some todo -> return Ok todo
                | None -> return Error(NotFound id)
        }

    let delete (repo: ITodoRepository) (id: int) : Task<Result<unit, DomainError>> =
        task {
            let! deleted = repo.Delete id
            if deleted then
                return Ok()
            else
                return Error(NotFound id)
        }

    let completeAll (repo: ITodoRepository) () : Task<int> =
        task {
            let! todos = repo.GetAll()
            let incomplete = todos |> List.filter (fun t -> not t.IsCompleted)
            let mutable count = 0
            for todo in incomplete do
                let! _ = repo.Update todo.Id None (Some true)
                count <- count + 1
            return count
        }
