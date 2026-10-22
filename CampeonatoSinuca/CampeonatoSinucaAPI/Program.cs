using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

List<Participante> participantes = new List<Participante>();

app.MapGet("/", () => "Campeonato de Sinuca");

app.Run();