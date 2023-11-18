using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ps_mosquito_asp.Models;
using System.Data;

namespace ps_mosquito_asp.Controllers
{
    [Authorize(Roles = "Administrador, R")]
    public class ColoresController : Controller
    {
        public ColoresController()
        {
            //string jsonPath = @"C:\\Users\\brianlml\\Desktop\\ps_mosquito_asp\\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jsonFileName = "mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
            string jsonPath = Path.Combine(baseDirectory, jsonFileName);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
        }
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var db = FirestoreDb.Create("mosquitobd-202b0");
            var colors = new List<ColorModel>();
            CollectionReference colorsRef = db.Collection("colors");
            QuerySnapshot snapshot = await colorsRef.GetSnapshotAsync();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var data = document.ToDictionary();
                    var color = new ColorModel
                    {
                        id = document.Id,
                        color = data.ContainsKey("color") ? data["color"].ToString() : null,
                        name = data.ContainsKey("name") ? data["name"].ToString() : null,
                        description = data.ContainsKey("description") ? data["description"].ToString() : null,
                    };
                    colors.Add(color);
                }
            }
            return View(colors);
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ColorModel colorPost)
        {
            if (ModelState.IsValid)
            {
                var db = FirestoreDb.Create("mosquitobd-202b0");
                CollectionReference colorsRef = db.Collection("colors");
                Dictionary<string, object> data1 = new Dictionary<string, object>()
                {
                    {"color", colorPost.color},
                    {"name", colorPost.name},
                    {"description", colorPost.description}
                };
                await colorsRef.AddAsync(data1);
                return RedirectToAction(nameof(Index));
            }
            return View(colorPost);
        }
        public async Task<IActionResult> EditColor(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var db = FirestoreDb.Create("mosquitobd-202b0");
            DocumentReference docRef = db.Collection("colors").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                return NotFound();
            }
                var data = snapshot.ToDictionary();
                var color = new ColorModel
                {
                    id = id,
                    color = data.ContainsKey("color") ? data["color"].ToString() : null,
                    name = data.ContainsKey("name") ? data["name"].ToString() : null,
                    description = data.ContainsKey("description") ? data["description"].ToString() : null,
                };
                return View(color);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditColor(string id, ColorModel colorPost)
        {
            if (id != colorPost.id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var db = FirestoreDb.Create("mosquitobd-202b0");
                    DocumentReference docRef = db.Collection("colors").Document(colorPost.id);
                    Dictionary<string, object> updates = new Dictionary<string, object>()
                {
                    {"color", colorPost.color},
                    {"name", colorPost.name},
                    {"description", colorPost.description}
                };
                    await docRef.UpdateAsync(updates);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el color.");
                    return View(colorPost);
                }
               
                return RedirectToAction(nameof(Index));
            }
            return View(colorPost);
        }

        //GET - DELETE
        public async Task<IActionResult> DeleteColor(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var db = FirestoreDb.Create("mosquitobd-202b0");
            DocumentReference docRef = db.Collection("colors").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                var color = new ColorModel
                {
                    id = id,
                    color = data.ContainsKey("color") ? data["color"].ToString() : null,
                    name = data.ContainsKey("name") ? data["name"].ToString() : null,
                    description = data.ContainsKey("description") ? data["description"].ToString() : null,
                };
                return await DeleteConfirmed(id);
                
            }
            return NotFound();
        }
        //POST - DELETE
        [HttpPost, ActionName("DeleteColor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var db = FirestoreDb.Create("mosquitobd-202b0");
                DocumentReference docRef = db.Collection("colors").Document(id);
                await docRef.DeleteAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "Ocurrió un error al eliminar el color.");
                return View();
            }
            
        }

    }
}
