using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace MauiApp6.Models
{
    public class Empresa
    {
      
        [PrimaryKey]
        public int IdEmpresa { get; set; }

   
        public string NombreEmpresa { get; set; } = string.Empty;

        public byte[]? LogoGrande { get; set; }
        public byte[]? LogoPequeño { get; set; }
        public byte[]? LogoTicket { get; set; }

        public bool Status { get; set; } = true;

        public string? Correo { get; set; }
    }
}
