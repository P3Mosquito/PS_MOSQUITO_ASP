using Firebase.Auth;

using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using ps_mosquito_asp.Models;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ps_mosquito_asp.Controllers
{
    public class HomeController : Controller
    {
        FirestoreDb _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            string projectId = "mosquitobd-202b0";
            //string jsonPath = @"C:\Users\HP\Desktop\dev-nuevo\PS_MOSQUITO_ASP\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            //string jsonPath = @"C:\Users\HP\Desktop\Mosquito\PS_MOSQUITO_ASP\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            //string jsonPath = @"C:\\Users\\brianlml\\Desktop\\ps_mosquito_asp\\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jsonFileName = "mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            string jsonPath = Path.Combine(baseDirectory, jsonFileName);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
			_db = FirestoreDb.Create(projectId);
        }
        #region OK
        public IActionResult Index()
        {
            var _db = FirestoreDb.Create("mosquitobd-202b0");
            CollectionReference taskRef = _db.Collection("task");
            Query query = taskRef;
            QuerySnapshot snapshot = query.GetSnapshotAsync().Result;

            List<TaskModel> tasks = new List<TaskModel>();

            foreach (DocumentSnapshot documentSnapshot in snapshot)
            {
                if (documentSnapshot.Exists)
                {
                    // Obtén los datos como un diccionario
                    Dictionary<string, object> data = documentSnapshot.ToDictionary();

                    // Crea un objeto TaskModel y asigna manualmente las propiedades
                    TaskModel task = new TaskModel
                    {
                        cantidadMax = data.ContainsKey("CantidadMax") ? Convert.ToDouble(data["CantidadMax"]) : null,
                        status = data.ContainsKey("Estado") ? (string)data["Estado"] : null,
                        name = data.ContainsKey("Nombre") ? data["Nombre"].ToString() : null,
                        supervisorId = data.ContainsKey("SupervisorId") ? data["SupervisorId"].ToString() : null,
                        zone = data.ContainsKey("Zona") ? data["Zona"].ToString() : null,
                        coordenadas = new List<LatLng>()
                    };

                    // Maneja la lista de coordenadas
                    if (data.ContainsKey("coordenadas") && data["coordenadas"] is List<object> coordenadas)
                    {
                        foreach (var coordenadaData in coordenadas)
                        {
                            if (coordenadaData is Dictionary<string, object> coordenada)
                            {
                                task.coordenadas.Add(new LatLng
                                {
                                    Lat = coordenada.ContainsKey("Lat") ? Convert.ToDouble(coordenada["Lat"]) : 0.0,
                                    Lng = coordenada.ContainsKey("Lng") ? Convert.ToDouble(coordenada["Lng"]) : 0.0
                                });
                            }
                        }
                    }

                    tasks.Add(task);
                }
            }

            return View(tasks);
        }
        #endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}