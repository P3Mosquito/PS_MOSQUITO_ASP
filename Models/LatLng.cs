using Google.Cloud.Firestore;

namespace ps_mosquito_asp.Models
{
    public class LatLng
    {
		[FirestoreProperty("Lat")]
		public double Lat { get; set; }
		[FirestoreProperty("Lng")]
		public double Lng { get; set; }
    }
}
