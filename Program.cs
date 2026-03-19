using Prova_FINS_TCP;

CancellationTokenSource cts = new();
CancellationToken token = cts.Token;
Console.Write("stringa di comando+indirzzo: es r_word_cio100[.0-15] o w_word_cio100[.0-15] => ");
var Indirizzo = Console.ReadLine()!;
Console.Write("dato => ");
var Dato = Console.ReadLine()!;
Console.Write("Tipo => ");
var Tipo = Console.ReadLine()!;
Tipo = Tipo.ToUpper();
if (Dato =="")
    Dato = "0";



(dynamic Value,bool IsValid) r = Tipo switch
{
    "BIT" =>  await new Comunicazione<bool>(Indirizzo, bool.Parse(Dato)).ComuncazioneTCP(token),
    "WORD" => await new Comunicazione<ushort>(Indirizzo, ushort.Parse(Dato)).ComuncazioneTCP(token),
    "DINT" => await new Comunicazione<int>(Indirizzo, int.Parse(Dato)).ComuncazioneTCP(token),
    "REAL" => await new Comunicazione<float>(Indirizzo, float.Parse(Dato)).ComuncazioneTCP(token),
    "LREAL" => await new Comunicazione<double>(Indirizzo, double.Parse(Dato)).ComuncazioneTCP(token),
    _ => throw new Exception("Erorre tipo non trovato"),

};
if (r.IsValid is true && r.Value is not null)
    Console.WriteLine($"Per la Lettura il valore è {r.Value}");
if (r.IsValid is true && r.Value is null)
    Console.WriteLine($"E andato tutto bene");
if (r.IsValid is false)
    Console.WriteLine($"c'è stato un erorre");
