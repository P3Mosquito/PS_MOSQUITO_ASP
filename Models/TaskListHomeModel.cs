namespace ps_mosquito_asp.Models
{
    public class TaskListHomeModel
    {
		
		public string? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? IDtarea { get; set; } // Directamente un string que contiene el ID de la tarea
        public string? Zona { get; set; }
        public List<string>? Coordenadas { get; set; }
        public List<string>? ImagenPaths { get; set; }
        public string? Tarea { get; set; }

		public List<LatLng>? CoordenadasList { get; set; }
	}
}
