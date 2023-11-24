namespace ps_mosquito_asp.Models
{
    public class TaskListModel
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string IDtarea { get; set; } // Directamente un string que contiene el ID de la tarea
        public string Coordenadas { get; set; }
        public List<string> ImagenPaths { get; set; }
        public string Tarea { get; set; }
    }

}
