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
        FirestoreDb db;
        public AccountController()
        {
            {
                auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyBd0_dWTNOTQOA4vxbao9kWX6yEUWPhmuk"));
                string projectId = "mosquitobd-202b0";
                string jsonPath = @"C:\\Users\\brianlml\\Desktop\\ps_mosquito_asp\\mosquitobd-202b0-firebase-adminsdk-7c4jx-8814777652.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
                FirestoreDb db = FirestoreDb.Create(projectId);
            }
        }
        public IActionResult Index()
        {
            return View();
        }

        //[Authorize]
        [Authorize(Roles = "Administrador,R")]
        public IActionResult Register()
        {
            return View();
        }

        // Register POST
        //[Authorize]
        [Authorize(Roles = "Administrador,R")]
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
                    //New
                    var db = FirestoreDb.Create("mosquitobd-202b0");
                    DocumentReference docRef = db.Collection("users").Document(fbAuthLink.User.LocalId);
                    DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                    string role = "R";
                    if (snapshot.Exists)
                    {
                        var userData = snapshot.ToDictionary();
                        role = userData.ContainsKey("role") ? userData["role"].ToString() : role;
                    }

                    //

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, fbAuthLink.User.Email),
                        new Claim(ClaimTypes.Name, fbAuthLink.User.LocalId),
                        new Claim(ClaimTypes.Role.ToString(),role)

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
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
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
	}
}
