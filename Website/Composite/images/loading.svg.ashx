﻿<%@ WebHandler Language="C#" Class="loading" %>

using System;
using System.Web;

public class loading : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
        TimeSpan cacheDuration = TimeSpan.FromDays(7);
        
        context.Response.ContentType = "image/svg+xml";
        context.Response.Cache.SetCacheability(HttpCacheability.Public);
        context.Response.Cache.SetExpires(DateTime.Now.Add(cacheDuration));
        context.Response.Cache.SetMaxAge(cacheDuration);
        context.Response.CacheControl = HttpCacheability.Public.ToString();
        context.Response.Cache.SetValidUntilExpires(true);
       
        context.Response.WriteFile(context.Request.MapPath("loading.svg"));
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}