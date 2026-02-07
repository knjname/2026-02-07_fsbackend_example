namespace FsApi.Todo.Domain

open System
open FsApi.SharedKernel

type Todo =
    { Id: int
      Title: string
      IsCompleted: bool
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

module Todo =

    let validateTitle (title: string) =
        if String.IsNullOrWhiteSpace(title) then
            Error(ValidationError "Title cannot be empty")
        elif title.Length > 200 then
            Error(ValidationError "Title must be 200 characters or less")
        else
            Ok(title.Trim())
