﻿(function ($, window, document, undefined) {

	//
	// Instant Search
	// ==========================================================

	$(function () {
		var box = $('#instasearch');
		if (box.length == 0 || box.data('instasearch') === false)
			return;

		var drop = $('#instasearch-drop'),
			logo = $('.shop-logo'),
			dropBody = drop.find('.instasearch-drop-body'),
			minLength = box.data("minlength"),
			url = box.data("url"),
			keyNav = null;

		box.on('input propertychange paste', function (e) {
			var term = box.val();
			doSearch(term);
		});

		box.parent().on('click', function (e) {
			e.stopPropagation();
		});

		box.on('focus', function (e) {
			expandBox();
		});

		box.on('keydown', function (e) {
			if (e.which == 13 /* Enter */) {
				if (keyNav && dropBody.find('.key-hover').length > 0) {
					// Do not post form when key navigation is in progress
					e.preventDefault();
				}
			}
		});

		box.on('keyup', function (e) {
			if (e.which == 27 /* ESC */) {
				closeDrop();
			}
		});

		$(document).on('click', function (e) {
			// Close drop on outside click
			shrinkBox();
			closeDrop();
		});

		function expandBox() {
			var logoWidth = logo.width();
			$('body').addClass('search-focused');
			logo.css('margin-left', (logoWidth * -1) + 'px');

			if (!_.str.isBlank(dropBody.text())) {
				logo.one(Prefixer.event.transitionEnd, function (e) {
					openDrop();
				});
			}
		}

		function shrinkBox() {
			$('body').removeClass('search-focused');
			logo.css('margin-left', '');
		}

		function openDrop() {
			if (!drop.hasClass('open')) {
				drop.addClass('open');
				beginKeyEvents();
			}
		}
		
		function closeDrop() {
			drop.removeClass('open');
			endKeyEvents();
		}

		function doSearch(term) {
			if (term.length < minLength) {
				closeDrop();
				dropBody.html('');
				return;
			}

			var spinner = $('#instasearch-progress');
			if (spinner.length === 0) {
				spinner = createCircularSpinner(20).attr('id', 'instasearch-progress').appendTo(box.parent());
			}
			// Don't show spinner when result is coming fast (< 100 ms.)
			var spinnerTimeout = setTimeout(function () { spinner.addClass('active'); }, 100)
			
			$.ajax({
				dataType: "html",
				url: url,
				data: { q: term },
				type: 'POST',
				success: function (html) {
					if (_.str.isBlank(html)) {
						closeDrop();
						dropBody.html('');
					}
					else {
						dropBody.html(html);
						openDrop();
					}
				},
				error: function () {
					closeDrop();
					dropBody.html('');
				},
				complete: function () {
					clearTimeout(spinnerTimeout);
					spinner.removeClass('active');
				}
			});
		}

		function beginKeyEvents() {
			if (keyNav)
				return;

			// start listening to Down, Up and Enter keys

			dropBody.keyNav({
				exclusiveKeyListener: false,
				scrollToKeyHoverItem: false,
				selectionItemSelector: ".instasearch-hit",
				selectedItemHoverClass: "key-hover",
				keyActions: [
					{ keyCode: 13, action: "select" }, //enter
					{ keyCode: 38, action: "up" }, //up
					{ keyCode: 40, action: "down" }, //down
				]
			});

			keyNav = dropBody.data("keyNav");

			dropBody.on("keyNav.selected", function (e) {
				// Triggered when user presses Enter after navigating to a hit with keyboard
				var el = $(e.selectedElement);
				var href = el.attr('href') || el.data('href');
				if (href) {
					closeDrop();
					location.replace(href);
				}
			});
		}

		function endKeyEvents() {
			if (keyNav) {
				dropBody.off("keyNav.selected");
				keyNav.destroy();
				keyNav = null;
			}
		}

		box.closest("form").on("submit", function (e) {
			if (!box.val()) {
				// Shake the form on submit but no term has been entered
				var frm = $(this);
				var shakeOpts = { direction: "right", distance: 4, times: 2 };
				frm.stop(true, true).effect("shake", shakeOpts, 400, function () {
					box.trigger("focus").removeClass("placeholder")
				});
				return false;
			}

			return true;
		});
	});


	//
	// Facets
	// ==========================================================

	$(function () {
		var widget = $('#faceted-search');
		if (widget.length === 0)
			return;

		var btn = $('.btn-toggle-filter-widget');
		if (btn.length === 0)
			return;

		var viewport = ResponsiveBootstrapToolkit;

		function collapseWidget() {
			if (btn.data('offcanvas')) return;

			// create offcanvas wrapper
			var offcanvas = $('<aside class="offcanvas offcanvas-left offcanvas-overlay" data-overlay="true"><div class="offcanvas-content offcanvas-scrollable"></div></aside>').appendTo('body');

			// handle .offcanvas-closer click
			offcanvas.one('click', '.offcanvas-closer', function (e) {
				offcanvas.offcanvas('hide');
			});

			// put widget into offcanvas wrapper
			widget.appendTo(offcanvas.children().first());
			btn.data('offcanvas', offcanvas);

			btn.attr('data-toggle', 'offcanvas')
		       .attr('data-placement', 'left')
		       .attr('data-disablescrolling', 'true')
               .data('target', offcanvas);
		}

		function restoreWidget() {
			if (!btn.data('offcanvas')) return;

			// move widget back to its origin
			var offcanvas = btn.data('offcanvas');
			widget.appendTo($('.faceted-search-container'));
			offcanvas.remove();

			btn.removeData('offcanvas')
		       .removeAttr('data-toggle')
		       .removeAttr('data-placement')
		       .removeAttr('data-disablescrolling')
		       .removeData('target');
		}

		function toggleOffCanvas() {
			var breakpoint = '<lg';
			if (viewport.is(breakpoint)) {
				collapseWidget();
			}
			else {
				restoreWidget();
			}
		}

		EventBroker.subscribe("page.resized", function (msg, viewport) {
			toggleOffCanvas();
		});

		_.delay(toggleOffCanvas, 10);
	});

})(jQuery, this, document);

