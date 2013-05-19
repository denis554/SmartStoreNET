﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using SmartStore.Core;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{

    public abstract class NavigationItemBuilder<TItem, TBuilder> : IHideObjectMembers
        where TItem : NavigationItem
        where TBuilder : NavigationItemBuilder<TItem, TBuilder>
    {

        protected NavigationItemBuilder(TItem item)
        {
            Guard.ArgumentNotNull(() => item);

            this.Item = item;
        }

        protected internal TItem Item
        {
            get;
            private set;
        }


        public TBuilder Action(RouteValueDictionary routeValues)
        {
            this.Item.Action(routeValues);
            return (this as TBuilder);
        }

        public TBuilder Action(string actionName, string controllerName)
        {
            return this.Action(actionName, controllerName, null);
        }

        public TBuilder Action(string actionName, string controllerName, object routeValues)
        {
            this.Item.Action(actionName, controllerName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            this.Item.Action(actionName, controllerName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder Route(string routeName)
        {
            return this.Route(routeName, null);
        }

        public TBuilder Route(string routeName, object routeValues)
        {
            this.Item.Route(routeName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder Route(string routeName, RouteValueDictionary routeValues)
        {
            this.Item.Route(routeName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder QueryParam(string paramName, params string[] booleanParamNames)
        {
            this.Item.ModifyParam(paramName, booleanParamNames);
            return (this as TBuilder);
        }

        public TBuilder Url(string value)
        {
            this.Item.Url(value);
            return (this as TBuilder);
        }

        public TBuilder HtmlAttributes(object attributes)
        {
            return this.HtmlAttributes(CollectionHelper.ObjectToDictionary(attributes));
        }

        public TBuilder HtmlAttributes(IDictionary<string, object> attributes)
        {
            this.Item.HtmlAttributes.Clear();
            this.Item.HtmlAttributes.Merge(attributes);
            return (this as TBuilder);
        }

        public TBuilder LinkHtmlAttributes(object attributes)
        {
            return this.LinkHtmlAttributes(CollectionHelper.ObjectToDictionary(attributes));
        }

        public TBuilder LinkHtmlAttributes(IDictionary<string, object> attributes)
        {
            this.Item.LinkHtmlAttributes.Clear();
            this.Item.LinkHtmlAttributes.Merge(attributes);
            return (this as TBuilder);
        }

        public TBuilder ImageUrl(string value)
        {
            this.Item.ImageUrl = value;
            return (this as TBuilder);
        }

        public TBuilder Icon(string value)
        {
            this.Item.Icon = value;
            return (this as TBuilder);
        }

        public TBuilder Text(string value)
        {
            this.Item.Text = value;
            return (this as TBuilder);
        }

        public TBuilder Badge(string value, BadgeStyle style = BadgeStyle.Default, bool condition = true)
        {
            if (condition)
            {
                this.Item.BadgeText = value;
                this.Item.BadgeStyle = style;
            }
            return (this as TBuilder);
        }

        public TBuilder Visible(bool value)
        {
            this.Item.Visible = value;
            return (this as TBuilder);
        }

        public TBuilder Encoded(bool value)
        {
            this.Item.Encoded = value;
            return (this as TBuilder);
        }

        public TBuilder Selected(bool value)
        {
            this.Item.Selected = value;
            return (this as TBuilder);
        }

        public TBuilder Enabled(bool value)
        {
            this.Item.Enabled = value;
            return (this as TBuilder);
        }

        public TItem ToItem()
        {
            return this.Item;
        }

    }

    public abstract class NavigationItemtWithContentBuilder<TItem, TBuilder> : NavigationItemBuilder<TItem, TBuilder>
        where TItem : NavigationItemWithContent
        where TBuilder : NavigationItemtWithContentBuilder<TItem, TBuilder>
    {

        public NavigationItemtWithContentBuilder(TItem item)
            : base(item)
        {
        }

        public TBuilder Content(string value)
        {
            return this.Content(x => new HelperResult(writer => writer.Write(value)));
        }

        public TBuilder Content(Func<dynamic, HelperResult> value)
        {
            return this.Content(value(null));
        }

        public TBuilder Content(HelperResult value)
        {
            this.Item.Content = value;
            return (this as TBuilder);
        }

        public TBuilder ContentHtmlAttributes(object attributes)
        {
            return this.ContentHtmlAttributes(CollectionHelper.ObjectToDictionary(attributes));
        }

        public TBuilder ContentHtmlAttributes(IDictionary<string, object> attributes)
        {
            this.Item.ContentHtmlAttributes.Clear();
            this.Item.ContentHtmlAttributes.Merge(attributes);
            return (this as TBuilder);
        }

    }

}
