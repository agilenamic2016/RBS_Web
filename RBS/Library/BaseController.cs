using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RBS.Library
{
    public abstract class BaseController : Controller
    {
        private RequestContext requestContext = new RequestContext();

        public Context context;

        public BaseController()
        {
            context = new Context();
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (Session["Context"] != null)
            {
                context = (Context)Session["Context"];
            }

            // Security and Permission check here.
            if (!context.IsLoginPage)
            {
                if (!context.IsAuthenticated)
                {
                    System.Web.HttpContext.Current.Response.Redirect(context.LOGIN_PAGE);
                }
            }

            // Disable browser cache
            System.Web.HttpContext.Current.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            System.Web.HttpContext.Current.Response.Cache.SetValidUntilExpires(false);
            System.Web.HttpContext.Current.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            System.Web.HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            System.Web.HttpContext.Current.Response.Cache.SetNoStore();
        }

        public Int32 checkIntNullableEmpty(object val)
        {
            return (val != null && val != DBNull.Value && !val.ToString().Trim().Equals("")) ? Convert.ToInt32(val.ToString()) : 0;
        }

        public Decimal checkDecimalNullableEmpty(object val)
        {
            return (val != null && val != DBNull.Value && !val.ToString().Trim().Equals("")) ? Convert.ToDecimal(val.ToString()) : 0;
        }

        public Double checkDoubleNullableEmpty(object val)
        {
            return (val != null && val != DBNull.Value && !val.ToString().Trim().Equals("")) ? Convert.ToDouble(val) : 0.00;
        }

    }
}