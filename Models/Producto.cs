using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.IO;



namespace MauiApp6.Models
{
    public class Producto
    {
        [PrimaryKey, AutoIncrement]
        public int IdInventario { get; set; }

        public string Codigo { get; set; }

        [MaxLength(100)] // El nombre no se puede repetir
        public string Descripcion { get; set; }

        public decimal Precio1 { get; set; } // O Costo
        
        public decimal Precio2 { get; set; } // O Costo

        public decimal Precio3 { get; set; } // O Costo
        
        public decimal Precio4 { get; set; } // O Costo

        public decimal Costo { get; set; } // O Costo

        public bool esGranel { get; set; } = false;

        public string ImagenBase64 { get; set; }

        [Ignore]
        public ImageSource ImgSource
        {
            get
            {
                if (string.IsNullOrEmpty(ImagenBase64))
                    return null;
                try
                {
                    var imageBytes = Convert.FromBase64String(ImagenBase64);
                    return ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
                catch
                {
                    return null;
                }
            }
        }



        [Ignore] // SQLite ignorará esta propiedad, solo vive en la pantalla
        public decimal CantidadCapturada { get; set; }

        private decimal _precioVenta;

        [Ignore]
        public decimal PrecioVenta
        {
            get => _precioVenta > 0 ? _precioVenta : Precio1;
            set => _precioVenta = value;
        }

        [Ignore]
        public decimal Subtotal => CantidadCapturada * PrecioVenta;

    }
}
