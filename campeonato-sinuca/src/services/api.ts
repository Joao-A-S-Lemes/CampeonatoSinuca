const URL_BASE = "http://localhost:5059";

export async function chamarApi<T>(
  caminho: string,
  metodo: "GET" | "POST" | "PUT" | "DELETE" = "GET",
  corpo?: unknown
): Promise<T> {
  let resposta: Response;
    try {
      resposta = await fetch(URL_BASE + caminho, {
      method: metodo,
      headers: { "Content-Type": "application/json" },
      body: corpo ? JSON.stringify(corpo) : undefined,
      });
        } catch {
          throw new Error("Não foi possível conectar à API. Verifique se ela está em execução.");
                } 
  const texto = await resposta.text();
  const dados = texto ? JSON.parse(texto) : null;

  if (!resposta.ok) {
    throw new Error(typeof dados === "string" ? dados : "Erro na requisição");
  }
  return dados as T;
}