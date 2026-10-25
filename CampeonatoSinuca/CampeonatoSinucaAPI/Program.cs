using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

app.MapGet("/", () => "Campeonato de Sinuca");

//Listar
app.MapGet("/api/participantes/listar", (
    [FromServices] AppDataContext ctx) =>
{
    if (ctx.Participantes.Count() == 0)
    {
        return Results.NotFound("A lista de participantes está vazia");
    }
    return Results.Ok(ctx.Participantes.ToList());
});

//Cadastrar
app.MapPost("/api/participantes/cadastrar", 
    ([FromBody] ParticipanteRequest? request,
    [FromServices] AppDataContext ctx) =>
{
    //Verifica a existencia da requisicao
    if (request is null)
    {
        return Results.BadRequest("O participante não pode ser nulo");
    }

    //---------- Nome ----------
    if (string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest("O nome não pode ser vazio");
    }
    string nomeTratado = request.Nome.Trim();

    //---------- CPF ----------
    if (string.IsNullOrWhiteSpace(request.Cpf))
    {
        return Results.BadRequest("O Cpf não pode ser vazio");
    }
    //Deixa apenas numeros na entrada do cpf
    string cpfTratado = new string(request.Cpf.Where(char.IsDigit).ToArray());
    //Verificacao simples de cpf pela quantidade de caracteres, uma verificacao mais seria utilizaria a formula de veracidade do cpf
    if (cpfTratado.Length != 11)
    {
        return Results.BadRequest("CPF Invalido");
    }

    //---------- Genero ----------
    if (string.IsNullOrWhiteSpace(request.Genero))
    {
        return Results.BadRequest("O campo Genero não pode ser vazio");
    }
    string generoTratado = request.Genero.ToLower().Trim();
    //Limita a entrada de genero para Masculino e Feminino para separacao por categoria
    if (generoTratado != "masculino" && generoTratado != "feminino")
    {
        return Results.BadRequest("O campo Gênero deve ser 'Masculino' ou 'Feminino'.");
    }
    //Deixa apenas a primeira letra maiuscula
    string generoFinal = char.ToUpper(generoTratado[0]) + generoTratado.Substring(1);

    //---------- Idade ----------
    if (request.Idade < 12 || request.Idade > 120)
    {
        return Results.BadRequest("O participante nao esta apto a competir");
    }

    //---------- Modalidades ----------
    if (request.ModalidadesIds is null || request.ModalidadesIds.Count == 0)
    {
        return Results.BadRequest("O participante deve escolher pelo menos uma modalidade");
    }
    List<int> modalidadesIds = request.ModalidadesIds.Distinct().ToList();
    if (modalidadesIds.Count != request.ModalidadesIds.Count)
    {
        return Results.BadRequest("Há modalidades repetidas na lista");
    }
    //Busca no banco as modalidades escolhidas, todas precisam existir
    List<Modalidade> modalidades = ctx.Modalidades
        .Where(m => modalidadesIds.Contains(m.Id))
        .ToList();
    if (modalidades.Count != modalidadesIds.Count)
    {
        return Results.BadRequest("Uma ou mais modalidades não existem");
    }

    //---------- CPF duplicado ----------
    //Verifica a existencia de um participante a partir do cpf, nao pelo nome pois o nome pode se repetir
    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == cpfTratado);
    if (participanteEncontrado is not null)
    {
        return Results.BadRequest("Esse participante já foi cadastrado!");
    }

    //---------- Gravacao ----------
    Participante participante = new Participante
    {
        Nome = nomeTratado,
        Cpf = cpfTratado,
        Idade = request.Idade,
        Genero = generoFinal
    };

    //Uma inscricao para cada modalidade escolhida, ligada ao participante
    foreach (Modalidade modalidade in modalidades)
    {
        ctx.Inscricoes.Add(new Inscricao
        {
            Participante = participante,
            Modalidade = modalidade
        });
    }

    //Um unico SaveChanges grava o participante e as inscricoes juntos
    ctx.SaveChanges();

    return Results.Created($"/api/participantes/buscar/{participante.Cpf}", new
    {
        participante.Id,
        participante.Nome,
        participante.Cpf,
        participante.Idade,
        participante.Genero,
        Modalidades = modalidades.Select(m => m.Nome).ToList()
    });
});

app.MapGet("/api/participantes/buscar/{Cpf}", 
    ([FromRoute] string cpf,
    [FromServices] AppDataContext ctx) =>
{
    string cpfTratado = new string(cpf.Where(char.IsDigit).ToArray());
    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == cpfTratado);
    if (participanteEncontrado is null)
    {
        return Results.NotFound("Participante nao encontrado!");
    }
    return Results.Ok(participanteEncontrado);    
});

app.MapDelete("/api/participantes/remover/{Cpf}", 
    ([FromRoute] string cpf,
    [FromServices] AppDataContext ctx) =>
{
    string cpfTratado = new string(cpf.Where(char.IsDigit).ToArray());
    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == cpfTratado);
    if (participanteEncontrado is null)
    {
        return Results.NotFound("Participante nao encontrado!");
    }
    ctx.Participantes.Remove(participanteEncontrado);
    ctx.SaveChanges();
    return Results.Ok(participanteEncontrado);    

});

