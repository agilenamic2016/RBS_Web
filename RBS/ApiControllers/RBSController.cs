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
using System.Text;

namespace RBS.ApiControllers
{
    public class RBSController : ApiController
    {
        private RBSContext db = new RBSContext();
        private Context context = new Context();
        //asd
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
                    //Log.Error("login info:"+tokenId, context.STR_USER, context.STR_LOGIN);
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

                        //string datetimenow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        //DateTime datetimenowfromzero= DateTime.ParseExact(datetimenow, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        //IQueryable<MeetingModel> meetings = db.Meetings.Where(a => a.BookingDate >= datetimenowfromzero);
                        //var queryUSer = db.Users.Where(c => c.ID == id).FirstOrDefault();
                        //string userName = queryUSer.Username;
                        //IQueryable<MeetingModel> meetings = from ml in db.Meetings
                        //                                    join pl in db.Participants on ml.ID equals pl.MeetingID
                        //                                    where (pl.UserID == id || ml.CreatedBy == userName) && ml.BookingDate >= datetimenowfromzero
                        //                                    select ml;
                        var todayDate = DateTime.Today;
                        var startingTime = MilitaryTime.ChangeToMilitaryTime(DateTime.Now);

                        var queryUSer = db.Users.Where(c => c.ID == id).FirstOrDefault();
                        string userName = queryUSer.Username;
                        string tempQuery = "SELECT A.* FROM MeetingModel A left JOIN ParticipantModel B on A.ID = B.MeetingID "
                                         + "WHERE (B.UserID='" + selectedUserId + "' or A.CreatedBy='" + userName + "') AND BookingDate >= '" + todayDate + "'";

                        var meetings = db.Meetings.SqlQuery(tempQuery).ToList().AsQueryable();

                        List < MeetingWithName > meetingList = new List<MeetingWithName>();
                        Mapper.Initialize(cfg => cfg.CreateMap<MeetingModel, MeetingDTO>());

