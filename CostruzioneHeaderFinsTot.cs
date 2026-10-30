using System.Buffers.Binary;
using System.Text;


namespace Prova_FINS_TCP
{
    public class CostruzioneHeaderFinsTot
    {

        /*   public enum NumeroWord : ushort
           {
               Byte = 1, //non sono Sicuro??
               INT = 1, //Segnio
               DINT = 2, //Segnio
               UDINT = 2, //Senza Segnio
               ULINT = 4, //Senza Segnio
               REAL = 2,
               LREAL = 4,
               WORD = 1,
               DWORD = 2,
               LWORD = 4,
           }*/
        public enum Command_Code : ushort
        {
            M_Read = 0x0101,
            M_Write = 0x0102,
        }

        public enum Memory_Area_Code : byte
        {
            BIT_CIO = 0x30,
            BIT_WR = 0x31,
            BIT_HR = 0x32,
            BIT_AR = 0x33,
            BIT_DM = 0x02,

            WORD_CIO = 0xB0,
            WORD_WR = 0xB1,
            WORD_HR = 0xB2,
            WORD_AR = 0xB3,
            WORD_DM = 0x82,
        }

        public enum FINS_TCP_Commandi : uint
        {
            Client_Server = 0x00000000,
            Servet_Client = 0x00000001,
            Frame_Send = 0x00000002,
            Freim_Error = 0x00000003,
            Conenection_Confirmation = 0x00000006,

        }
        private enum FINS_TCP_Lengt : uint
        {
            Client_Server_L = 0x0000000C,
            Servet_Client_L = 0x00000010,
            Freim_Error_L = 0x00000008,
            Conenection_Confirmation_L = 0x0000008,
        }

        private readonly struct FINS_payloadLen
        {
            private const uint MIN = 0x00000014;
            private const uint MAX = 0x000007E4;
            public static bool Valido(uint value) => value >= MIN && value <= MAX;

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
            node_addresss_for_allocation_have_been_used = 0x00000023,
        }

        public const int LungezzaHeadrPrimaConessione_Invio = 20;
        private const int LungezzaHeadrTCP = 16;
        private const int LungezzaHeadrFINS = 10;
        private const int LungezzaCommandi = 8; //Command code + memory area code + address + number of itmes
        private static readonly byte[] FINS = Encoding.ASCII.GetBytes("FINS");

        public static byte[] PrimaConessione_Client_Server()
        {
            var b = new byte[LungezzaHeadrPrimaConessione_Invio];
            var of = 0;

            FINS.CopyTo(b);
            of += 4;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<uint>(b, of, (uint)FINS_TCP_Lengt.Client_Server_L);
            of += 4;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<uint>(b, of, (uint)FINS_TCP_Commandi.Client_Server);
            of += 4;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<uint>(b, of, (uint)FINS_TCP_Eorri.Normal);
            of += 4;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<uint>(b, of, 0x00000000);
            return b;
        }

        public static void Controllo_FINS_TCP_Header_Primo(byte[] b)
        {
            var of = 0;
            if (!b.AsSpan(of, FINS.Length).SequenceEqual(FINS))
                throw new Exception("l'Heder_Fins_TCP Non Contine FINS");
            of += 4;
            var Lungezza = (FINS_TCP_Lengt)Scrittura_Lettura.LeggiTBigEndianDaBuffer<uint>(b, of);
            if (Lungezza != FINS_TCP_Lengt.Servet_Client_L)
                throw new Exception($"lungezza Fins_TCP non coretto");
            of += 4;
            var Commando = (FINS_TCP_Commandi)Scrittura_Lettura.LeggiTBigEndianDaBuffer<uint>(b, of);
            if (Commando != FINS_TCP_Commandi.Servet_Client)
                throw new Exception($"Fins_TCP command non valido");
            of += 4;
            var Erorre = (FINS_TCP_Eorri)Scrittura_Lettura.LeggiTBigEndianDaBuffer<uint>(b, of);
            if (Erorre != FINS_TCP_Eorri.Normal)
                throw new Exception($"Fins_TCP Erorre");

        }

