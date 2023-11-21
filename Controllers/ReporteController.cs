using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using ps_mosquito_asp.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ps_mosquito_asp.Controllers
{
    public class ReporteController : Controller
    {
        private readonly FirestoreDb _db;

        public ReporteController()
        {
            string projectId = "mosquitobd-202b0";
            //string jsonPath = @"C:\Users\HP\Desktop\Mosquito\PS_MOSQUITO_ASP\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            //string jsonPath = @"C:\\Users\\brianlml\\Desktop\\ps_mosquito_asp\\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jsonFileName = "mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            string jsonPath = Path.Combine(baseDirectory, jsonFileName);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
            _db = FirestoreDb.Create(projectId);
        }
        public async Task<IActionResult> Index()
        {
            CollectionReference usersRef = _db.Collection("users");
            Query query = usersRef.WhereEqualTo("role", "Supervisor");
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            var supervisors = new List<UserModelReport>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var data = document.ToDictionary();
                    var user = new UserModelReport
                    {
                        id = document.Id,
                        name = data.ContainsKey("name") ? data["name"].ToString() : null,
                        lastname = data.ContainsKey("lastname") ? data["lastname"].ToString() : null,
                        // ... otros campos
                    };

                    supervisors.Add(user);
                }
            }

            var allTasks = new Dictionary<string, List<TaskInfo>>();
            CollectionReference taskRef = _db.Collection("task");

            foreach (var supervisor in supervisors)
            {
                var taskQuery = taskRef.WhereEqualTo("SupervisorId", supervisor.id);
                var taskSnapshot = await taskQuery.GetSnapshotAsync();

                var tasksForSupervisor = taskSnapshot.Documents
                                                    .Where(doc => doc.Exists)
                                                    .Select(doc =>
                                                    {
                                                        var data = doc.ToDictionary();
                                                        var coordObj = data.ContainsKey("coordenadas") ? data["coordenadas"] : null;

                                                        var coordList = new List<Dictionary<string, double>>();

                                                        var colorId = data.ContainsKey("TipoColor") ? data["TipoColor"].ToString() : null;

                                                        var colorRef = _db.Collection("colors").Document(colorId);
                                                        var colorSnapshot = colorRef.GetSnapshotAsync().Result;
                                                        var colorValue = colorSnapshot.Exists ? colorSnapshot.GetValue<string>("color") : "#FFFFFF"; // Usar un valor por defecto si no hay color

                                                        if (coordObj is IEnumerable<object> coords)
                                                        {
                                                            foreach (var item in coords)
                                                            {
                                                                if (item is IDictionary<string, object> coordDict)
                                                                {
                                                                    coordList.Add(new Dictionary<string, double>
                                                                    {
                                                                        ["Lat"] = Convert.ToDouble(coordDict["Lat"]),
                                                                        ["Lng"] = Convert.ToDouble(coordDict["Lng"])
                                                                    });
                                                                }
                                                            }
                                                        }
                                                        // Reemplaza comillas dobles por comillas latinas dobles en la zona
                                                        var zona = data.ContainsKey("Zona") ? data["Zona"].ToString() : null;
                                                        if (!string.IsNullOrEmpty(zona))
                                                        {
                                                            zona = zona.Replace("\"", "¨");
                                                        }

                                                        return new TaskInfo
                                                        {
                                                            Id = doc.Id,
                                                            Zona = zona,
                                                            Estado = data.ContainsKey("Estado") ? data["Estado"].ToString() : null,
                                                            Coordenadas = coordList.Count > 0 ? coordList : null,
                                                            ColorValue = colorValue,
                                                            // ... otros campos si los necesitas
                                                        };

                                                    }).ToList();

                allTasks[supervisor.id] = tasksForSupervisor;
            }

            // Envía ambos, supervisores y tareas, al View
            ViewBag.AllTasks = Newtonsoft.Json.JsonConvert.SerializeObject(allTasks);


            //task-list
            CollectionReference taskListRef = _db.Collection("task_list");
            var taskListSnapshot = await taskListRef.GetSnapshotAsync();

            var taskList = new List<TaskListModel>();

            foreach (var doc in taskListSnapshot.Documents)
            {
                if (doc.Exists)
                {
                    var data = doc.ToDictionary();
                    var taskListModel = new TaskListModel
                    {
                        Id = doc.Id,
                        Nombre = data["Nombre"]?.ToString(),
                        Descripcion = data["descripcion"]?.ToString(),
                        Coordenadas = data["coordenadas"]?.ToString(),
                        Tarea = data["tarea"]?.ToString(),
                        ImagenPaths = data.ContainsKey("imagen_paths") ? ((IEnumerable)data["imagen_paths"]).Cast<string>().ToList() : null,
                        IDtarea = data["Id Tarea"] is Dictionary<string, object> idTareaDict && idTareaDict.ContainsKey("IDtarea") ? idTareaDict["IDtarea"].ToString() : string.Empty,
                    };

                    taskList.Add(taskListModel);
                }
            }

            // Aquí se hace el filtrado de taskList basado en los IDtarea de allTasks
            var taskListFiltrada = taskList.Where(task =>
                allTasks.Any(pair => pair.Value.Any(t => t.Id == task.IDtarea))).ToList();

            // Pasar la taskList filtrada al ViewBag
            ViewBag.TaskList = Newtonsoft.Json.JsonConvert.SerializeObject(taskListFiltrada);

            return View(supervisors);
        }
        public async Task<IActionResult> Details(string id)
        {
            DocumentReference docRef = _db.Collection("task_list").Document(id);
            DocumentSnapshot docSnapshot = await docRef.GetSnapshotAsync();

            if (!docSnapshot.Exists)
            {
                return NotFound();
            }

            // Crear una nueva instancia de tu modelo y asignar los campos manualmente.
            var task = new TaskListModel
            {
                Id = docSnapshot.Id,
                Nombre = docSnapshot.GetValue<string>("Nombre"),
                Descripcion = docSnapshot.GetValue<string>("descripcion"),
                Coordenadas = docSnapshot.GetValue<string>("coordenadas"),
                // Asumiendo que "imagen_paths" es un campo del tipo array en Firestore.
                ImagenPaths = docSnapshot.GetValue<List<string>>("imagen_paths"),
                Tarea = docSnapshot.GetValue<string>("tarea"),
            };
            // Extraer latitud y longitud del string de Coordenadas
            var coordPattern = new System.Text.RegularExpressions.Regex(@"Latitud: (?<lat>-?\d+\.\d+), Longitud: (?<lng>-?\d+\.\d+)");
            var match = coordPattern.Match(task.Coordenadas);

            if (match.Success)
            {
                ViewBag.Latitud = match.Groups["lat"].Value;
                ViewBag.Longitud = match.Groups["lng"].Value;
            }
            else
            {
                ViewBag.Latitud = "0";
                ViewBag.Longitud = "0";
            }

            return View(task);
        }



    }
}
