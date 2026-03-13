using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Prova_FINS_TCP
{
    public class Class_Prova_FINS
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TcpHeader
        {
            public uint FINS;
            public uint Lungezza;
            public uint Commando;
            public uint Erorre;
        };
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FinsHeader
        {
            public byte ICF;
            public byte RSV;
            public byte GCT;
            public byte DAN;
            public byte DA1;
            public byte DA2;
            public byte SNA;
            public byte SA1;
            public byte SA2;
            public byte SID;
        };
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Comandi
        {
            public ushort CodiciComandi;
            public byte MemoriaArea;
            public ushort Indirizzo;
            public byte Bit;
            public ushort NumeroElemeti;
        }
        //private static readonly  byte[] b = new byte[Marshal.SizeOf<TcpHeader>() + Marshal.SizeOf<FinsHeader>() + Marshal.SizeOf<Comandi>()];
       
      //  private readonly Span<byte> buffer = stackalloc byte[]; // 16 byte di hearet TCP + 10 byte di header FIN + 8 byte di comandi 
        private static void ScrivereTcpHeader1(Span<byte> buffer, uint Lungezza) // Fins_TCP 16 byte
        {
            TcpHeader Header = new TcpHeader
            {
                FINS = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("FINS"), 0),
                Lungezza = BinaryPrimitives.ReverseEndianness(Lungezza - 8), // Lungezza e TCP_Erorre + TCP_Commando + Fins header + Fins Command + Dati
                Commando = BinaryPrimitives.ReverseEndianness((uint)0x00000002),
                Erorre = BinaryPrimitives.ReverseEndianness((uint)0x00000000),
            };
            //MemoryMarshal.Cast<TcpHeader, byte>(new Span<TcpHeader>(new[] { Header })).CopyTo(b);
            MemoryMarshal.Write(buffer, Header);
        }
        private static void ScrivereFinsHeader(Span<byte> buffer) // Fins 10 byte
        {
            FinsHeader Header = new FinsHeader
            {
                ICF = 0x80, //ICF
                RSV = 0x00, //RSV
                GCT = 0x02, //GCT
                DAN = 0x00, //DNA
                DA1 = 0x00, //DA1. 0x00 PLC Locale, da 0x01 a 0x7f indirizzo del nodo PLC (1 a 254), 0xff Brocast
                DA2 = 0x00, //DA2
                SNA = 0x00, //SNA
                SA1 = 0x00, //SA1 Nodo del PC
                SA2 = 0x00, //SA2
                SID = 0x01, //SID codice sessione
            };
            // MemoryMarshal.Cast<FinsHeader, byte>(new Span<FinsHeader>(new[] { Header })).CopyTo(b.Slice(16));
            MemoryMarshal.Write(buffer[Marshal.SizeOf<TcpHeader>()..], Header);

        }
        private static void ComandiL(Span<byte> buffer, ushort CodiciComandi, byte MemoriaArea, ushort Indirizzo, byte Bit, ushort Word) // Fins_command_Lettura 8 byte
        {
            Comandi comandi = new Comandi
            {
                CodiciComandi = BinaryPrimitives.ReverseEndianness(CodiciComandi),
                MemoriaArea = MemoriaArea,
                Indirizzo = BinaryPrimitives.ReverseEndianness(Indirizzo),
                Bit = Bit,
                NumeroElemeti = BinaryPrimitives.ReverseEndianness(Word),
            };
            //  MemoryMarshal.Cast<Comandi, byte>(new Span<Comandi>(new[] { comandi })).CopyTo(b[(Marshal.SizeOf<TcpHeader>() + Marshal.SizeOf<FinsHeader>())..]);
            MemoryMarshal.Write(buffer[(Marshal.SizeOf<TcpHeader>() + Marshal.SizeOf<FinsHeader>())..], comandi);
        }

        public static byte[] ScrivereBIT(ushort CodiciComadi, byte MemoriaArea, ushort Indirizzo, byte Bit, ushort NumeroElemeti, bool value) // Fins_Freim_scrittura_BIT
        {
            Span<byte> span = stackalloc byte[Marshal.SizeOf<TcpHeader>() + Marshal.SizeOf<FinsHeader>() + Marshal.SizeOf<Comandi>()+ NumeroElemeti];
            ScrivereTcpHeader1(span, (uint)span.Length);
            ScrivereFinsHeader(span);
            ComandiL(span, CodiciComadi, MemoriaArea, Indirizzo, Bit, NumeroElemeti);
            span[Marshal.SizeOf<TcpHeader>() + Marshal.SizeOf<FinsHeader>() + Marshal.SizeOf<Comandi>()] = (byte)(value ? 1 : 0);
            return span.ToArray();

        }
        public static byte[] ScrivereWord(ushort CodiciComadi, byte MemoriaArea, ushort Indirizzo, byte Bit, ushort NumeroElemeti, ushort value) // Fins_Freim_scrittura_Word
        {
            Span<byte> span = stackalloc byte[Marshal.SizeOf<TcpHeader>() + Marshal.SizeOf<FinsHeader>() + Marshal.SizeOf<Comandi>() + NumeroElemeti];
            ScrivereTcpHeader1(span,(uint)span.Length);
            ScrivereFinsHeader(span);
            ComandiL(span, CodiciComadi, MemoriaArea, Indirizzo, Bit, NumeroElemeti);
            BinaryPrimitives.WriteUInt16BigEndian(span[(Marshal.SizeOf<TcpHeader>() + Marshal.SizeOf<FinsHeader>() + Marshal.SizeOf<Comandi>())..], value);
            return span.ToArray();

        }
        public static byte[] ScrivereStringa(ushort CodiciComadi, byte MemoriaArea, ushort Indirizzo, byte Bit, string value) // Fins_Freim_scrittura_striga
        {
            var T = Encoding.ASCII.GetBytes(value);
            Span<byte> span = stackalloc byte[Marshal.SizeOf<TcpHeader>() + Marshal.SizeOf<FinsHeader>() + Marshal.SizeOf<Comandi>() + (ushort)T.Length];
            ScrivereTcpHeader1(span, (uint)span.Length);
            ScrivereFinsHeader(span);
            ComandiL(span, CodiciComadi, MemoriaArea, Indirizzo, Bit, (ushort)T.Length);
            T.CopyTo(span[(Marshal.SizeOf<TcpHeader>() + Marshal.SizeOf<FinsHeader>() + Marshal.SizeOf<Comandi>())..]);
            return span.ToArray();

        }
        /*   public static string LeggiStringa(Span<byte> b, int length)
           {
               byte[] stringBytes = b.Slice(8, length).ToArray();
               return Encoding.ASCII.GetString(stringBytes);
           }
           public static ushort LeggiWord(Span<byte> b) // Fins_command_Lettura
           {

               return BinaryPrimitives.ReadUInt16BigEndian(b.Slice(8));

           }*/
       

    }
}


/*
 * AREA BIT
 * CIO 0x30
 * Work 0x31
 * H 0x32
 * A 0x33
 * 
 * AREA WORD
 * CIO 0xB0
 * Worh 0xB1
 * H 0xB2
 * A 0xB3
 * DM 0x82
 * EM 0x20-0x2F
 * 
 * comandi code 
 * MRS 0x01 acesso I/O area
 * SRC 0x01 Read
 * SRC 0x02 write
 * 
 */