module FsApi.Domain.Tests.TodoTests

open Expecto
open FsApi.Domain

[<Tests>]
let validateTitleTests =
    testList
        "Todo.validateTitle"
        [ testCase "normal string returns Ok with trimmed title"
          <| fun _ ->
              let result = Todo.validateTitle "Buy milk"
              Expect.equal result (Ok "Buy milk") "should return Ok with the title"

          testCase "string with surrounding whitespace returns Ok trimmed"
          <| fun _ ->
              let result = Todo.validateTitle "  hello world  "
              Expect.equal result (Ok "hello world") "should trim whitespace"

          testCase "exactly 200 characters returns Ok"
          <| fun _ ->
              let title = String.replicate 200 "a"
              let result = Todo.validateTitle title
              Expect.equal result (Ok title) "should accept 200 char title"

          testCase "empty string returns ValidationError"
          <| fun _ ->
              let result = Todo.validateTitle ""
              Expect.equal result (Error(ValidationError "Title cannot be empty")) "should reject empty"

          testCase "null returns ValidationError"
          <| fun _ ->
              let result = Todo.validateTitle null
              Expect.equal result (Error(ValidationError "Title cannot be empty")) "should reject null"

          testCase "whitespace only returns ValidationError"
          <| fun _ ->
              let result = Todo.validateTitle "   "
              Expect.equal result (Error(ValidationError "Title cannot be empty")) "should reject whitespace only"

          testCase "201 characters returns ValidationError"
          <| fun _ ->
              let title = String.replicate 201 "a"
              let result = Todo.validateTitle title

              Expect.equal
                  result
                  (Error(ValidationError "Title must be 200 characters or less"))
                  "should reject >200 chars" ]
