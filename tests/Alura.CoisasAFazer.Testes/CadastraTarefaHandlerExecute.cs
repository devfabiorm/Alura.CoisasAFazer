using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Alura.CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void DadaTarefaComInfoValidasDeveIncluirNoDb()
        {
            //Arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2021, 12, 31));
            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;
            var mock = new Mock<ILogger<CadastraTarefaHandler>>();
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);
            var handler = new CadastraTarefaHandler(repo, mock.Object);

            //Act
            handler.Execute(comando);

            //Assert
            var tarefa = repo.ObtemTarefas(t => t.Titulo == "Estudar xUnit").FirstOrDefault();
            Assert.NotNull(tarefa);
        }

        delegate void CapturaMensagemLog(LogLevel level, EventId eventId, object state, Exception exception, Func<object, Exception, string> function);

        [Fact]
        public void DadaTarefaComInfoValidasDeveLogar()
        {
            //Arrange
            var mensagemEsperada = "Aprofundando o conhecimento em testes usando Moq";
            var comando = new CadastraTarefa(mensagemEsperada, new Categoria("Estudo"), new DateTime(2021, 12, 31));
            
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            LogLevel levelCapturado = LogLevel.Error;
            string mensagemCapturada = string.Empty;

            CapturaMensagemLog captura = (level, eventId, state, exception, func) =>
            {
                levelCapturado = level;
                mensagemCapturada = func(state, exception);
            };

            var mock = new Mock<IRepositorioTarefas>();

            mockLogger.Setup(l =>
                l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()
                )).Callback(captura);
          
            var handler = new CadastraTarefaHandler(mock.Object, mockLogger.Object);

            //Act
            handler.Execute(comando);

            //Assert
            Assert.Equal(LogLevel.Debug, levelCapturado);
            Assert.Contains(mensagemEsperada, mensagemCapturada);
        }

        [Fact]
        public void QuandoExceptionForLancadaResultadoIsSuccessDeveSerFalse()
        {
            //Arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2021, 12, 31));
            var mock = new Mock<IRepositorioTarefas>();
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(new Exception("Houve um erro na inclusão de tarefas"));

            var repo = mock.Object;
            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            //Act
            CommandResult resultado = handler.Execute(comando);

            //Assert
            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void QuandoExceptionForLancadaDeveLogarAMensagemDaExcecao()
        {
            //Arrange
            var mensagemEsperada = "Houve um erro na inclusão de tarefas";
            var excecaoEsperada = new Exception(mensagemEsperada);
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2021, 12, 31));
            var mock = new Mock<IRepositorioTarefas>();
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(excecaoEsperada);

            var repo = mock.Object;
            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            //Act
            CommandResult resultado = handler.Execute(comando);

            //Assert
            mockLogger.Verify(l =>
                l.Log(
                    LogLevel.Error, //nível de log => LogError
                    It.IsAny<EventId>(), //Identificador do evento
                    It.IsAny<object>(), //objeto que será logado
                    excecaoEsperada, //excecao que será logada
                    It.IsAny<Func<object, Exception, string>>() //função que converte objeto+excecao >> string
                   ), 
                Times.Once());
        }
    }
}

