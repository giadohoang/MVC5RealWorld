using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC5RealWorld.Models.DB;
using MVC5RealWorld.Models.EntityManager;

namespace MVC5RealWorld.Security
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly string[] userAssignedRoles;
        public AuthorizeRoleAttribute(params string[] roles) {
            this.userAssignedRoles = roles;
        }

        //we override AuthorizeCore method
        //is the entry point for the authentication check.
        //THis is where we check the roles assigned for certain users and returns
        //the result specifying whether or not the user is allowed to access a page

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorize = false;
            using (DemoDBEntities db = new DemoDBEntities()) {
                UserManager UM = new UserManager();
                foreach (var roles in userAssignedRoles) {
                    authorize = UM.IsUserInRole(httpContext.User.Identity.Name, roles);
                    if (authorize)
                        return authorize;
                }
            }
            return authorize;
        }


        //Override HandleUnauthorizedRequest method helps
        //redirect un-authorized users to a certain page ~ the "UnAuthorized" page
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Home/UnAuthorized");
        }

    }
}