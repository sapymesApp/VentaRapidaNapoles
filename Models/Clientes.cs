using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp6.Models
{
    public class Clientes
    {
        [PrimaryKey, AutoIncrement]
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public decimal Saldo { get; set; }
        public string Direccion { get; set; }
        public bool Sincronizado { get; set; }
        public bool Descargado { get; set; }

        public bool Credito { get; set; }

        public string Telefono { get; set; }
        public string WhatsApp { get; set; }
    }
}
