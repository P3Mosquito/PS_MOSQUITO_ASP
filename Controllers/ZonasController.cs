﻿using Google.Cloud.Firestore;
using Google.Type;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using ps_mosquito_asp.Models;
using LatLng = ps_mosquito_asp.Models.LatLng;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace ps_mosquito_asp.Controllers
{
    [Authorize(Roles = "Administrador, R")]
    public class ZonasController : Controller
    {
        //FirestoreDb? _db;
        public ZonasController (){

            //string projectId = "mosquitobd-202b0";
            //string jsonPath = @"C:\Users\HP\Desktop\Mosquito\PS_MOSQUITO_ASP\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            //string jsonPath = @"C:\\Users\\brianlml\\Desktop\\ps_mosquito_asp\\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jsonFileName = "mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            string jsonPath = Path.Combine(baseDirectory, jsonFileName);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
            //FirestoreDb _db = FirestoreDb.Create(projectId);
        }

        public ActionResult Index()
        {
            var _db = FirestoreDb.Create("mosquitobd-202b0");

            // Accede a la colección "users" en Firestore
            CollectionReference usersRef = _db.Collection("users");
            // Realiza una consulta para obtener los usuarios con "role" igual a "Supervisor"
            Query query = usersRef.WhereEqualTo("role", "Supervisor");
            QuerySnapshot userSnapshot = query.GetSnapshotAsync().Result;

            // Crea una lista de usuarios que cumplen con el filtro
            var supervisors = new List<UserModel>();

            foreach (DocumentSnapshot document in userSnapshot.Documents)
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

            // Accede a la colección "colors" en Firestore
            CollectionReference colorsRef = _db.Collection("colors");
            Query colorsQuery = colorsRef;
            QuerySnapshot colorsSnapshot = colorsQuery.GetSnapshotAsync().Result;

            // Crea una lista de nombres de colores
            var colorNamesAndIds = new List<Tuple<string, string>>();

            foreach (DocumentSnapshot colorDocument in colorsSnapshot.Documents)
            {
                if (colorDocument.Exists)
                {
                    var colorData = colorDocument.ToDictionary();
                    var colorName = colorData.ContainsKey("name") ? colorData["name"].ToString() : null;
                    var colorId = colorDocument.Id; // Obtener el ID del documento
                    colorNamesAndIds.Add(new Tuple<string, string>(colorName, colorId));
                }
            }

            // Almacena la lista de nombres de colores en un ViewBag
            ViewBag.ColorNamesAndIds = colorNamesAndIds;

            return View(supervisors);
        }


        [HttpPost]
        public async Task<ActionResult> AssignTasks(string SupervisorId, int cantidadMax, string cityName, string polygonCoords, string tipoColorId)
        {
            if (SupervisorId != null)
            {
                List<LatLng>? coordinates = JsonConvert.DeserializeObject<List<LatLng>>(polygonCoords);
                var coordinatesDictList = coordinates?.Select(coord => new Dictionary<string, double>
        {
            {"Lat", coord.Lat},
            {"Lng", coord.Lng}
        }).ToList();

                var db = FirestoreDb.Create("mosquitobd-202b0");
                CollectionReference tasksRef = db.Collection("task");

                // Crea un nuevo objeto de tarea con los campos necesarios
                var newTask = new
                {
                    Estado = "Pendiente",
                    CantidadTareasRealizadas = 0,
                    TipoColor = tipoColorId,
                    SupervisorId = SupervisorId,
                    CantidadMax = cantidadMax,
                    Zona = cityName,
                    coordenadas = coordinatesDictList
                };

                // Agrega la tarea a Firestore y obtén el ID de la tarea creada
                DocumentReference addedTaskRef = await tasksRef.AddAsync(newTask);
                string taskId = addedTaskRef.Id;

                // Actualiza el documento con el ID de la tarea
                await addedTaskRef.UpdateAsync("IDtarea", taskId);

                return View();
            }
            else
            {
                ModelState.AddModelError("SupervisorId", "Debes seleccionar un supervisor.");
                return View();
            }
        }
    }
}
