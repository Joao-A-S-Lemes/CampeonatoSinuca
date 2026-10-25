using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>();

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

app.MapPost("/api/participante/cadastrar", 
    ([FromBody] Participante? participante,
    [FromServices] AppDataContext ctx) =>
{
    if (participante is null)
    {
        return Results.BadRequest("O participante não pode ser nulo");
    }
    
    if (string.IsNullOrWhiteSpace(participante.Nome))
    {
        return Results.BadRequest("O nome não pode ser vazio");
    }

    if (string.IsNullOrWhiteSpace(participante.Cpf))
    {
        return Results.BadRequest("O Cpf não pode ser vazio");
    }

    if (string.IsNullOrWhiteSpace(participante.Genero))
    {
        return Results.BadRequest("O campo Genero não pode ser vazio");
    }

    participante.Genero = participante.Genero.ToLower().Trim();
    if (participante.Genero != "masculino" && participante.Genero != "feminino")
    {
        return Results.BadRequest("O campo Gênero deve ser 'masculino' ou 'feminino'.");
    }

    if (participante.Cpf.Length != 11)
    {
        return Results.BadRequest("CPF Invalido");
    }

    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == participante.Cpf);
    if (participanteEncontrado is not null)
    {
        return Results.BadRequest("Esse participante já foi cadastrado!");
    }

    ctx.Participantes.Add(participante);
    ctx.SaveChanges();
    return Results.Created("", participante);
});


app.Run();