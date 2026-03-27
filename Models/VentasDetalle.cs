using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp6.Models
{
    public class VentasDetalle
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed] // Indexamos para que las búsquedas sean rápidas
        public int VentasId { get; set; } // Relación con la tabla Ventas

        [Indexed]
        public int ProductoId { get; set; } // Relación con la tabla Producto

        public decimal Cantidad { get; set; }

        public string Descripcion { get; set; }

        public decimal Precio { get; set; } // Guardamos el precio del momento, por si cambia el catálogo después

        public decimal Costo { get; set; } // Guardamos el precio del momento, por si cambia el catálogo después


        // NUEVO: Para guardar cuántos regresaron
        public decimal Devolucion { get; set; }

        // Propiedad calculada (no se guarda en BD, pero sirve para mostrar en pantalla)
        [Ignore]
        public decimal ImporteTotal => (Cantidad - Devolucion) * Precio;


        [Ignore]
        public decimal CostoTotal => (Cantidad - Devolucion) * Costo;
    }
}
