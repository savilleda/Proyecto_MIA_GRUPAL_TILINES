using System.Xml.Serialization;
namespace GestionEstudiantes
{
  // Estos atributos indican como se representara la clase Estudiante en XML
// XmlRoot define la etiqueta principal cuando se guarda un solo estudiante
// XmlType ayuda a mantener el mismo nombre de etiqueta cuando se usa dentro de listas
    [XmlRoot("estudiante")]
    [XmlType("estudiante")]
    public class Estudiante
    {
        // XmlElement define el nombre de la etiqueta que tendra esta propiedad
         //cuando el objeto Estudiante se guarde o lea desde un archivo XML
        [XmlElement("carne")]
        public string Carne { get; set; }

        [XmlElement("nombres")]
        public string Nombres { get; set; }

        [XmlElement("apellidos")]
        public string Apellidos { get; set; }

        [XmlElement("carrera")]
        public string Carrera { get; set; }

        [XmlElement("correo")]
        public string Correo { get; set; }

        // XmlIgnore evita guardar esta propiedad en el XML porque es un dato calculado
        // a partir de Nombres y Apellidos
        [XmlIgnore]
        public string NombreCompleto
        {
            get { return (Nombres + " " + Apellidos).Trim(); }
        }

        // XmlSerializer necesita un constructor público sin parámetros
        public Estudiante()
        {
            Carne = string.Empty;
            Nombres = string.Empty;
            Apellidos = string.Empty;
            Carrera = string.Empty;
            Correo = string.Empty;
        }
        //Este constructor permite crear un estudiante con todos sus datos
        public Estudiante(string carne, string nombres, string apellidos,
                          string carrera, string correo)
        {
            Carne = (carne ?? string.Empty).Trim();
            Nombres = (nombres ?? string.Empty).Trim();
            Apellidos = (apellidos ?? string.Empty).Trim();
            Carrera = (carrera ?? string.Empty).Trim();
            Correo = (correo ?? string.Empty).Trim();
        }
    }
}


