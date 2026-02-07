open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Oxpecker
open Oxpecker.OpenApi
open Scalar.AspNetCore
open FsApi.Infra
open FsApi.Api
open FsApi.Api.Handlers

let endpoints repo =
    [ GET
          [ route "/todos" (listTodos repo) |> addOpenApiSimple<unit, TodoResponse list>

            routef "/todos/{%i}" (getTodo repo) |> addOpenApiSimple<int, TodoResponse> ]
      POST
          [ route "/todos" (createTodo repo)
            |> addOpenApiSimple<CreateTodoRequest, TodoResponse> ]
      PUT
          [ routef "/todos/{%i}" (updateTodo repo)
            |> addOpenApiSimple<UpdateTodoRequest, TodoResponse> ]
      DELETE [ routef "/todos/{%i}" (deleteTodo repo) |> addOpenApiSimple<int, unit> ] ]

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddRouting().AddOxpecker().AddOpenApi() |> ignore

    let app = builder.Build()

    let connectionString = app.Configuration.GetConnectionString("DefaultConnection")

    Database.migrate connectionString

    let repo = TodoRepository.create connectionString

    app.UseRouting() |> ignore
    app.MapOpenApi() |> ignore
    app.MapScalarApiReference() |> ignore
    app.UseOxpecker(endpoints repo) |> ignore

    app.Run()
    0
