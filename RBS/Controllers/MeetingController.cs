using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using RBS.DAL;
using RBS.Library;
using RBS.Models;
using System;
using PagedList;

namespace RBS.Controllers
{
    public class MeetingController : BaseController
    {
        private RBSContext db = new RBSContext();

        // GET: Meeting
        public ActionResult Index(string searchTerm, int? page, string currentFilter)
        {
            ViewBag.SearchTerm = searchTerm;

            var todayDate = DateTime.Today;
            var meetings = db.Meetings.Include(p => p.Participants).Include(r => r.Room)
                                      .Where(b => b.BookingDate >= todayDate);

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
                meetings = meetings.Where(s => s.Title.Contains(searchTerm)
                                            || s.Room.Name.Contains(searchTerm));
            }

            meetings = meetings.OrderBy(d => d.BookingDate).ThenBy(s => s.StartingTime);
            int pageSize = Config.PageSize;
            int pageNumber = (page ?? 1);

            return View(meetings.ToPagedList(pageNumber, pageSize));
        }

        // GET: Meeting/Upcoming
        public ActionResult Upcoming(string searchTerm, int? page, string currentFilter)
        {
            ViewBag.SearchTerm = searchTerm;

            var todayDate = DateTime.Today;
            var meetings = db.Meetings.Include(p => p.Participants).Include(r => r.Room)
                                      .Where(b => b.BookingDate >= todayDate);

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
                meetings = meetings.Where(s => s.Title.Contains(searchTerm)
                                            || s.Room.Name.Contains(searchTerm));
            }

            meetings = meetings.OrderBy(d => d.BookingDate).ThenBy(s => s.StartingTime);
            int pageSize = Config.PageSize;
            int pageNumber = (page ?? 1);

            return View(meetings.ToPagedList(pageNumber, pageSize));
        }

        // GET: Meeting/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MeetingModel meetingModel = db.Meetings.Find(id);
            if (meetingModel == null)
            {
                return HttpNotFound();
            }
            return View(meetingModel);
        }

        // GET: Meeting/Create
        public ActionResult Create()
        {
            ViewBag.RoomID = new SelectList(db.Rooms, "ID", "Name");
            return View();
        }

        // POST: Meeting/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,RoomID,Title,Purpose,BookingDate,StartingTime,EndingTime,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,RecurenceType,SCCStartDate,SCCEndDate")] MeetingModel meetingModel)
        {
            if (ModelState.IsValid)
            {
                db.Meetings.Add(meetingModel);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RoomID = new SelectList(db.Rooms, "ID", "Name", meetingModel.RoomID);
            return View(meetingModel);
        }

        // GET: Meeting/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MeetingModel meetingModel = db.Meetings.Find(id);
            if (meetingModel == null)
            {
                return HttpNotFound();
            }
            ViewBag.RoomID = new SelectList(db.Rooms, "ID", "Name", meetingModel.RoomID);
            return View(meetingModel);
        }

        // POST: Meeting/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,RoomID,Title,Purpose,BookingDate,StartingTime,EndingTime,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,RecurenceType,SCCStartDate,SCCEndDate")] MeetingModel meetingModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(meetingModel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RoomID = new SelectList(db.Rooms, "ID", "Name", meetingModel.RoomID);
            return View(meetingModel);
        }

        // GET: Meeting/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MeetingModel meetingModel = db.Meetings.Find(id);
            if (meetingModel == null)
            {
                return HttpNotFound();
            }
            return View(meetingModel);
        }

        // POST: Meeting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MeetingModel meetingModel = db.Meetings.Find(id);
            db.Meetings.Remove(meetingModel);
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
