using System;
using System.Linq;
using System.Web.Http;
using RBS.DAL;
using RBS.Library;
using RBS.Models;
using RBS.DTO;
using Newtonsoft.Json.Linq;
using System.Data.Entity;
using System.Globalization;
using AutoMapper;
using System.Collections.Generic;
using System.Net.Mail;
using System.Web.Hosting;
using System.Configuration;
using System.IO;
using RBS.Notification;

namespace RBS.ApiControllers
{
    public class RBSController : ApiController
    {
        private RBSContext db = new RBSContext();
        private Context context = new Context();

        [HttpPost]
        public IHttpActionResult GetLogin(JObject jsonObj)
        {
            //string regid = "e3litPHp7IE:APA91bEzNi_7A0haYomQvr2MLqZsILmBsRRC2DdiJQACNwa7r73PmWIQ6MCnlTJU2BrtyAAm6RU61UUbSnDMKUdaiOdgeKfswyDIpfJqjT9yKTvaki5EAT-I9B9pPAY6t5dqHuLG5r-s";

            //BLNotification.PushNotification(regid, "Progress Meeting", "", "New Event", "");

            try
            {
                string userName = string.Empty;
                string password = string.Empty;
                string tokenId = string.Empty;

                if (jsonObj != null)
                {
                    userName = jsonObj["UserName"].ToString();
                    password = jsonObj["Password"].ToString();
                    tokenId = jsonObj["TokenID"].ToString();
                    Log.Error("login info:"+tokenId, context.STR_USER, context.STR_LOGIN);
                }

                if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(password))
                {
                    UserModel selectedUser = db.Users.FirstOrDefault(s => s.Username.Equals(userName));

                    if (selectedUser != null && selectedUser.IsActive)
                    {
                        // Hash password with salt
                        string passwordHash = Security.HashPlainText(password);

                        selectedUser = db.Users.FirstOrDefault(s => s.Username.Equals(userName) && s.Password.Equals(passwordHash));

                        if (selectedUser != null)
                        {
                            context.IsAuthenticated = true;

                            // Checking the login user still using the same tokenID or not, else update it to receive the notification
                            if (selectedUser.TokenID == null || !selectedUser.TokenID.Equals(tokenId))
                            {
                                selectedUser.TokenID = tokenId;
                                selectedUser.UpdatedBy = selectedUser.Username;
                                selectedUser.UpdatedDate = DateTime.Now;

                                db.Entry(selectedUser).State = EntityState.Modified;
                            }

                            // Create session key
                            SessionModel selectedSession = db.Sessions.FirstOrDefault(s => s.UserID == selectedUser.ID);

                            if (selectedSession == null)
                            {
                                SessionModel newSession = new SessionModel()
                                {
                                    UserID = selectedUser.ID,
                                    SessionKey = Guid.NewGuid().ToString("N"),
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
                                selectedSession.SessionKey = Guid.NewGuid().ToString("N");
                                selectedSession.UpdatedBy = context.SYSTEM_ID;
                                selectedSession.UpdatedDate = DateTime.Now;

                                db.Entry(selectedSession).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            // Store values into DTO
                            SessionDTO sessionObj = new SessionDTO();
                            sessionObj.SessionKey = selectedSession.SessionKey;
                            sessionObj.UserID = selectedUser.ID;
                            return Ok(sessionObj);
                        }
                        else
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, context.STR_LOGIN, ex);
                return BadRequest("There is an issue in the system. Contact administrator.");
            }
        }

        [HttpPost]
        public IHttpActionResult GetRooms(JObject jsonObj)
        {
            try
            {
                string sessionKey = string.Empty;
                string userId = string.Empty;

                if (jsonObj != null)
                {
                    sessionKey = jsonObj["SessionKey"].ToString();
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        IQueryable<RoomModel> rooms = db.Rooms;

                        return Ok(rooms);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "GetRooms", ex);
                return BadRequest("There is an issue in the system. Contact administrator.");
            }
        }

        [HttpPost]
        public IHttpActionResult GetUsers(JObject jsonObj)
        {
            try
            {
                string sessionKey = string.Empty;
                string userId = string.Empty;

                if (jsonObj != null)
                {
                    sessionKey = jsonObj["SessionKey"].ToString();
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        IQueryable<UserModel> users = db.Users.Where(a => a.IsActive == true);

                        List<UserDTO> userList = new List<UserDTO>();
                        Mapper.Initialize(cfg => cfg.CreateMap<UserModel, UserDTO>());

                        foreach (UserModel um in users)
                        {
                            UserDTO newDto = Mapper.Map<UserDTO>(um);
                            userList.Add(newDto);
                        }

                        return Ok(userList);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "GetUsers", ex);
                return BadRequest("There is an issue in the system. Contact administrator.");
            }
        }

        [HttpPost]
        public IHttpActionResult GetMeetingsByUserId(JObject jsonObj)
        {
            try
            {
                string sessionKey = string.Empty;
                string userId = string.Empty;
                string selectedUserId = string.Empty;

                if (jsonObj != null)
                {
                    sessionKey = jsonObj["SessionKey"].ToString();
                    selectedUserId = jsonObj["UserID"].ToString();
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        int id = Convert.ToInt32(selectedUserId);

                        string datetimenow = DateTime.Now.ToString("yyyy-MM-dd");
                        DateTime datetimenowfromzero= DateTime.ParseExact(datetimenow, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        IQueryable<MeetingModel> meetings = db.Meetings.Where(a => a.BookingDate >= datetimenowfromzero);

                        List<MeetingDTO> meetingList = new List<MeetingDTO>();
                        Mapper.Initialize(cfg => cfg.CreateMap<MeetingModel, MeetingDTO>());

                        foreach (MeetingModel mm in meetings)
                        {
                            MeetingDTO newDto = Mapper.Map<MeetingDTO>(mm);
                            meetingList.Add(newDto);
                        }

                        return Ok(meetingList);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "GetMeetingsByUserId", ex);
                return BadRequest("There is an issue in the system. Contact administrator.");
            }
        }

        [HttpPost]
        public IHttpActionResult GetAttendeesByMeetingId(JObject jsonObj)
        {
            try
            {
                string sessionKey = string.Empty;
                string userId = string.Empty;
                string meetingId = string.Empty;

                if (jsonObj != null)
                {
                    sessionKey = jsonObj["SessionKey"].ToString();
                    meetingId = jsonObj["MeetingID"].ToString();
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        int id = Convert.ToInt32(meetingId);

                        IQueryable<ParticipantModel> participants = db.Participants.Where(a => a.MeetingID == id);

                        List<UserDTO> userlist = new List<UserDTO>();
                        Mapper.Initialize(cfg => cfg.CreateMap<UserModel, UserDTO>());

                        foreach (ParticipantModel pm in participants)
                        {
                            UserModel um = db.Users.Find(pm.UserID);

                            if (um != null)
                            {
                                UserDTO newDto = Mapper.Map<UserDTO>(um);
                                userlist.Add(newDto);
                            }
                        }

                        return Ok(userlist);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "GetAttendeesByMeetingId", ex);
                return BadRequest("There is an issue in the system. Contact administrator.");
            }
        }

        [HttpPost]
        public IHttpActionResult GetTimeSlotByRoomId(JObject jsonObj)
        {
            try
            {
                string sessionKey = string.Empty;
                string date = string.Empty;
                string userId = string.Empty;
                string roomId = string.Empty;
                int roomIdInt=0;
                if (jsonObj != null)
                {
                    sessionKey = jsonObj["SessionKey"].ToString();
                    date = jsonObj["Date"].ToString();
                    roomId = jsonObj["RoomID"].ToString();
                    roomIdInt = int.Parse(roomId);
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        // Converting the string of date into date time for comparison purpose
                        DateTime selectedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        IQueryable<MeetingModel> timeSlots = db.Meetings.Where(u => u.BookingDate >= selectedDate && u.RoomID==roomIdInt);

                        List<MeetingDTO> slotList = new List<MeetingDTO>();
                        Mapper.Initialize(cfg => cfg.CreateMap<MeetingModel, MeetingDTO>());

                        foreach (MeetingModel mm in timeSlots)
                        {
                            MeetingDTO newDto = Mapper.Map<MeetingDTO>(mm);
                            slotList.Add(newDto);
                        }

                        return Ok(slotList);
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "GetTimeSlotByRoomId", ex);
                return BadRequest("There is an issue in the system. Contact administrator.");
            }
        }

        [HttpPost]
        public IHttpActionResult Booking(JObject jsonObj)
        {
            try
            {
                string sessionKey = string.Empty;
                string userId = string.Empty;
                string title = string.Empty;
                string purpose = string.Empty;
                string bookingDate = string.Empty;
                string startingTime = string.Empty;
                string sqlStart = string.Empty;
                string endingTime = string.Empty;
                string sqlEnd = string.Empty;
                string roomId = string.Empty;
                int recurenceType = 0;
                string sccStartDate = string.Empty;
                string sccEndDate = string.Empty;

                if (jsonObj != null)
                {
                    sessionKey = jsonObj["SessionKey"].ToString();
                    title = jsonObj["Title"].ToString();
                    purpose = jsonObj["Purpose"].ToString();
                    bookingDate = jsonObj["BookingDate"].ToString();
                    startingTime = jsonObj["StartingTime"].ToString();
                    endingTime = jsonObj["EndingTime"].ToString();
                    sqlStart = (Convert.ToInt32(jsonObj["StartingTime"]) + 1).ToString(); // + 1 to staring time in order to perform between condition 
                    sqlEnd = (Convert.ToInt32(jsonObj["EndingTime"]) - 1).ToString();     // - 1 to staring time in order to perform between condition 
                    roomId = jsonObj["RoomID"].ToString();
                    recurenceType = Convert.ToInt16(jsonObj["RecurrenceType"]);
                    sccStartDate = jsonObj["SCCStartDate"].ToString();
                    sccEndDate = jsonObj["SCCEndDate"].ToString();
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        if (recurenceType == 0) {
                            string tempQuery = "SELECT * FROM dbo.MeetingModel WHERE RoomID = " + Convert.ToInt32(roomId) + " AND BookingDate='" + bookingDate + " 00:00:00'" + " AND (" + sqlStart + " between StartingTime and EndingTime OR " + sqlEnd + " between StartingTime and EndingTime)";

                            // Checking whether this timeslot if booked
                            var bookedMeeting = db.Meetings.SqlQuery(tempQuery).ToList();

                            if (bookedMeeting.Count == 0)
                            {
                                MeetingModel meeting = new MeetingModel();
                                meeting.RoomID = Convert.ToInt32(roomId);
                                meeting.Title = title;
                                meeting.Purpose = purpose;
                                meeting.BookingDate = DateTime.ParseExact(bookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                meeting.StartingTime = startingTime;
                                meeting.EndingTime = endingTime;
                                meeting.CreatedBy = userId;
                                meeting.CreatedDate = DateTime.Now;
                                meeting.RecurenceType = recurenceType;
                                meeting.SCCStartDate = sccStartDate;
                                meeting.SCCEndDate = sccEndDate;

                                if (ModelState.IsValid)
                                {
                                    db.Meetings.Add(meeting);
                                    db.SaveChanges();

                                    IQueryable<MeetingModel> meetingList = db.Meetings.Where(u => u.RoomID == meeting.RoomID && u.BookingDate == meeting.BookingDate && u.StartingTime == meeting.StartingTime && u.EndingTime == endingTime);

                                    return Ok(meetingList);
                                }
                            }
                            else
                            {
                                return BadRequest("This time slot has been booked.");
                            }
                        }
                        else
                        {
                            List<DateTime> dates=new List<DateTime>();
                            DateTime newStartDate = DateTime.ParseExact(bookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            DateTime newEndDate = DateTime.ParseExact(sccEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var newRoomID = Convert.ToInt32(roomId);
                            var newPurpose = purpose;
                            var newStartingTime = startingTime;
                            var newEndingTime = endingTime;
                            var newRecurenceType = recurenceType;

                            if (recurenceType == 1) {
                                dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Daily);
                            }
                            else if(recurenceType == 2)
                            {
                                dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Weekly);
                            }
                            else if (recurenceType == 3)
                            {
                                dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Monthly);
                            }

                            int checkavalilable = 0;
                            foreach (var date in dates)
                            {
                                string tempQuery = "SELECT * FROM dbo.MeetingModel WHERE RoomID = " + Convert.ToInt32(roomId) + " AND BookingDate='" + bookingDate + " 00:00:00'" + " AND (" + sqlStart + " between StartingTime and EndingTime OR " + sqlEnd + " between StartingTime and EndingTime)";

                                // Checking whether this timeslot if booked
                                var bookedMeeting = db.Meetings.SqlQuery(tempQuery).ToList();

                                if (bookedMeeting.Count > 0)
                                {
                                    checkavalilable++;
                                }
                            }

                            if (checkavalilable == 0)
                            {
                                foreach (var date in dates)
                                {
                                    MeetingModel meeting = new MeetingModel();
                                    meeting.RoomID = Convert.ToInt32(roomId);
                                    meeting.Title = title;
                                    meeting.Purpose = purpose;
                                    meeting.BookingDate = DateTime.ParseExact(date.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    meeting.StartingTime = startingTime;
                                    meeting.EndingTime = endingTime;
                                    meeting.CreatedBy = userId;
                                    meeting.CreatedDate = DateTime.Now;
                                    meeting.RecurenceType = recurenceType;
                                    meeting.SCCStartDate = sccStartDate;
                                    meeting.SCCEndDate = sccEndDate;

                                    if (ModelState.IsValid)
                                    {
                                        db.Meetings.Add(meeting);
                                        db.SaveChanges();
                                    }
                                }
                                IQueryable<MeetingModel> meetingList = db.Meetings.Where(u => u.RoomID == newRoomID && u.Purpose == newPurpose && u.StartingTime == newStartingTime && u.EndingTime == newEndingTime && u.RecurenceType == newRecurenceType);

                                return Ok(meetingList);
                            }
                            else
                            {
                                return BadRequest("Time slot has been booked.");
                            }
                        }
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "Booking", ex);
                return BadRequest("There is an issue in the system. Contact administrator.");
            }
        }

        [HttpPost]
        public IHttpActionResult AddAttendee(JObject jsonObj)
        {
            try
            {
                string sessionKey = string.Empty;
                string userId = string.Empty;
                string meetingId = string.Empty;
                string users = string.Empty;
                List<string> userList = new List<string>();

                if (jsonObj != null)
                {
                    sessionKey = jsonObj["SessionKey"].ToString();
                    meetingId = jsonObj["MeetingID"].ToString();
                    var jArray = jsonObj["Users"]; // This will be array

                    foreach (JObject content in jArray.Children<JObject>())
                    {
                        foreach (JProperty prop in content.Properties())
                        {
                            userList.Add(prop.Value.ToString());
                        }
                    }
                }
                else {
                    return BadRequest();
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        MeetingModel mm = db.Meetings.Find(Convert.ToInt32(meetingId));

                        IQueryable<MeetingModel> meetingQuery = db.Meetings.Where(u => u.RoomID == mm.RoomID && u.Purpose == mm.Purpose && u.StartingTime == mm.StartingTime && u.EndingTime == mm.EndingTime && u.RecurenceType == mm.RecurenceType);

                        List<MeetingModel> meetingList = meetingQuery.ToList<MeetingModel>();

                        if (meetingList != null)
                        {
                            foreach (var meetingItem in meetingList)
                            {
                                foreach (var id in userList)
                                {
                                    ParticipantModel participant = new ParticipantModel();
                                    participant.MeetingID = Convert.ToInt32(meetingItem.ID);
                                    participant.UserID = Convert.ToInt32(id);
                                    participant.CreatedBy = userId;
                                    participant.CreatedDate = DateTime.Now;

                                    db.Participants.Add(participant);
                                    db.SaveChanges();
                                }
                            }

                            // After saving, sending invitation to the participants
                            CreateEmail(mm, userList);

                            //after saving, sending notification
                            sendNotification(mm, userList);
                            return Ok("Success");
                        }
                        else
                        {
                            return BadRequest("Meeting Not Found.");
                        }
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "AddAttendee", ex);
                return BadRequest("There is an issue in the system. Contact administrator.");
            }
        }

        [HttpPost]
        public IHttpActionResult DeleteBooking(JObject jsonObj)
        {
            try
            {
                string sessionKey = string.Empty;
                string userId = string.Empty;
                string meetingId = string.Empty;

                if (jsonObj != null)
                {
                    sessionKey = jsonObj["SessionKey"].ToString();
                    meetingId = jsonObj["MeetingID"].ToString();
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        MeetingModel meeting = db.Meetings.Find(Convert.ToInt32(meetingId));

                        if (meeting != null)
                        {
                            // Delete all partipants added to this meeting before removing
                            IQueryable<ParticipantModel> participants = db.Participants.Where(m => m.MeetingID == meeting.ID);

                            if (participants.Count() > 0)
                            {
                                foreach (var obj in participants)
                                {
                                    db.Participants.Remove(obj);
                                }
                            }

                            db.Meetings.Remove(meeting);
                            db.SaveChanges();

                            return Ok("Success");
                        }

                        return NotFound();
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "AddAttendee", ex);
                return BadRequest("There is an issue in the system. Contact administrator.");
            }
        }

        #region Methods
        //enum for various patterns
        public enum OccurrenceRate
        {
            Weekly,
            Daily,
            Monthly
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

        private bool IsSessionValid(string sessionKey, out string userId)
        {
            int validity = Config.SessionKeyValidity;

            bool isValid = false;
            string temp = string.Empty;
            UserModel userModel = new UserModel();
            userId = string.Empty;

            if (!string.IsNullOrEmpty(sessionKey))
            {
                SessionModel ss = db.Sessions.FirstOrDefault(s => s.SessionKey.Equals(sessionKey));

                if (ss != null)
                {
                    userModel = db.Users.Find(ss.UserID);

                    userId = userModel.Username;
                    isValid = true;

                    // No Longer needed, similar to fb, no time out, always login unless logout
                    //DateTime ssDt = new DateTime();

                    //if (ss.UpdatedDate.HasValue)
                    //{
                    //    ssDt = ss.UpdatedDate.Value;
                    //}
                    //else if (ss.CreatedDate.HasValue)
                    //{
                    //    ssDt = ss.CreatedDate.Value;
                    //}

                    //ssDt = ssDt.AddSeconds(validity);

                    //TimeSpan span = DateTime.Now.Subtract(ssDt);
                    //TimeSpan validitySpan = TimeSpan.FromSeconds(validity);

                    //if (TimeSpan.Compare(span, validitySpan) == -1)   // -1: span is shorter than validitySpan, 0: both are equal, 1: span is longer than validitySpan
                    //{
                    //    userId = userModel.Username;
                    //    isValid = true;
                    //}
                }
            }

            return isValid;
        }

        private void CreateEmail(MeetingModel mm, List<string> userList)
        {
            try {
                string location = mm.Room.Name;
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

                string path = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["EmailPath"].ToString());

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, title + ".ics");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath); // Delete old one
                }

                FileStream fs = File.Create(filePath);
                fs.Close();
                File.WriteAllLines(filePath, contents);

                //METHOD TO SEND EMAIL IS CALLED
                SendMail(filePath, userList);
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "CreateEmail", ex);
            }
            
        }

