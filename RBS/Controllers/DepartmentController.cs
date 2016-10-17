using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RBS.DAL;
using RBS.Models;

namespace RBS.Controllers
{
    public class DepartmentController : Controller
    {
        private RBSContext db = new RBSContext();

        // GET: Department
        public ActionResult Index()
        {
            return View(db.Departments.ToList());
        }

        // GET: Department/Details/5
        public ActionResult Details(int? id)
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

        // GET: Department/Create
        public ActionResult Create()
        {
            return View();
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
                db.Departments.Add(departmentModel);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(departmentModel);
        }

        // GET: Department/Edit/5
        public ActionResult Edit(int? id)
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

        // POST: Department/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate")] DepartmentModel departmentModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(departmentModel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(departmentModel);
        }

        // GET: Department/Delete/5
        public ActionResult Delete(int? id)
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
