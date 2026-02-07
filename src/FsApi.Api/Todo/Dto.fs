namespace FsApi.Api.Todo

open System
open FsApi.Todo.Domain

[<CLIMutable>]
type CreateTodoRequest = { Title: string }

[<CLIMutable>]
type UpdateTodoRequest =
    { Title: string option
      IsCompleted: bool option }

[<CLIMutable>]
type TodoResponse =
    { Id: int
      Title: string
      IsCompleted: bool
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

[<CLIMutable>]
type ErrorResponse = { Error: string }

module Dto =

    let toTodoResponse (todo: Todo) : TodoResponse =
        { Id = todo.Id
          Title = todo.Title
          IsCompleted = todo.IsCompleted
          CreatedAt = todo.CreatedAt
          UpdatedAt = todo.UpdatedAt }
