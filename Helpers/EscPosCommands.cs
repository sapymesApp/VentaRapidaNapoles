using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp6
{
    public class EscPosCommands
    {
        // Inicializar impresora (Borra buffer y restablece configuración)
        public static readonly byte[] Initialize = { 0x1B, 0x40 };

        // Alineación
        public static readonly byte[] AlignLeft = { 0x1B, 0x61, 0x00 };
        public static readonly byte[] AlignCenter = { 0x1B, 0x61, 0x01 };
        public static readonly byte[] AlignRight = { 0x1B, 0x61, 0x02 };

        // Negritas (Enfasis)
        public static readonly byte[] BoldOn = { 0x1B, 0x45, 0x01 };
        public static readonly byte[] BoldOff = { 0x1B, 0x45, 0x00 };

        // Tamaño de texto (GS ! n)
        // 0x00 = Normal, 0x11 = Doble Altura y Anchura, 0x01 = Doble Anchura
        public static readonly byte[] SizeNormal = { 0x1D, 0x21, 0x00 };
        public static readonly byte[] SizeDouble = { 0x1D, 0x21, 0x11 };
        public static readonly byte[] SizeLarge = { 0x1D, 0x21, 0x10 }; // Doble altura

        // Avance de papel
        public static readonly byte[] FeedLine = { 0x0A }; // Salto de línea simple
        public static readonly byte[] Feed3Lines = { 0x1B, 0x64, 0x03 }; // Avanzar 3 líneas


        // Comandos de Guillotina (Corte de papel)
        public static readonly byte[] FullCut = { 0x1D, 0x56, 0x00 };    // Corte Total
        public static readonly byte[] PartialCut = { 0x1D, 0x56, 0x01 }; // Corte Parcial

    }
}
