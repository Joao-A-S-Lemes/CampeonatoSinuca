import React, { useEffect, useState } from "react";
import { chamarApi } from "../services/api";

interface Modalidade {
  id: number;
  nome: string;
}

interface Partida {
  id: number;
  categoria: string;
  fase: string;
  participante1Id: number;
  participante1: string;
  participante2Id: number;
  participante2: string;
  vencedor: string | null;
}

interface RespostaGerar {
  modalidade: string;
  passaDireto: string[];
  ignorados: string[];
}

interface RespostaAvancar {
  avisos: string[];
  novasPartidas: unknown[];
}

function Chaveamento() {
  const [modalidades, setModalidades] = useState<Modalidade[]>([]);
  const [modalidadeId, setModalidadeId] = useState<number | null>(null);
  const [partidas, setPartidas] = useState<Partida[]>([]);
  const [avisos, setAvisos] = useState<string[]>([]);

  useEffect(() => {
    chamarApi<Modalidade[]>("/api/modalidades/listar")
      .then(setModalidades)
      .catch((e) => setAvisos([e.message]));
  }, []);

  async function carregarPartidas(id: number) {
    setPartidas(await chamarApi<Partida[]>(`/api/partidas/listar/${id}`));
  }

  async function escolher(id: number) {
    setModalidadeId(id);
    setAvisos([]);
    try {
      await carregarPartidas(id);
    } catch (erro) {
      setAvisos([(erro as Error).message]);
    }
  }

  async function gerar() {
    if (modalidadeId === null) return;
    try {
      const r = await chamarApi<RespostaGerar>(
        `/api/partidas/gerar/${modalidadeId}`,
        "POST"
      );
      const msgs: string[] = [];
      r.passaDireto.forEach((n) => msgs.push(`${n} passa direto para a próxima fase`));
      r.ignorados.forEach((n) => msgs.push(`${n} ficou sem adversário`));
      setAvisos(msgs.length ? msgs : ["Chaveamento gerado!"]);
      await carregarPartidas(modalidadeId);
    } catch (erro) {
      setAvisos([(erro as Error).message]);
    }
  }

  async function definirVencedor(partidaId: number, vencedorId: number) {
    if (modalidadeId === null) return;
    try {
      await chamarApi(`/api/partidas/vencedor/${partidaId}/${vencedorId}`, "PUT");
      setAvisos([]);
      await carregarPartidas(modalidadeId);
    } catch (erro) {
      setAvisos([(erro as Error).message]);
    }
  }

  async function avancar() {
    if (modalidadeId === null) return;
    try {
      const r = await chamarApi<RespostaAvancar>(
        `/api/partidas/avancar/${modalidadeId}`,
        "POST"
      );
      const msgs = [...r.avisos];
      if (r.novasPartidas.length > 0) {
        msgs.push(`${r.novasPartidas.length} nova(s) partida(s) criada(s)`);
      }
      setAvisos(msgs);
      await carregarPartidas(modalidadeId);
    } catch (erro) {
      setAvisos([(erro as Error).message]);
    }
  }

  const categorias = Array.from(new Set(partidas.map((p) => p.categoria)));

  return (
    <section className="pagina">
      <h2>Chaveamento</h2>

      <div className="modalidades-botoes">
        {modalidades.map((m) => (
          <button
            key={m.id}
            className={m.id === modalidadeId ? "ativo" : ""}
            onClick={() => escolher(m.id)}
          >
            {m.nome}
          </button>
        ))}
      </div>

      {modalidadeId !== null && (
        <div className="acoes">
          <button onClick={gerar}>Gerar chaveamento</button>
          <button onClick={avancar}>Avançar fase</button>
        </div>
      )}

      {avisos.map((a, i) => (
        <p key={i} className="mensagem">
          {a}
        </p>
      ))}

      {modalidadeId !== null && partidas.length === 0 && (
        <p>Nenhuma partida ainda. Clique em "Gerar chaveamento".</p>
      )}

      {categorias.map((cat) => (
        <div key={cat} className="categoria">
          <h3>{cat}</h3>
          {partidas
            .filter((p) => p.categoria === cat)
            .map((p) => (
              <div key={p.id} className="partida">
                <span className="fase">{p.fase}</span>
                <button
                  disabled={p.vencedor !== null}
                  onClick={() => definirVencedor(p.id, p.participante1Id)}
                >
                  {p.participante1}
                </button>
                <span>x</span>
                <button
                  disabled={p.vencedor !== null}
                  onClick={() => definirVencedor(p.id, p.participante2Id)}
                >
                  {p.participante2}
                </button>
                {p.vencedor && <strong className="vencedor">Vencedor: {p.vencedor}</strong>}
              </div>
            ))}
        </div>
      ))}
    </section>
  );
}

export default Chaveamento;