public class Partida
{
    public int Id { get; set; }
    public DateTime Data { get; set; }

    public int ModalidadeId { get; set; }
    public Modalidade? Modalidade { get; set; }

    public string Categoria { get; set; } = string.Empty;
    public string Fase { get; set; } = string.Empty;
    public int Participante1Id { get; set; }
    public Participante? Participante1 { get; set; }

    public int Participante2Id { get; set; }
    public Participante? Participante2 { get; set; }

    public int? VencedorId { get; set; }
    public Participante? Vencedor { get; set; }
}