﻿using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Web.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //home page
            routes.MapLocalizedRoute("HomePage",
                            "",
                            new { controller = "Home", action = "Index"},
                            new[] { "SmartStore.Web.Controllers" });


            //install
            routes.MapRoute("Installation",
                            "install",
                            new { controller = "Install", action = "Index" },
                            new[] { "SmartStore.Web.Controllers" });

            //products
            routes.MapLocalizedRoute("RecentlyViewedProducts",
                            "recentlyviewedproducts/",
                            new { controller = "Catalog", action = "RecentlyViewedProducts" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("RecentlyAddedProducts",
                            "newproducts/",
                            new { controller = "Catalog", action = "RecentlyAddedProducts" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("RecentlyAddedProductsRSS",
                            "newproducts/rss",
                            new { controller = "Catalog", action = "RecentlyAddedProductsRss" },
                            new[] { "SmartStore.Web.Controllers" });
            
            //comparing products
            routes.MapLocalizedRoute("AddProductToCompare",
                            "compareproducts/add/{productId}",
                            new { controller = "Catalog", action = "AddProductToCompareList" },
                            new { productId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CompareProducts",
                            "compareproducts/",
                            new { controller = "Catalog", action = "CompareProducts" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("RemoveProductFromCompareList",
                            "compareproducts/remove/{productId}",
                            new { controller = "Catalog", action = "RemoveProductFromCompareList"},
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ClearCompareList",
                            "clearcomparelist/",
                            new { controller = "Catalog", action = "ClearCompareList" },
                            new[] { "SmartStore.Web.Controllers" });
            
            // product email a friend
            routes.MapLocalizedRoute("ProductEmailAFriend",
                            "productemailafriend/{productId}",
                            new { controller = "Catalog", action = "ProductEmailAFriend" },
                            new { productId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            // ask product question form
            routes.MapLocalizedRoute("ProductAskQuestion",
                            "productaskquestion/{productId}",
                            new { controller = "Catalog", action = "ProductAskQuestion" },
                            new { productId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            //catalog
            routes.MapLocalizedRoute("ManufacturerList",
                            "manufacturer/all/",
                            new { controller = "Catalog", action = "ManufacturerAll" },
                            new[] { "SmartStore.Web.Controllers" });
            //downloads
            routes.MapRoute("GetSampleDownload",
                            "download/sample/{productvariantid}",
                            new { controller = "Download", action = "Sample"},
                            new { productvariantid = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapRoute("GetDownload",
                            "download/getdownload/{opvid}/{agree}",
                            new { controller = "Download", action = "GetDownload", agree = UrlParameter.Optional },
                            new { opvid = new GuidConstraint(false) },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapRoute("GetLicense",
                            "download/getlicense/{opvid}/",
                            new { controller = "Download", action = "GetLicense" },
                            new { opvid = new GuidConstraint(false) },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("DownloadUserAgreement",
                            "customer/useragreement/{opvid}",
                            new { controller = "Customer", action = "UserAgreement" },
                            new { opvid = new GuidConstraint(false) },
                            new[] { "SmartStore.Web.Controllers" });

            //reviews
            routes.MapLocalizedRoute("ProductReviews",
                            "productreviews/{productId}",
                            new { controller = "Catalog", action = "ProductReviews" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapRoute("SetProductReviewHelpfulness",
                            "setproductreviewhelpfulness",
                            new { controller = "Catalog", action = "SetProductReviewHelpfulness" },
                            new[] { "SmartStore.Web.Controllers" });

            //back in stock notifications
            routes.MapLocalizedRoute("BackInStockSubscribePopup",
                            "backinstocksubscribe/{productVariantId}",
                            new { controller = "Catalog", action = "BackInStockSubscribePopup" },
                            new { productVariantId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("DeleteBackInStockSubscription",
                            "backinstocksubscribe/delete/{subscriptionId}",
                            new { controller = "Customer", action = "DeleteBackInStockSubscription" },
                            new { subscriptionId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            //login, register
            routes.MapLocalizedRoute("Login",
                            "login/",
                            new { controller = "Customer", action = "Login" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("LoginCheckoutAsGuest",
                            "login/checkoutasguest",
                            new { controller = "Customer", action = "Login", checkoutAsGuest = true },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("Register",
                            "register/",
                            new { controller = "Customer", action = "Register" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("Logout",
                            "logout/",
                            new { controller = "Customer", action = "Logout" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("RegisterResult",
                            "registerresult/{resultId}",
                            new { controller = "Customer", action = "RegisterResult" },
                            new { resultId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckUsernameAvailability",
                            "customer/checkusernameavailability",
                            new { controller = "Customer", action = "CheckUsernameAvailability" },
                            new[] { "SmartStore.Web.Controllers" });

            //shopping cart
            routes.MapLocalizedRoute("ShoppingCart",
                            "cart/",
                            new { controller = "ShoppingCart", action = "Cart" },
                            new[] { "SmartStore.Web.Controllers" });
            //wishlist
            routes.MapLocalizedRoute("Wishlist",
                            "wishlist/{customerGuid}",
                            new { controller = "ShoppingCart", action = "Wishlist", customerGuid = UrlParameter.Optional },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("EmailWishlist",
                            "emailwishlist",
                            new { controller = "ShoppingCart", action = "EmailWishlist" },
                            new[] { "SmartStore.Web.Controllers" });
            //add product to cart (without any attributes and options)
            routes.MapLocalizedRoute("AddProductToCart",
                            "addproducttocart/{productId}",
                            new { controller = "ShoppingCart", action = "AddProductToCart" },
                            new { productId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            //add product variant to cart (with attributes and options)
            routes.MapLocalizedRoute("AddProductVariantToCart",
                            "addproductvarianttocart/{productVariantId}/{shoppingCartTypeId}",
                            new { controller = "ShoppingCart", action = "AddProductVariantToCart" },
                            new { productVariantId = @"\d+", shoppingCartTypeId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            //product attributes with "upload file" type
            routes.MapLocalizedRoute("UploadFileProductAttribute",
                            "uploadfileproductattribute/{productVariantId}/{productAttributeId}",
                            new { controller = "ShoppingCart", action = "UploadFileProductAttribute" },
                            new { productVariantId = @"\d+", productAttributeId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            
            //checkout
            routes.MapLocalizedRoute("Checkout",
                            "checkout/",
                            new { controller = "Checkout", action = "Index" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutOnePage",
                            "onepagecheckout/",
                            new { controller = "Checkout", action = "OnePageCheckout" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutShippingAddress",
                            "checkout/shippingaddress",
                            new { controller = "Checkout", action = "ShippingAddress" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutSelectShippingAddress",
                            "checkout/selectshippingaddress",
                            new { controller = "Checkout", action = "SelectShippingAddress" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutBillingAddress",
                            "checkout/billingaddress",
                            new { controller = "Checkout", action = "BillingAddress" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutSelectBillingAddress",
                            "checkout/selectbillingaddress",
                            new { controller = "Checkout", action = "SelectBillingAddress" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutShippingMethod",
                            "checkout/shippingmethod",
                            new { controller = "Checkout", action = "ShippingMethod" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutPaymentMethod",
                            "checkout/paymentmethod",
                            new { controller = "Checkout", action = "PaymentMethod" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutPaymentInfo",
                            "checkout/paymentinfo",
                            new { controller = "Checkout", action = "PaymentInfo" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutConfirm",
                            "checkout/confirm",
                            new { controller = "Checkout", action = "Confirm" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutCompleted",
                            "checkout/completed",
                            new { controller = "Checkout", action = "Completed" },
                            new[] { "SmartStore.Web.Controllers" });

            //orders
            routes.MapLocalizedRoute("OrderDetails",
                            "orderdetails/{orderId}",
                            new { controller = "Order", action = "Details" },
                            new { orderId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ShipmentDetails",
                            "orderdetails/shipment/{shipmentId}",
                            new { controller = "Order", action = "ShipmentDetails" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ReturnRequest",
                            "returnrequest/{orderId}",
                            new { controller = "ReturnRequest", action = "ReturnRequest" },
                            new { orderId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ReOrder",
                            "reorder/{orderId}",
                            new { controller = "Order", action = "ReOrder" },
                            new { orderId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("GetOrderPdfInvoice",
                            "orderdetails/pdf/{orderId}",
                            new { controller = "Order", action = "GetPdfInvoice" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("PrintOrderDetails",
                            "orderdetails/print/{orderId}",
                            new { controller = "Order", action = "PrintOrderDetails" },
                            new[] { "SmartStore.Web.Controllers" });
            

            //contact us
            routes.MapLocalizedRoute("ContactUs",
                            "contactus",
                            new { controller = "Common", action = "ContactUs" },
                            new[] { "SmartStore.Web.Controllers" });

            //store closed
            routes.MapLocalizedRoute("StoreClosed",
                            "storeclosed",
                            new { controller = "Common", action = "StoreClosed" },
                            new[] { "SmartStore.Web.Controllers" });

            //passwordrecovery
            routes.MapLocalizedRoute("PasswordRecovery",
                            "passwordrecovery",
                            new { controller = "Customer", action = "PasswordRecovery" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("PasswordRecoveryConfirm",
                            "passwordrecovery/confirm",
                            new { controller = "Customer", action = "PasswordRecoveryConfirm" },                            
                            new[] { "SmartStore.Web.Controllers" });

            //newsletters
            routes.MapLocalizedRoute("NewsletterActivation",
                            "newsletter/subscriptionactivation/{token}/{active}",
                            new { controller = "Newsletter", action = "SubscriptionActivation" },
                            new { token = new GuidConstraint(false) },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("SubscribeNewsletter",
                            "subscribenewsletter",
                            new { controller = "Newsletter", action = "SubscribeNewsletter" },
                            new[] { "SmartStore.Web.Controllers" });

            

            //customer
            routes.MapLocalizedRoute("CustomerMyAccount",
                            "customer/myaccount",
                            new { controller = "Customer", action = "MyAccount" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerInfo",
                            "customer/info",
                            new { controller = "Customer", action = "Info" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerAddresses",
                            "customer/addresses",
                            new { controller = "Customer", action = "Addresses" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerOrders",
                            "customer/orders",
                            new { controller = "Customer", action = "Orders" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerReturnRequests",
                            "customer/returnrequests",
                            new { controller = "Customer", action = "ReturnRequests" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerDownloadableProducts",
                            "customer/downloadableproducts",
                            new { controller = "Customer", action = "DownloadableProducts" },
                            new[] { "SmartStore.Web.Controllers" });


            routes.MapLocalizedRoute("CustomerBackInStockSubscriptions",
                            "customer/backinstocksubscriptions",
                            new { controller = "Customer", action = "BackInStockSubscriptions" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerBackInStockSubscriptionsPaged",
                            "customer/backinstocksubscriptions/{page}",
                            new { controller = "Customer", action = "BackInStockSubscriptions", page = UrlParameter.Optional },
                            new { page = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("CustomerRewardPoints",
                            "customer/rewardpoints",
                            new { controller = "Customer", action = "RewardPoints" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerChangePassword",
                            "customer/changepassword",
                            new { controller = "Customer", action = "ChangePassword" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerAvatar",
                            "customer/avatar",
                            new { controller = "Customer", action = "Avatar" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("AccountActivation",
                            "customer/activation",
                            new { controller = "Customer", action = "AccountActivation" },                            
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerProfile",
                            "profile/{id}",
                            new { controller = "Profile", action = "Index" },
                            new { id = @"\d+"},
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerProfilePaged",
                            "profile/{id}/page/{page}",
                            new { controller = "Profile", action = "Index"},
                            new {  id = @"\d+", page = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerForumSubscriptions",
                            "customer/forumsubscriptions",
                            new { controller = "Customer", action = "ForumSubscriptions"},
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerForumSubscriptionsPaged",
                            "customer/forumsubscriptions/{page}",
                            new { controller = "Customer", action = "ForumSubscriptions", page = UrlParameter.Optional },
                            new { page = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("DeleteForumSubscription",
                            "customer/forumsubscriptions/delete/{subscriptionId}",
                            new { controller = "Customer", action = "DeleteForumSubscription" },
                            new { subscriptionId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            //addresses
            routes.MapLocalizedRoute("CustomerAddressDelete",
                            "customer/addressdelete/{addressId}",
                            new { controller = "Customer", action = "AddressDelete" },
                            new { addressId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerAddressEdit",
                            "customer/addressedit/{addressId}",
                            new { controller = "Customer", action = "AddressEdit" },
                            new { addressId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("CustomerAddressAdd",
                            "customer/addressadd",
                            new { controller = "Customer", action = "AddressAdd" },
                            new[] { "SmartStore.Web.Controllers" });



            //blog
            routes.MapLocalizedRoute("Blog",
                            "blog",
                            new { controller = "Blog", action = "List" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("BlogByTag",
                            "blog/tag/{tag}",
                            new { controller = "Blog", action = "BlogByTag" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("BlogByMonth",
                            "blog/month/{month}",
                            new { controller = "Blog", action = "BlogByMonth" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("BlogRSS",
                            "blog/rss/{languageId}",
                            new { controller = "Blog", action = "ListRss" },
                            new { languageId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            //forum
            routes.MapLocalizedRoute("Boards",
                            "boards",
                            new { controller = "Boards", action = "Index" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ActiveDiscussions",
                            "boards/activediscussions",
                            new { controller = "Boards", action = "ActiveDiscussions" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ActiveDiscussionsRSS",
                            "boards/activediscussionsrss",
                            new { controller = "Boards", action = "ActiveDiscussionsRSS" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("PostEdit",
                            "boards/postedit/{id}",
                            new { controller = "Boards", action = "PostEdit" },
                            new { id = @"\d+"},
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("PostDelete",
                            "boards/postdelete/{id}",
                            new { controller = "Boards", action = "PostDelete" },
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("PostCreate",
                            "boards/postcreate/{id}",
                            new { controller = "Boards", action = "PostCreate"},
                            new { id = @"\d+"},
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("PostCreateQuote",
                            "boards/postcreate/{id}/{quote}",
                            new { controller = "Boards", action = "PostCreate"},
                            new { id = @"\d+", quote = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("TopicEdit",
                            "boards/topicedit/{id}",
                            new { controller = "Boards", action = "TopicEdit"},
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("TopicDelete",
                            "boards/topicdelete/{id}",
                            new { controller = "Boards", action = "TopicDelete"},
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("TopicCreate",
                            "boards/topiccreate/{id}",
                            new { controller = "Boards", action = "TopicCreate" },
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("TopicMove",
                            "boards/topicmove/{id}",
                            new { controller = "Boards", action = "TopicMove" },
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("TopicWatch",
                            "boards/topicwatch/{id}",
                            new { controller = "Boards", action = "TopicWatch" },
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("TopicSlug",
                            "boards/topic/{id}/{slug}",
                            new { controller = "Boards", action = "Topic", slug = UrlParameter.Optional },
                            new { id = @"\d+"},
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("TopicSlugPaged",
                            "boards/topic/{id}/{slug}/page/{page}",
                            new { controller = "Boards", action = "Topic", slug = UrlParameter.Optional, page = UrlParameter.Optional },
                            new { id = @"\d+", page = @"\d+"},
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ForumWatch",
                            "boards/forumwatch/{id}",
                            new { controller = "Boards", action = "ForumWatch" },
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ForumRSS",
                            "boards/forumrss/{id}",
                            new { controller = "Boards", action = "ForumRSS" },
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ForumSlug",
                            "boards/forum/{id}/{slug}",
                            new { controller = "Boards", action = "Forum", slug = UrlParameter.Optional },
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ForumSlugPaged",
                            "boards/forum/{id}/{slug}/page/{page}",
                            new { controller = "Boards", action = "Forum", slug = UrlParameter.Optional, page = UrlParameter.Optional },
                            new { id = @"\d+", page = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ForumGroupSlug",
                            "boards/forumgroup/{id}/{slug}",
                            new { controller = "Boards", action = "ForumGroup", slug = UrlParameter.Optional },
                            new { id = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("Search",
                            "boards/search",
                            new { controller = "Boards", action = "Search" },
                            new[] { "SmartStore.Web.Controllers" });


            //private messages
            routes.MapLocalizedRoute("PrivateMessages",
                            "privatemessages/{tab}",
                            new { controller = "PrivateMessages", action = "Index", tab = UrlParameter.Optional },
                            new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("PrivateMessagesPaged",
                            "privatemessages/{tab}/page/{page}",
                            new { controller = "PrivateMessages", action = "Index", tab = UrlParameter.Optional },
                            new { page = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("PrivateMessagesInbox",
                            "inboxupdate",
                            new { controller = "PrivateMessages", action = "InboxUpdate" },
                            new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("PrivateMessagesSent",
                            "sentupdate",
                            new { controller = "PrivateMessages", action = "SentUpdate" },
                            new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("SendPM",
                            "sendpm/{toCustomerId}",
                            new { controller = "PrivateMessages", action = "SendPM" },
                            new { toCustomerId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("SendPMReply",
                            "sendpm/{toCustomerId}/{replyToMessageId}",
                            new { controller = "PrivateMessages", action = "SendPM" },
                            new { toCustomerId = @"\d+", replyToMessageId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("ViewPM",
                            "viewpm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "ViewPM" },
                            new { privateMessageId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            routes.MapLocalizedRoute("DeletePM",
                            "deletepm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "DeletePM" },
                            new { privateMessageId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            //news
            routes.MapLocalizedRoute("NewsArchive",
                            "news",
                            new { controller = "News", action = "List" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("NewsRSS",
                            "news/rss/{languageId}",
                            new { controller = "News", action = "ListRss" },
                            new { languageId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });

            //topics
            routes.MapLocalizedRoute("Topic",
                            "t/{SystemName}",
                            new { controller = "Topic", action = "TopicDetails" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("TopicPopup",
                            "t-popup/{SystemName}",
                            new { controller = "Topic", action = "TopicDetailsPopup" },
                            new[] { "SmartStore.Web.Controllers" });
            //sitemaps
            routes.MapLocalizedRoute("Sitemap",
                            "sitemap",
                            new { controller = "Common", action = "Sitemap" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("SitemapSEO",
                            "sitemapseo",
                            new { controller = "Common", action = "SitemapSeo" },
                            new[] { "SmartStore.Web.Controllers" });

            //product tags
            routes.MapLocalizedRoute("ProductsByTag",
                            "producttag/{productTagId}/{SeName}",
                            new { controller = "Catalog", action = "ProductsByTag", SeName = UrlParameter.Optional },
                            new { productTagId = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ProductTagsAll",
                            "producttag/all/",
                            new { controller = "Catalog", action = "ProductTagsAll" },
                            new[] { "SmartStore.Web.Controllers" });

            //product search
            routes.MapLocalizedRoute("ProductSearch",
                            "search/",
                            new { controller = "Catalog", action = "Search" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ProductSearchAutoComplete",
                            "catalog/searchtermautocomplete",
                            new { controller = "Catalog", action = "SearchTermAutoComplete" },
                            new[] { "SmartStore.Web.Controllers" });

            //config
            routes.MapLocalizedRoute("Config",
                            "config",
                            new { controller = "Common", action = "Config" },
                            new[] { "SmartStore.Web.Controllers" });

            //some AJAX links
            routes.MapRoute("GetStatesByCountryId",
                            "country/getstatesbycountryid/",
                            new { controller = "Country", action = "GetStatesByCountryId" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ChangeDevice",
                            "changedevice/{dontusemobileversion}",
                            new { controller = "Common", action = "ChangeDevice" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ChangeCurrency",
                            "changecurrency/{customercurrency}",
                            new { controller = "Common", action = "CurrencySelected" },
                            new { customercurrency = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ChangeLanguage",
                            "changelanguage/{langid}",
                            new { controller = "Common", action = "SetLanguage" },
                            new { langid = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("ChangeTaxType",
                            "changetaxtype/{customertaxtype}",
                            new { controller = "Common", action = "TaxTypeSelected" },
                            new { customertaxtype = @"\d+" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("PollVote",
                            "poll/vote",
                            new { controller = "Poll", action = "Vote" },
                            new[] { "SmartStore.Web.Controllers" });
            routes.MapLocalizedRoute("TopicAuthenticate",
                            "topic/authenticate",
                            new { controller = "Topic", action = "Authenticate" },
                            new[] { "SmartStore.Web.Controllers" });
            // codehint: sm-add
            routes.MapLocalizedRoute("ProductFilters",
                            "filter/",
                            new { controller = "Filter", action = "Products" },
                            new[] { "SmartStore.Web.Controllers" });
			routes.MapLocalizedRoute("ProductsMultiSelect",
							"filter/productsmultiselect",
							new { controller = "Filter", action = "ProductsMultiSelect" },
							new[] { "SmartStore.Web.Controllers" });

            //robots.txt
            routes.MapRoute("robots.txt",
                            "robots.txt",
                            new { controller = "Common", action = "RobotsTextFile" },
                            new[] { "SmartStore.Web.Controllers" });

            //page not found
            routes.MapLocalizedRoute("PageNotFound",
                            "page-not-found",
                            new { controller = "Common", action = "PageNotFound" },
                            new[] { "SmartStore.Web.Controllers" });
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
