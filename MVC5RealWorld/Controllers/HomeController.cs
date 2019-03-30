using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC5RealWorld.Security;
using MVC5RealWorld.Models.EntityManager;
using MVC5RealWorld.Models.ViewModel;
//this controller will be controller for default page
//we'll create "Index" and "Welcome" view for this controller
namespace MVC5RealWorld.Controllers
{
    public class HomeController : Controller
    {
        //Index method serves as default redirect page

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        //Welcome method will be the page where we redirect users after
        //they have authenticated successfully
        //We decorated with the [Authorize] attribute so that this method will only be
        //available for the looged-in or authenticated users

        [Authorize]
        public ActionResult Welcome() {
            return View();
        }

        //We decorated the AdminOnly action with our custom authorization filter by passing the value
        //of "Admin" as the role name.
        //This means that only allow admin users have access to the AdminOnly page.
        //To support multiple role access, just add another role name by separating it
        //with a comma:
        //[AuthorizaRoles("Admin","Manager")]
        //Value of "Admin" and "Manager" should match the role names from your database.
        [AuthorizeRole("Admin")]
        public ActionResult AdminOnly() {
            return View();
        }

        public ActionResult UnAuthorized() {
            return View();
        }

        //This code is decorated with the customer AuthorizeRole attribute that we
        //have created in before, so that only admin user can invoke that method.
        //What it does is it calls the GetUserDataView() method
        //by passing in the loginName as the parameter,
        //and then return the result in the Partial View

        [AuthorizeRole("Admin")]
        public ActionResult ManageUserPartial( string status = "") {
            if (User.Identity.IsAuthenticated) {
                string loginName = User.Identity.Name;
                UserManager UM = new UserManager();
                UserDataView UDV = UM.GetUserDataView(loginName);
                //we are creating a message string based on a certain operation and store the result in
                //ViewBag.
                //This is to let user see if a certain operation succeeds.
                string message = string.Empty;
                if (status.Equals("update")) message = "Update Successful";
                else if (status.Equals("delete")) message = "Delete Successful";

                ViewBag.Message = message;
                return PartialView(UDV);
            }
            return RedirectToAction("Index", "Home");
        }

        //This method responsible for collecting data that is sent from the View for update.
        //It then calls the method UpdateUserAccount() and pass the UserProfileView model view
        //as the parameter. The UpdateUserData() method will be called through an AJAX request.
        //Note: We can also use Web API to send and receive data via AJAX
        [AuthorizeRole("Admin")]
        public ActionResult UpdateUserData(int userID, string loginName, string password,
                                            string firstName, string lastName, string gender, int roleID = 0)
        {
            UserProfileView UPV = new UserProfileView();
            UPV.SYSUserID = userID;
            UPV.LoginName = loginName;
            UPV.Password = password;
            UPV.FirstName = firstName;
            UPV.LastName = lastName;
            UPV.Gender = gender;

            if (roleID > 0)
                UPV.LOOKUPRoleID = roleID;

                UserManager UM = new UserManager();
                UM.UpdateUserAccount(UPV);

            return Json(new { success = true });

        }
    }

    
}