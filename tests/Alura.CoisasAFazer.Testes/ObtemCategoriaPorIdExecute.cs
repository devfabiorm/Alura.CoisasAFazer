using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Moq;
using Xunit;

namespace Alura.CoisasAFazer.Testes
{
    public class ObtemCategoriaPorIdExecute
    {
        [Fact]
        public void QuandoIdForExistenteDeveChamarObtemCategoriaPorIdUmaUnicaVez()
        {
            //Arrage
            var idCategoria = 20;
            var comando = new ObtemCategoriaPorId(idCategoria);
            var mock = new Mock<IRepositorioTarefas>();

            var repo = mock.Object;
            var handler = new ObtemCategoriaPorIdHandler(repo);
            //Act
            handler.Execute(comando);


            //Assert
            mock.Verify(r => r.ObtemCategoriaPorId(idCategoria), Times.Once());
        }
    }
}
