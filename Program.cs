using Prova_FINS_TCP;

CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken token = cts.Token;
var tcpService = new Comuncazione<double>("r_word_cio111", 888888888888.99); //2147483647

var (Valore, Isvalid) = await tcpService.ComuncazioneTCP(token);

if (Isvalid is true && Valore != default)
    Console.WriteLine($"Per la Lettura il valore è {Valore}");
if (Isvalid is true && Valore == default)
    Console.WriteLine($"E andato tutto bene");
