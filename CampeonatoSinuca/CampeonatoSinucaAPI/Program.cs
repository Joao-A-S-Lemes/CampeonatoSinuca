using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

//Separar categorias
static string Categoria(Participante p) =>
    p.Genero + " " + (p.Idade < 18 ? "Junior" : "Adulto");

//Nomear as fases conforme avançam
static string NomeDaFase(int jogadores)
{
    if (jogadores <= 2) return "Final";
    if (jogadores <= 4) return "Semifinal";
    if (jogadores <= 8) return "Quartas";
    if (jogadores <= 16) return "Oitavas";
    return "Fase preliminar";
}

app.MapGet("/", () => "Campeonato de Sinuca");

//Listar
app.MapGet("/api/participantes/listar", (
    [FromServices] AppDataContext ctx) =>
{
    if (ctx.Participantes.Count() == 0 )
    {
        return Results.Ok("Nenhum participante cadastrado.");
    }
    var participantes = ctx.Participantes
        .Select(p => new
        {
            p.Id,
            p.Nome,
            p.Cpf,
            p.Idade,
            p.Genero,
            Modalidades = ctx.Inscricoes
                .Where(i => i.ParticipanteId == p.Id)
                .Select(i => i.Modalidade!.Nome)
                .ToList()
        })
        .ToList();

    return Results.Ok(participantes);
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

    //Nome
    if (string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest("O nome não pode ser vazio");
    }
    string nomeTratado = request.Nome.Trim();

    //Cpf
    if (string.IsNullOrWhiteSpace(request.Cpf))
    {
        return Results.BadRequest("O Cpf não pode ser vazio");
    }
    //Deixa apenas numeros na entrada do cpf
    string cpfTratado = new string(request.Cpf.Where(char.IsDigit).ToArray());
    //Verificacao simples de cpf pela quantidade de caracteres, uma verificacao mais seria utilizaria a formula de veracidade do cpf
    if (cpfTratado.Length != 11)
    {
        return Results.BadRequest("CPF Inválido");
    }

    //Genero
    if (string.IsNullOrWhiteSpace(request.Genero))
    {
        return Results.BadRequest("O campo Gênero não pode ser vazio");
    }
    string generoTratado = request.Genero.ToLower().Trim();
    //Limita a entrada de genero para Masculino e Feminino para separacao por categoria
    if (generoTratado != "masculino" && generoTratado != "feminino")
    {
        return Results.BadRequest("O campo Gênero deve ser 'Masculino' ou 'Feminino'.");
    }
    //Deixa apenas a primeira letra maiuscula
    string generoFinal = char.ToUpper(generoTratado[0]) + generoTratado.Substring(1);

    //Idade
    if (request.Idade < 12 || request.Idade > 120)
    {
        return Results.BadRequest("O participante não está apto a competir");
    }

    //Modadlidades
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

    //Verifica a existencia de um participante a partir do cpf, nao pelo nome pois o nome pode se repetir
    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == cpfTratado);
    if (participanteEncontrado is not null)
    {
        return Results.BadRequest("Esse participante já foi cadastrado!");
    }

    //Gravacao
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
        return Results.NotFound("Participante não encontrado!");
    }
    return Results.Ok(new
    {
        participanteEncontrado.Id,
        participanteEncontrado.Nome,
        participanteEncontrado.Cpf,
        participanteEncontrado.Idade,
        participanteEncontrado.Genero,
        Modalidades = ctx.Inscricoes
            .Where(i => i.ParticipanteId == participanteEncontrado.Id)
            .Select(i => i.Modalidade!.Nome)
            .ToList()
    });  
});

