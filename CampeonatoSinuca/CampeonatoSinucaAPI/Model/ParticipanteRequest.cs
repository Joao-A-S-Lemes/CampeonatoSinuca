public class ParticipanteRequest{
    
    public string Nome {get;set;} = String.Empty;
    public string Cpf {get;set;} = String.Empty;
    public int Idade {get;set;}
    public string Genero {get;set;} = String.Empty;
     public List<int> ModalidadesIds { get; set; } = new List<int>();
}