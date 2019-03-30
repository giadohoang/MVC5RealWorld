using System;
using System.Collections.Generic;//this namespace contains interfaces and classes 
//that define generic collections that allow us to create strongly-typed
//collections.
using System.Linq;
using System.Web;
using MVC5RealWorld.Models.ViewModel;
using MVC5RealWorld.Models.DB;
//using System.Collections.Generic;

namespace MVC5RealWorld.Models.EntityManager
{
    public class UserManager
    {

        public void AddUserAccount(UserSignUpView user) {

            using (DemoDBEntities db = new DemoDBEntities()) {

                SYSUser SU = new SYSUser();
                SU.LoginName = user.LoginName;
                SU.PasswordEncryptedText = user.Password;
                SU.RowCreatedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                SU.RowModifiedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                SU.RowCreatedDateTime = DateTime.Now;
                SU.RowModifiedDateTime = DateTime.Now;

                db.SYSUsers.Add(SU);
                db.SaveChanges();

                SYSUserProfile SUP = new SYSUserProfile();
                SUP.SYSUserID = SU.SYSUserID;
                SUP.FirstName = user.FirstName;
                SUP.LastName = user.LastName;
                SUP.Gender = user.Gender;
                SUP.RowCreatedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                SUP.RowModifiedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                SUP.RowCreatedDateTime = DateTime.Now;
                SUP.RowModifiedDateTime = DateTime.Now;

                db.SYSUserProfiles.Add(SUP);
                db.SaveChanges();

                if (user.LOOKUPRoleID > 0) {
                    SYSUserRole SUR = new SYSUserRole();
                    SUR.LOOKUPRoleID = user.LOOKUPRoleID;
                    SUR.SYSUserID = user.SYSUserID;
                    SUR.IsActive = true;
                    SUR.RowCreatedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                    SUR.RowModifiedSYSUserID = user.SYSUserID > 0 ? user.SYSUserID : 1;
                    SUR.RowCreatedDateTime = DateTime.Now;
                    SUR.RowModifiedDateTime = DateTime.Now;

                    db.SYSUserRoles.Add(SUR);
                    db.SaveChanges();
                }

            }
        }

        public bool IsLoginNameExist(string loginName) {
            using (DemoDBEntities db = new DemoDBEntities()) {
                return db.SYSUsers.Where(o => o.LoginName.Equals(loginName)).Any();
            }
        }

        //This method gets the corresponding password from the
        //database for a specific login name using a LINQ query
        public string GetUserPassword(string loginName) {
            using (DemoDBEntities db = new DemoDBEntities()) {
                var user = db.SYSUsers.Where(o => o.LoginName.ToLower().Equals(loginName));
                if (user.Any())
                    return user.FirstOrDefault().PasswordEncryptedText;
                else
                    return string.Empty;
            }
        }

        //Take loginName and roleName as parameters.
        //Check for the eisting records in the user's table
        //and then validate if the corresponding user has roles assigned to it

        public bool IsUserInRole(string loginName, string roleName) {
            using (DemoDBEntities db = new DemoDBEntities())
            {
                SYSUser SU = db.SYSUsers.Where(o =>
                o.LoginName.ToLower().Equals(loginName))?.FirstOrDefault();

                if (SU != null)
                {
                    var roles = from q in db.SYSUserRoles
                                join r in db.LOOKUPRoles on q.LOOKUPRoleID equals r.LOOKUPRoleID
                                where r.RoleName.Equals(roleName) && q.SYSUserID.Equals(SU.SYSUserID)
                                select r.RoleName;

                    if (roles != null)
                    {
                        return roles.Any();
                    }
                }

                return false;
            }
        }
        
        //The main method there is the GetUserDataView(),
        //and what it does is it gets all user
        //profiles and toles from our database
        //The UserProfile property holds the users data.
        //Finally, the UserRoles and UserGender properties
        //will be used using the editing and updating of the user data.
        //We'll use there values to populate the dropdown
        //lists for roles and gender

        public List<LOOKUPAvailableRole> GetAllRoles() {
            using (DemoDBEntities db = new DemoDBEntities()) {
                var roles = db.LOOKUPRoles.Select(o => new LOOKUPAvailableRole {
                    LOOKUPRoleID = o.LOOKUPRoleID,
                    RoleName = o.RoleName,
                    RoleDescription = o.RoleDescription
                }).ToList();

                return roles;
            }
        }

        public int GetUserID(string loginName) {
            using (DemoDBEntities db = new DemoDBEntities()) {
                var user = db.SYSUsers.Where(o => o.LoginName.Equals(loginName));
                if (user.Any()) return user.FirstOrDefault().SYSUserID;
            }
            return 0;
        }



        public List<UserProfileView> GetAllUserProfiles() {
            List<UserProfileView> profiles = new List<UserProfileView>();

            using (DemoDBEntities db = new DemoDBEntities()) {
                UserProfileView UPV;
                var users = db.SYSUsers.ToList();

                foreach (SYSUser u in db.SYSUsers) {
                    UPV = new UserProfileView();
                    UPV.SYSUserID = u.SYSUserID;
                    UPV.LoginName = u.LoginName;
                    UPV.Password = u.PasswordEncryptedText;

                    var SUP = db.SYSUserProfiles.Find(u.SYSUserID);
                    if (SUP != null) {
                        UPV.FirstName = SUP.FirstName;
                        UPV.LastName = SUP.LastName;
                        UPV.Gender = SUP.Gender;
                    }

                    var SUR = db.SYSUserRoles.Where(o => o.SYSUserID.Equals(u.SYSUserID));

                    if (SUR.Any()) {
                        var userRole = SUR.FirstOrDefault();
                        UPV.LOOKUPRoleID = userRole.LOOKUPRoleID;
                        UPV.RoleName = userRole.LOOKUPRole.RoleName;
                        UPV.IsRoleActive = userRole.IsActive;
                    }

                    profiles.Add(UPV);
                }
            }

            return profiles;
        }

