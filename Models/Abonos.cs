using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp6.Models
{
    public class Abonos
    {
        [PrimaryKey, AutoIncrement]
        public int idAbono { get; set; }

        [Indexed] // Indexamos para que las búsquedas sean rápidas
        public int idVenta { get; set; } // Relación con la tabla Ventas

        [Indexed] // Indexamos para que las búsquedas sean rápidas
        public int idCliente { get; set; } // Relación con la tabla Clientes

        public int idEmpresa { get; set; } // Relación con la tabla Empresas

        public int idDispositivo {  get; set; }
            
        public DateTime Fecha { get; set; }

        public decimal Abono { get; set; }

        public bool Sincronizado { get; set; } = false;
    }
}
