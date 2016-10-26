using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using RBS.DAL;
using RBS.Library;
using RBS.Models;
using System;
using PagedList;
using System.Collections.Generic;
using RBS.Notification;
using System.Net.Mail;
using System.Configuration;
using System.IO;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using RBS.DTO;
using AutoMapper;

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

            // Normal user only able to view the meeting list that created by him
            if (!context.IsAdmin)
                meetings = meetings.Where(e => e.CreatedBy.Equals(context.UserID));

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
            var startingTime = MilitaryTime.ChangeToMilitaryTime(DateTime.Now);
            string tempQuery = "SELECT A.* FROM MeetingModel A INNER JOIN ParticipantModel B on A.ID = B.MeetingID INNER JOIN UserModel C on B.UserID = C.ID "
                             + "WHERE C.Username = '" + context.UserID + "' AND BookingDate >= '" + todayDate + "' AND StartingTime >= '" + startingTime + "'";

            var meetings = db.Meetings.SqlQuery(tempQuery).ToList().AsQueryable();

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

            // Assign the list of participant and display
            IList<ParticipantModel> participantList = db.Participants.Where(u => u.MeetingID == id).ToList();
            if (participantList.Count > 0)
                meetingModel.Participants = participantList;

            if (meetingModel == null)
            {
                return HttpNotFound();
            }
            return View(meetingModel);
        }

        // GET: Meeting/UpcomingDetails/5
        public ActionResult UpcomingDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MeetingModel meetingModel = db.Meetings.Find(id);

            // Assign the list of participant and display
            IList<ParticipantModel> participantList = db.Participants.Where(u => u.MeetingID == id).ToList();
            if (participantList.Count > 0)
                meetingModel.Participants = participantList;

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
            ViewBag.RecurenceType = GetOccurrenceRate(null);

            return View();
        }

        // POST: Meeting/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,RoomID,Title,Purpose,BookingDate,StartingTime,EndingTime,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,RecurenceType,SCCStartDate,SCCEndDate")] MeetingModel meetingModel)
        {
            // Convert the Starting time and Ending time to String (4 Char)
            string strStart = string.Empty;
            string strEnd = string.Empty;

            strStart = MilitaryTime.ChangeToMilitaryTime(DateTime.Parse(meetingModel.StartingTime));
            strEnd = MilitaryTime.ChangeToMilitaryTime(DateTime.Parse(meetingModel.EndingTime));

            meetingModel.StartingTime = strStart;
            meetingModel.EndingTime = strEnd;

            string sqlStart = (Convert.ToInt32(strStart) + 1).ToString(); // + 1 to staring time in order to perform between condition 
            string sqlEnd = (Convert.ToInt32(strEnd) - 1).ToString();     // - 1 to staring time in order to perform between condition 

            if (ModelState.IsValid)
            {
                if (meetingModel.RecurenceType == 0)
                {
                    string tempQuery = "SELECT * FROM dbo.MeetingModel WHERE RoomID = " + meetingModel.RoomID + " AND BookingDate='" + meetingModel.BookingDate + "' AND (" + sqlStart + " between StartingTime and EndingTime OR " + sqlEnd + " between StartingTime and EndingTime)";

                    // Checking whether this timeslot if booked
                    var bookedMeeting = db.Meetings.SqlQuery(tempQuery).ToList();

                    if (bookedMeeting.Count == 0)
                    {
                        //MeetingModel meeting = new MeetingModel();
                        //meeting.RoomID = meetingModel.RoomID;
                        //meeting.Title = meetingModel.Title;
                        //meeting.Purpose = meetingModel.Purpose;
                        //meeting.BookingDate = meetingModel.BookingDate;
                        //meeting.StartingTime = meetingModel.StartingTime;
                        //meeting.EndingTime = meetingModel.EndingTime;
                        //meeting.RecurenceType = meetingModel.RecurenceType;
                        //meeting.SCCStartDate = meetingModel.SCCStartDate;
                        //meeting.SCCEndDate = meetingModel.SCCEndDate;
                        //meeting.CreatedBy = context.UserID;
                        //meeting.CreatedDate = DateTime.Now;

                        //db.Meetings.Add(meeting);
                        //db.SaveChanges();

                        // Change to store in session, later save to DB at one time
                        Session["Meeting"] = meetingModel;
                        return RedirectToAction("AddAttendee");
                        
                    }
                    else
                    {
                        UserModel user = db.Users.Where(u => u.Username.Equals(meetingModel.CreatedBy)).SingleOrDefault();

                        if (user != null)
                            ViewBag.ErrorMessage = "This time slot had been booked by " + user.Name + " on " + meetingModel.BookingDate.Value.ToString("yyyy-MM-dd") + " at " + meetingModel.StartingTime + " to " + meetingModel.EndingTime;
                        else
                            ViewBag.ErrorMessage = "The selected time slot had been booked";
                    }
                }
                else
                {
                    List<DateTime> dates = new List<DateTime>();
                    DateTime newStartDate = meetingModel.BookingDate.Value;
                    DateTime newEndDate = DateTime.ParseExact(meetingModel.SCCEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    if (meetingModel.RecurenceType == 1)
                    {
                        dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Daily);
                    }
                    else if (meetingModel.RecurenceType == 2)
                    {
                        dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Weekly);
                    }
                    else if (meetingModel.RecurenceType == 3)
                    {
                        dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Monthly);
                    }

                    int checkavalilable = 0;
                    foreach (var date in dates)
                    {
                        string tempQuery = "SELECT * FROM dbo.MeetingModel WHERE RoomID = " + meetingModel.RoomID + " AND BookingDate='" + meetingModel.BookingDate + "' AND (" + sqlStart + " between StartingTime and EndingTime OR " + sqlEnd + " between StartingTime and EndingTime)";

                        // Checking whether this timeslot if booked
                        var bookedMeeting = db.Meetings.SqlQuery(tempQuery).ToList();

                        if (bookedMeeting.Count > 0)
                        {
                            checkavalilable++;
                        }
                    }

                    if (checkavalilable == 0)
                    {
                        //foreach (var date in dates)
                        //{
                        //    MeetingModel meeting = new MeetingModel();
                        //    meeting.RoomID = meetingModel.RoomID;
                        //    meeting.Title = meetingModel.Title;
                        //    meeting.Purpose = meetingModel.Purpose;
                        //    meeting.BookingDate = DateTime.ParseExact(date.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        //    meeting.StartingTime = meetingModel.StartingTime;
                        //    meeting.EndingTime = meetingModel.EndingTime;
                        //    meeting.RecurenceType = meetingModel.RecurenceType;
                        //    meeting.SCCStartDate = meetingModel.SCCStartDate;
                        //    meeting.SCCEndDate = meetingModel.SCCEndDate;
                        //    meeting.CreatedBy = context.UserID;
                        //    meeting.CreatedDate = DateTime.Now;

                        //    if (ModelState.IsValid)
                        //    {
                        //        db.Meetings.Add(meeting);
                        //        db.SaveChanges();
                        //    }
                        //}

                        // Change to store in session, later save to DB at one time
                        Session["Meeting"] = meetingModel;
                        return RedirectToAction("AddAttendee");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "The selected time slot had been booked";
                    }
                }
            }

            ViewBag.RoomID = new SelectList(db.Rooms, "ID", "Name", meetingModel.RoomID);
            ViewBag.RecurenceType = GetOccurrenceRate(meetingModel.RecurenceType);

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
            // Convert the Starting time and Ending time to String (4 Char)
            string strStart = string.Empty;
            string strEnd = string.Empty;

            strStart = MilitaryTime.ChangeToMilitaryTime(DateTime.Parse(meetingModel.StartingTime));
            strEnd = MilitaryTime.ChangeToMilitaryTime(DateTime.Parse(meetingModel.EndingTime));

            meetingModel.StartingTime = strStart;
            meetingModel.EndingTime = strEnd;

            string sqlStart = (Convert.ToInt32(strStart) + 1).ToString(); // + 1 to staring time in order to perform between condition 
            string sqlEnd = (Convert.ToInt32(strEnd) - 1).ToString();     // - 1 to staring time in order to perform between condition 

            if (ModelState.IsValid)
            {
                string tempQuery = "SELECT * FROM dbo.MeetingModel WHERE ID <> " + meetingModel.ID + " AND RoomID = " + meetingModel.RoomID + " AND BookingDate='" + meetingModel.BookingDate + "' AND (" + sqlStart + " between StartingTime and EndingTime OR " + sqlEnd + " between StartingTime and EndingTime)";

                // Checking whether this timeslot if booked
                var bookedMeeting = db.Meetings.SqlQuery(tempQuery).ToList();

                if (bookedMeeting.Count == 0)
                {
                    MeetingModel meeting = new MeetingModel();
                    meeting.ID = meetingModel.ID;
                    meeting.RoomID = meetingModel.RoomID;
                    meeting.Title = meetingModel.Title;
                    meeting.Purpose = meetingModel.Purpose;
                    meeting.BookingDate = meetingModel.BookingDate;
                    meeting.StartingTime = meetingModel.StartingTime;
                    meeting.EndingTime = meetingModel.EndingTime;
                    meeting.RecurenceType = meetingModel.RecurenceType;
                    meeting.SCCStartDate = meetingModel.SCCStartDate;
                    meeting.SCCEndDate = meetingModel.SCCEndDate;
                    meeting.CreatedBy = meetingModel.CreatedBy;
                    meeting.CreatedDate = meetingModel.CreatedDate;
                    meeting.UpdatedBy = context.UserID;
                    meeting.UpdatedDate = DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        db.Entry(meeting).State = EntityState.Modified;
                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    UserModel user = db.Users.Where(u => u.Username.Equals(meetingModel.CreatedBy)).SingleOrDefault();

                    if (user != null)
                        ViewBag.ErrorMessage = "This time slot had been booked by " + user.Name + " on " + meetingModel.BookingDate.Value.ToString("yyyy-MM-dd") + " at " + meetingModel.StartingTime + " to " + meetingModel.EndingTime;
                    else
                        ViewBag.ErrorMessage = "The selected time slot had been booked";
                }
            }

            ViewBag.RoomID = new SelectList(db.Rooms, "ID", "Name", meetingModel.RoomID);

            return View(meetingModel);
        }

        // GET: Meeting/AddAttendee
        public ActionResult AddAttendee()
        {
            MeetingModel meetingModel = (MeetingModel)Session["Meeting"];

            if (meetingModel != null)
            {
                IQueryable<UserModel> users = db.Users.Where(a => a.IsActive == true);

                List<UserDTO> userList = new List<UserDTO>();
                Mapper.Initialize(cfg => cfg.CreateMap<UserModel, UserDTO>());

                foreach (UserModel um in users)
                {
                    um.Department = null;
                    UserDTO newDto = Mapper.Map<UserDTO>(um);
                    userList.Add(newDto);
                }

                userList = userList.OrderBy(u => u.Name).ToList();
                IQueryable<DepartmentModel> departments = db.Departments.OrderBy(u => u.Name);

                string userjson = JsonConvert.SerializeObject(userList);
                string deptjson = JsonConvert.SerializeObject(departments.ToList());

                ViewBag.Users = userjson;
                ViewBag.Departments = deptjson;

                return View();
            }
            else
            {
                return RedirectToAction("Create");
            }
        }

        [HttpPost]
        public ActionResult AddAttendee(string selectedUsers, string selectedDepts)
        {
            if (!String.IsNullOrEmpty(selectedUsers) || !String.IsNullOrEmpty(selectedDepts))
            {
                MeetingModel meetingModel = (MeetingModel)Session["Meeting"];

                if (meetingModel != null)
                {
                    List<int> userIds = new List<int>();

                    if (selectedUsers.Length > 0)
                    {
                        string[] ids = selectedUsers.Trim().Split(',');

                        for (int i = 0; i < ids.Length; i++)
                        {
                            int temp = Convert.ToInt32(ids[i]);
                            userIds.Add(temp);
                        }
                    }
                    else if (selectedDepts.Length > 0)
                    {
                        string tempQuery = "SELECT ID FROM dbo.UserModel WHERE DepartmentID IN (" + selectedDepts + ")";

                        using (var context = new RBSContext())
                        {
                            userIds = context.Database.SqlQuery<int>("SELECT ID FROM dbo.UserModel WHERE DepartmentID IN (" + selectedDepts + ")").ToList();
                        }
                    }

                    if (meetingModel.RecurenceType == 0)
                    {
                        MeetingModel meeting = new MeetingModel();
                        meeting.RoomID = meetingModel.RoomID;
                        meeting.Title = meetingModel.Title;
                        meeting.Purpose = meetingModel.Purpose;
                        meeting.BookingDate = meetingModel.BookingDate;
                        meeting.StartingTime = meetingModel.StartingTime;
                        meeting.EndingTime = meetingModel.EndingTime;
                        meeting.RecurenceType = meetingModel.RecurenceType;
                        meeting.SCCStartDate = meetingModel.SCCStartDate;
                        meeting.SCCEndDate = meetingModel.SCCEndDate;
                        meeting.CreatedBy = context.UserID;
                        meeting.CreatedDate = DateTime.Now;

                        db.Meetings.Add(meeting);
                        db.SaveChanges();

                        // After saving the db, proceed to add participants
                        foreach (var id in userIds)
                        {
                            ParticipantModel participant = new ParticipantModel();
                            participant.MeetingID = Convert.ToInt32(meeting.ID);
                            participant.UserID = id;
                            participant.CreatedBy = context.UserID;
                            participant.CreatedDate = DateTime.Now;

                            db.Participants.Add(participant);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        List<DateTime> dates = new List<DateTime>();
                        DateTime newStartDate = meetingModel.BookingDate.Value;
                        DateTime newEndDate = DateTime.ParseExact(meetingModel.SCCEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                        if (meetingModel.RecurenceType == 1)
                        {
                            dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Daily);
                        }
                        else if (meetingModel.RecurenceType == 2)
                        {
                            dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Weekly);
                        }
                        else if (meetingModel.RecurenceType == 3)
                        {
                            dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Monthly);
                        }

                        foreach (var date in dates)
                        {
                            MeetingModel meeting = new MeetingModel();
                            meeting.RoomID = meetingModel.RoomID;
                            meeting.Title = meetingModel.Title;
                            meeting.Purpose = meetingModel.Purpose;
                            meeting.BookingDate = DateTime.ParseExact(date.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            meeting.StartingTime = meetingModel.StartingTime;
                            meeting.EndingTime = meetingModel.EndingTime;
                            meeting.RecurenceType = meetingModel.RecurenceType;
                            meeting.SCCStartDate = meetingModel.SCCStartDate;
                            meeting.SCCEndDate = meetingModel.SCCEndDate;
                            meeting.CreatedBy = context.UserID;
                            meeting.CreatedDate = DateTime.Now;

                            db.Meetings.Add(meeting);
                            db.SaveChanges();

                            // After saving the db, proceed to add participants
                            foreach (var id in userIds)
                            {
                                ParticipantModel participant = new ParticipantModel();
                                participant.MeetingID = Convert.ToInt32(meeting.ID);
                                participant.UserID = id;
                                participant.CreatedBy = context.UserID;
                                participant.CreatedDate = DateTime.Now;

                                db.Participants.Add(participant);
                                db.SaveChanges();
                            }
                        }
                    }

                    // After saving, sending invitation to the participants
                    CreateEmail(meetingModel, userIds);

                    //after saving, sending notification
                    sendNotification(meetingModel, userIds);

                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Create");
                }
            }
            else
            {
                IQueryable<UserModel> users = db.Users.Where(a => a.IsActive == true);

                List<UserDTO> userList = new List<UserDTO>();
                Mapper.Initialize(cfg => cfg.CreateMap<UserModel, UserDTO>());

                foreach (UserModel um in users)
                {
                    um.Department = null;
                    UserDTO newDto = Mapper.Map<UserDTO>(um);
                    userList.Add(newDto);
                }

                userList = userList.OrderBy(u => u.Name).ToList();
                IQueryable<DepartmentModel> departments = db.Departments.OrderBy(u => u.Name);

                string userjson = JsonConvert.SerializeObject(userList);
                string deptjson = JsonConvert.SerializeObject(departments.ToList());

                ViewBag.Users = userjson;
                ViewBag.Departments = deptjson;
                ViewBag.ErrorMessage = "Please select at least one user or one department.";

                return View();
            }
        }

        // GET: Meeting/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MeetingModel meetingModel = db.Meetings.Find(id);

            // Assign the list of participant and display
            IList<ParticipantModel> participantList = db.Participants.Where(u => u.MeetingID == id).ToList();
            if (participantList.Count > 0)
                meetingModel.Participants = participantList;

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

            if (meetingModel != null)
            {
                // Remove the Attendees for the particular meeting first.
                using (var context = new RBSContext())
                {
                    context.Database.ExecuteSqlCommand("DELETE FROM [ParticipantModel] WHERE MeetingID = " + meetingModel.ID);

                    context.SaveChanges();
                }

                db.Meetings.Remove(meetingModel);
                db.SaveChanges();
            }

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

        #region Methods

        //enum for various patterns
        //Added here in order to get the standard value for RecurenceType Value, once - 0, Daily - 1, Weekly - 2, Monthly - 3
        public enum OccurrenceRate
        {
            Once,
            Daily,
            Weekly,
            Monthly
        }

        public static SelectList GetOccurrenceRate(int? occurrenceEnumType)
        {
            Array values = Enum.GetValues(typeof(OccurrenceRate));
            List<ListItem> items = new List<ListItem>(values.Length);

            foreach (var i in values)
            {
                items.Add(new ListItem
                {
                    Text = Enum.GetName(typeof(OccurrenceRate), i),
                    Value = ((int)i).ToString()
                });
            }

            if (occurrenceEnumType == null)
                return new SelectList(items, "Value", "Text");
            else
                return new SelectList(items, "Value", "Text", occurrenceEnumType.Value);
        }

        public static List<DateTime> GetOccurrences(DateTime startDate, DateTime endDate, OccurrenceRate rate)
        {
            List<DateTime> occurrences = new List<DateTime>();

            var nextDate = startDate;

            while (true)
            {
                if (nextDate <= endDate)
                {
                    occurrences.Add(nextDate);
                }
                else
                {
                    break;
                }

                switch (rate)
                {
                    case OccurrenceRate.Weekly:
                        {
                            nextDate = nextDate.AddDays(7);
                            break;
                        }
                    case OccurrenceRate.Daily:
                        {
                            nextDate = nextDate.AddDays(1);
                            break;
                        }
                    case OccurrenceRate.Monthly:
                        {
                            nextDate = nextDate.AddMonths(1);
                            break;
                        }
                }
            }

            return occurrences;
        }

        private void CreateEmail(MeetingModel mm, List<int> userList)
        {
            try
            {
                RoomModel room = db.Rooms.Find(mm.RoomID);

                string location = room.Name;
                string title = mm.Title;
                string purpose = mm.Purpose;

                DateTime startingTime = MilitaryTime.ParseMilitaryTime(mm.StartingTime, mm.BookingDate.Value.Year, mm.BookingDate.Value.Month, mm.BookingDate.Value.Day);
                DateTime endingTime = MilitaryTime.ParseMilitaryTime(mm.EndingTime, mm.BookingDate.Value.Year, mm.BookingDate.Value.Month, mm.BookingDate.Value.Day);

                //PUTTING THE MEETING DETAILS INTO AN ARRAY OF STRING

                String[] contents = { "BEGIN:VCALENDAR",
                              "PRODID:-//Flo Inc.//FloSoft//EN",
                              "BEGIN:VEVENT",
                              "DTSTART:" + startingTime.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z"),
                              "DTEND:" + endingTime.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z"),
                              "LOCATION:" + location,
                         "DESCRIPTION;ENCODING=QUOTED-PRINTABLE:" + purpose,
                              "SUMMARY:" + title, "PRIORITY:3",
                         "END:VEVENT", "END:VCALENDAR" };

                string path = ConfigurationManager.AppSettings["EmailPath"].ToString();

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, title + ".ics");

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); // Delete old one
                }

                FileStream fs = System.IO.File.Create(filePath);
                fs.Close();
                System.IO.File.WriteAllLines(filePath, contents);

                //METHOD TO SEND EMAIL IS CALLED
                SendMail(filePath, userList, mm);
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "CreateEmail", ex);
            }

        }

        private void SendMail(string filePath, List<int> userList, MeetingModel mm)
        {
            //CONFIGURE BASIC CONTENTS OF AN EMAIL
            bool hasValidEmail = false;

            string FromName = "Room Booking System";
            string FromEmail = ConfigurationManager.AppSettings["emailID"].ToString();

            MailMessage mailMessage = new MailMessage();

            // Loop through the participants to add into ToEmail, username = email
            foreach (var id in userList)
            {
                UserModel user = db.Users.Find(id);

                if (user != null)
                {
                    hasValidEmail = true;
                    mailMessage.To.Add(new MailAddress(user.Username));
                }
            }

            // Only Trigger sending email when there is at least one user being added
            if (hasValidEmail)
            {
                string emailPwd = ConfigurationManager.AppSettings["emailPwd"].ToString();
                string smtpServer = ConfigurationManager.AppSettings["smtpServer"].ToString();
                int smtpPort = Convert.ToInt16(ConfigurationManager.AppSettings["smtpPort"].ToString());
                SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
                smtp.Credentials = new NetworkCredential(FromEmail, emailPwd);
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                mailMessage.From = new MailAddress(FromEmail, FromName);
                mailMessage.Subject = "Meeting Invitation from RBS";

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("You have invited to attend a meeting, please take a look in the attachment.");

                // Display Info
                sb.AppendLine("Date of Meeting: " + mm.BookingDate.Value.ToString("yyyy-MM-dd") + "");
                sb.AppendLine("Starting Time: " + mm.StartingTime);
                sb.AppendLine("Ending Time: " + mm.EndingTime);
                sb.AppendLine();

                string tempQuery = "SELECT * FROM dbo.ParticipantModel WHERE MeetingID = " + mm.ID;

                // Checking whether this timeslot if booked
                var participants = db.Participants.SqlQuery(tempQuery).ToList();

                List<ParticipantModel> pList = participants.ToList<ParticipantModel>();

                if (pList.Count > 0)
                {
                    int i = 1;

                    foreach (ParticipantModel p in pList)
                    {
                        sb.AppendLine(i + ") " + db.Users.Find(p.UserID).Name);
                        i++;
                    }
                }

                mailMessage.Body = sb.AppendLine().ToString();

                //MAKE AN ATTACHMENT OUT OF THE .ICS FILE CREATED
                Attachment mailAttachment = new Attachment(filePath);

                //ADD THE ATTACHMENT TO THE EMAIL
                mailMessage.Attachments.Add(mailAttachment);
                smtp.Send(mailMessage);
            }
        }

        private void sendNotification(MeetingModel mm, List<int> userList)
        {
            try
            {
                string Title = "New Event";
                string message = mm.Title;

                foreach (var id in userList)
                {
                    UserModel user = db.Users.Find(id);

                    if (user != null)
                    {
                        string tokenID = user.TokenID.Trim();
                        tokenID = tokenID.Replace(" ", "");
                        BLNotification.PushNotification(tokenID, message, "", Title, "");
                        Log.Error("Notification", context.STR_USER, "userid= " + id + " token id:" + tokenID + " message:" + message + " Title:" + Title);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Notification", context.STR_USER, "sendNotification", ex);
            }
        }

        #endregion

    }
}
