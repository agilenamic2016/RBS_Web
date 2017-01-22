using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Net.Sockets;
using RBS.Library;

namespace RBS.Notification
{
    public static class BLNotification
    {

        #region "PUSH ANDROID NOTIFICATION"
        public static void PushNotification(string deviceRegisteredID, string message, string tickerText, 
            string contentTitle, string msgToAct)
        {
            string deviceId = deviceRegisteredID;
            string nid = Guid.NewGuid().ToString();
            string apikey= "AIzaSyCllREF6HWX6ejSh81kNgx820Y1QGe9OXg";

            string response = string.Empty;
            string mta_new = msgToAct;

            if (deviceId != string.Empty)
            {
                string postData = string.Empty;
                if (deviceRegisteredID.Length > 100)
                {
                    postData = "{ \"registration_ids\": [ \"" + deviceId + "\" ], " +
                                        "\"data\": {\"activityMessage\":\"" + mta_new + "\", " +
                                        "\"tickerText\":\"" + tickerText + "\", " +
                                        "\"title\":\"" + contentTitle + "\", " +
                                        "\"flag\":\"" + 0 + "\", " +
                                        "\"message\": \"" + message + "\"}}";

                    response = SendGCMNotification(apikey, postData);
                    Log.Error("android Notification", "", "sendNotification=" + response);
                }
                else
                {
                    PushAppleNotification(deviceRegisteredID, message, mta_new, 0, contentTitle, 0);
                }
            }
        }


        private static string SendGCMNotification(string apiKey, string postData, string postDataContentType = "application/json")
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateServerCertificate);

            //
            //  MESSAGE CONTENT
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            //
            //  CREATE REQUEST
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("https://android.googleapis.com/gcm/send");
            Request.Method = "POST";
            Request.KeepAlive = false;
            Request.ContentType = postDataContentType;
            Request.Headers.Add(string.Format("Authorization: key={0}", apiKey));
            Request.ContentLength = byteArray.Length;

            Stream dataStream = Request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //
            //  SEND MESSAGE
            try
            {
                WebResponse Response = Request.GetResponse();
                HttpStatusCode ResponseCode = ((HttpWebResponse)Response).StatusCode;
                if (ResponseCode.Equals(HttpStatusCode.Unauthorized) || ResponseCode.Equals(HttpStatusCode.Forbidden))
                {
                    var text = "Unauthorized - need new token";
                   // ErrorLog.LogErrorDb("Error", System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(),System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), text.ToString());
                }
                else if (!ResponseCode.Equals(HttpStatusCode.OK))
                {
                    var text = "Response from web service isn't OK";
                    //ErrorLog.LogErrorDb("Error", System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(),System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), text.ToString());
                }

                StreamReader Reader = new StreamReader(Response.GetResponseStream());
                string responseLine = Reader.ReadToEnd();
                Reader.Close();

                return responseLine;
            }
            catch(Exception ex)
            {
                //ErrorLog.LogErrorDb("Error", System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(),System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), ex.ToString());
                return "error";
            }
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static void SavePromoNotification(string promo_id, string notification_id)
        {
            //try
            //{
            //    promo_notification pn = new promo_notification();
            //    pn.ID = Guid.NewGuid().ToString();
            //    pn.PROMO_ID = promo_id;
            //    pn.NOTIFICATION_ID = notification_id;
            //    dataContext.AddTopromo_notification(pn);
            //    dataContext.SaveChanges();
            //}
            //catch (Exception ex)
            //{
            //    ErrorLog.LogErrorDb("Error", System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(),
            //        System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), ex.ToString());
            //}
        }

        private static void SaveNotification(string nid, string reg_id, string ticker, string title, string text, string message, string response, int notification_type)
        {
            //try
            //{
            //    notification n = new notification();
            //    n.ID = nid;
            //    n.REGISTRATION_ID = reg_id;
            //    n.TICKER = ticker;
            //    n.CONTENT_TITLE = title;
            //    n.CONTENT_TEXT = text;
            //    n.CONTENT_MESSAGE = message;
            //    n.RESPONSE_MESSAGE = response;
            //    n.DATE_PUSH = CustomClass.Common.ToDateTime(DateTime.Now);
            //    n.NOTIFICATION_TYPE = notification_type;
            //    dataContext.AddTonotifications(n);
            //    dataContext.SaveChanges();
            //}
            //catch (Exception ex)
            //{
            //    ErrorLog.LogErrorDb("Error", System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(),
            //        System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), ex.ToString());
            //}
        }
        #endregion

