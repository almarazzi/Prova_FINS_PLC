using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;



namespace Prova_FINS_TCP
{
    public class Comunicazione<T>
    {

        private readonly T _Value;
        private readonly string _LetturaScrittura = "";
        private readonly ushort _IndirizzoPrimaParte;
        private CostruzioneHeaderFinsTot.Memory_Area_Code _memory_Area_Code;
        private readonly byte _BIT;
        private byte _Nodo;
        public Comunicazione(string LetturaScrittura, ushort IndirizzoPrimaParte, byte BIT, T Value)
        {
            _Value = Value;
            _IndirizzoPrimaParte = IndirizzoPrimaParte;
            _BIT = BIT;
            _LetturaScrittura = LetturaScrittura;
        }

        private byte[] Costruzione()
        {
            if (_LetturaScrittura == "R")
            {
                var Numerobyte = 0;
                if (typeof(T) == typeof(bool))
                    Numerobyte = 1;
                else if (typeof(T) == typeof(string))
                    Numerobyte = 255;
                else
                    Numerobyte = Unsafe.SizeOf<T>();
                return CostruzioneHeaderFinsTot.CostruzioneFreim(CostruzioneHeaderFinsTot.Command_Code.M_Read, _memory_Area_Code, _IndirizzoPrimaParte, _BIT, _Nodo, Numerobyte);
            }
            else
            {
                return _Value switch
                {
                    bool V => Scrittura_Lettura.ScrivereBIT(_memory_Area_Code, _IndirizzoPrimaParte, _BIT, V, _Nodo),
                    ushort V => Scrittura_Lettura.ScrivereWord(_memory_Area_Code, _IndirizzoPrimaParte, V, _Nodo),
                    int V => Scrittura_Lettura.ScrivereDINT(_memory_Area_Code, _IndirizzoPrimaParte, V, _Nodo),
                    float V => Scrittura_Lettura.ScrivereReal(_memory_Area_Code, _IndirizzoPrimaParte, V, _Nodo),
                    double V => Scrittura_Lettura.ScrivereLReal(_memory_Area_Code, _IndirizzoPrimaParte, V, _Nodo),
                    string V => Scrittura_Lettura.ScrivereStringa(_memory_Area_Code, _IndirizzoPrimaParte, V, _Nodo),
                    _ => throw new Exception($"Tipo non supportato: {typeof(T).Name}"),

                };
            }

        }

        private static T Lettura(byte[] buffer)
        {
            object risultato = typeof(T) switch
            {
                var t when t == typeof(bool) => Scrittura_Lettura.IntepretazioneBIT(buffer),
                var t when t == typeof(ushort) => Scrittura_Lettura.IntepretazioneWord(buffer),
                var t when t == typeof(int) => Scrittura_Lettura.IntepretazioneDINT(buffer),
                var t when t == typeof(float) => Scrittura_Lettura.IntepretazioneReal(buffer),
                var t when t == typeof(double) => Scrittura_Lettura.IntepretazioneLReal(buffer),
                var t when t == typeof(string) => Scrittura_Lettura.IntepretazioneString(buffer),
                _ => throw new Exception($"Tipo non supportato: {typeof(T).Name}"),
            };
            return (T)risultato;
        }


        public  async Task<(T, bool)> ComuncazioneTCP(CancellationToken cancellationToken)
        {

            var BufferDiRitorno = new byte[1024];
            var Prima = CostruzioneHeaderFinsTot.PrimaConessione_Client_Server();

            try
            {
                var tcp = new TcpClient();
                await tcp.ConnectAsync("127.0.0.1", 9600);
                var stream = tcp.GetStream();

                //primaConessione
                await stream.WriteAsync(Prima, cancellationToken);
                await stream.ReadExactlyAsync(BufferDiRitorno, 0, 24, cancellationToken);
                CostruzioneHeaderFinsTot.Controllo_FINS_TCP_Header_Primo(BufferDiRitorno);
                _Nodo = BufferDiRitorno[19];

                var buffer = Costruzione();
                Array.Clear(BufferDiRitorno);
                uint l = 0;
                bool ok;
                do
                {
                    await stream.WriteAsync(buffer, cancellationToken);
                    await stream.ReadExactlyAsync(BufferDiRitorno, 0, 16, cancellationToken);
                    (l, ok) = CostruzioneHeaderFinsTot.Controllo_Ritrono(BufferDiRitorno);

                } while (ok == false);

                var BufferDati_valid = new byte[l - 8];
                await stream.ReadExactlyAsync(BufferDati_valid, cancellationToken);
                var Codice_End = Scrittura_Lettura.LeggiTBigEndianDaBuffer<ushort>(BufferDati_valid.AsSpan(12, 2), 0);
                if (Codice_End != 0x00)
                    throw new Exception($"Erorre il codice di ritorno e {Codice_End}");
                tcp.Close();
                if (_LetturaScrittura == "R")
                {
                    var Valore = Lettura(BufferDati_valid);
                    return (Valore, true);
                }
                else if (_LetturaScrittura == "W")
                {
                    return (default!, true);
                }
                else
                    return (default!, false);

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Errore durante la connessione o la scrittura: {ex.Message}");
                return (default!, false);
            }
        }


    }
}
