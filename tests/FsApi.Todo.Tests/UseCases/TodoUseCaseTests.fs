module FsApi.Todo.Tests.TodoUseCaseTests

open System
open System.Threading.Tasks
open Expecto
open FsApi.SharedKernel
open FsApi.Todo.Domain
open FsApi.Todo

let sampleTodo =
    { Id = 1
      Title = "Test Todo"
      IsCompleted = false
      CreatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
      UpdatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) }

let emptyRepo: ITodoRepository =
    { GetAll = fun () -> Task.FromResult([])
      GetById = fun _ -> Task.FromResult(None)
      Create = fun title -> Task.FromResult({ sampleTodo with Title = title })
      Update = fun _ _ _ -> Task.FromResult(None)
      Delete = fun _ -> Task.FromResult(false) }

[<Tests>]
let getAllTests =
    testList
        "TodoUseCases.getAll"
        [ testCase "returns Ok with todo list from repo"
          <| fun _ ->
              let repo =
                  { emptyRepo with
                      GetAll = fun () -> Task.FromResult([ sampleTodo ]) }

              let result =
                  TodoUseCases.getAll repo () |> Async.AwaitTask |> Async.RunSynchronously

              Expect.equal result (Ok [ sampleTodo ]) "should return all todos"

          testCase "returns Ok with empty list when no todos"
          <| fun _ ->
              let result =
                  TodoUseCases.getAll emptyRepo () |> Async.AwaitTask |> Async.RunSynchronously

              Expect.equal result (Ok []) "should return empty list" ]

[<Tests>]
let getByIdTests =
    testList
        "TodoUseCases.getById"
        [ testCase "returns Ok todo when found"
          <| fun _ ->
              let repo =
                  { emptyRepo with
                      GetById = fun id -> Task.FromResult(if id = 1 then Some sampleTodo else None) }

              let result =
                  TodoUseCases.getById repo 1 |> Async.AwaitTask |> Async.RunSynchronously

              Expect.equal result (Ok sampleTodo) "should return the todo"

          testCase "returns Error NotFound when not found"
          <| fun _ ->
              let result =
                  TodoUseCases.getById emptyRepo 99 |> Async.AwaitTask |> Async.RunSynchronously

              Expect.equal result (Error(NotFound 99)) "should return NotFound" ]

[<Tests>]
let createTests =
    testList
        "TodoUseCases.create"
        [ testCase "returns Ok todo with valid title"
          <| fun _ ->
              let created = { sampleTodo with Title = "New Todo" }

              let repo =
                  { emptyRepo with
                      Create = fun title -> Task.FromResult({ created with Title = title }) }

              let result =
                  TodoUseCases.create repo "New Todo" |> Async.AwaitTask |> Async.RunSynchronously

              Expect.equal result (Ok { created with Title = "New Todo" }) "should create todo"

          testCase "returns Error ValidationError with empty title"
          <| fun _ ->
              let result =
                  TodoUseCases.create emptyRepo "" |> Async.AwaitTask |> Async.RunSynchronously

              Expect.equal result (Error(ValidationError "Title cannot be empty")) "should reject empty title" ]

[<Tests>]
let updateTests =
    testList
        "TodoUseCases.update"
        [ testCase "returns Ok todo when found and title updated"
          <| fun _ ->
              let updated = { sampleTodo with Title = "Updated" }

              let repo =
                  { emptyRepo with
                      Update =
                          fun id title _ ->
                              if id = 1 then
                                  Task.FromResult(
                                      Some
                                          { updated with
                                              Title = title |> Option.defaultValue updated.Title }
                                  )
                              else
                                  Task.FromResult(None) }

              let result =
                  TodoUseCases.update repo 1 (Some "Updated") None
                  |> Async.AwaitTask
                  |> Async.RunSynchronously

              Expect.equal result (Ok updated) "should update title"

          testCase "returns Error NotFound when todo does not exist"
          <| fun _ ->
              let result =
                  TodoUseCases.update emptyRepo 99 (Some "Title") None
                  |> Async.AwaitTask
                  |> Async.RunSynchronously

              Expect.equal result (Error(NotFound 99)) "should return NotFound"

          testCase "returns Error ValidationError with invalid title"
          <| fun _ ->
              let result =
                  TodoUseCases.update emptyRepo 1 (Some "") None
                  |> Async.AwaitTask
                  |> Async.RunSynchronously

              Expect.equal result (Error(ValidationError "Title cannot be empty")) "should reject empty title"

          testCase "returns Ok todo when only isCompleted updated"
          <| fun _ ->
              let updated = { sampleTodo with IsCompleted = true }

              let repo =
                  { emptyRepo with
                      Update =
                          fun id _ _ ->
                              if id = 1 then
                                  Task.FromResult(Some updated)
                              else
                                  Task.FromResult(None) }

              let result =
                  TodoUseCases.update repo 1 None (Some true)
                  |> Async.AwaitTask
                  |> Async.RunSynchronously

              Expect.equal result (Ok updated) "should update isCompleted" ]

[<Tests>]
let deleteTests =
    testList
        "TodoUseCases.delete"
        [ testCase "returns Ok when deleted"
          <| fun _ ->
              let repo =
                  { emptyRepo with
                      Delete = fun id -> Task.FromResult(id = 1: bool) }

              let result = TodoUseCases.delete repo 1 |> Async.AwaitTask |> Async.RunSynchronously
              Expect.equal result (Ok()) "should return Ok"

          testCase "returns Error NotFound when not found"
          <| fun _ ->
              let result =
                  TodoUseCases.delete emptyRepo 99 |> Async.AwaitTask |> Async.RunSynchronously

              Expect.equal result (Error(NotFound 99)) "should return NotFound" ]

[<Tests>]
let completeAllTests =
    testList
        "TodoUseCases.completeAll"
        [ testCase "returns count of incomplete todos completed"
          <| fun _ ->
              let todo1 =
                  { sampleTodo with
                      Id = 1
                      IsCompleted = false }

              let todo2 =
                  { sampleTodo with
                      Id = 2
                      IsCompleted = false }

              let todo3 =
                  { sampleTodo with
                      Id = 3
                      IsCompleted = true }

              let repo =
                  { emptyRepo with
                      GetAll = fun () -> Task.FromResult([ todo1; todo2; todo3 ])
                      Update = fun _ _ _ -> Task.FromResult(Some sampleTodo) }

              let result =
                  TodoUseCases.completeAll repo () |> Async.AwaitTask |> Async.RunSynchronously

              Expect.equal result 2 "should complete 2 incomplete todos"

          testCase "returns 0 when all todos already completed"
          <| fun _ ->
              let todo1 =
                  { sampleTodo with
                      Id = 1
                      IsCompleted = true }

              let todo2 =
                  { sampleTodo with
                      Id = 2
                      IsCompleted = true }

              let repo =
                  { emptyRepo with
                      GetAll = fun () -> Task.FromResult([ todo1; todo2 ]) }

              let result =
                  TodoUseCases.completeAll repo () |> Async.AwaitTask |> Async.RunSynchronously

              Expect.equal result 0 "should return 0" ]
