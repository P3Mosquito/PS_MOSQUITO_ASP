using Firebase.Auth;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using ps_mosquito_asp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ps_mosquito_asp.Controllers
{
    public class AccountController : Controller
    {
        FirebaseAuthProvider auth;
        //FirestoreDb? db;
        public AccountController()
        {
            {
                auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyBd0_dWTNOTQOA4vxbao9kWX6yEUWPhmuk"));
                //string projectId = "mosquitobd-202b0";
                string jsonPath = @"C:\\Users\\brianlml\\Desktop\\ps_mosquito_asp\\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
                //FirestoreDb db = FirestoreDb.Create(projectId);
            }
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Administrador,R")]
        public IActionResult Register()
        {
            return View();
        }

        // Register POST
        [Authorize(Roles = "Administrador,R")]
        [HttpPost]
        public async Task<IActionResult> Register(UserModel loginModel)
        {
            // Verifica si el modelo cumple con todas las validaciones
            if (!ModelState.IsValid)
            {
                // Si no es válido, regresa a la vista con los errores de validación
                return View(loginModel);
            }
            string namePart = loginModel.name!.Substring(0, 3).ToLower();
            string ciPart = new String(loginModel.ci!.Where(Char.IsDigit).Take(3).ToArray());
            string generatedPassword = namePart + ciPart;
            try
            {
                // Crear el usuario en Firebase Authentication
                var authResult = await auth.CreateUserWithEmailAndPasswordAsync(loginModel.email, generatedPassword);
                // Loguear al nuevo usuario
                //var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(loginModel.email, loginModel.password);
                //string token = fbAuthLink.FirebaseToken;
                // Guardar los datos adicionales en Firestore
                //if (token != null)
                //{
                    // Crear un objeto para los datos adicionales
                    var additionalDataDict = new Dictionary<string, object>
                    {
                        { "name", loginModel.name! },
                        { "lastname", loginModel.lastname! },
                        { "ci", loginModel.ci! },
                        { "role", loginModel.role! },
                        { "position", loginModel.position! },
                        { "email", loginModel.email! },
                        //{ "password",loginModel.password = generatedPassword}
                        { "password", generatedPassword }
                    };
                    // Obtiene la instancia de Firestore
                    var db = FirestoreDb.Create("mosquitobd-202b0");
                    // Agregar los datos adicionales a Firestore
                    var userRef = db.Collection("users").Document(authResult.User.LocalId);
                    await userRef.SetAsync(additionalDataDict);
                    // Guardar el token en una variable de sesión
                    //HttpContext.Session.SetString("_UserToken", token);
                    return RedirectToAction("Index");
                //}
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                //ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                string errorMessage = firebaseEx?.error.message switch
                {
                    "EMAIL_NOT_FOUND" => "El correo electrónico no se encontró.",
                    "INVALID_PASSWORD" => "La contraseña no es válida.",
                    "USER_DISABLED" => "La cuenta de usuario ha sido deshabilitada.",
                    "EMAIL_EXISTS" => "El correo ya existe.",
                    // Añade más códigos y mensajes según sea necesario
                    _ => "Ocurrió un error desconocido." // Mensaje genérico para errores no mapeados
                };

                ModelState.AddModelError(String.Empty, errorMessage);
                return View(loginModel);
            }
            //return View();
        }
        //View
        public IActionResult Login()
        {
            return View();
        }
        
        //Login POST
        [HttpPost]
        public async Task<IActionResult> Login(UserModel loginModel)
        {
            try
            {
                //log in an existing user
                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(loginModel.email, loginModel.password);
                string token = fbAuthLink.FirebaseToken;
                //save the token to a session variable
                if (token != null)
                {
                    HttpContext.Session.SetString("_UserToken", token);
                    var db = FirestoreDb.Create("mosquitobd-202b0");
                    DocumentReference docRef = db.Collection("users").Document(fbAuthLink.User.LocalId);
                    DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                    string? role = "R";
                    if (snapshot.Exists)
                    {
                        var userData = snapshot.ToDictionary();
                        role = userData.ContainsKey("role") ? userData["role"].ToString() : role;
                    }
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, fbAuthLink.User.Email),
                        new Claim(ClaimTypes.Name, fbAuthLink.User.LocalId),
                        new Claim(ClaimTypes.Role.ToString(),role!)

                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true, // La cookie persistirá incluso después de cerrar el navegador
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(15) // Tiempo de expiración de la cookie
                    };
                    if (role == "Administrador" || role == "R")
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                        return RedirectToAction("Register", "Account");
                    }
                    else
                    {
                        ViewData["Error"] = "No puede iniciar sesion con una cuenta de Supervisor.";
                        return View(loginModel);
                    }
                }
            }
            catch (FirebaseAuthException ex)
            {
                // Deserializar la respuesta de error para obtener el código de error específico.
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);

                // Mapeo del código de error de Firebase al mensaje de error del modelo.
                string userErrorMessage = firebaseEx?.error.message switch
                {
                    "INVALID_LOGIN_CREDENTIALS" => "Las credenciales no son válidas. Verifique su email y contraseña.",
                    "INVALID_PASSWORD" => "La contraseña es incorrecta. Inténtelo de nuevo o restablezca su contraseña.",
                    _ => "Las credenciales no son válidas."
                };

                ModelState.AddModelError(string.Empty, userErrorMessage);
                return View(loginModel);
            }

            return View();
        }
		//Logout
		public async Task<IActionResult> LogOut()
		{
			HttpContext.Session.Remove("_UserToken");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //return RedirectToAction("Login");
            return RedirectToAction("Index", "Home");
        }

        //Add view for list of users
        [Authorize(Roles = "Administrador,R")]
        public async Task<IActionResult> Users()
        {
            var db = FirestoreDb.Create("mosquitobd-202b0");
            var usersRef = db.Collection("users");
            var query = usersRef.OrderBy("name");
            var querySnapshot = await query.GetSnapshotAsync();
            var users = new List<UserModel>();

            foreach (var document in querySnapshot.Documents)
            {
                // Crea un diccionario a partir del documento
                Dictionary<string, object> dictionary = document.ToDictionary();

                // Crea una nueva instancia de UserModel y asigna los valores manualmente
                var user = new UserModel
                {
                    id = document.Id,
                    name = dictionary.ContainsKey("name") ? dictionary["name"] as string : null,
                    lastname = dictionary.ContainsKey("lastname") ? dictionary["lastname"] as string : null,
                    ci = dictionary.ContainsKey("ci") ? dictionary["ci"] as string : null,
                    role = dictionary.ContainsKey("role") ? dictionary["role"] as string : null,
                    position = dictionary.ContainsKey("position") ? dictionary["position"] as string : null,
                    email = dictionary.ContainsKey("email") ? dictionary["email"] as string : null,
                    password = dictionary.ContainsKey("password") ? dictionary["password"] as string : null,
                    // Asegúrate de añadir todas las propiedades necesarias aquí.
                };

                users.Add(user);
            }

            return View(users);
        }

        [Authorize(Roles = "Administrador,R")]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var db = FirestoreDb.Create("mosquitobd-202b0");
            DocumentReference docRef = db.Collection("users").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return NotFound();
            }

            var userData = snapshot.ToDictionary();
            var user = new UserModel
            {
                id = id,
                name = userData.ContainsKey("name") ? userData["name"].ToString() : "",
                lastname = userData.ContainsKey("lastname") ? userData["lastname"].ToString() : "",
                ci = userData.ContainsKey("ci") ? userData["ci"].ToString() : "",
                role = userData.ContainsKey("role") ? userData["role"].ToString() : "",
                position = userData.ContainsKey("position") ? userData["position"].ToString() : "",
                email = userData.ContainsKey("email") ? userData["email"].ToString() : "",
                // No se incluye la contraseña por cuestiones de seguridad.
            };

            return View(user);
        }


        // POST: Account/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,R")]
        public async Task<IActionResult> EditUser(string id, UserModel userModel)
        {
            if (id != userModel.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var db = FirestoreDb.Create("mosquitobd-202b0");
                    DocumentReference docRef = db.Collection("users").Document(id);

                    // Convertir UserModel a Dictionary para actualizar el documento en Firestore
                    var updates = new Dictionary<string, object>
            {
                { "name", userModel.name },
                { "lastname", userModel.lastname },
                { "ci", userModel.ci },
                { "role", userModel.role },
                { "position", userModel.position },
                { "email", userModel.email },
                // No actualizar la contraseña aquí por razones de seguridad
            };

                    await docRef.UpdateAsync(updates);
                }
                catch (Exception ex)
                {
                    // Manejar cualquier error que pueda ocurrir
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el usuario.");
                    return View(userModel);
                }

                return RedirectToAction(nameof(Users)); // Redirigir a la lista de usuarios
            }

            return View(userModel);
        }

        // GET: Account/DeleteUser/5
        [Authorize(Roles = "Administrador,R")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var db = FirestoreDb.Create("mosquitobd-202b0");
            DocumentReference docRef = db.Collection("users").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return NotFound();
            }

            //UserModel user = snapshot.ConvertTo<UserModel>();

            var userData = snapshot.ToDictionary();
            var user = new UserModel
            {
                id = id,
                name = userData.ContainsKey("name") ? userData["name"].ToString() : "",
                lastname = userData.ContainsKey("lastname") ? userData["lastname"].ToString() : "",
                ci = userData.ContainsKey("ci") ? userData["ci"].ToString() : "",
                role = userData.ContainsKey("role") ? userData["role"].ToString() : "",
                position = userData.ContainsKey("position") ? userData["position"].ToString() : "",
                email = userData.ContainsKey("email") ? userData["email"].ToString() : "",
                // No se incluye la contraseña por cuestiones de seguridad.
            };

            // Consider whether you want to show a confirmation page or not
            // If you don't want to show a confirmation page, you can redirect to a direct deletion
            //return await DeleteConfirmed(id);

            return View(user);
        }

        // POST: Account/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,R")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var db = FirestoreDb.Create("mosquitobd-202b0");
                DocumentReference docRef = db.Collection("users").Document(id);
                await docRef.DeleteAsync();

                // If you also want to delete the authentication record from Firebase Authentication,
                // you would use Firebase Admin SDK to delete the user by uid here.
                // Note that you would need to handle the Firebase Authentication separately.

                return RedirectToAction(nameof(Users)); // Redirect to the list of users
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur
                ModelState.AddModelError("", "Hubo un error al eliminar el usuario.");
                return View();
            }
        }

    }
}
