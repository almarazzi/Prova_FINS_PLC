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

dynamic? Valore = null;
bool Isvalid = false;
switch (Tipo)
{
    case "BIT":
        (Valore, Isvalid) = await new Comunicazione<bool>(Indirizzo, bool.Parse(Dato)).ComuncazioneTCP(token);
        break;
    case "WORD":
        (Valore,Isvalid) = await  new Comunicazione<ushort>(Indirizzo, ushort.Parse(Dato)).ComuncazioneTCP(token);
        break;
    case "DINT":
        (Valore, Isvalid) = await new Comunicazione<int>(Indirizzo, int.Parse(Dato)).ComuncazioneTCP(token);
        break;
    case "REAL":
        (Valore, Isvalid) = await new Comunicazione<float>(Indirizzo, float.Parse(Dato)).ComuncazioneTCP(token);
        break;
    case "LREAL":
        (Valore, Isvalid) = await new Comunicazione<double>(Indirizzo, double.Parse(Dato)).ComuncazioneTCP(token);
        break;
    case "STRING":
        (Valore, Isvalid) = await new Comunicazione<string>(Indirizzo, Dato).ComuncazioneTCP(token);
        break;

}

if (Isvalid is true && Valore is not null)
    Console.WriteLine($"Per la Lettura il valore è {Valore}");
if (Isvalid is true && Valore is null)
    Console.WriteLine($"E andato tutto bene");
if (Isvalid is false)
    Console.WriteLine($"c'è stato un erorre");
