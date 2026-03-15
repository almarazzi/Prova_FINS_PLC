using System.Buffers.Binary;
using System.Text;

namespace Prova_FINS_TCP
{
    public class Class_Prova_FINS
    {

        private enum FINS_TCP_Lengt : uint
        {
            Client_Server_L = 0x0000000C,
            Servet_Client_L = 0x00000010,
            Freim_Error_L = 0x00000008,
            Conenection_Confirmation_L = 0x0000008,
        }
        private enum FINS_TCP_Eorri : uint
        {
            Normal = 0x00000000,
            Header_is_not_FINS_ = 0x00000001,
            Data_length_is_too_long = 0x00000002,
            Command_not_supported = 0x00000003,
            All_commections_are_use = 0x00000020,
            Access_a_protected_node_from_an_unspecified_ip = 0x00000022,
            Client_node_address_is_out_range = 0x00000023,
            Seme_node_address_are_use_by_client_server = 0x00000024,
            node_addresss_for_allocation_have_been_used = 00000023,
        }

        private enum FINS_TCP_Commandi : uint
        {
            Client_Server = 0x00000000,
            Servet_Client = 0x00000001,
            Freim_Send = 0x00000002,
            Freim_Error = 0x00000003,
            Conenection_Confirmation = 0x00000006,

        }
        private enum Command_Code : ushort
        {
            M_Read = 0x0101,
            M_Write = 0x0102,
        }
        public enum Memory_Area_Code : byte
        {
            Bit_CIO = 0x30,
            Bit_WR = 0x31,
            Bit_HR = 0x32,
            Bit_AR = 0x33,
            Bit_DM = 0x02,

            Word_CIO = 0xB0,
            Word_WR = 0xB1,
            Word_HR = 0xB3,
            Word_AR = 0xB3,
            Word_DM = 0x82,
        }
        private const int LungezzaHeadrPrimaConessione_Invio = 20;
        private const int LungezzaHeadrTCP = 16;
        private const int LungezzaHederFINS = 10;
        private const int LungezzaCommandi = 8; //Command code + memory area code + address + number of itmes
        private static readonly byte[] FINS = Encoding.ASCII.GetBytes("FINS");

        public static byte[] PrimaConessione_Client_Server()
        {
            var b = new byte[LungezzaHeadrPrimaConessione_Invio];
            var of = 0;

            FINS.CopyTo(b);
            of += 4;
            BinaryPrimitives.WriteUInt32BigEndian(b.AsSpan(of, sizeof(FINS_TCP_Lengt)), (uint)FINS_TCP_Lengt.Client_Server_L);
            of += 4;
            BinaryPrimitives.WriteUInt32BigEndian(b.AsSpan(of, sizeof(FINS_TCP_Commandi)), (uint)FINS_TCP_Commandi.Client_Server);
            of += 4;
            BinaryPrimitives.WriteUInt32BigEndian(b.AsSpan(of, sizeof(FINS_TCP_Eorri)), (uint)FINS_TCP_Eorri.Normal);
            of += 4;
            BinaryPrimitives.WriteUInt32BigEndian(b.AsSpan(of, sizeof(uint)), 0x00000000);
            return b;
        }

