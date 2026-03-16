using Prova_FINS_TCP;
using System.Net.Sockets;


CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken token = cts.Token;

var Ritorno = new byte[1024];
var g = Class_Prova_FINS.PrimaConessione_Client_Server();
byte Nodo = 0;

//ushort SizeElemento = Word * 2;

try
{
    var TCP = new TcpClient();

    await TCP.ConnectAsync("127.0.0.1", 9600);
    NetworkStream stream = TCP.GetStream();

    await stream.WriteAsync(g,token);
    await stream.ReadExactlyAsync(Ritorno, 0, 24, token);
    Class_Prova_FINS.Controllo_FINS_TCP_Headr_Primo(Ritorno);
    Nodo = Ritorno[19];

    var g1 = Class_Prova_FINS.ScrivereStringa(Class_Prova_FINS.Memory_Area_Code.Word_CIO, 300, "ciao si", Nodo); 
    //var g1 = Class_Prova_FINS.ScrivereWord(Class_Prova_FINS.Memory_Area_Code.Word_CIO, 100, 100, Nodo); 
    //var g1 = Class_Prova_FINS.ScrivereBIT(Class_Prova_FINS.Memory_Area_Code.Bit_CIO, 100, 0, true, Nodo);

    //var g1 = Class_Prova_FINS.CostruzioneFreim(Class_Prova_FINS.Command_Code.M_Read,Class_Prova_FINS.Memory_Area_Code.Word_CIO, 100,0 ,Class_Prova_FINS.NumeroWord.WORD, Nodo);

    Array.Clear(Ritorno);
    uint l = 0;
    bool ok;
    do
    {
        await stream.WriteAsync(g1, token);
        await stream.ReadExactlyAsync(Ritorno, 0, 16, token);
        (l,ok) = Class_Prova_FINS.Controllo_Ritrono(Ritorno);

    } while (ok == false);

    var b = new byte[l - 8];
    await stream.ReadExactlyAsync(b, token);
   // var g11 = Class_Prova_FINS.IntepretazioneWord(b,20);


}
catch (Exception ex)
{

    Console.WriteLine($"Errore durante la connessione o la scrittura: {ex.Message}");
}







