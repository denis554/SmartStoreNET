/*****************************************/
// Name: Javascript Textarea BBCode Markup Editor
// Version: 1.3
// Author: Balakrishnan
// Last Modified Date: 25/jan/2009
// License: Free
// URL: http://www.corpocrat.com
/******************************************/

var textarea;
var content;
var webRoot;

function edToolbar(obj) {
    document.write("<div class=\"bb-toolbar btn-toolbar\">");
    document.write("<div class='btn-group mb-1'>");
    document.write("<button type='button' class='btn btn-secondary btn-small' name='btnBold' onClick=\"doAddTags('[b]','[/b]','" + obj + "')\"><i class='fa fa-bold'></i></button>");
    document.write("<button type='button' class='btn btn-secondary btn-small' name='btnItalic' onClick=\"doAddTags('[i]','[/i]','" + obj + "')\"><i class='fa fa-italic'></i></button>");
    document.write("<button type='button' class='btn btn-secondary btn-small' name='btnUnderline' onClick=\"doAddTags('[u]','[/u]','" + obj + "')\"><i class='fa fa-underline'></i></button>");
    document.write("<button type='button' class='btn btn-secondary btn-small' name='btnLink' onClick=\"doURL('" + obj + "')\"><i class='fa fa-link'></i></button>");
    document.write("<button type='button' class='btn btn-secondary btn-small' name='btnCode' onClick=\"doAddTags('[code]','[/code]','" + obj + "')\"><i class='fa fa-code'></i></button>");
    document.write("<button type='button' class='btn btn-secondary btn-small' name='btnQuote' onClick=\"doAddTags('[quote]','[/quote]','" + obj + "')\"><i class='fa fa-quote-left'></i></button>");
    document.write("</div>");
    document.write("</div>");
}

function doURL(obj) {
    textarea = document.getElementById(obj);
    var url = prompt('Enter the URL:', 'http://');
    var scrollTop = textarea.scrollTop;
    var scrollLeft = textarea.scrollLeft;
    if (url != '' && url != null) {
        if (document.selection) {
            textarea.focus();
            var sel = document.selection.createRange();
            if (sel.text == "") {
                sel.text = '[url]' + url + '[/url]';
            }
            else {
                sel.text = '[url=' + url + ']' + sel.text + '[/url]';
            }
        }
        else {
            var len = textarea.value.length;
            var start = textarea.selectionStart;
            var end = textarea.selectionEnd;

            var sel = textarea.value.substring(start, end);

            if (sel == "") {
                var rep = '[url]' + url + '[/url]';
            }
            else {
                var rep = '[url=' + url + ']' + sel + '[/url]';
            }

            textarea.value = textarea.value.substring(0, start) + rep + textarea.value.substring(end, len);
            textarea.scrollTop = scrollTop;
            textarea.scrollLeft = scrollLeft;
        }
    }
}

function doAddTags(tag1, tag2, obj) {
    textarea = document.getElementById(obj);
    // Code for IE
    if (document.selection) {
        textarea.focus();
        var sel = document.selection.createRange();
        sel.text = tag1 + sel.text + tag2;
    }
    else {  // Code for Mozilla Firefox
        var len = textarea.value.length;
        var start = textarea.selectionStart;
        var end = textarea.selectionEnd;
        var scrollTop = textarea.scrollTop;
        var scrollLeft = textarea.scrollLeft;
        var sel = textarea.value.substring(start, end);
        var rep = tag1 + sel + tag2;
        textarea.value = textarea.value.substring(0, start) + rep + textarea.value.substring(end, len);
        textarea.scrollTop = scrollTop;
        textarea.scrollLeft = scrollLeft;
    }
}