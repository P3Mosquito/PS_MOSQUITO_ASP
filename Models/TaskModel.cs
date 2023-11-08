using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace ps_mosquito_asp.Models
{
    public class TaskModel
    {
		//public int? id { get; set; }
		[FirestoreProperty("CantidadMax")]
        [Range(1, 50, ErrorMessage = "La cantidad máxima debe ser un valor entre 1 y 50.")]
        public double? cantidadMax { get; set; }
		[FirestoreProperty("Estado")]
		public string? status { get; set; }
		[FirestoreProperty("Nombre")]
		public string? name { get; set; }
		[FirestoreProperty("SupervisorId")]
		public string? supervisorId { get; set; }
		[FirestoreProperty("Zona")]
		public string? zone { get; set; }
		[FirestoreProperty("coordenadas")]
		public List<LatLng>? coordenadas { get; set; }
    }
}
