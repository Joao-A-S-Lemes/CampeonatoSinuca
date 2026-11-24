import React, { useState } from "react";
import Participantes from "./pages/Participantes";
import Chaveamento from "./pages/Chaveamento";
import Resultados from "./pages/Resultados";

type Aba = "participantes" | "chaveamento" | "resultados";

function App() {
  const [aba, setAba] = useState<Aba>("participantes");

  return (
    <div>
      <h1>🎱 Campeonato de Sinuca</h1>
      <nav>
        <button className={aba === "participantes" ? "ativo" : ""} onClick={() => setAba("participantes")}>Participantes</button>
        <button className={aba === "chaveamento" ? "ativo" : ""} onClick={() => setAba("chaveamento")}>Chaveamento</button>
        <button className={aba === "resultados" ? "ativo" : ""} onClick={() => setAba("resultados")}>Resultados</button>
      </nav>
      {aba === "participantes" && <Participantes />}
      {aba === "chaveamento" && <Chaveamento />}
      {aba === "resultados" && <Resultados />}
    </div>
  );
}

export default App;