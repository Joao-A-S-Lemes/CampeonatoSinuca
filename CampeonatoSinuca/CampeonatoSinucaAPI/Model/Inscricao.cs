public class Inscricao
{
    public int Id { get; set; }

    public int ParticipanteId { get; set; }
    public Participante? Participante { get; set; }

    public int ModalidadeId { get; set; }
    public Modalidade? Modalidade { get; set; }
}