app.MapDelete("/api/participantes/remover/{Cpf}", 
    ([FromRoute] string cpf,
    [FromServices] AppDataContext ctx) =>
{
    string cpfTratado = new string(cpf.Where(char.IsDigit).ToArray());
    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == cpfTratado);
    if (participanteEncontrado is null)
    {
        return Results.NotFound("Participante não encontrado!");
    }
    //Nao pode remover quem ja tem partida
    bool temPartida = ctx.Partidas.Any(p =>
        p.Participante1Id == participanteEncontrado.Id ||
        p.Participante2Id == participanteEncontrado.Id);
    if (temPartida) 
    {
        return Results.BadRequest("O Participante já possui pelo menos 1 partida e por isso não pode ser removido!");
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
    //Busca do participante
    string cpfTratado = new string(cpf.Where(char.IsDigit).ToArray());
    Participante? participanteEncontrado = ctx.Participantes.FirstOrDefault(x => x.Cpf == cpfTratado);
    if (participanteEncontrado is null)
    {
        return Results.NotFound("Participante não encontrado!");
    }

    //Nome
    if (string.IsNullOrWhiteSpace(participanteAlterado.Nome))
    {
        return Results.BadRequest("O nome não pode ser vazio");
    }
    string nomeTratado = participanteAlterado.Nome.Trim();

    //Genero
    if (string.IsNullOrWhiteSpace(participanteAlterado.Genero))
    {
        return Results.BadRequest("O campo Gênero não pode ser vazio");
    }
    string generoTratado = participanteAlterado.Genero.ToLower().Trim();
    if (generoTratado != "masculino" && generoTratado != "feminino")
    {
        return Results.BadRequest("O campo Gênero deve ser 'Masculino' ou 'Feminino'.");
    }
    string generoFinal = char.ToUpper(generoTratado[0]) + generoTratado.Substring(1);

    //Idade
    if (participanteAlterado.Idade < 12 || participanteAlterado.Idade > 120)
    {
        return Results.BadRequest("O participante não está apto a competir");
    }

    //Modalidades
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

    //Atualizacao (so depois de todas as validacoes)
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

//Gerar chaveamento (primeira fase de cada categoria)
app.MapPost("/api/partidas/gerar/{modalidadeId}",
    ([FromRoute] int modalidadeId,
    [FromServices] AppDataContext ctx) =>
{
    Modalidade? modalidade = ctx.Modalidades.Find(modalidadeId);
    if (modalidade is null)
    {
        return Results.NotFound("Modalidade não encontrada!");
    }

    //Não gera duas vezes a mesma modalidade
    if (ctx.Partidas.Any(p => p.ModalidadeId == modalidadeId))
    {
        return Results.BadRequest("O chaveamento dessa modalidade ja foi gerado");
    }

    //Inscritos da modalidade, em ordem aleatoria
    List<Participante> inscritos = ctx.Inscricoes
        .Where(i => i.ModalidadeId == modalidadeId)
        .Select(i => i.Participante!)
        .ToList()
        .OrderBy(_ => Random.Shared.Next())
        .ToList();

    List<Partida> partidas = new List<Partida>();
    List<string> passaDireto = new List<string>();
    List<string> ignorados = new List<string>();

    foreach (var grupo in inscritos.GroupBy(p => Categoria(p)))
    {
        List<Participante> lista = grupo.ToList();

        //Sem adversário
        if (lista.Count < 2)
        {
            ignorados.Add(lista[0].Nome + " (" + grupo.Key + ")");
            continue;
        }

        string fase = NomeDaFase(lista.Count);
        for (int i = 0; i + 1 < lista.Count; i += 2)
        {
            partidas.Add(new Partida
            {
                ModalidadeId = modalidadeId,
                Participante1Id = lista[i].Id,
                Participante2Id = lista[i + 1].Id,
                Data = DateTime.Now,
                Categoria = grupo.Key,
                Fase = fase
            });
        }

        //Se for um número ímpar de participantes, um passa direto pra fase seguinte
        if (lista.Count % 2 == 1)
        {
            passaDireto.Add(lista.Last().Nome + " (" + grupo.Key + ")");
        }
    }

    if (partidas.Count == 0)
    {
        return Results.BadRequest("Nao há participantes suficientes na mesma categoria para formar partidas");
    }

    ctx.Partidas.AddRange(partidas);
    ctx.SaveChanges();

    Dictionary<int, Participante> porId = inscritos.ToDictionary(p => p.Id);

    return Results.Ok(new
    {
        Modalidade = modalidade.Nome,
        PassaDireto = passaDireto,
        Ignorados = ignorados,
        Partidas = partidas.Select(p => new
        {
            p.Id,
            p.Categoria,
            p.Fase,
            Participante1 = porId[p.Participante1Id].Nome,
            Participante2 = porId[p.Participante2Id].Nome
        }).ToList()
    });
});

//Avancar fase
app.MapPost("/api/partidas/avancar/{modalidadeId}",
    ([FromRoute] int modalidadeId,
    [FromServices] AppDataContext ctx) =>
{
    Modalidade? modalidade = ctx.Modalidades.Find(modalidadeId);
    if (modalidade is null)
    {
        return Results.NotFound("Modalidade não encontrada!");
    }

    List<Partida> todas = ctx.Partidas
        .Where(p => p.ModalidadeId == modalidadeId)
        .OrderBy(p => p.Id)
        .ToList();
    if (todas.Count == 0)
    {
        return Results.BadRequest("Gere o chaveamento da modalidade primeiro");
    }

    Dictionary<int, Participante> porId = ctx.Participantes.ToDictionary(p => p.Id);
    List<int> inscritosIds = ctx.Inscricoes
        .Where(i => i.ModalidadeId == modalidadeId)
        .Select(i => i.ParticipanteId)
        .ToList();

    List<Partida> novas = new List<Partida>();
    List<string> avisos = new List<string>();

    //Cada categoria tem o seu proprio torneio
    foreach (var grupo in todas.GroupBy(p => p.Categoria))
    {
        string categoria = grupo.Key;

        //Partidas da disputa (sem o terceiro lugar) e a fase em andamento
        List<Partida> disputa = grupo.Where(p => p.Fase != "Terceiro lugar").ToList();
        string faseAtual = disputa.Last().Fase;
        List<Partida> rodada = disputa.Where(p => p.Fase == faseAtual).ToList();

        if (rodada.Any(p => p.VencedorId == null))
        {
            avisos.Add(categoria + ": aguardando resultados da fase " + faseAtual);
            continue;
        }
        if (faseAtual == "Final")
        {
            avisos.Add(categoria + ": campeonato encerrado");
            continue;
        }

        //Eliminados: quem perdeu alguma partida da disputa
        HashSet<int> eliminados = new HashSet<int>();
        foreach (Partida p in disputa)
        {
            eliminados.Add(p.VencedorId == p.Participante1Id ? p.Participante2Id : p.Participante1Id);
        }

        //Inscritos da categoria que ainda não foram eliminados
        List<int> classificados = inscritosIds
            .Where(id => Categoria(porId[id]) == categoria && !eliminados.Contains(id))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();
        if (classificados.Count < 2)
        {
            continue;
        }

        string proximaFase = NomeDaFase(classificados.Count);
        for (int i = 0; i + 1 < classificados.Count; i += 2)
        {
            novas.Add(new Partida
            {
                ModalidadeId = modalidadeId,
                Participante1Id = classificados[i],
                Participante2Id = classificados[i + 1],
                Data = DateTime.Now,
                Categoria = categoria,
                Fase = proximaFase
            });
        }
        if (classificados.Count % 2 == 1)
        {
            avisos.Add(categoria + ": " + porId[classificados.Last()].Nome + " passa direto para a proxima fase");
        }

        //Os 2 perdedores da semifinal disputam o terceiro lugar
        if (proximaFase == "Final" && rodada.Count == 2)
        {
            List<int> perdedores = rodada
                .Select(p => p.VencedorId == p.Participante1Id ? p.Participante2Id : p.Participante1Id)
                .ToList();
            novas.Add(new Partida
            {
                ModalidadeId = modalidadeId,
                Participante1Id = perdedores[0],
                Participante2Id = perdedores[1],
                Data = DateTime.Now,
                Categoria = categoria,
                Fase = "Terceiro lugar"
            });
        }
    }

    if (novas.Count > 0)
    {
        ctx.Partidas.AddRange(novas);
        ctx.SaveChanges();
    }

    return Results.Ok(new
    {
        Avisos = avisos,
        NovasPartidas = novas.Select(p => new
        {
            p.Id,
            p.Categoria,
            p.Fase,
            Participante1 = porId[p.Participante1Id].Nome,
            Participante2 = porId[p.Participante2Id].Nome
        }).ToList()
    });
});

//Classificacao final por categoria
app.MapGet("/api/partidas/classificacao/{modalidadeId}", (
    [FromRoute] int modalidadeId,
    [FromServices] AppDataContext ctx) =>
{
    Modalidade? modalidade = ctx.Modalidades.Find(modalidadeId);
    if (modalidade is null)
    {
        return Results.NotFound("Modalidade não encontrada!");
    }

    Dictionary<int, Participante> porId = ctx.Participantes.ToDictionary(p => p.Id);
    List<Partida> partidas = ctx.Partidas
        .Where(p => p.ModalidadeId == modalidadeId)
        .ToList();

    string? Nome(int? id) => id == null ? null : porId[id.Value].Nome;
    int Perdedor(Partida p) => p.VencedorId == p.Participante1Id ? p.Participante2Id : p.Participante1Id;

    var categorias = partidas
        .GroupBy(p => p.Categoria)
        .OrderBy(g => g.Key)
        .Select(g =>
        {
            Partida? final = g.FirstOrDefault(p => p.Fase == "Final");
            Partida? terceiro = g.FirstOrDefault(p => p.Fase == "Terceiro lugar");
            bool finalDecidida = final?.VencedorId != null;

            return new
            {
                Categoria = g.Key,
                Status = finalDecidida ? "Final decidida" : "Em andamento",
                Campeao = finalDecidida ? Nome(final!.VencedorId) : null,
                Vice = finalDecidida ? Nome(Perdedor(final!)) : null,
                Terceiro = terceiro?.VencedorId != null ? Nome(terceiro.VencedorId) : null
            };
        })
        .ToList();

    return Results.Ok(new
    {
        Modalidade = modalidade.Nome,
        Categorias = categorias
    });
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

    //Não permite trocar o resultado depois de registrado
    if (partida.VencedorId != null)
    {
        return Results.BadRequest("Essa partida já tem vencedor registrado");
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

//Ranking por modalidade
app.MapGet("/api/partidas/ranking/{modalidadeId}",
    ([FromRoute] int modalidadeId,
    [FromServices] AppDataContext ctx) =>
{
    Modalidade? modalidade = ctx.Modalidades.Find(modalidadeId);
    if (modalidade is null)
    {
        return Results.NotFound("Modalidade inválida!");
    }

    //Conta as vitorias de cada participante nessa modalidade
    var vitorias = ctx.Partidas
        .Where(p => p.ModalidadeId == modalidadeId && p.VencedorId != null)
        .GroupBy(p => p.VencedorId!.Value)
        .Select(g => new { ParticipanteId = g.Key, Vitorias = g.Count() })
        .OrderByDescending(r => r.Vitorias)
        .ToList();

    //Troca o id pelo nome 
    var ranking = vitorias.Select(r => new
    {
        Participante = ctx.Participantes.Find(r.ParticipanteId)!.Nome,
        r.Vitorias
    }).ToList();

    return Results.Ok(new
    {
        Modalidade = modalidade.Nome,
        Ranking = ranking
    });
});

//Listar modalidades disponíveis
app.MapGet("/api/modalidades/listar", (
    [FromServices] AppDataContext ctx) =>
{
    return Results.Ok(ctx.Modalidades.ToList());
});

//Listar partidas de uma modalidade
app.MapGet("/api/partidas/listar/{modalidadeId}", (
    [FromRoute] int modalidadeId,
    [FromServices] AppDataContext ctx) =>
{
    Dictionary<int, Participante> porId = ctx.Participantes.ToDictionary(p => p.Id);

    var partidas = ctx.Partidas
        .Where(p => p.ModalidadeId == modalidadeId)
        .OrderBy(p => p.Categoria)
        .ThenBy(p => p.Id)
        .ToList()
        .Select(p => new
        {
            p.Id,
            p.Categoria,
            p.Fase,
            p.Participante1Id,
            Participante1 = porId[p.Participante1Id].Nome,
            p.Participante2Id,
            Participante2 = porId[p.Participante2Id].Nome,
            Vencedor = p.VencedorId == null ? null : porId[p.VencedorId.Value].Nome
        })
        .ToList();

    return Results.Ok(partidas);
});

app.Run();