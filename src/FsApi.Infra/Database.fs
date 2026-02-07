namespace FsApi.Infra

open Microsoft.Extensions.DependencyInjection
open FluentMigrator.Runner

module Database =

    let migrate (connectionString: string) =
        let serviceProvider =
            ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(fun rb ->
                    rb
                        .AddPostgres()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof<Migrations.CreateTodosTable>.Assembly).For.Migrations()
                    |> ignore)
                .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore)
                .BuildServiceProvider(false)

        use scope = serviceProvider.CreateScope()
        let runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>()
        runner.MigrateUp()
