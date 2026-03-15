using Prova_FINS_TCP;
using System.Net.Sockets;


CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken token = cts.Token;

var Ritorno = new byte[1024];
var g = Class_Prova_FINS.PrimaConessione_Client_Server();
byte Nodo = 0;

try
{
    var TCP = new TcpClient();

    await TCP.ConnectAsync("127.0.0.1", 9600);
    NetworkStream stream = TCP.GetStream();

    await stream.WriteAsync(g);
    await stream.ReadAsync(Ritorno);
    Class_Prova_FINS.Controllo_FINS_TCP_Headr_Primo(Ritorno);
    Nodo = Ritorno[19];

      var g1 = Class_Prova_FINS.ScrivereStringa((ushort)Class_Prova_FINS.Command_Code.M_Write, (byte)Class_Prova_FINS.Memory_Area_Code.Word_CIO, 100, 0, "ciao\n", Nodo);

 //  var g1 = Class_Prova_FINS.Lettura((ushort)Class_Prova_FINS.Command_Code.M_Read, (byte)Class_Prova_FINS.Memory_Area_Code.Word_CIO, 100, 0, 255, Nodo);

    await stream.WriteAsync(g1);
    await stream.ReadAsync(Ritorno);
    Class_Prova_FINS.Controllo_Ritrono_Scrittura(Ritorno);
   // var f = Class_Prova_FINS.IntepretazioneString(Ritorno);

}
catch (Exception ex)
{

    Console.WriteLine($"Errore durante la connessione o la scrittura: {ex.Message}");
}







