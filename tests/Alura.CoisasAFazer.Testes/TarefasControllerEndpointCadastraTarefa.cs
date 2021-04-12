using Xunit;
using Alura.CoisasAFazer.WebApp.Controllers;
using Alura.CoisasAFazer.WebApp.Models;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Alura.CoisasAFazer.Services.Handlers;
using Alura.CoisasAFazer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Alura.CoisasAFazer.Core.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Http;

namespace Alura.CoisasAFazer.Testes
{
    public class TarefasControllerEndpointCadastraTarefa
    {
        [Fact]
        public void DadaTarefaComInformacoesValidasDeveRetornar200()
        {
            //Arrange
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext", new InMemoryDatabaseRoot())
                .Options;

            var contexto = new DbTarefasContext(options);

            contexto.Categorias.Add(new Categoria(20, "Estudo"));
            contexto.SaveChanges();

            var repo = new RepositorioTarefa(contexto);

            var controlador = new TarefasController(repo, mockLogger.Object);

            var model = new CadastraTarefaVM();
            model.Titulo = "Estudar xUnit";
            model.IdCategoria = 20;
            model.Prazo = new DateTime(2021, 12, 31);

            //Act
            var retorno = controlador.EndpointCadastraTarefa(model);

            //Assert
            Assert.IsType<OkResult>(retorno);
        }

        [Fact]
        public void QuandoExcecaoForLancadaDeveRetornarStatusCode500()
        {
            //Arrange
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var mock = new Mock<IRepositorioTarefas>();

            mock.Setup(r => r.ObtemCategoriaPorId(20)).Returns(new Categoria(20, "Estudo"));
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(new Exception("Houve um erro"));

            var repo = mock.Object;

            var controlador = new TarefasController(repo, mockLogger.Object);

            var model = new CadastraTarefaVM();
            model.Titulo = "Estudar xUnit";
            model.IdCategoria = 20;
            model.Prazo = new DateTime(2021, 12, 31);

            //Act
            var retorno = controlador.EndpointCadastraTarefa(model);

            //Assert
            Assert.IsType<StatusCodeResult>(retorno);
            var statusCodeRetornado = (retorno as StatusCodeResult).StatusCode;
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeRetornado);
        }
    }
}
