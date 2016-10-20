using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using RBS.DAL;
using RBS.Library;
using RBS.Models;
using RBS.ViewModels;
using System;
using PagedList;

namespace RBS.Controllers
{
    public class UserController : BaseController
    {
        private RBSContext db = new RBSContext();

        public ActionResult Login()
        {
            if (!context.IsAuthenticated)
            {
                // Record previous page url
                if (Request.UrlReferrer != null)
                {
                    context.PreviousPage = Request.UrlReferrer.ToString();
                }

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Meeting");
            }
        }

        [HttpPost]
        public ActionResult Login([Bind(Include = "Username, Password")] UserViewModel userModel)
        {
            Authenticate(userModel);

            if (context.IsAuthenticated)
            {
                if (context.PreviousPage.Length > 0 && !context.PreviousPage.ToLower().Contains(context.STR_LOGIN.ToLower()))
                {
                    if (!Response.IsRequestBeingRedirected)
                    {
                        System.Web.HttpContext.Current.Response.Redirect(context.PreviousPage);
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Meeting");
                }
            }
            else
            {
                ViewBag.ErrorMessage = context.STR_ERROR_MSG_LOGIN_FAIL;
            }

            return View();
        }

        public ActionResult Logout()
        {
            context = new Context();

            Session["Context"] = context;
            Session["IsAuthenticated"] = context.IsAuthenticated.ToString();
            Session["UserID"] = context.UserID;

            return RedirectToAction("Login", "User");
        }

        // GET: User
        public ActionResult Index(string searchTerm, int? page, string currentFilter)
        {
            if (context.IsAdmin)
            {
                ViewBag.SearchTerm = searchTerm;

                var users = db.Users.Include(d => d.Department).Include(r => r.Role);

                if (searchTerm != null)
                {
                    page = 1;
                }
                else
                {
                    searchTerm = currentFilter;
                }

                ViewBag.CurrentFilter = searchTerm;

                if (!String.IsNullOrEmpty(searchTerm))
                {
                    users = users.Where(s => s.Username.Contains(searchTerm)
                                        || s.Name.Contains(searchTerm)
                                        || s.Role.Name.Contains(searchTerm));
                }

                users = users.OrderBy(d => d.DepartmentID).ThenBy(s => s.Name);
                int pageSize = Config.PageSize;
                int pageNumber = (page ?? 1);

                return View(users.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: User/Details/5
        public ActionResult Details(int? id)
        {
            if (context.IsAdmin)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UserModel userModel = db.Users.Find(id);
                if (userModel == null)
                {
                    return HttpNotFound();
                }
                return View(userModel);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: User/Create
        public ActionResult Create()
        {
            if (context.IsAdmin)
            {
                ViewBag.RoleID = new SelectList(db.Roles, "ID", "Name");
                ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name");
                return View();
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: User/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,RoleID,DepartmentID,Name,Username,Password,TokenID,IsActive,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate")] UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                userModel.CreatedBy = context.UserID;
                userModel.CreatedDate = DateTime.Now;
                
                // Prevent duplicate name
                UserModel um = (from s in db.Users where s.Username.Equals(userModel.Username) select s).FirstOrDefault();

                if (um == null)
                {
                    db.Users.Add(userModel);

                    if (!string.IsNullOrEmpty(userModel.Password))
                    {
                        if (userModel.Password.Length >= 8)
                        {
                            // Default is active for creation
                            userModel.IsActive = true;
                            userModel.Password = Security.HashPlainText(userModel.Password);

                            db.SaveChanges();

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.ErrorMessage = context.STR_ERROR_MSG_PASSWORD_POLICY;
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = context.STR_ERROR_MSG_PASSWORD_POLICY;
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = context.STR_ERROR_MSG_USERNAME_DUPLICATE;
                }
            }

            ViewBag.RoleID = new SelectList(db.Roles, "ID", "Name", userModel.RoleID);
            ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", userModel.DepartmentID);
            return View(userModel);
        }

        // GET: User/Edit/5
        public ActionResult Edit(int? id)
        {
            if (context.IsAdmin)
            { 
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            
                UserModel userModel = db.Users.Find(id);
                if (userModel == null)
                {
                    return HttpNotFound();
                }

                ViewBag.RoleID = new SelectList(db.Roles, "ID", "Name", userModel.RoleID);
                ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", userModel.DepartmentID);
                return View(userModel);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,RoleID,DepartmentID,Name,Username,Password,TokenID,IsActive,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate")] UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                userModel.UpdatedBy = context.UserID;
                userModel.UpdatedDate = DateTime.Now;

                db.Entry(userModel).State = EntityState.Modified;

                // Do not update these fields
                db.Entry(userModel).Property(p => p.Username).IsModified = false;
                db.Entry(userModel).Property(p => p.Password).IsModified = false;
                db.Entry(userModel).Property(p => p.CreatedDate).IsModified = false;
                db.Entry(userModel).Property(p => p.CreatedBy).IsModified = false;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RoleID = new SelectList(db.Roles, "ID", "Name", userModel.RoleID);
            ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", userModel.DepartmentID);
            return View(userModel);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "CurrentPassword,NewPassword,ConfirmPassword")] PasswordViewModel passwordViewModel)
        {
            if (ModelState.IsValid)
            {
                UserModel userModel = (from s in db.Users where s.Username.Equals(context.UserID) select s).FirstOrDefault();

                // Hash password with salt
                string passwordHash = Security.HashPlainText(passwordViewModel.CurrentPassword);

                // Proceed only when current password is correct
                if (userModel.Password.Equals(passwordHash))
                {
                    userModel.Password = passwordViewModel.NewPassword;

                    userModel.UpdatedBy = context.UserID;  // This is username
                    userModel.UpdatedDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(userModel.Password))
                    {
                        if (userModel.Password.Length >= 8)
                        {
                            userModel.Password = Security.HashPlainText(passwordViewModel.NewPassword);
                            db.SaveChanges();

                            return RedirectToAction("Index", "User");
                        }
                        else
                        {
                            ViewBag.ErrorMessage = context.STR_ERROR_MSG_PASSWORD_POLICY;
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = context.STR_ERROR_MSG_PASSWORD_WRONG;
                }
            }

            return View();
        }

        // GET: User/Delete/5
        public ActionResult Delete(int? id)
        {
            if (context.IsAdmin)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                UserModel userModel = db.Users.Find(id);
                if (userModel == null)
                {
                    return HttpNotFound();
                }
                return View(userModel);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserModel userModel = db.Users.Find(id);
            db.Users.Remove(userModel);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Authenticate(UserViewModel inputUser)
        {
            // This section will replace with SSO
            UserModel selectedUser = db.Users.FirstOrDefault(s => s.Username.Equals(inputUser.Username));

            if (selectedUser != null && selectedUser.IsActive)
            {
                // Hash password with salt
                string passwordHash = Security.HashPlainText(inputUser.Password);

                selectedUser = db.Users.FirstOrDefault(s => s.Username.Equals(inputUser.Username) && s.Password.Equals(passwordHash));
                

                if (selectedUser != null)
                {
                    context.IsAuthenticated = true;

                    // Create session key
                    SessionModel selectedSession = db.Sessions.FirstOrDefault(s => s.UserID == selectedUser.ID);

                    if (selectedSession == null)
                    {
                        SessionModel newSession = new SessionModel()
                        {
                            UserID = selectedUser.ID,
                            SessionKey = System.Guid.NewGuid().ToString("N"),
                            CreatedBy = context.SYSTEM_ID,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now
                        };

                        selectedSession = newSession;

                        db.Sessions.Add(newSession);
                        db.SaveChanges();
                    }
                    else
                    {
                        selectedSession.SessionKey = System.Guid.NewGuid().ToString("N");
                        selectedSession.UpdatedBy = context.SYSTEM_ID;
                        selectedSession.UpdatedDate = DateTime.Now;

                        db.Entry(selectedSession).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    // Store values into session
                    context.SessionKey = selectedSession.SessionKey;
                    context.UserID = selectedSession.User.Username;

                    if (selectedUser.Role.Name.ToLower().Equals("admin"))
                    {
                        context.IsAdmin = true;
                    }

                    Session["Context"] = context;
                    Session["IsAuthenticated"] = context.IsAuthenticated.ToString();
                    Session["UserID"] = context.UserID;
                    Session["IsAdmin"] = context.IsAdmin;
                }
                else
                {
                    ViewBag.ErrorMessage = context.STR_ERROR_MSG_LOGIN_FAIL;
                }
            }
            else
            {
                ViewBag.ErrorMessage = context.STR_ERROR_MSG_LOGIN_FAIL;
            }
        }

    }
}