                        foreach (MeetingModel mm in meetings)
                        {
                            MeetingDTO newDto = Mapper.Map<MeetingDTO>(mm);

                            MeetingWithName MWN = new MeetingWithName();
                            MWN.ID = newDto.ID;
                            MWN.RoomID = newDto.RoomID;
                            MWN.Title = newDto.Title;
                            MWN.Purpose = newDto.Purpose;
                            MWN.BookingDate = newDto.BookingDate;
                            MWN.StartingTime = newDto.StartingTime;
                            MWN.EndingTime = newDto.EndingTime;
                            MWN.CreatedBy = newDto.CreatedBy;
                            MWN.CreatedDate = newDto.CreatedDate;
                            MWN.UpdatedBy = newDto.UpdatedBy;
                            MWN.UpdatedDate = newDto.UpdatedDate;
                            MWN.RecurenceType = newDto.RecurenceType;
                            MWN.SCCStartDate = newDto.SCCStartDate;
                            MWN.SCCEndDate = newDto.SCCEndDate;

                            UserModel ss = db.Users.FirstOrDefault(s => s.Username.Equals(newDto.CreatedBy));
                            MWN.UserName = ss.Name;

                            meetingList.Add(MWN);
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

                        List<MeetingWithName> slotList = new List<MeetingWithName>();
                        Mapper.Initialize(cfg => cfg.CreateMap<MeetingModel, MeetingDTO>());

                        foreach (MeetingModel mm in timeSlots)
                        {
                            MeetingDTO newDto = Mapper.Map<MeetingDTO>(mm);
                            MeetingWithName MWN = new MeetingWithName();
                            MWN.ID = newDto.ID;
                            MWN.RoomID = newDto.RoomID;
                            MWN.Title = newDto.Title;
                            MWN.Purpose = newDto.Purpose;
                            MWN.BookingDate = newDto.BookingDate;
                            MWN.StartingTime = newDto.StartingTime;
                            MWN.EndingTime = newDto.EndingTime;
                            MWN.CreatedBy = newDto.CreatedBy;
                            MWN.CreatedDate = newDto.CreatedDate;
                            MWN.UpdatedBy = newDto.UpdatedBy;
                            MWN.UpdatedDate = newDto.UpdatedDate;
                            MWN.RecurenceType = newDto.RecurenceType;
                            MWN.SCCStartDate = newDto.SCCStartDate;
                            MWN.SCCEndDate = newDto.SCCEndDate;

                            UserModel ss = db.Users.FirstOrDefault(s => s.Username.Equals(newDto.CreatedBy));
                            MWN.UserName = ss.Name;

                            slotList.Add(MWN);
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
                string BookedMsg = "Time slot has been booked:";
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
                                meeting.Notification = "60";
                                meeting.NotificationStatus = "0";

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
                                int BookedCount = 1;
                                foreach (MeetingModel bm in bookedMeeting)
                                {
                                    
                                    DateTime bookedMeetingDate = Convert.ToDateTime(bm.BookingDate);
                                    string BookedMeetingDateStr = bookedMeetingDate.ToString("dd/MM/yyyy");
                                    var bookedMeetingStartTime = bm.StartingTime;
                                    var bookedMeetingEndTime = bm.EndingTime;
                                    var bookedMeetingOwner = bm.CreatedBy;
                                    var bookedMeetingTitle = bm.Title;
                                    UserModel ss = db.Users.FirstOrDefault(s => s.Username.Equals(bookedMeetingOwner));
                                    var intervalTime = bookedMeetingStartTime.Substring(0, 2) + ":" + bookedMeetingStartTime.Substring(2, 2) + "-" + bookedMeetingEndTime.Substring(0, 2) + ":" + bookedMeetingEndTime.Substring(2, 2);
                                    BookedMsg = BookedMsg + "\n" + BookedCount.ToString() + ")" + " Meeting: " + bookedMeetingTitle + "\n   Date Time: " + BookedMeetingDateStr + " " + intervalTime + "\n   Booked By: " + ss.Name;
                                    BookedCount++;
                                }
                                // To be returned the user who booking this time slot
                                return BadRequest(BookedMsg);
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
                            string BookedMsgRecurrecnce = "Time slot has been booked:";
                            int BookedMsgRecurrecnceCount = 1;
                            foreach (var date in dates)
                            {

                                var dateToCompare = date.ToString("yyyy-MM-dd");
                                string tempQuery = "SELECT * FROM dbo.MeetingModel WHERE RoomID = " + Convert.ToInt32(roomId) + " AND BookingDate='" + dateToCompare + " 00:00:00'" + " AND (" + sqlStart + " between StartingTime and EndingTime OR " + sqlEnd + " between StartingTime and EndingTime)";

                                // Checking whether this timeslot if booked
                                var bookedMeeting = db.Meetings.SqlQuery(tempQuery).ToList();

                                if (bookedMeeting.Count > 0)
                                {
                                    checkavalilable++;

                                    
                                    foreach (MeetingModel bm in bookedMeeting)
                                    {
                                        DateTime bookedMeetingDate = Convert.ToDateTime(bm.BookingDate);
                                        string BookedMeetingDateStr = bookedMeetingDate.ToString("dd/MM/yyyy");
                                        var bookedMeetingStartTime = bm.StartingTime;
                                        var bookedMeetingEndTime = bm.EndingTime;
                                        var bookedMeetingOwner = bm.CreatedBy;
                                        var bookedMeetingTitle = bm.Title;
                                        UserModel ss = db.Users.FirstOrDefault(s => s.Username.Equals(bookedMeetingOwner));
                                        var intervalTime = bookedMeetingStartTime.Substring(0, 2) + ":" + bookedMeetingStartTime.Substring(2, 2) + "-" + bookedMeetingEndTime.Substring(0, 2) + ":" + bookedMeetingEndTime.Substring(2, 2);
                                        BookedMsgRecurrecnce = BookedMsgRecurrecnce + "\n" + BookedMsgRecurrecnceCount.ToString() + ")" + " Meeting: " + bookedMeetingTitle + "\n   Date Time: " + BookedMeetingDateStr + " " + intervalTime + "\n   Booked By: " + ss.Name;
                                        BookedMsgRecurrecnceCount++;
                                    }
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
                                    meeting.Notification = "60";
                                    meeting.NotificationStatus = "0";
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
                                // To be returned the user who booking this time slot
                                return BadRequest(BookedMsgRecurrecnce);
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
        public IHttpActionResult BookingNew(JObject jsonObj)
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
                string BookedMsg = "Time slot has been booked:";
                string notification = string.Empty;

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
                    notification = jsonObj["Notification"].ToString();
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        if (recurenceType == 0)
                        {
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
                                meeting.Notification = notification;
                                meeting.NotificationStatus = "0";

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
                                int BookedCount = 1;
                                foreach (MeetingModel bm in bookedMeeting)
                                {

                                    DateTime bookedMeetingDate = Convert.ToDateTime(bm.BookingDate);
                                    string BookedMeetingDateStr = bookedMeetingDate.ToString("dd/MM/yyyy");
                                    var bookedMeetingStartTime = bm.StartingTime;
                                    var bookedMeetingEndTime = bm.EndingTime;
                                    var bookedMeetingOwner = bm.CreatedBy;
                                    var bookedMeetingTitle = bm.Title;
                                    UserModel ss = db.Users.FirstOrDefault(s => s.Username.Equals(bookedMeetingOwner));
                                    var intervalTime = bookedMeetingStartTime.Substring(0, 2) + ":" + bookedMeetingStartTime.Substring(2, 2) + "-" + bookedMeetingEndTime.Substring(0, 2) + ":" + bookedMeetingEndTime.Substring(2, 2);
                                    BookedMsg = BookedMsg + "\n" + BookedCount.ToString() + ")" + " Meeting: " + bookedMeetingTitle + "\n   Date Time: " + BookedMeetingDateStr + " " + intervalTime + "\n   Booked By: " + ss.Name;
                                    BookedCount++;
                                }
                                // To be returned the user who booking this time slot
                                return BadRequest(BookedMsg);
                            }
                        }
                        else
                        {
                            List<DateTime> dates = new List<DateTime>();
                            DateTime newStartDate = DateTime.ParseExact(bookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            DateTime newEndDate = DateTime.ParseExact(sccEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var newRoomID = Convert.ToInt32(roomId);
                            var newPurpose = purpose;
                            var newStartingTime = startingTime;
                            var newEndingTime = endingTime;
                            var newRecurenceType = recurenceType;

                            if (recurenceType == 1)
                            {
                                dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Daily);
                            }
                            else if (recurenceType == 2)
                            {
                                dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Weekly);
                            }
                            else if (recurenceType == 3)
                            {
                                dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Monthly);
                            }

                            int checkavalilable = 0;
                            string BookedMsgRecurrecnce = "Time slot has been booked:";
                            int BookedMsgRecurrecnceCount = 1;
                            foreach (var date in dates)
                            {
                                var dateToCompare = date.ToString("yyyy-MM-dd");
                                string tempQuery = "SELECT * FROM dbo.MeetingModel WHERE RoomID = " + Convert.ToInt32(roomId) + " AND BookingDate='" + dateToCompare + " 00:00:00'" + " AND (" + sqlStart + " between StartingTime and EndingTime OR " + sqlEnd + " between StartingTime and EndingTime)";

                                // Checking whether this timeslot if booked
                                var bookedMeeting = db.Meetings.SqlQuery(tempQuery).ToList();

                                if (bookedMeeting.Count > 0)
                                {
                                    checkavalilable++;


                                    foreach (MeetingModel bm in bookedMeeting)
                                    {
                                        DateTime bookedMeetingDate = Convert.ToDateTime(bm.BookingDate);
                                        string BookedMeetingDateStr = bookedMeetingDate.ToString("dd/MM/yyyy");
                                        var bookedMeetingStartTime = bm.StartingTime;
                                        var bookedMeetingEndTime = bm.EndingTime;
                                        var bookedMeetingOwner = bm.CreatedBy;
                                        var bookedMeetingTitle = bm.Title;
                                        UserModel ss = db.Users.FirstOrDefault(s => s.Username.Equals(bookedMeetingOwner));
                                        var intervalTime = bookedMeetingStartTime.Substring(0, 2) + ":" + bookedMeetingStartTime.Substring(2, 2) + "-" + bookedMeetingEndTime.Substring(0, 2) + ":" + bookedMeetingEndTime.Substring(2, 2);
                                        BookedMsgRecurrecnce = BookedMsgRecurrecnce + "\n" + BookedMsgRecurrecnceCount.ToString() + ")" + " Meeting: " + bookedMeetingTitle + "\n   Date Time: " + BookedMeetingDateStr + " " + intervalTime + "\n   Booked By: " + ss.Name;
                                        BookedMsgRecurrecnceCount++;
                                    }
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
                                    meeting.Notification = notification;
                                    meeting.NotificationStatus = "0";
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
                                // To be returned the user who booking this time slot
                                return BadRequest(BookedMsgRecurrecnce);
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
                                    participant.CreatedBy = id;
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
        public IHttpActionResult UpdateBooking(JObject jsonObj)
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
                string meetingId = string.Empty;

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
                    meetingId = jsonObj["MeetingID"].ToString();
                }

                if (!String.IsNullOrEmpty(sessionKey))
                {
                    if (IsSessionValid(sessionKey, out userId))
                    {
                        if (recurenceType == 0)
                        {
                            // Exclude the meeting time itself
                            string tempQuery = "SELECT * FROM dbo.MeetingModel WHERE ID <> " + Convert.ToInt32(meetingId) + " AND RoomID = " + Convert.ToInt32(roomId) + " AND BookingDate='" + bookingDate + " 00:00:00'" + " AND (" + sqlStart + " between StartingTime and EndingTime OR " + sqlEnd + " between StartingTime and EndingTime)";

                            // Checking whether this timeslot if booked
                            var bookedMeeting = db.Meetings.SqlQuery(tempQuery).ToList();

                            if (bookedMeeting.Count == 0)
                            {
                                MeetingModel meeting = new MeetingModel();
                                meeting.ID = Convert.ToInt32(meetingId);
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
                                    db.Entry(meeting).State = EntityState.Modified;
                                    db.SaveChanges();

                                    IQueryable<MeetingModel> meetingList = db.Meetings.Where(u => u.RoomID == meeting.RoomID && u.BookingDate == meeting.BookingDate && u.StartingTime == meeting.StartingTime && u.EndingTime == endingTime);

                                    return Ok(meetingList);
                                }
                            }
                            else
                            {
                                // To be returned the user who booking this time slot
                                return BadRequest("This time slot has been booked.");
                            }
                        }
                        else
                        {
                            List<DateTime> dates = new List<DateTime>();
                            DateTime newStartDate = DateTime.ParseExact(bookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            DateTime newEndDate = DateTime.ParseExact(sccEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var newRoomID = Convert.ToInt32(roomId);
                            var newPurpose = purpose;
                            var newStartingTime = startingTime;
                            var newEndingTime = endingTime;
                            var newRecurenceType = recurenceType;

                            if (recurenceType == 1)
                            {
                                dates = GetOccurrences(newStartDate, newEndDate, OccurrenceRate.Daily);
                            }
                            else if (recurenceType == 2)
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
                                // Exclude the meeting time itself
                                string tempQuery = "SELECT * FROM dbo.MeetingModel WHERE ID <> " + Convert.ToInt32(meetingId) + " AND RoomID = " + Convert.ToInt32(roomId) + " AND BookingDate='" + bookingDate + " 00:00:00'" + " AND (" + sqlStart + " between StartingTime and EndingTime OR " + sqlEnd + " between StartingTime and EndingTime)";

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
                                    meeting.ID = Convert.ToInt32(meetingId);
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
                                        db.Entry(meeting).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                                IQueryable<MeetingModel> meetingList = db.Meetings.Where(u => u.RoomID == newRoomID && u.Purpose == newPurpose && u.StartingTime == newStartingTime && u.EndingTime == newEndingTime && u.RecurenceType == newRecurenceType);

                                return Ok(meetingList);
                            }
                            else
                            {
                                // To be returned the user who booking this time slot
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
        //Added here in order to get the standard value for RecurenceType Value, once - 0, Daily - 1, Weekly - 2, Monthly - 3
        public enum OccurrenceRate
        {
            Once,
            Daily,
            Weekly,
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
                SendMail(filePath, userList, mm);
            }
            catch (Exception ex)
            {
                Log.Error("Mobile App", context.STR_USER, "CreateEmail", ex);
            }
            
        }

        private void SendMail(string filePath, List<string> userList, MeetingModel mm)
        {
            //CONFIGURE BASIC CONTENTS OF AN EMAIL
            bool hasValidEmail = false;

            string FromName = "Room Booking System";
            string FromEmail = ConfigurationManager.AppSettings["emailID"].ToString();

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
            
            // Only Trigger sending email when there is at least one user being added
            if (hasValidEmail)
            {
                string emailPwd = ConfigurationManager.AppSettings["emailPwd"].ToString();
                string smtpServer = ConfigurationManager.AppSettings["smtpServer"].ToString();
                int smtpPort = Convert.ToInt16(ConfigurationManager.AppSettings["smtpPort"].ToString());
                SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
                smtp.Credentials = new System.Net.NetworkCredential(FromEmail, emailPwd);
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
                        //Log.Error("Notification", context.STR_USER, "userid= "+ id + " token id:" + tokenID + " message:"+ message + " Title:"+Title);
                    }   
                }

                //send to creator
                var creator = db.Users.Where(c => c.Username == mm.CreatedBy).FirstOrDefault();
                if (creator != null)
                {
                    string tokenID = creator.TokenID.Trim();
                    tokenID = tokenID.Replace(" ", "");
                    BLNotification.PushNotification(tokenID, message, "", Title, "");
                }
            }
            catch(Exception ex) {
                Log.Error("Notification", context.STR_USER, "sendNotification", ex);
            }
        }

        #endregion
    }
}
