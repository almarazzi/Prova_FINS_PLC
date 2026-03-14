using Prova_FINS_TCP;
using System.Net.Sockets;


CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken token = cts.Token;

var Ritorno = new byte[60];
var g = Class_Prova_FINS.PrimaConessione_Client_Server();


try
{
    var TCP = new TcpClient();

   await TCP.ConnectAsync("127.0.0.1", 9600);
    NetworkStream stream = TCP.GetStream();

  await  stream.WriteAsync(g);
    await stream.ReadAsync(Ritorno);
    var f = Class_Prova_FINS.Controllo_FINS_TCP_Headr_Primo(Ritorno);
    if (f)
    {
      
       var g1 = Class_Prova_FINS.ScrivereStringa(0x0102, 0xB0, 100, 0,  "si sono un bel ragazzo tu\n si\n", Ritorno[19]);
        Console.WriteLine(BitConverter.ToString(g1));
      await   stream.WriteAsync(g1);
        await stream.ReadAsync(Ritorno);
    }
    
}
catch (Exception ex)
{

    Console.WriteLine($"Errore durante la connessione o la scrittura: {ex.Message}");
}







