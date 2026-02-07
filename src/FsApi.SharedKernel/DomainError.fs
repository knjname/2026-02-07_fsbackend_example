namespace FsApi.SharedKernel

type DomainError =
    | NotFound of int
    | ValidationError of string