        #region "PUSH APPLE NOTIFICATION"
        public static void PushAppleNotification(string DeviceRegisteredID, string message, string mta, int notification_type, string title,int devtype = 2)
        {
            try
            {
                //system_config sc = dataContext.system_config.FirstOrDefault();

                // var payload1 = new NotificationPayload("Device token","Message",Badge,"Sound");
                var payload1 = new NotificationPayload(DeviceRegisteredID, message, "1", 1, "default", title);
                string APNS_P12_KEY = "pushiStoneRBS.p12";
                string APNS_P12_PWD = "P@ssword1";

                payload1.AddCustom("flag", notification_type);
                payload1.AddCustom("mta", mta);

                //string path = "C:\\Users\\Programmer\\Desktop\\Moon-APNS-master\\Libraries";
                //string path = "D:\\DEV_PROJECT\\SKLIEW_LOCAL\\MOBILE_REWARDS\\MOBILE_REWARDS_SERVER\\MOBILE_REWARDS_SERVER.Lib.BizLogic\\bin";
                //string path = "C:\\Project\\PSSB_PROJECT\\MOBILE_REWARDS\\bin";
                string path2 = (System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:///", "");
                path2 = path2.Replace("/", "\\");
                path2 = path2.Substring(0, path2.LastIndexOf("bin") + 3);

                var p = new List<NotificationPayload> { payload1 };
                string responseMessage = string.Empty;

                if (devtype == 1)//use development cert
                {
                    var push = new PushAppleNotification(true, path2 + "\\" + APNS_P12_KEY, APNS_P12_PWD, false); //local
                    var rejected = push.SendToApple(p);
                    foreach (var item in rejected)
                    {
                        responseMessage += (item + " | ");
                    }
                }
                else // use production
                {
                    var push = new PushAppleNotification(false, path2 + "\\" + APNS_P12_KEY, APNS_P12_PWD, true);  //production
                    var rejected = push.SendToApple(p);
                    foreach (var item in rejected)
                    {
                        responseMessage += (item + " | ");
                    }
                }


            }
            catch (Exception ex)
            {
                //ErrorLog.LogErrorDb("Error", System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(),System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), ex.ToString());
            }
        }
        #endregion
    }

    public class NotificationPayload
    {
        public NotificationAlert Alert { get; set; }

        public string DeviceToken { get; set; }

        public int? Badge { get; set; }

        public string Sound { get; set; }

        public string Content { get; set; }

        internal int PayloadId { get; set; }



        public Dictionary<string, object[]> CustomItems
        {
            get;
            private set;
        }

        public NotificationPayload(string deviceToken)
        {
            DeviceToken = deviceToken;
            Alert = new NotificationAlert();
            CustomItems = new Dictionary<string, object[]>();
        }

        public NotificationPayload(string deviceToken, string alert, string title)
        {
            DeviceToken = deviceToken;
            Alert = new NotificationAlert() { Body = alert, Title = title };
            CustomItems = new Dictionary<string, object[]>();
        }

        public NotificationPayload(string deviceToken, string alert, int badge, string title)
        {
            DeviceToken = deviceToken;
            Alert = new NotificationAlert() { Body = alert, Title = title };
            Badge = badge;
            CustomItems = new Dictionary<string, object[]>();
        }

        public NotificationPayload(string deviceToken, string alert, int badge, string sound, string title)
        {
            DeviceToken = deviceToken;
            Alert = new NotificationAlert() { Body = alert, Title = title };
            Badge = badge;
            Sound = sound;
            CustomItems = new Dictionary<string, object[]>();
        }

        public NotificationPayload(string deviceToken, string alert, string content, int badge, string sound, string title)
        {
            DeviceToken = deviceToken;
            Alert = new NotificationAlert() { Body = alert, Title = title };
            Content = content;
            Badge = badge;
            Sound = sound;
            CustomItems = new Dictionary<string, object[]>();
        }

        public void AddCustom(string key, params object[] values)
        {
            if (values != null)
                this.CustomItems.Add(key, values);
        }

