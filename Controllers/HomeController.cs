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
            string jsonPath = @"C:\\Users\\brianlml\\Desktop\\ps_mosquito_asp\\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
			_db = FirestoreDb.Create(projectId);
        }

		public IActionResult Index()
		{
			var _db = FirestoreDb.Create("mosquitobd-202b0");
			CollectionReference taskRef = _db.Collection("task_list");
			Query query = taskRef;
			QuerySnapshot snapshot = query.GetSnapshotAsync().Result;

			List<TaskListHomeModel> tasks = new List<TaskListHomeModel>();

			foreach (DocumentSnapshot documentSnapshot in snapshot)
			{
				if (documentSnapshot.Exists)
				{
					// Obtén los datos como un diccionario
					Dictionary<string, object> data = documentSnapshot.ToDictionary();
					// Crea un objeto TaskModel y asigna manualmente las propiedades

					TaskListHomeModel task = new TaskListHomeModel
					{
                        //supervisorId = data.ContainsKey("SupervisorId") ? data["SupervisorId"].ToString() : null,
                        Id = data.ContainsKey("Id") ? data["Id"].ToString() : null,
                        Nombre = data.ContainsKey("Nombre") ? data["Nombre"].ToString() : null,
                        Descripcion = data.ContainsKey("descripcion") ? data["descripcion"].ToString() : null,
                        //Zona = data.ContainsKey("Zona") ? data["Zona"].ToString() : null,
                        //Coordenadas = data.ContainsKey("coordenadas") ? data["coordenadas"].ToString() : null,
                        ImagenPaths = data.ContainsKey("ImagenPaths") ? (List<string>)data["ImagenPaths"] : null,
                        Tarea = data.ContainsKey("tarea") ? data["tarea"].ToString() : null                     
                    };

					if (data.ContainsKey("Id Tarea") && data["Id Tarea"] is Dictionary<string, object> idTarea)
					{
						task.Zona = idTarea.ContainsKey("Zona") ? idTarea["Zona"].ToString() : null;
					}
                    task.CoordenadasList = new List<LatLng>();
                    if (data.ContainsKey("coordenadas") && data["coordenadas"] is string coordenadasString)
					{
						// Utiliza expresiones regulares para extraer las coordenadas
						var match = Regex.Match(coordenadasString, @"Latitud:\s*(-?\d+\.\d+),\s*Longitud:\s*(-?\d+\.\d+)");

                        // Verifica si se encontraron coincidencias
                        if (match.Success)
                        {
                            // Obtener los valores de latitud y longitud como cadenas
                            string latitudStr = match.Groups[1].Value;
                            string longitudStr = match.Groups[2].Value;

                            // Utilizar la cultura invariante para asegurar el formato correcto sin depender de la configuración regional
                            CultureInfo culture = CultureInfo.InvariantCulture;

                            // Convertir las cadenas a double
                            if (double.TryParse(latitudStr, NumberStyles.Float, culture, out double latitud) &&
                                double.TryParse(longitudStr, NumberStyles.Float, culture, out double longitud))
                            {
                                task.CoordenadasList.Add(new LatLng { Lat = latitud, Lng = longitud });
                            }
                            else
                            {
                                task.CoordenadasList.Add(new LatLng { Lat = 0.0, Lng = 0.0 });
                            }
                        }
                    }
                    tasks.Add(task);
				}
			}

			return View(tasks);
		}

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