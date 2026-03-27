using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp6.Models
{
    public class VentaRequest
    {
        public Ventas Venta { get; set; }
        public List<VentasDetalle> Detalles { get; set; }
    }
}
