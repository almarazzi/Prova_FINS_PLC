using Prova_FINS_TCP;
using System.Text.RegularExpressions;
using static Prova_FINS_TCP.CostruzioneHeaderFinsTot;

CancellationTokenSource cts = new();
CancellationToken token = cts.Token;
Console.Write("stringa di comando+indirzzo: es r_word_cio100[.0-15] o w_word_cio100[.0-15] => ");
var Indirizzo = Console.ReadLine()!;

var R = new Regex(@"^([R,W])_([A-Z_]+)([0-9]+)[?:.]?([0-9]|1[0-5])?$");
var Spazi = new Regex(@"\s+");
var Modifice = Spazi.Replace(Indirizzo, "").ToUpper();
var Indirizzo1 = R.Match(Modifice);
if (!Indirizzo1.Success)
    throw new Exception("Indirizzo non corisponde");
var LetturaScrittura = Indirizzo1.Groups[1].Value;
var memory_Area_Code = Enum.Parse<Memory_Area_Code>(Indirizzo1.Groups[2].Value);
var IndirizzoPrimaParte = ushort.Parse(Indirizzo1.Groups[3].Value);
byte bit = 0;
if (Indirizzo1.Groups[4].Value != "")
    bit = byte.Parse(Indirizzo1.Groups[4].Value);
var Dato="";
if (LetturaScrittura == "W")
{
    Console.Write("dato => ");
    Dato = Console.ReadLine()!;
}
Console.Write("Tipo => ");
var Tipo = Console.ReadLine()!;
Tipo = Tipo.ToUpper();


(dynamic Value, bool IsValid) r = Tipo switch
{
    "BIT" => await new Comunicazione<bool>(LetturaScrittura,IndirizzoPrimaParte,bit, (LetturaScrittura == "W") ? bool.Parse(Dato): default ).ComuncazioneTCP(token),
    "WORD" => await new Comunicazione<ushort>(LetturaScrittura, IndirizzoPrimaParte, bit,(LetturaScrittura == "W") ? ushort.Parse(Dato) : default).ComuncazioneTCP(token),
    "DINT" => await new Comunicazione<int>(LetturaScrittura, IndirizzoPrimaParte, bit, (LetturaScrittura == "W")  ?int.Parse(Dato) : default ).ComuncazioneTCP(token),
    "REAL" => await new Comunicazione<float>(LetturaScrittura, IndirizzoPrimaParte, bit, (LetturaScrittura == "W") ? float.Parse(Dato) : default).ComuncazioneTCP(token),
    "LREAL" => await new Comunicazione<double>(LetturaScrittura, IndirizzoPrimaParte, bit, (LetturaScrittura == "W") ? double.Parse(Dato):default).ComuncazioneTCP(token),
    "STRING" => await new Comunicazione<string>(LetturaScrittura, IndirizzoPrimaParte, bit, (LetturaScrittura == "W") ? Dato : "").ComuncazioneTCP(token),
    _ => throw new Exception("Erorre tipo non trovato"),

};
if (r.IsValid is true && r.Value is not null)
    Console.WriteLine($"Per la Lettura il valore è {r.Value}");
if (r.IsValid is true && r.Value is null)
    Console.WriteLine($"E andato tutto bene");
if (r.IsValid is false)
    Console.WriteLine($"c'è stato un erorre");



// Per comunicare con il simens usare snap7  https://www.mesta-automation.com/how-to-write-a-siemens-s7-plc-driver-with-c-and-sharp7/