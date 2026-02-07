namespace FsApi.Todo

open System.Threading.Tasks
open FsApi.Todo.Domain

type ITodoRepository =
    { GetAll: unit -> Task<Todo list>
      GetById: int -> Task<Todo option>
      Create: string -> Task<Todo>
      Update: int -> string option -> bool option -> Task<Todo option>
      Delete: int -> Task<bool> }
