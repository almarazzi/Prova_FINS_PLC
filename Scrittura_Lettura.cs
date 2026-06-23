using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using static Prova_FINS_TCP.CostruzioneHeaderFinsTot;

namespace Prova_FINS_TCP
{
    public class Scrittura_Lettura
    {

        public static byte[] ScrivereBIT(Memory_Area_Code MemoriaArea, ushort Indirizzo, byte Bit, bool value, byte NodoClient) // Fins_Freim_scrittura_BIT
        {
            var N_Byte = sizeof(bool);
            var b = CostruzioneFreim(Command_Code.M_Write, MemoriaArea, Indirizzo, Bit, NodoClient, N_Byte);
            b[b.Length - N_Byte] = (byte)(value ? 1 : 0);
            return b;

        }

        public static byte[] ScrivereWord(Memory_Area_Code MemoriaArea, ushort Indirizzo, ushort value, byte NodoClient) // Fins_Freim_scrittura_Word
        {
            var N_Byte = sizeof(ushort);
            var b = CostruzioneFreim(Command_Code.M_Write, MemoriaArea, Indirizzo, 0, NodoClient, N_Byte);

            ScriveTBigEndianInBuffer<ushort>(b, b.Length - N_Byte, value);
            return b;
        }
        public static byte[] ScrivereDINT(Memory_Area_Code MemoriaArea, ushort Indirizzo, int value, byte NodoClient) // Fins_Freim_scrittura_Dint
        {
            var N_Byte = sizeof(int);
            var b = CostruzioneFreim(Command_Code.M_Write, MemoriaArea, Indirizzo, 0, NodoClient, N_Byte);
            ScriveTBigEndianInBuffer<int>(b, b.Length - N_Byte, value);
            SwapWord(b, b.Length - N_Byte, N_Byte);
            return b;
        }

        public static byte[] ScrivereReal(Memory_Area_Code MemoriaArea, ushort Indirizzo, float value, byte NodoClient) // Fins_Freim_scrittura_Real
        {
            var N_Byte = sizeof(float);
            var b = CostruzioneFreim(Command_Code.M_Write, MemoriaArea, Indirizzo, 0, NodoClient, N_Byte);

            ScriveTBigEndianInBuffer<float>(b, b.Length - N_Byte, value);
            SwapWord(b, b.Length - N_Byte, N_Byte);
            return b;
        }
        public static byte[] ScrivereLReal(Memory_Area_Code MemoriaArea, ushort Indirizzo, double value, byte NodoClient) // Fins_Freim_scrittura_Lreal
        {
            var N_Byte = sizeof(double);
            var b = CostruzioneFreim(Command_Code.M_Write, MemoriaArea, Indirizzo, 0, NodoClient, N_Byte);

            ScriveTBigEndianInBuffer<double>(b, b.Length - N_Byte, value);
            SwapWord(b, b.Length - N_Byte, N_Byte);
            return b;
        }


        public static byte[] ScrivereStringa(Memory_Area_Code MemoriaArea, ushort Indirizzo, string value, byte NodoClient) // Fins_Freim_scrittura_striga
        {

            var T = Encoding.ASCII.GetBytes(value).ToArray();
            if (T.Length % 2 != 0) // stinge non dispari ma sepre pari
            {
                T = T.Concat(new byte[] { 0x00 }).ToArray();
            }
            var b = CostruzioneFreim(Command_Code.M_Write, MemoriaArea, Indirizzo, 0, NodoClient, T.Length);
            T.CopyTo(b, b.Length - T.Length);
            return b;

        }

        public static bool IntepretazioneBIT(byte[] b) //Intepretazione_BIT
        {
            return b[14] == 0x01;
        }

        public static ushort IntepretazioneWord(byte[] b) //Intepretazione_Word
        {
            var f = LeggiTBigEndianDaBuffer<ushort>(b, 14);
            return f;
        }
        public static int IntepretazioneDINT(byte[] b) //Intepretazione_Dint
        {
            SwapWord(b, 14, 4);
            var f = LeggiTBigEndianDaBuffer<int>(b, 14);
            return f;
        }
        public static float IntepretazioneReal(byte[] b) //Intepretazione_Real
        {
            SwapWord(b, 14, 4);
            var f = LeggiTBigEndianDaBuffer<float>(b, 14);
            return f;
        }
        public static double IntepretazioneLReal(byte[] b) //Intepretazione_Lreal
        {
            SwapWord(b, 14, 8);
            var f = LeggiTBigEndianDaBuffer<double>(b, 14);
            return f;
        }

        public static string IntepretazioneString(byte[] b) //Intepretazione_Stringa
        {
            var c = 0;
            for (int i = 14; i < 255; i++)
            {
                if (b[i] == 0)
                {
                    break;
                }
                c++;
            }
            return Encoding.ASCII.GetString(b, 14, c);
        }

        public static void SwapWord(Span<byte> buffer, int of, int byteDati)
        {
            var k = buffer.Length - 1;
            for (int i = 0; i < byteDati / 2; i += 2)
            {

                var temp = (buffer[of + i], buffer[of + i + 1]);
                (buffer[of + i], buffer[of + i + 1]) = (buffer[k - 1 - i], buffer[k - i]);
                (buffer[k - 1 - i], buffer[k - i]) = temp;
            }
        }




        public static T LeggiTBigEndianDaBuffer<T>(Span<byte> buffer, int of) where T : unmanaged
        {
            ulong V = 0;
            var size = Unsafe.SizeOf<T>();
            for (int i = 0; i < size; i++)
            {
                V <<= 8;
                V += buffer[of + i];
            }
            return Unsafe.As<ulong, T>(ref V);
        }
        public static void ScriveTBigEndianInBuffer<T>(Span<byte> buffer, int of, T valore) where T : unmanaged
        {
            var size = Unsafe.SizeOf<T>();
            var V = Unsafe.As<T, ulong>(ref valore);
            for (int i = size - 1; i >= 0; i--)
            {
                buffer[of + i] = (byte)(V & 0xff);
                V >>= 8;

            }

        }


    }

}
