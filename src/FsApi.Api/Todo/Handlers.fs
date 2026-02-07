namespace FsApi.Api.Todo

open Microsoft.AspNetCore.Http
open Oxpecker
open FsApi.SharedKernel
open FsApi.Todo
open FsApi.Todo.Domain

module Handlers =

    let private writeError (statusCode: int) (msg: string) : EndpointHandler =
        fun ctx ->
            ctx.SetStatusCode statusCode
            ctx.WriteJson({ Error = msg })

    let private handleDomainError (error: DomainError) : EndpointHandler =
        match error with
        | NotFound id -> writeError 404 $"Todo with id {id} not found"
        | ValidationError msg -> writeError 400 msg

    let listTodos (repo: ITodoRepository) : EndpointHandler =
        fun ctx ->
            task {
                let! result = TodoUseCases.getAll repo ()

                match result with
                | Ok todos -> return! ctx.WriteJson(todos |> List.map Dto.toTodoResponse)
                | Error e -> return! handleDomainError e ctx
            }

    let getTodo (repo: ITodoRepository) (id: int) : EndpointHandler =
        fun ctx ->
            task {
                let! result = TodoUseCases.getById repo id

                match result with
                | Ok todo -> return! ctx.WriteJson(Dto.toTodoResponse todo)
                | Error e -> return! handleDomainError e ctx
            }

    let createTodo (repo: ITodoRepository) : EndpointHandler =
        fun ctx ->
            task {
                let! body = ctx.BindJson<CreateTodoRequest>()
                let! result = TodoUseCases.create repo body.Title

                match result with
                | Ok todo ->
                    ctx.SetStatusCode 201
                    return! ctx.WriteJson(Dto.toTodoResponse todo)
                | Error e -> return! handleDomainError e ctx
            }

    let updateTodo (repo: ITodoRepository) (id: int) : EndpointHandler =
        fun ctx ->
            task {
                let! body = ctx.BindJson<UpdateTodoRequest>()
                let! result = TodoUseCases.update repo id body.Title body.IsCompleted

                match result with
                | Ok todo -> return! ctx.WriteJson(Dto.toTodoResponse todo)
                | Error e -> return! handleDomainError e ctx
            }

    let deleteTodo (repo: ITodoRepository) (id: int) : EndpointHandler =
        fun ctx ->
            task {
                let! result = TodoUseCases.delete repo id

                match result with
                | Ok() ->
                    ctx.SetStatusCode 204
                    return ()
                | Error e -> return! handleDomainError e ctx
            }
