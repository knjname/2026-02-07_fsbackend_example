FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY src/FsApi.SharedKernel/FsApi.SharedKernel.fsproj src/FsApi.SharedKernel/
COPY src/FsApi.Todo/FsApi.Todo.fsproj src/FsApi.Todo/
COPY src/FsApi.Infra/FsApi.Infra.fsproj src/FsApi.Infra/
COPY src/FsApi.Api/FsApi.Api.fsproj src/FsApi.Api/
COPY src/FsApi.Batch/FsApi.Batch.fsproj src/FsApi.Batch/
RUN dotnet restore src/FsApi.Api/FsApi.Api.fsproj && \
    dotnet restore src/FsApi.Batch/FsApi.Batch.fsproj

COPY src/ src/
RUN dotnet publish src/FsApi.Api/FsApi.Api.fsproj -c Release -o /app/api --no-restore && \
    dotnet publish src/FsApi.Batch/FsApi.Batch.fsproj -c Release -o /app/batch --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS api
WORKDIR /app

COPY --from=build /app/api .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "FsApi.Api.dll"]

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS batch
WORKDIR /app

COPY --from=build /app/batch .

USER $APP_UID
ENTRYPOINT ["dotnet", "FsApi.Batch.dll"]