        public string ToJson()
        {
            JObject json = new JObject();

            JObject aps = new JObject();

            if (!this.Alert.IsEmpty)
            {
                if (!string.IsNullOrEmpty(this.Alert.Body)
                    && string.IsNullOrEmpty(this.Alert.LocalizedKey)
                    && string.IsNullOrEmpty(this.Alert.ActionLocalizedKey)
                    && (this.Alert.LocalizedArgs == null || this.Alert.LocalizedArgs.Count <= 0))
                {
                    JObject jsonAlert = new JObject();

                    if (!string.IsNullOrEmpty(this.Alert.Title))
                        jsonAlert["title"] = new JValue(this.Alert.Title);

                    if (!string.IsNullOrEmpty(this.Alert.Body))
                        jsonAlert["body"] = new JValue(this.Alert.Body);

                    aps["alert"] = jsonAlert;
                }
                else
                {
                    JObject jsonAlert = new JObject();

                    if (!string.IsNullOrEmpty(this.Alert.LocalizedKey))
                        jsonAlert["loc-key"] = new JValue(this.Alert.LocalizedKey);

                    if (this.Alert.LocalizedArgs != null && this.Alert.LocalizedArgs.Count > 0)
                        jsonAlert["loc-args"] = new JArray(this.Alert.LocalizedArgs.ToArray());

                    if (!string.IsNullOrEmpty(this.Alert.Title))
                        jsonAlert["title"] = new JValue(this.Alert.Title);

                    if (!string.IsNullOrEmpty(this.Alert.Body))
                        jsonAlert["body"] = new JValue(this.Alert.Body);

                    if (!string.IsNullOrEmpty(this.Alert.ActionLocalizedKey))
                        jsonAlert["action-loc-key"] = new JValue(this.Alert.ActionLocalizedKey);

                    aps["alert"] = jsonAlert;
                }
            }

            if (this.Badge.HasValue)
                aps["badge"] = new JValue(this.Badge.Value);

            if (!string.IsNullOrEmpty(this.Sound))
                aps["sound"] = new JValue(this.Sound);

            if (!string.IsNullOrEmpty(this.Content))
                aps["content-available"] = new JValue(this.Content);

            json["aps"] = aps;

            foreach (string key in this.CustomItems.Keys)
            {
                if (this.CustomItems[key].Length == 1)
                    json[key] = new JValue(this.CustomItems[key][0]);
                else if (this.CustomItems[key].Length > 1)
                    json[key] = new JArray(this.CustomItems[key]);
            }

            string rawString = json.ToString(Newtonsoft.Json.Formatting.None, null);

            StringBuilder encodedString = new StringBuilder();
            foreach (char c in rawString)
            {
                if ((int)c < 32 || (int)c > 127)
                    encodedString.Append("\\u" + String.Format("{0:x4}", Convert.ToUInt32(c)));
                else
                    encodedString.Append(c);
            }
            return rawString;// encodedString.ToString();
        }

        public override string ToString()
        {
            return ToJson();
        }
    }

    public class NotificationAlert
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NotificationAlert()
        {
            Title = null;
            Body = null;
            ActionLocalizedKey = null;
            LocalizedKey = null;
            LocalizedArgs = new List<object>();
        }

        /// <summary>
        /// Body Text of the Notification's Alert
        /// </summary>
        public string Body
        {
            get;
            set;
        }
        public string Title
        {
            get;
            set;
        }
        /// <summary>
        /// Action Button's Localized Key
        /// </summary>
        public string ActionLocalizedKey
        {
            get;
            set;
        }

        /// <summary>
        /// Localized Key
        /// </summary>
        public string LocalizedKey
        {
            get;
            set;
        }

        /// <summary>
        /// Localized Argument List
        /// </summary>
        public List<object> LocalizedArgs
        {
            get;
            set;
        }

        public void AddLocalizedArgs(params object[] values)
        {
            this.LocalizedArgs.AddRange(values);
        }

