using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

List<Participante> participantes = new List<Participante>();

app.MapGet("/", () => "Campeonato de Sinuca");

//Listar
app.MapGet("/api/participantes/listar", (
    [FromServices] AppDataContext ctx) =>
{
    if (ctx.Participantes.Count() == 0)
    {
        return Results.BadRequest("A lista de participantes está vazia");
    }
    return Results.Ok(ctx.Participantes.ToList());
});

//Cadastrar
app.MapPost("/api/participante/cadastrar", 
    ([FromBody] Participante? participante,
    [FromServices] AppDataContext ctx) =>
{
    //Verifica a existencia do participante
    if (participante is null)
    {
        return Results.BadRequest("O participante não pode ser nulo");
    }

    //Verifica a entrada do nome
    if (string.IsNullOrWhiteSpace(participante.Nome))
    {
        return Results.BadRequest("O nome não pode ser vazio");
    }
    string nomeTratado = participante.Nome.Trim();
    participante.Nome = nomeTratado;

    //Verifica a entrada do cpf
    if (string.IsNullOrWhiteSpace(participante.Cpf))
    {
        return Results.BadRequest("O Cpf não pode ser vazio");
    }
    //Deixa apenas numeros na entrada do cpf
    string cpfTratado = new string(participante.Cpf.Where(char.IsDigit).ToArray());
    //Verificacao simples de cpf pela quantidade de caracteres, uma verificacao mais seria utilizaria a formula de veracidade do cpf
    if (cpfTratado.Length != 11)
    {
        return Results.BadRequest("CPF Invalido");
    }
    participante.Cpf = cpfTratado;
    //Verifica a entrada do genero
    if (string.IsNullOrWhiteSpace(participante.Genero))
    {
        return Results.BadRequest("O campo Genero não pode ser vazio");
    }
    //Deixa a entrada de genero toda em minusculo
    string generoTratado = participante.Genero.ToLower().Trim();
    //Limita a entrada de genero para Masculino e Feminino para separacao por categoria
    if (generoTratado != "masculino" && generoTratado != "feminino")
    {
        return Results.BadRequest("O campo Gênero deve ser 'Masculino' ou 'Feminino'.");
    }
    //Muda a forma para apenas a primeira letra maiuscula
    participante.Genero = char.ToUpper(generoTratado[0]) + generoTratado.Substring(1);
    
    if (participante.Idade < 12 || participante.Idade > 120)
    {
        return Results.BadRequest("O participante nao esta apto a competir");
    }
    //Verifica a existencia de um participante a partir do cpf, nao pelo nome pois o nome pode se repetir
    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == cpfTratado);
    if (participanteEncontrado is not null)
    {
        return Results.BadRequest("Esse participante já foi cadastrado!");
    }
    //salva o particiapante
    ctx.Participantes.Add(participante);
    ctx.SaveChanges();
    return Results.Created("", participante);
});

app.MapGet("/api/participante/buscar/{Cpf}", 
    ([FromRoute] string cpf,
    [FromServices] AppDataContext ctx) =>
{
    //Expressão lambda
    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == cpf);
    if (participanteEncontrado is null)
    {
        return Results.NotFound("Participante nao encontrado!");
    }
    return Results.Ok(participanteEncontrado);    
});

app.Run();