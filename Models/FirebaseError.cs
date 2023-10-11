using Newtonsoft.Json;

namespace ps_mosquito_asp.Models
{
    public class FirebaseError
    {
        public Error error { get; set; }
    }
    public class Error
    {
        public int code { get; set; }
        public string? message { get; set; }
        public List<Error>? errors { get; set; }
    }
    //var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
    //firebaseEx.error.message
}