        public static void Controllo_FINS_TCP_Headr_Primo(byte[] b)
        {
            var of = 0;
            if (!b.AsSpan(of, FINS.Length).ContainsAny(FINS))
                throw new Exception("l'Heder_Fins_TCP Non Contine FINS");
            of += 4;
            if (BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(of, sizeof(FINS_TCP_Lengt))) != (uint)FINS_TCP_Lengt.Servet_Client_L)
                throw new Exception($"lungezza Fins_TCP non coretto");
            of += 4;
            if (BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(of, sizeof(FINS_TCP_Commandi))) != (uint)FINS_TCP_Commandi.Servet_Client)
                throw new Exception($"Fins_TCP command non valido");
            of += 4;
            if (BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(of, sizeof(FINS_TCP_Eorri))) != (uint)FINS_TCP_Eorri.Normal)
                throw new Exception($"Fins_TCP Erorre");

        }

        public static uint Controllo_Ritrono(byte[] b)
        {
            var of = 0;
            if (!b.AsSpan(of, FINS.Length).ContainsAny(FINS))
                throw new Exception("l'Heder_Fins_TCP Non Contine FINS");
            of += 4;
            if (BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(of, sizeof(FINS_TCP_Lengt))) is uint payloadLen && (payloadLen < 0x00000014 || payloadLen > 0x000007E4))
                throw new Exception($"lungezza Fins_TCP non coretto");
            of += 4;
            if (BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(of, sizeof(FINS_TCP_Commandi))) != (uint)FINS_TCP_Commandi.Freim_Send)
                throw new Exception($"Fins_TCP command non valido");
            of += 4;
            if (BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(of, sizeof(FINS_TCP_Eorri))) != (uint)FINS_TCP_Eorri.Normal)
                throw new Exception($"Fins_TCP Erorre");

            return payloadLen;
        }

     
        private static void ScrivereTcpHeader1(Span<byte> buffer, uint Lungezza) // Fins_TCP 16 byte
        {

            Encoding.ASCII.GetBytes("FINS", buffer);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(4), Lungezza - 8); // Lungezza e TCP_Erorre + TCP_Commando + Fins header + Fins Command + Dati
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(8), (uint)FINS_TCP_Commandi.Freim_Send);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(12), (uint)FINS_TCP_Eorri.Normal);

            //MemoryMarshal.Cast<TcpHeader, byte>(new Span<TcpHeader>(new[] { Header })).CopyTo(b);

        }
        private static void ScrivereFinsHeader(Span<byte> buffer, byte NodoClient) // Fins 10 byte
        {

            buffer[0] = 0x80; //ICF
            buffer[1] = 0x00; //RSV
            buffer[2] = 0x02; //GCT
            buffer[3] = 0x00; //DNA
            buffer[4] = 0x01; //DA1. 0x00 PLC Locale, da 0x01 a 0x7f indirizzo del nodo PLC (1 a 254), 0xff Brocast
            buffer[5] = 0x00; //DA2
            buffer[6] = 0x00; //SNA
            buffer[7] = NodoClient; //SA1 Nodo del PC
            buffer[8] = 0x00; //SA2
            buffer[9] = 0x00; //SID codice sessione

        }
        private static void Comandi(Span<byte> buffer, Command_Code CodiciComandi, Memory_Area_Code MemoriaArea, ushort Indirizzo, byte Bit, ushort SizeElemento) // Fins_command_Lettura 8 byte
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer, (ushort)CodiciComandi);
            buffer[2] = (byte)MemoriaArea;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(3), Indirizzo);
            buffer[5] = Bit;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(6), (ushort)(SizeElemento/2));

        }


       public static byte[] Lettura(Memory_Area_Code MemoriaArea, ushort Indirizzo, byte Bit, ushort NumeroWord, byte NodoClient) // Fins_Freim_Lettura
        {
            var b = new byte[LungezzaHeadrTCP + LungezzaHederFINS + LungezzaCommandi];
            var of = 0;
            ScrivereTcpHeader1(b, (uint)b.Length);
            of += LungezzaHeadrTCP;
            ScrivereFinsHeader(b.AsSpan(of, LungezzaHederFINS), NodoClient);
            of += LungezzaHederFINS;
            Comandi(b.AsSpan(of, LungezzaCommandi), Command_Code.M_Read, MemoriaArea, Indirizzo, Bit, NumeroWord);
            return b;

        }
        private static byte[] Scrivere(Memory_Area_Code MemoriaArea, ushort Indirizzo, byte Bit, ushort SizeElemento, byte NodoClient) // Fins_Freim_scrittura
        {
            var b = new byte[LungezzaHeadrTCP + LungezzaHederFINS + LungezzaCommandi + SizeElemento];
            var of = 0;
            ScrivereTcpHeader1(b, (uint)b.Length);
            of += LungezzaHeadrTCP;
            ScrivereFinsHeader(b.AsSpan(of, LungezzaHederFINS), NodoClient);
            of += LungezzaHederFINS;
            Comandi(b.AsSpan(of, LungezzaCommandi), Command_Code.M_Write, MemoriaArea, Indirizzo, Bit, SizeElemento);
            return b;

        }

        public static byte[] ScrivereBIT(Memory_Area_Code MemoriaArea, ushort Indirizzo, byte Bit, bool value, byte NodoClient) // Fins_Freim_scrittura_BIT
        {
            var b = Scrivere(MemoriaArea, Indirizzo, Bit,1, NodoClient);
            b[LungezzaHeadrTCP + LungezzaHederFINS + LungezzaCommandi] = (byte)(value ? 1 : 0);
            return b;

        }
        public static bool IntepretazioneBIT(byte[] b) //Intepretazione_BIT
        {
            
            return b[30] == 0x01;

        }
        public static byte[] ScrivereWord(Memory_Area_Code MemoriaArea, ushort Indirizzo, ushort value, byte NodoClient) // Fins_Freim_scrittura_Word
        {
            var b = Scrivere(MemoriaArea, Indirizzo, 0, 2, NodoClient);
            BinaryPrimitives.WriteUInt16BigEndian(b.AsSpan(LungezzaHeadrTCP + LungezzaHederFINS + LungezzaCommandi, 2), value);
            return b;

        }

        public static uint IntepretazioneWord(byte[] b) //Intepretazione_Word
        {

            return BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(30, sizeof(ushort)));

        }
        public static byte[] ScrivereStringa(Memory_Area_Code MemoriaArea, ushort Indirizzo, string value, byte NodoClient) // Fins_Freim_scrittura_striga
        {
            var T = Encoding.ASCII.GetBytes(value);
            var b = Scrivere(MemoriaArea, Indirizzo, 0,(ushort)(T.Length), NodoClient);
            T.CopyTo(b, LungezzaHeadrTCP + LungezzaHederFINS + LungezzaCommandi);
            return b;

        }
        public static string IntepretazioneString(byte[] b) //Intepretazione_Stringa
        {
            var c = 0;
            for (int i = 30; i < 255; i++)
            {
                if (b[i] == 0)
                {
                    break;
                }
                c++;
            }
            return Encoding.ASCII.GetString(b, 30, c);
        }

    }
}

