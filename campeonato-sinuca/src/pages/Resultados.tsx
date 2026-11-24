import React, { useEffect, useState } from "react";
import { chamarApi } from "../services/api";

interface Modalidade {
  id: number;
  nome: string;
}

interface CategoriaClassificacao {
  categoria: string;
  status: string;
  campeao: string | null;
  vice: string | null;
  terceiro: string | null;
}

interface RespostaClassificacao {
  modalidade: string;
  categorias: CategoriaClassificacao[];
}

interface ItemRanking {
  participante: string;
  vitorias: number;
}

interface RespostaRanking {
  modalidade: string;
  ranking: ItemRanking[];
}

function Resultados() {
  const [modalidades, setModalidades] = useState<Modalidade[]>([]);
  const [modalidadeId, setModalidadeId] = useState<number | null>(null);
  const [categorias, setCategorias] = useState<CategoriaClassificacao[]>([]);
  const [ranking, setRanking] = useState<ItemRanking[]>([]);
  const [erro, setErro] = useState("");

  useEffect(() => {
    chamarApi<Modalidade[]>("/api/modalidades/listar")
      .then(setModalidades)
      .catch((e) => setErro(e.message));
  }, []);

  async function escolher(id: number) {
    setModalidadeId(id);
    setErro("");
    try {
      const classificacao = await chamarApi<RespostaClassificacao>(
        `/api/partidas/classificacao/${id}`
      );
      const vitorias = await chamarApi<RespostaRanking>(
        `/api/partidas/ranking/${id}`
      );
      setCategorias(classificacao.categorias);
      setRanking(vitorias.ranking);
    } catch (e) {
      setErro((e as Error).message);
    }
  }

  return (
    <section className="pagina">
      <h2>Resultados</h2>

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

      {erro && <p className="mensagem">{erro}</p>}

      {modalidadeId !== null && (
        <>
          <h3>Classificação por categoria</h3>
          {categorias.length === 0 && <p>Nenhuma partida nessa modalidade ainda.</p>}
          {categorias.map((c) => (
            <div key={c.categoria} className="categoria">
              <h4>
                {c.categoria} <span className="status">({c.status})</span>
              </h4>
              <p>🥇 Campeão: {c.campeao ?? "-"}</p>
              <p>🥈 Vice: {c.vice ?? "-"}</p>
              <p>🥉 Terceiro: {c.terceiro ?? "-"}</p>
            </div>
          ))}

          <h3>Ranking de vitórias</h3>
          {ranking.length === 0 && <p>Ninguém venceu partidas ainda.</p>}
          {ranking.length > 0 && (
            <table className="tabela">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Participante</th>
                  <th>Vitórias</th>
                </tr>
              </thead>
              <tbody>
                {ranking.map((r, i) => (
                  <tr key={r.participante}>
                    <td>{i + 1}</td>
                    <td>{r.participante}</td>
                    <td>{r.vitorias}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </>
      )}
    </section>
  );
}

export default Resultados;