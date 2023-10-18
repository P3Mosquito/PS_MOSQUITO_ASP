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

            return View(supervisors);
        }

        [HttpPost]
        public ActionResult AssignTasks(string SupervisorId, int CantidadTareas)
        {
            if (SupervisorId != null)
            {
                // Realiza la lógica para asignar las tareas al supervisor seleccionado
                var db = FirestoreDb.Create("mosquitobd-202b0");

                // Genera una referencia a la colección "tareas" en Firestore
                CollectionReference tasksRef = db.Collection("task");

                // Itera para agregar la cantidad de tareas especificada
                for (int i = 0; i < CantidadTareas; i++)
                {
                    // Crea un nuevo documento de tarea con los datos necesarios
                    var newTask = new
                    {
                        Nombre = "Tarea " + (i + 1),
                        SupervisorId = SupervisorId,
                        // Otros campos de la tarea si los tienes
                    };

                    // Agrega la tarea a Firestore
                    DocumentReference addedTaskRef = tasksRef.AddAsync(newTask).Result;
                }

                return View();
            }
            else
            {
                ModelState.AddModelError("SupervisorId", "Debes seleccionar un supervisor.");
            }

            return View();
        }
    }
}
