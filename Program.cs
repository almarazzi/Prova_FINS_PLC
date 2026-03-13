using Prova_FINS_TCP;
using System.Net.Sockets;


var a = Class_Prova_FINS.ScrivereBIT(0x0102, 0x30, 100, 0, 1, true);
try
{
    using (var TCP = new TcpClient())
    {
        await TCP.ConnectAsync("192.168.1.10", 9600);
        using (NetworkStream stream = TCP.GetStream())
        {
            CancellationTokenSource token = new CancellationTokenSource();
            CancellationToken cancellationToken = token.Token;

            await stream.WriteAsync(a, cancellationToken);


        }
    }
}
catch (Exception ex)
{

    Console.WriteLine($"Errore durante la connessione o la scrittura: {ex.Message}");
}





