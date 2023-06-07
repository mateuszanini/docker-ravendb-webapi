using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

var builder = WebApplication.CreateBuilder(args);
var nomeBaseDados = "Passaro";

using var repositorioDocumentos = new DocumentStore
{
    Conventions = new DocumentConventions
    {
        IdentityPartsSeparator = '-'
    },
    Database = nomeBaseDados,
    Urls = new[] { builder.Configuration.GetConnectionString("RavenDB") }
};

repositorioDocumentos.Initialize();

await CertificarExistenciaBaseDados(nomeBaseDados);
await ImplantarDadosAsync("Corvo", "Curucaca", "Tucano", "Papagaio");

async Task CertificarExistenciaBaseDados(string nomeBaseDados)
{
    nomeBaseDados ??= repositorioDocumentos.Database;

    try
    {
        await repositorioDocumentos.Maintenance.ForDatabase(nomeBaseDados).SendAsync(new GetStatisticsOperation());
    }
    catch (DatabaseDoesNotExistException)
    {
        try
        {
            await repositorioDocumentos.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(nomeBaseDados)));
        }
        catch (ConcurrencyException) { }
    }
}

async Task ImplantarDadosAsync(params string[] especies)
{
    using var sessao = repositorioDocumentos.OpenAsyncSession();

    if (especies.Any() && !await sessao.Query<Passaro>().AnyAsync())
    {

        foreach (var especie in especies)
            await sessao.StoreAsync(new Passaro(especie));

        await sessao.SaveChangesAsync();
    }
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services
//    .AddCors(opcoes =>
//    {
//        opcoes.AddDefaultPolicy(construtor =>
//        {
//            construtor.WithOrigins("*").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
//        });
//    });
//.AddCors(opcoes =>
//{
//    opcoes.AddDefaultPolicy(construtor =>
//    {
//        construtor.WithOrigins("http://localhost:80").AllowAnyHeader().AllowAnyMethod();
//    });
//});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

//app.UseCors(opcoes => opcoes
//    .AllowAnyMethod()
//    .AllowAnyHeader()
//    .AllowAnyOrigin()
//    .SetIsOriginAllowed(origin => true)
//);

//app.UseCors(construtor =>
//{
//    construtor.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
//});

app.MapGet("/passaro", async () =>
{
    try
    {
        using var sessao = repositorioDocumentos.OpenAsyncSession();
        var passaros = await sessao.Query<Passaro>().ToListAsync();

        return Results.Ok(passaros);
    }
    catch (Exception excecao)
    {
        return Results.BadRequest(excecao.Message);
    }
});

app.MapGet("/passaro/{id}", async (string? id) =>
{
    try
    {
        using var sessao = repositorioDocumentos.OpenAsyncSession();

        if (await sessao.LoadAsync<Passaro>(id) is Passaro passaro)
            return Results.Ok(passaro);

        return Results.NotFound();
    }
    catch (Exception excecao)
    {
        return Results.BadRequest(excecao.Message);
    }
});

app.MapPost("/passaro", async (string especie) =>
{
    try
    {
        using var sessao = repositorioDocumentos.OpenAsyncSession();
        var passaro = new Passaro(especie);
        await sessao.StoreAsync(passaro);
        await sessao.SaveChangesAsync();

        return Results.Ok(passaro);
    }
    catch (Exception excecao)
    {
        return Results.BadRequest(excecao.Message);
    }
});

app.MapPut("/passaro/{id}", async (string id, string especie) =>
{
    try
    {
        using var sessao = repositorioDocumentos.OpenAsyncSession();
        var passaro = new Passaro(especie, id);
        await sessao.StoreAsync(passaro);
        await sessao.SaveChangesAsync();

        return Results.Ok(passaro);
    }
    catch (Exception excecao)
    {
        return Results.BadRequest(excecao.Message);
    }
});

app.MapDelete("/passaro/{id}", async (string id) =>
{
    try
    {
        using var sessao = repositorioDocumentos.OpenAsyncSession();
        sessao.Delete(id);
        await sessao.SaveChangesAsync();
        return Results.Ok();
    }
    catch (Exception excecao)
    {
        return Results.BadRequest(excecao.Message);
    }
});

await app.RunAsync();

internal record Passaro(string Especie, string? Id = null);