FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY src/FsApi.SharedKernel/FsApi.SharedKernel.fsproj src/FsApi.SharedKernel/
COPY src/FsApi.Todo/FsApi.Todo.fsproj src/FsApi.Todo/
COPY src/FsApi.Infra/FsApi.Infra.fsproj src/FsApi.Infra/
COPY src/FsApi.Api/FsApi.Api.fsproj src/FsApi.Api/
RUN dotnet restore src/FsApi.Api/FsApi.Api.fsproj

COPY src/ src/
RUN dotnet publish src/FsApi.Api/FsApi.Api.fsproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "FsApi.Api.dll"]
