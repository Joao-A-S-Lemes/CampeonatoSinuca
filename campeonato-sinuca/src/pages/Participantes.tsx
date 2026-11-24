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

function Participantes() {
  const [lista, setLista] = useState<Participante[]>([]);
  const [modalidades, setModalidades] = useState<Modalidade[]>([]);
  const [form, setForm] = useState(formVazio);
  const [editandoCpf, setEditandoCpf] = useState<string | null>(null);
  const [mensagem, setMensagem] = useState("");

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
  }

  async function salvar(e: React.FormEvent) {
    e.preventDefault();
    setMensagem("");
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
    }
  }

  return (
    <section className="pagina">
      <h2>Participantes</h2>

      <form className="formulario" onSubmit={salvar}>
        <input
          placeholder="Nome"
          value={form.nome}
          onChange={(e) => setForm({ ...form, nome: e.target.value })}
        />
        <input
          placeholder="CPF"
          value={form.cpf}
          disabled={editandoCpf !== null}
          onChange={(e) => setForm({ ...form, cpf: e.target.value })}
        />
        <input
          placeholder="Idade"
          type="number"
          value={form.idade}
          onChange={(e) => setForm({ ...form, idade: e.target.value })}
        />
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

        <button type="submit">{editandoCpf ? "Salvar alterações" : "Cadastrar"}</button>
        {editandoCpf && (
          <button type="button" onClick={cancelar}>
            Cancelar
          </button>
        )}
      </form>

      {mensagem && <p className="mensagem">{mensagem}</p>}

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
          {lista.map((p) => (
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
    </section>
  );
}

export default Participantes;