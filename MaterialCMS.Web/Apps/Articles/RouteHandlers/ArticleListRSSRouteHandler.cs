﻿using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Routing;
using MaterialCMS.Apps;
using MaterialCMS.Entities.Documents.Web;
using MaterialCMS.Helpers;
using MaterialCMS.Services;
using MaterialCMS.Web.Apps.Articles.Pages;
using MaterialCMS.Website.Routing;

namespace MaterialCMS.Web.Apps.Articles.RouteHandlers
{
    public class ArticleListRSSRouteHandler : IMaterialCMSRouteHandler
    {
        private readonly IControllerManager _controllerManager;
        private readonly IDocumentService _documentService;

        public ArticleListRSSRouteHandler(IDocumentService documentService, IControllerManager controllerManager)
        {
            _documentService = documentService;
            _controllerManager = controllerManager;
        }

        public int Priority
        {
            get { return 50; }
        }

        public bool Handle(RequestContext context)
        {
            ArticleList articleList = GetArticleList(context.HttpContext.Request.Url.AbsolutePath);
            if (articleList != null)
            {
                IControllerFactory controllerFactory = _controllerManager.ControllerFactory;
                var controller = controllerFactory.CreateController(context, "ArticleRSS") as Controller;
                controller.ControllerContext = new ControllerContext(context, controller);
                var routeValueDictionary = new RouteValueDictionary();
                routeValueDictionary["controller"] = "ArticleRSS";
                routeValueDictionary["action"] = "Show";
                routeValueDictionary["page"] = articleList;
                controller.RouteData.Values.Merge(routeValueDictionary);
                controller.RouteData.DataTokens["app"] = MaterialCMSApp.AppWebpages[articleList.GetType()];

                var asyncController = (controller as IAsyncController);
                asyncController.BeginExecute(context, asyncController.EndExecute, null);
                return true;
            }
            return false;
        }

        public Webpage GetWebpage(string url)
        {
            return GetArticleList(url);
        }

        private ArticleList GetArticleList(string url)
        {
            string fileName = Path.GetFileName(url);
            if (fileName == null || !fileName.Equals("rss.xml", StringComparison.InvariantCultureIgnoreCase))
                return null;
            string containerName = url.Substring(1, url.Length - 9);
            return _documentService.GetDocumentByUrl<ArticleList>(containerName);
        }
    }
}