        /// <summary>
        /// Determines if the Alert is empty and should be excluded from the Notification Payload
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (!string.IsNullOrEmpty(Body)
                    || !string.IsNullOrEmpty(ActionLocalizedKey)
                    || !string.IsNullOrEmpty(LocalizedKey)
                    || (LocalizedArgs != null && LocalizedArgs.Count > 0))
                    return false;
                else
                    return true;
            }
        }
    }

    public class PushAppleNotification
    {
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private TcpClient _apnsClient;
        private SslStream _apnsStream;
        private X509Certificate _certificate;
        private X509CertificateCollection _certificates;

        public string P12File { get; set; }
        public string P12FilePassword { get; set; }


        // Default configurations for APNS
        private const string ProductionHost = "gateway.push.apple.com";
        private const string SandboxHost = "gateway.sandbox.push.apple.com";
        private const int NotificationPort = 2195;

        // Default configurations for Feedback Service
        private const string ProductionFeedbackHost = "feedback.push.apple.com";
        private const string SandboxFeedbackHost = "feedback.sandbox.push.apple.com";
        private const int FeedbackPort = 2196;


        private bool _conected = false;
        private bool _IsProduction = false;

        private readonly string _host;
        private readonly string _feedbackHost;

        private List<NotificationPayload> _notifications = new List<NotificationPayload>();
        private List<string> _rejected = new List<string>();

        private Dictionary<int, string> _errorList = new Dictionary<int, string>();


        public PushAppleNotification(bool useSandbox, string p12File, string p12FilePassword, bool isProduction)
        {
            if (isProduction)
                _IsProduction = true;

            if (useSandbox)
            {
                _host = SandboxHost;
                _feedbackHost = SandboxFeedbackHost;
            }
            else
            {
                _host = ProductionHost;
                _feedbackHost = ProductionFeedbackHost;
            }

            //Load Certificates in to collection.

            //avoid internal error, use this
            _certificate = new X509Certificate2(System.IO.File.ReadAllBytes(p12File), p12FilePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            //original is this
            //_certificate = string.IsNullOrEmpty(p12FilePassword)? new X509Certificate2(File.ReadAllBytes(p12File)): new X509Certificate2(File.ReadAllBytes(p12File), p12FilePassword);

            _certificates = new X509CertificateCollection { _certificate };

            // Loading Apple error response list.
            _errorList.Add(0, "No errors encountered");
            _errorList.Add(1, "Processing error");
            _errorList.Add(2, "Missing device token");
            _errorList.Add(3, "Missing topic");
            _errorList.Add(4, "Missing payload");
            _errorList.Add(5, "Invalid token size");
            _errorList.Add(6, "Invalid topic size");
            _errorList.Add(7, "Invalid payload size");
            _errorList.Add(8, "Invalid token");
            _errorList.Add(255, "None (unknown)");
        }

        public List<string> SendToApple(List<NotificationPayload> queue)
        {
            //Logger.Info("Payload queue received.");
            _notifications = queue;
            if (queue.Count < 8999)
            {
                SendQueueToapple(_notifications);
            }
            else
            {
                const int pageSize = 8999;
                int numberOfPages = (queue.Count / pageSize) + (queue.Count % pageSize == 0 ? 0 : 1);
                int currentPage = 0;

                while (currentPage < numberOfPages)
                {
                    _notifications = (queue.Skip(currentPage * pageSize).Take(pageSize)).ToList();
                    SendQueueToapple(_notifications);
                    currentPage++;
                }
            }
            //Close the connection
            Disconnect();
            return _rejected;
        }

        private void SendQueueToapple(IEnumerable<NotificationPayload> queue)
        {
            int i = 1000;
            foreach (var item in queue)
            {
                if (!_conected)
                {
                    Connect(_host, NotificationPort, _certificates);
                    var response = new byte[6];
                    _apnsStream.BeginRead(response, 0, 6, ReadResponse, new MyAsyncInfo(response, _apnsStream));
                }
                try
                {
                    if (item.DeviceToken.Length == 64) //check lenght of device token, if its shorter or longer stop generating Payload.
                    {
                        item.PayloadId = i;
                        byte[] payload = GeneratePayload(item);
                        _apnsStream.Write(payload);
                        //Logger.Info("Notification successfully sent to APNS server for Device Toekn : " + item.DeviceToken);
                        Thread.Sleep(1000); //Wait to get the response from apple.
                    }
                    else { }
                       // Logger.Error("Invalid device token length, possible simulator entry: " + item.DeviceToken);
                }
                catch (Exception ex)
                {
                    //Logger.Error("An error occurred on sending payload for device token {0} - {1}", item.DeviceToken, ex.Message);
                    _conected = false;
                }
                i++;
            }
        }

        private void ReadResponse(IAsyncResult ar)
        {
            if (!_conected)
                return;
            string payLoadId = "";
            int payLoadIndex = 0;
            try
            {
                var info = ar.AsyncState as MyAsyncInfo;
                info.MyStream.ReadTimeout = 100;
                if (_apnsStream.CanRead)
                {
                    var command = Convert.ToInt16(info.ByteArray[0]);
                    var status = Convert.ToInt16(info.ByteArray[1]);
                    var ID = new byte[4];
                    Array.Copy(info.ByteArray, 2, ID, 0, 4);

                    payLoadId = Encoding.Default.GetString(ID);
                    payLoadIndex = ((int.Parse(payLoadId)) - 1000);
                   // Logger.Error("Apple rejected palyload for device token : " + _notifications[payLoadIndex].DeviceToken);
                    //Logger.Error("Apple Error code : " + _errorList[status]);
                    //Logger.Error("Connection terminated by Apple.");
                    _rejected.Add(_notifications[payLoadIndex].DeviceToken);
                    _conected = false;
                }
            }
            catch (Exception ex)
            {
               // Logger.Error("An error occurred while reading Apple response for token {0} - {1}", _notifications[payLoadIndex].DeviceToken, ex.Message);
            }
        }

        private void Connect(string host, int port, X509CertificateCollection certificates)
        {
            //Logger.Info("Connecting to apple server.");
            try
            {
                _apnsClient = new TcpClient();
                _apnsClient.Connect(host, port);
            }
            catch (SocketException ex)
            {
              //  Logger.Error("An error occurred while connecting to APNS servers - " + ex.Message);
            }

            var sslOpened = OpenSslStream(host, certificates);

            if (sslOpened)
            {
                _conected = true;
                //Logger.Info("Conected.");
            }

        }

        private void Disconnect()
        {
            try
            {
                Thread.Sleep(500);
                _apnsClient.Close();
                _apnsStream.Close();
                _apnsStream.Dispose();
                _apnsStream = null;
                _conected = false;
                //Logger.Info("Disconnected.");
            }
            catch (Exception ex)
            {
                //Logger.Error("An error occurred while disconnecting. - " + ex.Message);
            }
        }

        private bool OpenSslStream(string host, X509CertificateCollection certificates)
        {
            //Logger.Info("Creating SSL connection.");
            _apnsStream = new SslStream(_apnsClient.GetStream(), false, validateServerCertificate, SelectLocalCertificate);

            try
            {
                if (_IsProduction)
                    _apnsStream.AuthenticateAsClient(host, certificates, System.Security.Authentication.SslProtocols.Tls, false);
                else
                    _apnsStream.AuthenticateAsClient(host, certificates, System.Security.Authentication.SslProtocols.Tls, false);
            }
            catch (System.Security.Authentication.AuthenticationException ex)
            {
                //Logger.Error(ex.Message);
                return false;
            }

            if (!_apnsStream.IsMutuallyAuthenticated)
            {
               // Logger.Error("SSL Stream Failed to Authenticate");
                return false;
            }

            if (!_apnsStream.CanWrite)
            {
                //Logger.Error("SSL Stream is not Writable");
                return false;
            }
            return true;
        }

        private bool validateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Dont care about server's cert
        }

        private X509Certificate SelectLocalCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return _certificate;
        }

        private static byte[] GeneratePayload(NotificationPayload payload)
        {
            try
            {
                //convert Devide token to HEX value.
                byte[] deviceToken = new byte[payload.DeviceToken.Length / 2];
                for (int i = 0; i < deviceToken.Length; i++)
                    deviceToken[i] = byte.Parse(payload.DeviceToken.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);

                var memoryStream = new MemoryStream();

                // Command
                memoryStream.WriteByte(1); // Changed command Type 

                //Adding ID to Payload
                memoryStream.Write(Encoding.ASCII.GetBytes(payload.PayloadId.ToString()), 0, payload.PayloadId.ToString().Length);

                //Adding ExpiryDate to Payload
                int epoch = (int)(DateTime.UtcNow.AddMinutes(300) - new DateTime(1970, 1, 1)).TotalSeconds;
                byte[] timeStamp = BitConverter.GetBytes(epoch);
                memoryStream.Write(timeStamp, 0, timeStamp.Length);

                byte[] tokenLength = BitConverter.GetBytes((Int16)32);
                Array.Reverse(tokenLength);
                // device token length
                memoryStream.Write(tokenLength, 0, 2);

                // Token
                memoryStream.Write(deviceToken, 0, 32);

                // String length
                string apnMessage = payload.ToJson();
                //Logger.Info("Payload generated for " + payload.DeviceToken + " : " + apnMessage);

                byte[] apnMessageLength = BitConverter.GetBytes((Int16)apnMessage.Length);
                Array.Reverse(apnMessageLength);

                // message length
                memoryStream.Write(apnMessageLength, 0, 2);

                // Write the message
                memoryStream.Write(Encoding.ASCII.GetBytes(apnMessage), 0, apnMessage.Length);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                //Logger.Error("Unable to generate payload - " + ex.Message);
                return null;
            }
        }

        public List<Feedback> GetFeedBack()
        {
            try
            {
                var feedbacks = new List<Feedback>();
                //Logger.Info("Connecting to feedback service.");

                if (!_conected)
                    Connect(_feedbackHost, FeedbackPort, _certificates);

                if (_conected)
                {
                    //Set up
                    byte[] buffer = new byte[38];
                    int recd = 0;
                    DateTime minTimestamp = DateTime.Now.AddYears(-1);

                    //Get the first feedback
                    recd = _apnsStream.Read(buffer, 0, buffer.Length);
                    //Logger.Info("Feedback response received.");

                    if (recd == 0) { }
                        //Logger.Info("Feedback response is empty.");

                    //Continue while we have results and are not disposing
                    while (recd > 0)
                    {
                        //Logger.Info("processing feedback response");
                        var fb = new Feedback();

                        //Get our seconds since 1970 ?
                        byte[] bSeconds = new byte[4];
                        byte[] bDeviceToken = new byte[32];

                        Array.Copy(buffer, 0, bSeconds, 0, 4);

                        //Check endianness
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(bSeconds);

                        int tSeconds = BitConverter.ToInt32(bSeconds, 0);

                        //Add seconds since 1970 to that date, in UTC and then get it locally
                        fb.Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(tSeconds).ToLocalTime();


                        //Now copy out the device token
                        Array.Copy(buffer, 6, bDeviceToken, 0, 32);

                        fb.DeviceToken = BitConverter.ToString(bDeviceToken).Replace("-", "").ToLower().Trim();

                        //Make sure we have a good feedback tuple
                        if (fb.DeviceToken.Length == 64 && fb.Timestamp > minTimestamp)
                        {
                            //Raise event
                            //this.Feedback(this, fb);
                            feedbacks.Add(fb);
                        }

                        //Clear our array to reuse it
                        Array.Clear(buffer, 0, buffer.Length);

                        //Read the next feedback
                        recd = _apnsStream.Read(buffer, 0, buffer.Length);
                    }
                    //clode the connection here !
                    Disconnect();
                    //if (feedbacks.Count > 0)
                        //Logger.Info("Total {0} feedbacks received.", feedbacks.Count);
                    return feedbacks;
                }
            }
            catch (Exception ex)
            {
                //Logger.Error("Error occurred on receiving feed back. - " + ex.Message);
                return null;
            }
            return null;
        }
    }

    public class MyAsyncInfo
    {
        public Byte[] ByteArray { get; set; }
        public SslStream MyStream { get; set; }

        public MyAsyncInfo(Byte[] array, SslStream stream)
        {
            ByteArray = array;
            MyStream = stream;
        }
    }

    public class Feedback
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public Feedback()
        {
            this.DeviceToken = string.Empty;
            this.Timestamp = DateTime.MinValue;
        }

        /// <summary>
        /// Device Token string in hex form without any spaces or dashes
        /// </summary>
        public string DeviceToken
        {
            get;
            set;
        }

        /// <summary>
        /// Timestamp of the Feedback for when Apple received the notice to stop sending notifications to the device
        /// </summary>
        public DateTime Timestamp
        {
            get;
            set;
        }
    }
}