﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using log4net;
using System.Text;
using Microbrewit.Model;
using System.Net.Http;
using System.IdentityModel.Tokens;
using ServiceStack.Redis.Generic;
using ServiceStack.Text;
using Microbrewit.Model.DTOs;
using ServiceStack.Redis;
using System.Configuration;


namespace Microbrewit.Api.Util
{
    public class LoginValidationAttribute : ActionFilterAttribute
    {
        #region Private Fields
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly int expire = int.Parse(ConfigurationManager.AppSettings["expire"]);
        private static readonly string redisStore = ConfigurationManager.AppSettings["redis"];
	    #endregion

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var authentication = actionContext.Request.Headers.Authorization.Parameter.ToString();
            Log.Debug("auth: " + authentication);
            string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authentication));
            Log.Debug("decodedToken: " + decodedToken);
            string username = decodedToken.Substring(0, decodedToken.IndexOf(":"));
            string password = decodedToken.Substring(decodedToken.IndexOf(":") + 1);

            UserCredentials userCredentials = null;
            using (var context = new MicrobrewitContext())
            {
                userCredentials = context.UserCredentials.Include("User").Where(u => u.Username.Equals(username)).FirstOrDefault();

            }
            if (userCredentials != null && password.Equals(Encrypting.Decrypt(userCredentials.Password, userCredentials.SharedSecret)))
            {
                base.OnActionExecuting(actionContext);

               
                //JwtHeader jwtHeader = new JwtHeader{{typ, JWT})
                var jwtString = Encrypting.JWTDecrypt(userCredentials.User);

                using (var redisClient = new RedisClient(redisStore))
                    {
                         var timespan = TimeSpan.FromMinutes(expire);
                         redisClient.SetEntry(jwtString, userCredentials.Username, timespan);
                    }
                // seting token to header
                HttpContext.Current.Response.AddHeader("Authorization-Token", jwtString);
            }
            else
            {
               // returns unauthorised if not password maches.
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
        }
    }
}