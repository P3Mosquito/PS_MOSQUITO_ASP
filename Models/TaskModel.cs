using Google.Cloud.Firestore;

namespace ps_mosquito_asp.Models
{
    public class TaskModel
    {
		//public int? id { get; set; }
		[FirestoreProperty("CantidadMax")]
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
