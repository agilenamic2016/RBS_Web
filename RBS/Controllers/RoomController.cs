using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using RBS.DAL;
using RBS.Library;
using RBS.Models;
using PagedList;
using System.Web;
using System.IO;
using System.Configuration;

namespace RBS.Controllers
{
    public class RoomController : BaseController
    {
        private RBSContext db = new RBSContext();

        // GET: Room
        public ActionResult Index(string searchTerm, int? page, string currentFilter)
        {
            ViewBag.SearchTerm = searchTerm;

            IQueryable<RoomModel> rooms = db.Rooms;

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
                rooms = rooms.Where(s => s.Name.Contains(searchTerm));
            }

            rooms = rooms.OrderBy(s => s.Name);
            int pageSize = Config.PageSize;
            int pageNumber = (page ?? 1);

            return View(rooms.ToPagedList(pageNumber, pageSize));
        }

        // GET: Room/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RoomModel roomModel = db.Rooms.Find(id);
            if (roomModel == null)
            {
                return HttpNotFound();
            }

            ViewBag.path = Path.Combine(ConfigurationManager.AppSettings["PhotoPath"].ToString(), roomModel.PhotoFileName);
            return View(roomModel);
        }

        // GET: Room/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Room/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate")] RoomModel roomModel, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                roomModel.CreatedBy = context.UserID;
                roomModel.CreatedDate = DateTime.Now;

                // Checking the file content - Required Field
                if (file != null && file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string ext = Path.GetExtension(fileName);

                    if (Config.AllowedExtension.Contains(ext.ToLower()))
                    {
                        // Prevent duplicate name
                        RoomModel room = (from s in db.Rooms where s.Name.Equals(roomModel.Name) select s).FirstOrDefault();
                        if (room == null)
                        {
                            // Storing file first before db.savechanges
                            string path = Server.MapPath(ConfigurationManager.AppSettings["PhotoPath"].ToString());

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            file.SaveAs(path + "/" + fileName);

                            roomModel.PhotoFileName = fileName;
                            roomModel.PhotoFilePath = path;

                            db.Rooms.Add(roomModel);
                            db.SaveChanges();

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.ErrorMessage = context.STR_ERROR_MSG_MEETINGROOM_DUPLICATE;
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Only " + Config.AllowedExtension + " file is allowed. ";    
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid File.";    
                }
            }

            return View(roomModel);
        }

        // GET: Room/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RoomModel roomModel = db.Rooms.Find(id);
            if (roomModel == null)
            {
                return HttpNotFound();
            }

            ViewBag.path = Path.Combine(ConfigurationManager.AppSettings["PhotoPath"].ToString(), roomModel.PhotoFileName);
            return View(roomModel);
        }

        // POST: Room/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,PhotoFilePath,PhotoFileName,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate")] RoomModel roomModel, HttpPostedFileBase file)
        {
            string errorMsg = string.Empty;

            if (ModelState.IsValid)
            {
                RoomModel oldRoom = new RoomModel();

                roomModel.UpdatedBy = context.UserID;
                roomModel.UpdatedDate = DateTime.Now;

                // Checking the file content - Optional Field in edit
                if (file != null && file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string ext = Path.GetExtension(fileName);

                    if (Config.AllowedExtension.Contains(ext.ToLower()))
                    {
                        // Assign current to old for later processing
                        oldRoom = roomModel;

                        // Storing file first before db.savechanges
                        string path = Server.MapPath(ConfigurationManager.AppSettings["PhotoPath"].ToString());

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        file.SaveAs(path + "/" + fileName);

                        roomModel.PhotoFileName = fileName;
                        roomModel.PhotoFilePath = path;
                    }
                    else
                    {
                        errorMsg = "Only " + Config.AllowedExtension + " file is allowed. ";
                    }
                }

                if (errorMsg.Length == 0)
                {
                    // Prevent duplicate name
                    RoomModel room = (from s in db.Rooms where s.Name.Equals(roomModel.Name) select s).FirstOrDefault();
                    if (room == null)
                    {
                        db.Entry(roomModel).State = EntityState.Modified;
                        db.SaveChanges();

                        // After saving, remove the old photo
                        if (file != null && file.ContentLength > 0)
                        {
                            string oldpath = Server.MapPath(ConfigurationManager.AppSettings["PhotoPath"].ToString());
                            oldpath = Path.Combine(oldpath, oldRoom.PhotoFileName);

                            if (Directory.Exists(oldpath))
                            {
                                System.IO.File.Delete(oldpath);
                            }
                        }

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = context.STR_ERROR_MSG_MEETINGROOM_DUPLICATE;
                    }
                }
            }

            if (errorMsg.Length > 0)
                ViewBag.ErrorMessage += " , " + errorMsg;

            return View(roomModel);
        }

        // GET: Room/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RoomModel roomModel = db.Rooms.Find(id);
            if (roomModel == null)
            {
                return HttpNotFound();
            }
            return View(roomModel);
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RoomModel roomModel = db.Rooms.Find(id);
            db.Rooms.Remove(roomModel);
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