        public UserDataView GetUserDataView(string loginName) {
            UserDataView UDV = new UserDataView();
            List<UserProfileView> profiles = GetAllUserProfiles();
            List<LOOKUPAvailableRole> roles = GetAllRoles();

            int? userAssignedRoleID = 0, userID = 0;
            string userGender = string.Empty;

            userID = GetUserID(loginName);
            using (DemoDBEntities db = new DemoDBEntities()) {
                userAssignedRoleID = db.SYSUserRoles.Where(o => o.SYSUserID ==
                userID)?.FirstOrDefault().LOOKUPRoleID;

                userGender = db.SYSUserProfiles.Where(o => o.SYSUserID == userID)?.FirstOrDefault().Gender;
            }

            List<Gender> genders = new List<Gender>();
            genders.Add(new Gender
            {
                Text = "Male",
                Value = "M"
            });

            genders.Add(new Gender
            {

                Text = "Female",
                Value = "F"
            });

            UDV.UserProfile = profiles;
            UDV.UserRoles = new UserRoles {
                SelectedRoleID = userAssignedRoleID, UserRoleList = roles
        };

            UDV.UserGender = new UserGender
            {
                SelectedGender = userGender, Gender = genders
            };

            return UDV;
        

        }

        //This method takes a UserProfileView object as the parameter.
        //This parameter is coming from a stringly-typed View.
        //It first issies a query to the database using the LINQ syntax
        //to get the specific user data by passing the SYSUserID
        //It then updates the SYSUser object with the corresponding data from the UserProfileView object.

        public void UpdateUserAccount(UserProfileView user) {
            using (DemoDBEntities db = new DemoDBEntities()) {
                using (var dbContextTransaction = db.Database.BeginTransaction()) {
                    try
                    {
                        SYSUser SU = db.SYSUsers.Find(user.SYSUserID);
                        SU.LoginName = user.LoginName;
                        SU.PasswordEncryptedText = user.Password;
                        SU.RowCreatedSYSUserID = user.SYSUserID;
                        SU.RowModifiedSYSUserID = user.SYSUserID;
                        SU.RowCreatedDateTime = DateTime.Now;
                        SU.RowModifiedDateTime = DateTime.Now;

                        db.SaveChanges();

                        //This query gets the associated SYSUserProfiles data and then updates the corresponding
                        //values. After that, it then looks for the associated LOOKUPRoleID for a certain user.
                        //If the user does not have role assigned to it, then it adds a 
                        //new record to the database, otherwise just update the table.
                        var userProfile = db.SYSUserProfiles.Where(o => o.SYSUserID == user.SYSUserID);
                        if (userProfile.Any())
                        {
                            SYSUserProfile SUP = userProfile.FirstOrDefault();
                            SUP.SYSUserID = SU.SYSUserID;
                            SUP.FirstName = user.FirstName;
                            SUP.LastName = user.LastName;
                            SUP.Gender = user.Gender;
                            SUP.RowCreatedSYSUserID = user.SYSUserID;
                            SUP.RowModifiedSYSUserID = user.SYSUserID;
                            SUP.RowCreatedDateTime = DateTime.Now;
                            SUP.RowModifiedDateTime = DateTime.Now;

                            db.SaveChanges();
                        }

                        if (user.LOOKUPRoleID > 0)
                        {
                            var userRole = db.SYSUserRoles.Where(o => o.SYSUserID == user.SYSUserID);
                            SYSUserRole SUR = null;
                            if (userRole.Any())
                            {
                                SUR = userRole.FirstOrDefault();
                                SUR.LOOKUPRoleID = user.LOOKUPRoleID;
                                SUR.SYSUserID = user.SYSUserID;
                                SUR.IsActive = true;
                                SUR.RowCreatedSYSUserID = user.SYSUserID;
                                SUR.RowModifiedSYSUserID = user.SYSUserID;
                                SUR.RowCreatedDateTime = DateTime.Now;
                                SUR.RowModifiedDateTime = DateTime.Now;
                            }
                            else
                            {
                                SUR = new SYSUserRole();
                                SUR.LOOKUPRoleID = user.LOOKUPRoleID;
                                SUR.SYSUserID = user.SYSUserID;
                                SUR.IsActive = true;
                                SUR.RowCreatedSYSUserID = user.SYSUserID;
                                SUR.RowModifiedSYSUserID = user.SYSUserID;
                                SUR.RowCreatedDateTime = DateTime.Now;
                                SUR.RowModifiedDateTime = DateTime.Now;
                                db.SYSUserRoles.Add(SUR);
                            }
                            db.SaveChanges();
                        }

                        //We used a simple transaction within this method.
                        //This is because the tables SYSUser, SYSUserProfile and SYSUserRole have
                        //dependencies to each other, and we need to make sure that we only
                        //commit changes to the database if the operation for each table
                        //is successful.
                        //The Database.BeginTransaction() is only available in EF 6 onwards
                        dbContextTransaction.Commit();
                    }
                    catch {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

    }
}