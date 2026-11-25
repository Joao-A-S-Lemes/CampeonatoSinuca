import React, { useEffect, useState } from "react";
import { chamarApi } from "../services/api";

interface Modalidade {
  id: number;
  nome: string;
}

interface Participante {
  id: number;
  nome: string;
  cpf: string;
  idade: number;
  genero: string;
  modalidades: string[];
}

const formVazio = {
  nome: "",
  cpf: "",
  idade: "",
  genero: "Masculino",
  modalidadesIds: [] as number[],
};

// Mesmas regras da API: nome preenchido, CPF com 11 dígitos,
// idade entre 12 e 120 e pelo menos uma modalidade.
function validar(form: typeof formVazio): Record<string, string> {
  const erros: Record<string, string> = {};
  const cpf = form.cpf.replace(/\D/g, "");
  const idade = Number(form.idade);

  if (!form.nome.trim()) erros.nome = "Informe o nome.";
  if (cpf.length !== 11) erros.cpf = "O CPF deve ter 11 dígitos.";
  if (!Number.isInteger(idade) || idade < 12 || idade > 120)
    erros.idade = "A idade deve estar entre 12 e 120 anos.";
  if (form.modalidadesIds.length === 0)
    erros.modalidades = "Escolha pelo menos uma modalidade.";

  return erros;
}

function normalizar(texto: string): string {
  return texto.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
}

function Participantes() {
  const [lista, setLista] = useState<Participante[]>([]);
  const [modalidades, setModalidades] = useState<Modalidade[]>([]);
  const [form, setForm] = useState(formVazio);
  const [editandoCpf, setEditandoCpf] = useState<string | null>(null);
  const [mensagem, setMensagem] = useState("");
  const [erros, setErros] = useState<Record<string, string>>({});
  const [busca, setBusca] = useState("");

  async function carregar() {
    const dados = await chamarApi<Participante[] | string>(
      "/api/participantes/listar"
    );
    setLista(Array.isArray(dados) ? dados : []);
  }

  useEffect(() => {
    chamarApi<Modalidade[]>("/api/modalidades/listar")
      .then(setModalidades)
      .catch((e) => setMensagem(e.message));
    carregar().catch((e) => setMensagem(e.message));
  }, []);

  function alternarModalidade(id: number) {
    setForm((f) => ({
      ...f,
      modalidadesIds: f.modalidadesIds.includes(id)
        ? f.modalidadesIds.filter((x) => x !== id)
        : [...f.modalidadesIds, id],
    }));
  }

  function cancelar() {
    setForm(formVazio);
    setEditandoCpf(null);
    setErros({});
  }

  async function salvar(e: React.FormEvent) {
    e.preventDefault();
    setMensagem("");

    const encontrados = validar(form);
    setErros(encontrados);
    if (Object.keys(encontrados).length > 0) return;

    const corpo = {
      nome: form.nome,
      cpf: form.cpf,
      idade: Number(form.idade),
      genero: form.genero,
      modalidadesIds: form.modalidadesIds,
    };
    try {
      if (editandoCpf) {
        await chamarApi(`/api/participantes/alterar/${editandoCpf}`, "PUT", corpo);
      } else {
        await chamarApi("/api/participantes/cadastrar", "POST", corpo);
      }
      cancelar();
      await carregar();
      setMensagem("Salvo com sucesso!");
    } catch (erro) {
      setMensagem((erro as Error).message);
    }
  }

  function editar(p: Participante) {
    setEditandoCpf(p.cpf);
    setErros({});
    setForm({
      nome: p.nome,
      cpf: p.cpf,
      idade: String(p.idade),
      genero: p.genero,
      modalidadesIds: modalidades
        .filter((m) => p.modalidades.includes(m.nome))
        .map((m) => m.id),
    });
  }

  async function remover(cpf: string) {
    if (!window.confirm("Remover este participante?")) return;
    try {
      await chamarApi(`/api/participantes/remover/${cpf}`, "DELETE");
      await carregar();
      setMensagem("Participante removido.");
    } catch (erro) {
    setMensagem((erro as Error).message);
    window.alert((erro as Error).message);
    }
  }

  const textoBusca = busca.trim();
  const cpfBuscado = textoBusca.replace(/\D/g, "");
  const buscaEhCpf = cpfBuscado !== "" && /^[\d.\-\s]+$/.test(textoBusca);

  const listaFiltrada = lista.filter((p) =>
    buscaEhCpf
      ? p.cpf.replace(/\D/g, "").includes(cpfBuscado)
      : normalizar(p.nome).includes(normalizar(textoBusca))
  );

  return (
    <section className="pagina">
      <h2>Participantes</h2>

      <form className="formulario" onSubmit={salvar}>
        <input
          placeholder="Nome"
          value={form.nome}
          onChange={(e) => setForm({ ...form, nome: e.target.value })}
        />
        {erros.nome && <small className="erro-campo">{erros.nome}</small>}

        <input
          placeholder="CPF"
          value={form.cpf}
          disabled={editandoCpf !== null}
          onChange={(e) => setForm({ ...form, cpf: e.target.value })}
        />
        {erros.cpf && <small className="erro-campo">{erros.cpf}</small>}

        <input
          placeholder="Idade"
          type="number"
          value={form.idade}
          onChange={(e) => setForm({ ...form, idade: e.target.value })}
        />
        {erros.idade && <small className="erro-campo">{erros.idade}</small>}

        <select
          value={form.genero}
          onChange={(e) => setForm({ ...form, genero: e.target.value })}
        >
          <option>Masculino</option>
          <option>Feminino</option>
        </select>

        <div className="modalidades">
          {modalidades.map((m) => (
            <label key={m.id}>
              <input
                type="checkbox"
                checked={form.modalidadesIds.includes(m.id)}
                onChange={() => alternarModalidade(m.id)}
              />
              {m.nome}
            </label>
          ))}
        </div>
        {erros.modalidades && (
          <small className="erro-campo">{erros.modalidades}</small>
        )}

        <button type="submit">{editandoCpf ? "Salvar alterações" : "Cadastrar"}</button>
        {editandoCpf && (
          <button type="button" onClick={cancelar}>
            Cancelar
          </button>
        )}
      </form>

      {mensagem && <p className="mensagem">{mensagem}</p>}

      <input
        className="busca"
        placeholder="Buscar por nome ou CPF"
        value={busca}
        onChange={(e) => setBusca(e.target.value)}
      />

      <table className="tabela">
        <thead>
          <tr>
            <th>Nome</th>
            <th>CPF</th>
            <th>Idade</th>
            <th>Gênero</th>
            <th>Modalidades</th>
            <th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {listaFiltrada.map((p) => (
            <tr key={p.id}>
              <td>{p.nome}</td>
              <td>{p.cpf}</td>
              <td>{p.idade}</td>
              <td>{p.genero}</td>
              <td>{p.modalidades.join(", ")}</td>
              <td>
                <button onClick={() => editar(p)}>Editar</button>
                <button onClick={() => remover(p.cpf)}>Remover</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {lista.length > 0 && listaFiltrada.length === 0 && (
        <p>Nenhum participante encontrado.</p>
      )}
    </section>
  );
}

export default Participantes;
