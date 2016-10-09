using System;
using System.Linq;
using RBS.DAL;
using RBS.Models;

namespace RBS.Library
{
    public static class Config
	{
        private static Context context = new Context();

        public static int SessionKeyValidity
        {
            get
            {
                return convertToInt(getConfig("SessionKeyValidityInSecond"));
            }
        }

        public static int PageSize
        {
            get
            {
                return convertToInt(getConfig("PageSize"));
            }
        }

        public static string AllowedExtension
        {
            get
            {
                return getConfig("AllowedExtension");
            }
        }

        public static string DebuggingMode
        {
            get 
            {
                return getConfig("DebuggingMode");
            }
        }

        private static string getConfig(string key)
        {
            string str = string.Empty;

            RBSContext db = new RBSContext();

            ConfigModel config = db.Configs.FirstOrDefault(s => s.Key.Equals(key));

            if (config != null)
            {
                str = config.Value;
            }
			else
            {
                Log.Error(context.UserID, "Missing Config", key);
            }

            return str;
        }

		private static int convertToInt(string str)
        {
            int num = 0;

            if (str.Length > 0)
            {
                Int32.TryParse(str, out num);
            }

            return num;
        }
	}
}