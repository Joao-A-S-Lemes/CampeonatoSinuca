# 🎱 Campeonato de Sinuca — API de Inscrições e Chaveamento

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

O **Campeonato de Sinuca** é uma API REST desenvolvida em C# (.NET 8, Minimal API) com Entity Framework e banco SQLite, criada para administrar a inscrição de jogadores e a disputa de um torneio de sinuca. O organizador cadastra cada participante informando nome, CPF, idade, gênero (Masculino ou Feminino) e as modalidades em que deseja competir: **8 Ball**, **9 Ball** e **Snooker 15 reds**. Os dados são validados e gravados no banco, e cada escolha de modalidade vira uma inscrição que liga o participante a uma modalidade previamente cadastrada. A partir das inscrições, o sistema gera o chaveamento de cada modalidade, separando os jogadores em categorias por gênero e faixa etária (Júnior ou Adulto). Em seguida, permite registrar o vencedor de cada partida e avançar as fases do mata-mata (quartas de final, semifinal e final), incluindo a disputa de terceiro lugar, até a classificação final de cada categoria, com campeão, vice e terceiro colocado. Assim, os dados cadastrados são usados em funcionalidades reais de organização de um campeonato, e não apenas armazenados.

---

## 3. Funcionalidades

- Cadastro de participantes com validação de nome, CPF, idade e gênero
- Listagem de todos os participantes, com as modalidades de cada um
- Busca de participante por CPF
- Alteração de dados e de modalidades de um participante
- Remoção de participante (bloqueada se ele já tiver partidas)
- Inscrição em uma ou mais modalidades (8 Ball, 9 Ball, Snooker 15 reds)
- Listagem das modalidades disponíveis
- Geração do chaveamento de uma modalidade, com sorteio dos confrontos separado por categoria (gênero e faixa etária)
- Listagem das partidas de uma modalidade
- Registro do vencedor de uma partida
- Avanço automático de fase (quartas, semifinal e final), com disputa de terceiro lugar
- Classificação final por categoria (campeão, vice e terceiro)
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

Gera a **primeira fase** do torneio de uma modalidade. O funcionamento é o seguinte:

1. **Validações:** a modalidade precisa existir, e o chaveamento só pode ser gerado **uma vez** por modalidade. Uma segunda chamada é recusada, o que evita partidas duplicadas.
2. **Quem entra:** todos os participantes inscritos na modalidade, em ordem aleatória.
3. **Categorias:** os inscritos são separados em categorias formadas por **gênero + faixa etária**. Menores de 18 anos são *Júnior* e os demais são *Adulto*, o que resulta em até quatro categorias: *Masculino Adulto*, *Feminino Adulto*, *Masculino Junior* e *Feminino Junior*. O sorteio dos pares acontece **somente dentro de cada categoria**, portanto uma partida nunca coloca, por exemplo, um adulto contra um júnior ou um homem contra uma mulher.
4. **Pares e fase:** em cada categoria, os jogadores sorteados formam pares de dois em dois. O nome da fase depende de quantos jogadores a categoria tem: até 2 jogadores, *Final*; até 4, *Semifinal*; até 8, *Quartas*; até 16, *Oitavas*; acima disso, *Fase preliminar*.
5. **Número ímpar:** quando a categoria tem um número ímpar de jogadores, o último sorteado **passa direto** para a fase seguinte, sem jogar. Os nomes aparecem na lista `passaDireto` da resposta.
6. **Categoria com um único jogador:** como não há adversário, o jogador não entra em nenhuma partida e aparece na lista `ignorados`. Se nenhuma categoria conseguir formar partidas, a resposta é um erro explicando o motivo.

A resposta traz a modalidade, as listas `passaDireto` e `ignorados` e as partidas criadas, cada uma com Id, categoria, fase e nomes dos dois jogadores.

### 4.7 Listagem de partidas — `GET /api/partidas/listar/{modalidadeId}`

Devolve todas as partidas de uma modalidade, ordenadas por categoria e pela ordem de criação. Cada partida mostra a fase, os Ids e nomes dos dois jogadores e o nome do vencedor (vazio enquanto a partida não foi decidida). Os Ids dos jogadores são os usados para registrar o vencedor.

### 4.8 Registro do vencedor — `PUT /api/partidas/vencedor/{partidaId}/{vencedorId}`

Define quem venceu uma partida. O sistema confere que a partida existe e que o vencedor informado é **um dos dois jogadores daquela partida**; caso contrário, a requisição é recusada. Depois do registro, o resultado passa a ser usado pelo avanço de fase, pela classificação final e pelo ranking de vitórias.

### 4.9 Avanço de fase — `POST /api/partidas/avancar/{modalidadeId}`

Conduz o mata-mata. A operação é feita **categoria por categoria**, pois cada categoria tem o seu próprio torneio:

