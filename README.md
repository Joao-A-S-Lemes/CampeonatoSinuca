# 🎱 Campeonato de Sinuca — API de Inscrições e Chaveamento

> ⚠️ **ANTES DE ENTREGAR:** procurem por `PREENCHER` neste arquivo e completem tudo. Depois apaguem estas observações em destaque.

---

## 1. Identificação do projeto

- **Título:** Campeonato de Sinuca — API de Inscrições e Chaveamento
- **Disciplina:** Desenvolvimento de Software Visual — Projeto Final (Parte 1)
- **Integrantes:**
  - João Antonio Sousa Lemes
  - Leonardo da Cruz Fierdzoski
- **Curso e turma:** Ciência da computação 4º Período

---

## 2. Resumo

O **Campeonato de Sinuca** é uma API REST desenvolvida em C# (.NET 8, Minimal API) com Entity Framework e banco SQLite, criada para administrar a inscrição de jogadores em um torneio de sinuca. O organizador cadastra cada participante informando nome, CPF, idade, gênero (Masculino ou Feminino) e as modalidades em que deseja competir: **8 Ball**, **9 Ball** e **Snooker 15 reds**. Os dados são validados e gravados no banco, e cada escolha de modalidade vira uma inscrição que liga o participante a uma modalidade previamente cadastrada. A partir das inscrições, o sistema gera o chaveamento das partidas de cada modalidade, permite registrar o vencedor de cada partida e consulta o ranking de vitórias por modalidade. Assim, os dados cadastrados são usados em funcionalidades reais de organização de um campeonato, e não apenas armazenados.

---

## 3. Funcionalidades

- Cadastro de participantes com validação de nome, CPF, idade e gênero
- Listagem de todos os participantes, com as modalidades de cada um
- Busca de participante por CPF
- Alteração de dados e de modalidades de um participante
- Remoção de participante (bloqueada se ele já tiver partidas)
- Inscrição em uma ou mais modalidades (8 Ball, 9 Ball, Snooker 15 reds)
- Listagem das modalidades disponíveis
- Geração do chaveamento (partidas) de uma modalidade
- Registro do vencedor de uma partida
- Ranking de vitórias por modalidade

---

## 4. Descrição das funcionalidades

### 4.1 Cadastro de participante — `POST /api/participantes/cadastrar`

Recebe os dados do jogador e a lista de Ids das modalidades escolhidas. Antes de gravar, o sistema valida:

- **Nome:** não pode ser vazio (espaços nas pontas são removidos).
- **CPF:** são mantidos apenas os dígitos, e precisam ser 11. O CPF é único: não é possível cadastrar o mesmo participante duas vezes. A validação é pelo tamanho; o cálculo dos dígitos verificadores não foi implementado.
- **Gênero:** aceita apenas *Masculino* ou *Feminino*, em qualquer combinação de maiúsculas e minúsculas, e é gravado de forma padronizada.
- **Idade:** entre 12 e 120 anos.
- **Modalidades:** pelo menos uma, sem repetições, e todas precisam existir no banco.

Se tudo estiver válido, o participante e suas inscrições são gravados juntos, em uma única operação. Se qualquer validação falhar, nada é gravado.

### 4.2 Listagem e busca — `GET /api/participantes/listar` e `GET /api/participantes/buscar/{cpf}`

A listagem devolve todos os participantes; a busca devolve um participante pelo CPF (com ou sem pontuação). Nos dois casos, a resposta inclui os nomes das modalidades em que o participante está inscrito.

### 4.3 Alteração — `PUT /api/participantes/alterar/{cpf}`

Altera nome, gênero, idade e a lista de modalidades, com as mesmas validações do cadastro. O **CPF não pode ser alterado**, pois identifica o participante. As inscrições antigas são substituídas pelas novas na mesma operação.

### 4.4 Remoção — `DELETE /api/participantes/remover/{cpf}`

Remove o participante e as suas inscrições. Se ele já participou de alguma partida, a remoção é recusada, para preservar o histórico do campeonato.

### 4.5 Modalidades e inscrições — `GET /api/modalidades/listar`

