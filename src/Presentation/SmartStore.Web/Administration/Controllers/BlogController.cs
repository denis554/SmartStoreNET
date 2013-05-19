﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Blogs;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Blogs;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class BlogController : AdminControllerBase
	{
		#region Fields

        private readonly IBlogService _blogService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerContentService _customerContentService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly AdminAreaSettings _adminAreaSettings;

        #endregion

		#region Constructors

        public BlogController(IBlogService blogService, ILanguageService languageService,
            IDateTimeHelper dateTimeHelper, ICustomerContentService customerContentService,
            ILocalizationService localizationService, IPermissionService permissionService,
            IUrlRecordService urlRecordService, AdminAreaSettings adminAreaSettings)
        {
            this._blogService = blogService;
            this._languageService = languageService;
            this._dateTimeHelper = dateTimeHelper;
            this._customerContentService = customerContentService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._urlRecordService = urlRecordService;
            this._adminAreaSettings = adminAreaSettings;
		}

		#endregion 
        
		#region Blog posts

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blogPosts = _blogService.GetAllBlogPosts(0, null, null, 0, _adminAreaSettings.GridPageSize, true);
            var gridModel = new GridModel<BlogPostModel>
            {
                Data = blogPosts.Select(x =>
                {
                    var m = x.ToModel();
                    if (x.StartDateUtc.HasValue)
                        m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                    if (x.EndDateUtc.HasValue)
                        m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.LanguageName = x.Language.Name;
                    m.Comments = x.ApprovedCommentCount + x.NotApprovedCommentCount;
                    return m;
                }),
                Total = blogPosts.TotalCount
            };
			return View(gridModel);
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blogPosts = _blogService.GetAllBlogPosts(0, null, null, command.Page - 1, command.PageSize, true);
            var gridModel = new GridModel<BlogPostModel>
            {
                Data = blogPosts.Select(x =>
                {
                    var m = x.ToModel();
                    if (x.StartDateUtc.HasValue)
                        m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                    if (x.EndDateUtc.HasValue)
                        m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.LanguageName = x.Language.Name;
                    m.Comments = x.ApprovedCommentCount + x.NotApprovedCommentCount;
                    return m;
                }),
                Total = blogPosts.TotalCount
            };
			return new JsonResult
			{
				Data = gridModel
			};
		}
        
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new BlogPostModel();
            //default values
            model.AllowComments = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(BlogPostModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var blogPost = model.ToEntity();
                blogPost.StartDateUtc = model.StartDate;
                blogPost.EndDateUtc = model.EndDate;
                blogPost.CreatedOnUtc = DateTime.UtcNow;
                _blogService.InsertBlogPost(blogPost);

                //search engine name
                var seName = blogPost.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(blogPost, seName, blogPost.LanguageId);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            return View(model);
        }

		public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = blogPost.ToModel();
            model.StartDate = blogPost.StartDateUtc;
            model.EndDate = blogPost.EndDateUtc;
            return View(model);
		}

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		public ActionResult Edit(BlogPostModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blogPost = _blogService.GetBlogPostById(model.Id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                blogPost = model.ToEntity(blogPost);
                blogPost.StartDateUtc = model.StartDate;
                blogPost.EndDateUtc = model.EndDate;
                _blogService.UpdateBlogPost(blogPost);

                //search engine name
                var seName = blogPost.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(blogPost, seName, blogPost.LanguageId);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            return View(model);
		}

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            _blogService.DeleteBlogPost(blogPost);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Deleted"));
			return RedirectToAction("List");
		}

		#endregion

        #region Comments

        public ActionResult Comments(int? filterByBlogPostId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            ViewBag.FilterByBlogPostId = filterByBlogPostId;
            var model = new GridModel<BlogCommentModel>();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Comments(int? filterByBlogPostId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            IList<BlogComment> comments;
            if (filterByBlogPostId.HasValue)
            {
                //filter comments by blog
                var blogPost = _blogService.GetBlogPostById(filterByBlogPostId.Value);
                comments = blogPost.BlogComments.OrderBy(bc => bc.CreatedOnUtc).ToList();
            }
            else
            {
                //load all blog comments
                comments = _customerContentService.GetAllCustomerContent<BlogComment>(0, null);
            }

            var gridModel = new GridModel<BlogCommentModel>
            {
                Data = comments.PagedForCommand(command).Select(blogComment =>
                {
                    var commentModel = new BlogCommentModel();
                    commentModel.Id = blogComment.Id;
                    commentModel.BlogPostId = blogComment.BlogPostId;
                    commentModel.BlogPostTitle = blogComment.BlogPost.Title;
                    commentModel.CustomerId = blogComment.CustomerId;
                    commentModel.IpAddress = blogComment.IpAddress;
                    commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(blogComment.CreatedOnUtc, DateTimeKind.Utc);
                    commentModel.Comment = Core.Html.HtmlUtils.FormatText(blogComment.CommentText, false, true, false, false, false, false);
                    return commentModel;
                }),
                Total = comments.Count,
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        [GridAction(EnableCustomBinding = true)]
        public ActionResult CommentDelete(int? filterByBlogPostId, int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var comment = _customerContentService.GetCustomerContentById(id) as BlogComment;
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            var blogPost = comment.BlogPost;
            _customerContentService.DeleteCustomerContent(comment);
            //update totals
            _blogService.UpdateCommentTotals(blogPost);

            return Comments(filterByBlogPostId, command);
        }


        #endregion
    }
}