1. Se alguma partida da fase atual ainda **não tem vencedor**, a categoria é pulada e a resposta avisa que está aguardando resultados.
2. Se a fase atual é a **Final**, a resposta avisa que o campeonato daquela categoria está encerrado.
3. Caso contrário, o sistema descobre quem **ainda não perdeu** nenhuma partida da disputa, sorteia novos pares apenas entre esses jogadores e cria a **próxima fase** (por exemplo, de Quartas para Semifinal). Se o número de classificados for ímpar, um deles passa direto, e a resposta informa o nome.
4. Ao criar a **Final** a partir de uma semifinal com duas partidas, o sistema cria também a partida de **Terceiro lugar**, entre os dois jogadores que perderam a semifinal.

A operação pode ser chamada várias vezes sem efeitos colaterais: enquanto a fase não estiver completa, apenas informa o que falta. A resposta traz a lista de avisos e as novas partidas criadas.

### 4.10 Classificação final — `GET /api/partidas/classificacao/{modalidadeId}`

Mostra, para cada categoria da modalidade, a situação do torneio:

- **Campeão:** vencedor da Final.
- **Vice:** jogador que perdeu a Final.
- **Terceiro:** vencedor da partida de Terceiro lugar.
- **Status:** *Final decidida* ou *Em andamento*.

Enquanto a final não foi decidida, os campos de colocação ficam vazios. O terceiro lugar só existe quando a fase anterior à final teve duas partidas (semifinal completa); em categorias pequenas, que começam direto na Final, não há disputa de terceiro lugar.

### 4.11 Ranking de vitórias — `GET /api/partidas/ranking/{modalidadeId}`

Conta quantas partidas cada participante venceu na modalidade, somando todas as categorias e fases, e devolve a lista ordenada do maior para o menor número de vitórias. Aparecem apenas os participantes com pelo menos uma vitória. A resposta traz o nome da modalidade e, para cada participante, o nome e a quantidade de vitórias.

### 4.12 Fluxo de um campeonato

1. Cadastrar os participantes e suas modalidades.
2. Gerar o chaveamento de cada modalidade (`gerar`).
3. Consultar as partidas (`listar`) e registrar o vencedor de cada uma (`vencedor`).
4. Avançar a fase (`avancar`) quando todas as partidas da fase atual tiverem vencedor.
5. Repetir os passos 3 e 4 até a Final e o Terceiro lugar serem decididos.
6. Consultar a classificação final (`classificacao`) e o ranking de vitórias (`ranking`).

---

## 5. Modelo de dados

O banco tem **quatro tabelas**:

| Tabela | Descrição | Relacionamentos |
|---|---|---|
| `Participantes` | Jogadores (nome, CPF único, idade, gênero) | — |
| `Modalidades` | 8 Ball, 9 Ball, Snooker 15 reds | — |
| `Inscricoes` | Participante inscrito em uma modalidade | `ParticipanteId` → Participantes, `ModalidadeId` → Modalidades (par único) |
| `Partidas` | Confronto entre dois participantes numa modalidade, com categoria e fase | `ModalidadeId`, `Participante1Id`, `Participante2Id`, `VencedorId` (opcional) |

Os relacionamentos da `Partida` com `Participante` usam exclusão restrita: não é possível apagar um participante que tenha partidas, o que também protege o histórico do campeonato. A categoria e a fase são gravadas na própria partida, em texto (por exemplo, *Masculino Adulto* e *Semifinal*).

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

**Ferramenta utilizada:** Claude (Anthropic), usada pelos dois integrantes.

**Forma de uso (João):**

- Revisão do andamento do projeto em relação aos requisitos do professor e sugestão de modelagem (entidades `Participante`, `Modalidade`, `Inscricao` e `Partida`).
- Explicações sobre o funcionamento de migrations do Entity Framework, do banco SQLite, de chaves estrangeiras e do `Include`.
- Revisão iterativa das validações do cadastro e da alteração de participantes, em que a equipe enviava o código e a IA apontava erros.
- Redação do esqueleto deste README e do resumo, com base nos requisitos do professor e no código do projeto.

**Forma de uso (Leonardo):**

- Orientação para a parte do campeonato (`Partida`, chaveamento, vencedor, avanço de fase, classificação e ranking).
- Explicações de conceitos, como relacionamentos no Entity Framework, migrations, e ajuda para resolver erros do caminho.
- Definição das regras do mata-mata em conversa com a IA: categorias por gênero e faixa etária, fases por número de jogadores, jogador que passa direto em número ímpar e disputa de terceiro lugar.
- Geração de listas de participantes de teste (JSON), com 16 inscritos por modalidade e distribuição planejada por categoria.
- Revisão do funcionamento do chaveamento e do avanço de fases, com sugestões de melhorias e correções de erros.

**Revisões realizadas pela equipe:**

- O código sugerido foi digitado, testado e ajustado pela equipe; os testes foram feitos pelos arquivos `.http`, e os erros encontrados (por exemplo, um JSON de teste malformado e comparações incorretas nas validações) foram corrigidos pela equipe.
- O mata-mata foi testado de ponta a ponta: geração das quartas, registro dos vencedores, avanço até a semifinal, final e terceiro lugar, e conferência da classificação final de cada categoria.
- O texto deste README foi lido e conferido pela equipe contra o comportamento real da API.