# 🎱 Campeonato de Sinuca — Front-end (React)

Interface web do **Campeonato de Sinuca**, desenvolvida como Parte 2 do Projeto Final de Desenvolvimento de Software Visual. Ela consome a API em C# do mesmo repositório e permite cadastrar participantes, gerar o chaveamento, registrar os resultados e consultar a classificação.

Para a descrição completa do projeto, do modelo de dados e dos endpoints, veja o [README principal](../README.md).

---

## Tecnologias

- React com TypeScript (projeto criado com Create React App)
- `fetch` nativo para chamar a API, sem bibliotecas de requisição ou de interface
- CSS puro (`src/index.css`), gerado com ferramenta de IA

---

## Como executar

**Pré-requisitos:** Node.js (versão LTS) com npm, e a API em execução.

1. Suba a API (veja o [README principal](../README.md#5-como-executar)). Ela deve estar em `http://localhost:5059`.
2. Neste diretório, instale as dependências e inicie o front:

```bash
npm install     # apenas na primeira vez
npm start
```

3. Abra `http://localhost:3000`.

### Observações

- **CORS:** a API só aceita requisições vindas de `http://localhost:3000`. Se o front rodar em outra porta, ajuste a política `PermitirReact` no `Program.cs` da API.
- **URL da API:** está definida na constante `URL_BASE` em `src/services/api.ts`. Se a API subir em outra porta, altere ali.
- **Dados de exemplo:** para não cadastrar um por um, rode o arquivo `CampeonatoSinuca/CampeonatoSinucaAPI/Tests/CadastroParticipantes.http` com a API ligada (cadastra 32 participantes).

### Scripts disponíveis

| Comando | O que faz |
|---|---|
| `npm start` | Inicia o front em modo de desenvolvimento (porta 3000) |
| `npm run build` | Gera a versão de produção na pasta `build/` |
| `npm test` | Executa os testes, se houver |

---

## Telas

A navegação é feita por três abas no topo da página (`App.tsx`).

### Participantes

- Formulário com nome, CPF, idade, gênero e as modalidades (caixas de seleção).
- Cadastro de novos participantes e edição dos existentes. Na edição o CPF fica bloqueado, pois ele identifica o participante.
- Remoção com confirmação. A API recusa remover quem já tem partidas, e o motivo aparece na tela.
- Tabela com todos os participantes e as modalidades de cada um.

### Chaveamento

1. Escolha a modalidade (8 Ball, 9 Ball ou Snooker 15 reds).
2. Clique em **Gerar chaveamento** para sortear a primeira fase. A tela avisa quem passa direto para a próxima fase e quem ficou sem adversário.
3. As partidas aparecem agrupadas por categoria (gênero e faixa etária). Clique no nome do jogador que venceu para registrar o resultado.
4. Quando todas as partidas da fase tiverem vencedor, clique em **Avançar fase**. O ciclo se repete até a Final e a disputa de terceiro lugar.

### Resultados

- **Classificação por categoria:** campeão, vice e terceiro colocado, com o status da categoria (*Em andamento* ou *Final decidida*).
- **Ranking de vitórias:** quantas partidas cada participante venceu na modalidade escolhida.

---

## Estrutura do código

```
src/
├── pages/
│   ├── Participantes.tsx    Cadastro, edição, remoção e listagem
│   ├── Chaveamento.tsx      Gerar chaveamento, definir vencedor, avançar fase
│   └── Resultados.tsx       Classificação e ranking
├── services/
│   └── api.ts               chamarApi(): URL base, JSON e tratamento de erros
├── App.tsx                  Navegação entre as abas
├── index.tsx                Ponto de entrada da aplicação
└── index.css                Estilos
```

A função `chamarApi<T>(caminho, metodo, corpo)` é usada por todas as telas. Ela envia e recebe JSON e, quando a API responde com erro (por exemplo, CPF já cadastrado), lança uma exceção com a mensagem devolvida, que a tela exibe ao usuário.