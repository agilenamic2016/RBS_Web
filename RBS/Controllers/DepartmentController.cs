using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using RBS.DAL;
using RBS.Models;
using RBS.Library;
using PagedList;

namespace RBS.Controllers
{
    public class DepartmentController : BaseController
    {
        private RBSContext db = new RBSContext();

        // GET: Department
        public ActionResult Index(string searchTerm, int? page, string currentFilter)
        {
            if (context.IsAdmin)
            {
                ViewBag.SearchTerm = searchTerm;

                IQueryable<DepartmentModel> departments = db.Departments;

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
                    departments = departments.Where(s => s.Name.Contains(searchTerm));
                }

                departments = departments.OrderBy(s => s.Name);
                int pageSize = Config.PageSize;
                int pageNumber = (page ?? 1);

                return View(departments.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Department/Details/5
        public ActionResult Details(int? id)
        {
            if (context.IsAdmin)
            { 
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DepartmentModel departmentModel = db.Departments.Find(id);
                if (departmentModel == null)
                {
                    return HttpNotFound();
                }
                return View(departmentModel);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Department/Create
        public ActionResult Create()
        {
            if (context.IsAdmin)
            { 
                return View();
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Department/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate")] DepartmentModel departmentModel)
        {
            if (ModelState.IsValid)
            {
                departmentModel.CreatedBy = context.UserID;
                departmentModel.CreatedDate = DateTime.Now;

                db.Departments.Add(departmentModel);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(departmentModel);
        }

        // GET: Department/Edit/5
        public ActionResult Edit(int? id)
        {
            if (context.IsAdmin)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DepartmentModel departmentModel = db.Departments.Find(id);
                if (departmentModel == null)
                {
                    return HttpNotFound();
                }
                return View(departmentModel);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Department/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate")] DepartmentModel departmentModel)
        {
            if (ModelState.IsValid)
            {
                departmentModel.UpdatedBy = context.UserID;
                departmentModel.UpdatedDate = DateTime.Now;

                db.Entry(departmentModel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(departmentModel);
        }

        // GET: Department/Delete/5
        public ActionResult Delete(int? id)
        {
            if (context.IsAdmin)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DepartmentModel departmentModel = db.Departments.Find(id);
                if (departmentModel == null)
                {
                    return HttpNotFound();
                }
                return View(departmentModel);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Department/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DepartmentModel departmentModel = db.Departments.Find(id);
            db.Departments.Remove(departmentModel);
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
    }
}
