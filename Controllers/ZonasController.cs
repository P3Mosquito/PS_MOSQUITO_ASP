using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ps_mosquito_asp.Models;

namespace ps_mosquito_asp.Controllers
{
    public class ZonasController : Controller
    {
        FirestoreDb? _db;
        public ZonasController (){

            string projectId = "mosquitobd-202b0";
            string jsonPath = @"C:\\Users\\brianlml\\Desktop\\ps_mosquito_asp\\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
            FirestoreDb _db = FirestoreDb.Create(projectId);
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult Index()
        {
            var _db = FirestoreDb.Create("mosquitobd-202b0");
            // Accede a la colección "users" en Firestore
            CollectionReference usersRef = _db.Collection("users");

            // Realiza una consulta para obtener los usuarios con "role" igual a "Supervisor"
            Query query = usersRef.WhereEqualTo("role", "Supervisor");
            QuerySnapshot snapshot = query.GetSnapshotAsync().Result;

            // Crea una lista de usuarios que cumplen con el filtro
            var supervisors = new List<UserModel>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    //// Convierte cada documento en un objeto UserModel (ajusta la clase UserModel según tus necesidades)
                    //var user = document.ConvertTo<UserModel>();
                    //supervisors.Add(user);
                    // Obtiene los campos del documento Firestore
                    var data = document.ToDictionary();

                    // Crea un nuevo objeto UserModel y asigna los valores manualmente
                    var user = new UserModel
                    {
                        id = document.Id,
                        name = data.ContainsKey("name") ? data["name"].ToString() : null,
                        // Ajusta el nombre del campo según corresponda                                                 
                        // Asigna otros campos aquí
                        lastname = data.ContainsKey("lastname") ? data["lastname"].ToString() : null,
                        ci = data.ContainsKey("ci") ? data["ci"].ToString() : null,
                        role = data.ContainsKey("role") ? data["role"].ToString() : null,
                        position = data.ContainsKey("position") ? data["position"].ToString() : null,
                        email = data.ContainsKey("email") ? data["email"].ToString() : null,
                        password = data.ContainsKey("password") ? data["password"].ToString() : null
                    };

                    supervisors.Add(user);
                }
            }

            // Pasa la lista de supervisores a la vista
            //ViewBag.Supervisors = new SelectList(supervisors,"name");

            return View(supervisors);
        }


    }
}
