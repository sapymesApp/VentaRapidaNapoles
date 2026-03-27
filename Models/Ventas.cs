using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp6.Models
{
    public class Ventas
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int Folio { get; set; }

        public int idEmpresa { get; set; }

        public int idCliente { get; set; } 
        
        public int idDispositivo { get; set; }

        public int idUsuario { get; set; }

        [Ignore]
        public string ClienteNombre { get; set; }


        public DateTime Fecha { get; set; }

        public string Observaciones { get; set; } // "Ventas por merma", "Venta", etc.

        public decimal Total { get; set; } // Suma total de la ventas

        public decimal Saldo { get; set; } // Saldo de la ventas

        public decimal Devolucion { get; set; }

        public decimal Abono { get; set; }

        // NUEVO: Estado de la ventas (Abierta / Finalizada)
        public string Status { get; set; } = "Abierta";

        public int VentaNumero { get; set; } // 1, 2, o 3 para borradores

        public bool Sincronizado { get; set; }=false;
    }
}