As três modalidades são criadas automaticamente na criação do banco. A listagem mostra os Ids, usados no cadastro. A **inscrição** é a entidade que liga um participante a uma modalidade (relação muitos-para-muitos), e o banco impede a mesma inscrição duplicada.

### 4.6 Chaveamento — `POST /api/partidas/gerar/{modalidadeId}`

> ✍️ **PREENCHER (Leonardo):** descrever como as partidas são geradas (quem entra, como os pares são formados, o que acontece com número ímpar de inscritos, limitações), e, se for o caso, como o chaveamento trata gênero e faixa de idade.

### 4.7 Registro do vencedor — `PUT /api/partidas/vencedor/{partidaId}/{vencedorId}`

> ✍️ **PREENCHER (Leonardo):** descrever a regra (o vencedor precisa ser um dos dois jogadores da partida) e o que acontece depois do registro.

### 4.8 Ranking — `GET /api/partidas/ranking/{modalidadeId}`

> ✍️ **PREENCHER (Leonardo):** descrever como o ranking é calculado e o que ele devolve.

---

## 5. Modelo de dados

O banco tem **quatro tabelas**:

| Tabela | Descrição | Relacionamentos |
|---|---|---|
| `Participantes` | Jogadores (nome, CPF único, idade, gênero) | — |
| `Modalidades` | 8 Ball, 9 Ball, Snooker 15 reds | — |
| `Inscricoes` | Participante inscrito em uma modalidade | `ParticipanteId` → Participantes, `ModalidadeId` → Modalidades (par único) |
| `Partidas` | Confronto entre dois participantes numa modalidade | `ModalidadeId`, `Participante1Id`, `Participante2Id`, `VencedorId` (opcional) |

---

## 6. Tecnologias

- C# com .NET SDK 8 (Minimal API), padrão REST com JSON
- Entity Framework Core 8 (migrations)
- SQLite
- Git e GitHub para versionamento e colaboração

---

## 7. Como executar

Pré-requisito: .NET SDK 8.

```bash
cd CampeonatoSinuca/CampeonatoSinucaAPI
dotnet tool install --global dotnet-ef   # apenas na primeira vez
dotnet ef database update                # cria o banco Data/Participantes.db
dotnet run
```

O arquivo do banco não é versionado: cada pessoa gera o seu com `dotnet ef database update`. A API sobe em `http://localhost:5059`.

Para testar, use os arquivos da pasta `Tests/` (extensão `.http`, compatíveis com a extensão REST Client do VS Code):

- `CadastroParticipantes.http` — cadastra 32 participantes de exemplo
- `CRUD.http` — listar, buscar, remover e cadastrar
- `Chaveamento.http` — listar modalidades e gerar o chaveamento
- `Partidas.http` — registrar vencedor e consultar o ranking

---

## 8. Repositório

https://github.com/Joao-A-S-Lemes/CampeonatoSinuca

---

## 9. Uso de IA

**Ferramenta utilizada:** Claude (Anthropic). `PREENCHER (Leonardo): informar se ele usou outra ferramenta ou a mesma.`

**Forma de uso (João):**

- Revisão do andamento do projeto em relação aos requisitos do professor e sugestão de modelagem (entidades `Participante`, `Modalidade`, `Inscricao` e `Partida`).
- Explicações sobre o funcionamento de migrations do Entity Framework, do banco SQLite, de chaves estrangeiras e do `Include`.
- Revisão iterativa das validações do cadastro e da alteração de participantes, em que a equipe enviava o código e a IA apontava erros.
- Redação do esqueleto deste README e do resumo, com base nos requisitos do professor e no código do projeto.

**Forma de uso (Leonardo):** `PREENCHER: descrever os prompts e o uso na parte do chaveamento, resultado e ranking.`

**Revisões realizadas pela equipe:**

- O código sugerido foi digitado, testado e ajustado pela equipe; os testes foram feitos pelos arquivos `.http`, e os erros encontrados (por exemplo, um JSON de teste malformado e comparações incorretas nas validações) foram corrigidos pela equipe.
- O texto deste README foi lido e conferido pela equipe contra o comportamento real da API. `PREENCHER: confirmar e acrescentar o que a equipe efetivamente revisou/alterou.`