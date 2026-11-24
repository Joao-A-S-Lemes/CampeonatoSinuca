import React, { useEffect, useState } from "react";
import { chamarApi } from "./services/api";

function App() {
  const [dados, setDados] = useState<unknown>(null);
  const [erro, setErro] = useState("");

  useEffect(() => {
    chamarApi("/api/modalidades/listar")
      .then(setDados)
      .catch((e) => setErro(e.message));
  }, []);

  return (
    <div>
      <h1>Campeonato de Sinuca</h1>
      {erro && <p>Erro: {erro}</p>}
      <pre>{JSON.stringify(dados, null, 2)}</pre>
    </div>
  );
}

export default App;