        private void SendMail(string filePath, List<string> userList)
        {
            //CONFIGURE BASIC CONTENTS OF AN EMAIL
            bool hasValidEmail = false;

            string FromName = "Room Booking System";
            //string FromEmail = "agilenamic@gmail.com";
            string FromEmail = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["emailID"].ToString());

            MailMessage mailMessage = new MailMessage();

            // Loop through the participants to add into ToEmail, username = email
            foreach (var id in userList)
            {
                UserModel user = db.Users.Find(Convert.ToInt32(id));

                if (user != null)
                {
                    hasValidEmail = true;
                    mailMessage.To.Add(new MailAddress(user.Username));
                }
            }
            
            //string ToName = "Testing";
            //string ToEmail = "chee.ann@hotmail.com";
            
            // Only Trigger sending email when there is at least one user being added
            if (hasValidEmail)
            {
                string emailPwd = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["emailPwd"].ToString());
                string smtpServer = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["emailPwd"].ToString());
                int smtpPort = Convert.ToInt16(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["smtpPort"].ToString()));
                SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
                smtp.Credentials = new System.Net.NetworkCredential(FromEmail, emailPwd);
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                mailMessage.From = new MailAddress(FromEmail, FromName);
                mailMessage.Subject = "Meeting Invitation from RBS";
                mailMessage.Body = "You have invited to attend a meeting, please take a look in the attachment.";

                //MAKE AN ATTACHMENT OUT OF THE .ICS FILE CREATED
                Attachment mailAttachment = new Attachment(filePath);

                //ADD THE ATTACHMENT TO THE EMAIL
                mailMessage.Attachments.Add(mailAttachment);
                smtp.Send(mailMessage);
            }
        }

        private void sendNotification(MeetingModel mm, List<string> userList) {
            try {
                string Title = "New Event";
                string message = mm.Title;

                foreach (var id in userList)
                {
                    UserModel user = db.Users.Find(Convert.ToInt32(id));

                    if (user != null){
                        string tokenID = user.TokenID.Trim();
                        tokenID = tokenID.Replace(" ","");
                        BLNotification.PushNotification(tokenID, message, "", Title, "");
                        Log.Error("Notification", context.STR_USER, "userid= "+ id + " token id:" + tokenID + " message:"+ message + " Title:"+Title);
                    }   
                }
            }
            catch(Exception ex) {
                Log.Error("Notification", context.STR_USER, "sendNotification", ex);
            }
        }
        #endregion
    }
}
