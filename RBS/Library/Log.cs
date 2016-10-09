using System;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Data.Entity.Core.Objects;
using RBS.DAL;
using RBS.Models;

namespace RBS.Library
{
    public static class Log
    {
        private static RBSContext db = new RBSContext();
        private static Context context = new Context();

        public static void Error(string userID, string page, string action)
        {
            LogModel log = new LogModel
            {
                Type = "ERROR",
                Page = page,
                Action = action,
                Source = userID,
                CreatedBy = "System",
                CreatedDate = DateTime.Now
            };

            db.Logs.Add(log);
            db.SaveChanges();
        }

        public static void Error(string userID, string page, string action, Exception e)
        {
            LogModel log = new LogModel
            {
                Type = "ERROR",
                Page = page,
                Action = action,
                Source = userID,
                Data = e.ToString(),
                CreatedBy = "System",
                CreatedDate = DateTime.Now
            };

            db.Logs.Add(log);
            db.SaveChanges();
        }

        public static void Info(string userID, string page, string action)
        {
            if (!action.Equals(context.STR_READ))
            {
                LogModel log = new LogModel
                {
                    Type = "INFO",
                    Page = page,
                    Action = action,
                    Source = userID,
                    CreatedBy = "System",
                    CreatedDate = DateTime.Now
                };

                db.Logs.Add(log);
                db.SaveChanges();
            }
        }

        public static void Info(string page, string action, string data, string userID)
        {
            LogModel log = new LogModel
            {
                Type = "INFO",
                Page = page,
                Action = action,
                Data = data,
                Source = userID,
                CreatedBy = "System",
                CreatedDate = DateTime.Now
            };

            db.Logs.Add(log);
            db.SaveChanges();
        }

        public static void Debug(string page, string action, string data, string userID)
        {
            // Log when Debugging switch is on
            if (Config.DebuggingMode.ToUpper().Equals("ON"))
            {
                LogModel log = new LogModel
                {
                    Type = "DEBUG",
                    Page = page,
                    Action = action,
                    Data = data,
                    Source = userID,
                    CreatedBy = "System",
                    CreatedDate = DateTime.Now
                };

                db.Logs.Add(log);
                db.SaveChanges();
            }
        }
    }
}