        public static uint Controllo_Ritrono(byte[] b)
        {
            var of = 0;
            if (!b.AsSpan(of, FINS.Length).SequenceEqual(FINS))
                throw new Exception("l'Heder_Fins_TCP Non Contine FINS");
            of += 4;
            var payloadLen = Scrittura_Lettura.LeggiTBigEndianDaBuffer<uint>(b, of);
            if (!FINS_payloadLen.Valido(payloadLen))
                throw new Exception($"lungezza Fins_TCP non coretto");
            of += 4;
            var Commando = (FINS_TCP_Commandi)Scrittura_Lettura.LeggiTBigEndianDaBuffer<uint>(b, of);
            if (Commando != FINS_TCP_Commandi.Frame_Send && Commando != FINS_TCP_Commandi.Conenection_Confirmation)
            {
                throw new Exception($"Fins_TCP command non valido");
            }
            of += 4;
            var Erorre = (FINS_TCP_Eorri)Scrittura_Lettura.LeggiTBigEndianDaBuffer<uint>(b, of);
            if (Erorre != FINS_TCP_Eorri.Normal)
                throw new Exception($"Fins_TCP Erorre");
            return (payloadLen);
        }


        private static void ScrivereTcpHeader1(Span<byte> buffer, uint Lungezza) // Fins_TCP 16 byte
        {
            var of = 0;
            Encoding.ASCII.GetBytes("FINS", buffer);
            of += 4;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<uint>(buffer, of, Lungezza - 8); // Lungezza e TCP_Erorre + TCP_Commando + Fins header + Fins Command + Dati
            of += 4;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<uint>(buffer, of, (uint)FINS_TCP_Commandi.Frame_Send);
            of += 4;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<uint>(buffer, of, (uint)FINS_TCP_Eorri.Normal);
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
        private static void Comandi(Span<byte> buffer, Command_Code CodiciComandi, Memory_Area_Code MemoriaArea, ushort Indirizzo, byte Bit, int NumeroByteDati) // Fins_command_Lettura 8 byte
        {
            var of = 0;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<ushort>(buffer, of, (ushort)CodiciComandi);
            of += 2;
            buffer[of] = (byte)MemoriaArea;
            of += 1;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<ushort>(buffer, of, Indirizzo);
            of += 2;
            buffer[of] = Bit;
            of += 1;
            Scrittura_Lettura.ScriveTBigEndianInBuffer<ushort>(buffer, of, (ushort)(NumeroByteDati / 2));  // Numero di word 
        }

        public static byte[] CostruzioneFreim(Command_Code Command, Memory_Area_Code MemoriaArea, ushort Indirizzo, byte Bit, byte NodoClient, int NumeroByteDati) // Fins_Freim
        {
            byte[] b;
            if (Command == Command_Code.M_Write)
            {
                b = new byte[LungezzaHeadrTCP + LungezzaHeadrFINS + LungezzaCommandi + NumeroByteDati];
            }
            else
            {
                b = new byte[LungezzaHeadrTCP + LungezzaHeadrFINS + LungezzaCommandi];
            }
            var of = 0;
            ScrivereTcpHeader1(b, (uint)b.Length);
            of += LungezzaHeadrTCP;
            ScrivereFinsHeader(b.AsSpan(of, LungezzaHeadrFINS), NodoClient);
            of += LungezzaHeadrFINS;
            if (MemoriaArea is (Memory_Area_Code.BIT_DM or Memory_Area_Code.BIT_CIO or Memory_Area_Code.BIT_WR or Memory_Area_Code.BIT_HR or Memory_Area_Code.BIT_HR or Memory_Area_Code.BIT_AR))
                NumeroByteDati = 2;
            Comandi(b.AsSpan(of, LungezzaCommandi), Command, MemoriaArea, Indirizzo, Bit, NumeroByteDati);
            return b;

        }
    }
}

