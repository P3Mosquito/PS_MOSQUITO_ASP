using Firebase.Auth;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using ps_mosquito_asp.Models;

namespace ps_mosquito_asp.Controllers
{
    public class AccountController : Controller
    {
        FirebaseAuthProvider auth;
        FirestoreDb db;
        public AccountController()
        {
            {
                auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyBd0_dWTNOTQOA4vxbao9kWX6yEUWPhmuk"));
                string projectId = "mosquitobd-202b0";
                string jsonPath = @"C:\Users\HP\Desktop\roly3 Mosquito\PS_MOSQUITO_ASP\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
                FirestoreDb db = FirestoreDb.Create(projectId);
            }
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        // Register POST
        [HttpPost]
        public async Task<IActionResult> Register(UserModel loginModel)
        {
            try
            {
                // Crear el usuario en Firebase Authentication
                var authResult = await auth.CreateUserWithEmailAndPasswordAsync(loginModel.email, loginModel.password);

                // Loguear al nuevo usuario
                var fbAuthLink = await auth
                    .SignInWithEmailAndPasswordAsync(loginModel.email, loginModel.password);

                string token = fbAuthLink.FirebaseToken;

                // Guardar los datos adicionales en Firestore
                if (token != null)
                {
                    // Crear un objeto para los datos adicionales
                    var additionalDataDict = new Dictionary<string, object>
                    {
                        { "name", loginModel.name },
                        { "lastname", loginModel.lastname },
                        { "ci", loginModel.ci },
                        { "role", loginModel.role },
                        { "position", loginModel.position },
                        { "email", loginModel.email },
                        { "password", loginModel.password }

                        // Agrega otros campos personalizados aquí si es necesario
                    };
                    // Obtiene la instancia de Firestore
                    var db = FirestoreDb.Create("mosquitobd-202b0");

                    // Agregar los datos adicionales a Firestore
                    var userRef = db.Collection("users").Document(authResult.User.LocalId);
                    await userRef.SetAsync(additionalDataDict);

                    // Guardar el token en una variable de sesión
                    HttpContext.Session.SetString("_UserToken", token);

                    return RedirectToAction("Index");
                }
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                return View(loginModel);
            }

            return View();
        }

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
                var fbAuthLink = await auth
                                .SignInWithEmailAndPasswordAsync(loginModel.email, loginModel.password);
                string token = fbAuthLink.FirebaseToken;
                //save the token to a session variable
                if (token != null)
                {
                    HttpContext.Session.SetString("_UserToken", token);

                    return RedirectToAction("Register","Account");
                   
                }

            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                return View(loginModel);
            }

            return View();
        }

		//Logout
		public IActionResult LogOut()
		{
			HttpContext.Session.Remove("_UserToken");
			return RedirectToAction("Login");
		}
	}
}
