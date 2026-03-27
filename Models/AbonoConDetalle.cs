using System;

namespace MauiApp6.Models
{
    public class AbonoConDetalle
    {
        public int IdAbono { get; set; }
        public int VentasId { get; set; }
        public string ClienteNombre { get; set; }
        public DateTime Fecha { get; set; }
        public decimal AbonoMonto { get; set; }
    }
}
