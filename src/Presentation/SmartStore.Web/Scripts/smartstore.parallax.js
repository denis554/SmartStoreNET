﻿/*
*  Project: Parallax scrolling effect
*  Author: Murat Cakir, SmartStore AG
*  Date: 15.10.2018
*/

; (function ($, window, document, undefined) {

    var initialized = false,
        stages = [],
        // to adjust speed on smaller devices
        speedRatios = { xs: 0.5, sm: 0.6, md: 0.7, lg: 0.9, xl: 1 },
        viewport = ResponsiveBootstrapToolkit;

    function update() {
        _.each(stages, function (item, i) {
            var el = $(item.el);
            var winHeight = window.innerHeight;
            var scrollTop = window.pageYOffset;
            var top = el.offset().top;
            var height = el.outerHeight(false);

            // Check if totally above or totally below viewport
            var visible = !(top + height < scrollTop || top > scrollTop + winHeight);

            if (!visible)
                return;

            if (item.filter && !viewport.is(item.filter)) {
                if (item.initialized) {
                    // Restore original styling
                    el.css('background-position', item.originalPosition);
                    el.css('background-attachment', item.originalAttachment);
                    item.initialized = false;
                }

                return;
            }             

            speed = item.ratio * speedRatios[viewport.current()];

            if (item.type === 'bg') {
                if (!item.initialized) {
                    // for smoother scrolling
                    el.css('background-attachment', 'fixed');
                    item.initialized = true;
                }

                // set bg parallax offset
                var ypos = Math.round((top - scrollTop) * speed) + item.offset;
                el.css('background-position-y', ypos + "px");
            }
            else if (item.type === 'content') {
                var bottom = top + height,
                    rate = 100 / (bottom + winHeight - top) * ((scrollTop + winHeight) - top),
                    ytransform = (rate - 50) * (speed * -3);

                el.css(window.Prefixer.css('transform'), 'translate3d(0, ' + ytransform + 'px, 0)');
            }
        });
    }

    var Parallax = SmartStore.parallax = {
        init: function (options /*{ context: Element, selector: string}*/) {
            var opts = options || {};
            var ctx = $(opts.context || document.body);
            var selector = opts.selector || '.parallax';

            stages = _.map(ctx.find(selector).toArray(), function (val, key) {
                var el = $(val);
                return {
                    el: val,
                    type: el.data('parallax-type') || 'bg',
                    filter: el.data('parallax-filter'),
                    offset: (el.data('parallax-offset') || 0) * -1,
                    ratio: el.data('parallax-speed') || 0.5,
                    originalPosition: el.css('background-position'),
                    originalAttachment: el.css('background-attachment'),
                    initialized: false
                };
            });

            if (!initialized) {
                $(window).on('resize scroll', update);
                update();
            }
        }
    };

})(jQuery, window, document);