//Alterar
app.MapPut("/api/participantes/alterar/{Cpf}", 
    ([FromRoute] string cpf, 
    [FromBody] ParticipanteRequest participanteAlterado,
    [FromServices] AppDataContext ctx) =>
{
    //---------- Busca do participante ----------
    string cpfTratado = new string(cpf.Where(char.IsDigit).ToArray());
    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == cpfTratado);
    if (participanteEncontrado is null)
    {
        return Results.NotFound("Participante nao encontrado!");
    }

    //---------- Nome ----------
    if (string.IsNullOrWhiteSpace(participanteAlterado.Nome))
    {
        return Results.BadRequest("O nome não pode ser vazio");
    }
    string nomeTratado = participanteAlterado.Nome.Trim();

    //---------- Genero ----------
    if (string.IsNullOrWhiteSpace(participanteAlterado.Genero))
    {
        return Results.BadRequest("O campo Genero não pode ser vazio");
    }
    string generoTratado = participanteAlterado.Genero.ToLower().Trim();
    if (generoTratado != "masculino" && generoTratado != "feminino")
    {
        return Results.BadRequest("O campo Gênero deve ser 'Masculino' ou 'Feminino'.");
    }
    string generoFinal = char.ToUpper(generoTratado[0]) + generoTratado.Substring(1);

    //---------- Idade ----------
    if (participanteAlterado.Idade < 12 || participanteAlterado.Idade > 120)
    {
        return Results.BadRequest("O participante nao esta apto a competir");
    }

    //---------- Modalidades ----------
    if (participanteAlterado.ModalidadesIds is null || participanteAlterado.ModalidadesIds.Count == 0)
    {
        return Results.BadRequest("O participante deve escolher pelo menos uma modalidade");
    }
    List<int> modalidadesIds = participanteAlterado.ModalidadesIds.Distinct().ToList();
    if (modalidadesIds.Count != participanteAlterado.ModalidadesIds.Count)
    {
        return Results.BadRequest("Há modalidades repetidas na lista");
    }
    //Busca no banco as modalidades escolhidas, todas precisam existir
    List<Modalidade> modalidades = ctx.Modalidades
        .Where(m => modalidadesIds.Contains(m.Id))
        .ToList();
    if (modalidades.Count != modalidadesIds.Count)
    {
        return Results.BadRequest("Uma ou mais modalidades não existem");
    }

    //---------- Atualizacao (so depois de todas as validacoes) ----------
    //O CPF nao e alterado, pois identifica o participante
    participanteEncontrado.Nome = nomeTratado;
    participanteEncontrado.Genero = generoFinal;
    participanteEncontrado.Idade = participanteAlterado.Idade;

    //Remove as inscricoes atuais do participante
    List<Inscricao> inscricoesAtuais = ctx.Inscricoes
        .Where(i => i.ParticipanteId == participanteEncontrado.Id)
        .ToList();
    ctx.Inscricoes.RemoveRange(inscricoesAtuais);

    //Cria uma inscricao para cada modalidade da lista nova
    foreach (Modalidade modalidade in modalidades)
    {
        ctx.Inscricoes.Add(new Inscricao
        {
            ParticipanteId = participanteEncontrado.Id,
            Modalidade = modalidade
        });
    }

    //Um unico SaveChanges grava dados pessoais e inscricoes juntos
    ctx.SaveChanges();

    return Results.Ok(new
    {
        participanteEncontrado.Id,
        participanteEncontrado.Nome,
        participanteEncontrado.Cpf,
        participanteEncontrado.Idade,
        participanteEncontrado.Genero,
        Modalidades = modalidades.Select(m => m.Nome).ToList()
    });
});

//Gerar chaveamento
app.MapPost("/api/partidas/gerar/{modalidadeId}",
    ([FromRoute] int modalidadeId,
    [FromServices] AppDataContext ctx) =>
{
    //Pega os participantes inscritos nessa modalidade, em ordem aleatoria
    List<int> inscritos = ctx.Inscricoes
        .Where(i => i.ModalidadeId == modalidadeId)
        .Select(i => i.ParticipanteId)
        .ToList()
        .OrderBy(_ => Random.Shared.Next())
        .ToList();

    if (inscritos.Count < 2)
    {
        return Results.BadRequest("São necessários pelo menos 2 participantes inscritos");
    }

    //Forma os pares
    List<Partida> partidas = new List<Partida>();
    for (int i = 0; i + 1 < inscritos.Count; i += 2)
    {
        partidas.Add(new Partida
        {
            ModalidadeId = modalidadeId,
            Participante1Id = inscritos[i],
            Participante2Id = inscritos[i + 1],
            Data = DateTime.Now
        });
    }

    ctx.Partidas.AddRange(partidas);
    ctx.SaveChanges();

    return Results.Ok(partidas);
});

//Registrar vencedor
app.MapPut("/api/partidas/vencedor/{partidaId}/{vencedorId}",
    ([FromRoute] int partidaId,
    [FromRoute] int vencedorId,
    [FromServices] AppDataContext ctx) =>
{
    Partida? partida = ctx.Partidas.Find(partidaId);
    if (partida is null)
    {
        return Results.NotFound("Partida não encontrada!");
    }

    //O vencedor precisa ser um dos dois jogadores da partida
    if (vencedorId != partida.Participante1Id && vencedorId != partida.Participante2Id)
    {
        return Results.BadRequest("O vencedor deve ser um dos participantes da partida");
    }

    partida.VencedorId = vencedorId;
    ctx.SaveChanges();

    return Results.Ok(partida);
});

app.Run();