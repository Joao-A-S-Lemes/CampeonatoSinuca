using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

List<Participante> participantes = new List<Participante>();

app.MapGet("/", () => "Campeonato de Sinuca");

app.MapGet("/api/participantes/listar", (
    [FromServices] AppDataContext ctx) =>
{
    if (ctx.Participantes.Count() == 0)
    {
        return Results.BadRequest("A lista de participantes está vazia");
    }
    return Results.Ok(ctx.Participantes.ToList());
});

app.Run();