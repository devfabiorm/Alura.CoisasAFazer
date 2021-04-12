using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Alura.CoisasAFazer.Testes
{
    public class GerenciaPrazoDasTarefasHandlerExecute
    {
        [Fact]
        public void QuandoTarefasEstiveremAtrasadaDeveMudarSeuStatus()
        {
            //Arrange
            var compCateg = new Categoria(1, "Compras");
            var casaCateg = new Categoria(2, "Casa");
            var trabCateg = new Categoria(3, "Trabalho");
            var saudCateg = new Categoria(4, "Saúde");
            var higiCateg = new Categoria(5, "Higiene");

            var tarefas = new List<Tarefa>
            {
                new Tarefa(1, "Tirar Lixo", casaCateg, new DateTime(2019, 12, 31), null, StatusTarefa.Criada),
                new Tarefa(4, "Fazer Almoço", casaCateg, new DateTime(2018, 12, 1), null, StatusTarefa.Criada),
                new Tarefa(9, "Ir à academia", saudCateg, new DateTime(2020, 11, 4), null, StatusTarefa.Criada),
                new Tarefa(7, "Concluir o Relatório", trabCateg, new DateTime(2020, 4, 3), null, StatusTarefa.Pendente),
                new Tarefa(10, "Beber água", saudCateg, new DateTime(2018, 5, 7), null, StatusTarefa.Criada),

                new Tarefa(8, "Comparecer à reunião", trabCateg, new DateTime(2021, 4, 1), null, StatusTarefa.Concluida),
                new Tarefa(2, "Arrumar a cama", casaCateg, new DateTime(2021, 6, 5), null, StatusTarefa.Concluida),
                new Tarefa(3, "Escovar os dentes", casaCateg, new DateTime(2021, 4, 10), null, StatusTarefa.Criada),
                new Tarefa(5, "Comprar presente para o João", compCateg, new DateTime(2021, 12, 31), null, StatusTarefa.Concluida),
                new Tarefa(6, "Comprar ração", compCateg, new DateTime(2021, 5, 3), null, StatusTarefa.Concluida),
            };

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
               .UseInMemoryDatabase("DbTarefasContext", new InMemoryDatabaseRoot())
               .Options;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            repo.IncluirTarefas(tarefas.ToArray());

            var comando = new GerenciaPrazoDasTarefas(new DateTime(2021, 1, 1));
            var handler = new GerenciaPrazoDasTarefasHandler(repo);

            //Act
            handler.Execute(comando);

            //Assert
            var tarefasEmAtraso = repo.ObtemTarefas(t => t.Status == StatusTarefa.EmAtraso);
            Assert.Equal(5, tarefasEmAtraso.Count());
        }

        [Fact]
        public void QuandoInvocadoDeveChamarAtualizarTarefasNaQtdeVezesDoTotalDeTarefasAtrasadas()
        {
            var categ = new Categoria("Dummy");

            var tarefas = new List<Tarefa>
            {
                new Tarefa(1, "Tirar Lixo", categ, new DateTime(2019, 12, 31), null, StatusTarefa.Criada),
                new Tarefa(4, "Fazer Almoço", categ, new DateTime(2018, 12, 1), null, StatusTarefa.Criada),
                new Tarefa(9, "Ir à academia", categ, new DateTime(2020, 11, 4), null, StatusTarefa.Criada),
            };

            var mock = new Mock<IRepositorioTarefas>();

            mock.Setup(r => r.ObtemTarefas(It.IsAny<Func<Tarefa, bool>>())).Returns(tarefas);

            var repo = mock.Object;

            var comando = new GerenciaPrazoDasTarefas(new DateTime(2021, 1, 1));
            var handler = new GerenciaPrazoDasTarefasHandler(repo);

            //Act
            handler.Execute(comando);

            //Assert
            mock.Verify(r => r.AtualizarTarefas(It.IsAny<Tarefa[]>()), Times.Once());
        }
    }
}
