var Checkout = {
    loadWaiting: false,
    failureUrl: false,
    steps: new Array(),

    init: function (failureUrl) {
        this.loadWaiting = false;
        this.failureUrl = failureUrl;
        this.steps = ['billing', 'shipping', 'shipping_method', 'payment_method', 'payment_info', 'confirm_order'];

        Accordion.disallowAccessToNextSections = true;
    },

    ajaxFailure: function () {
        location.href = Checkout.failureUrl;
    },
    
    _disableEnableAll: function (element, isDisabled) {
        var descendants = element.find('*');
        $(descendants).each(function() {
            if (isDisabled) {
                $(this).attr('disabled', 'disabled');
            } else {
                $(this).removeAttr('disabled');
            }
        });

        if (isDisabled) {
                element.attr('disabled', 'disabled');
            } else {
                element.removeAttr('disabled');
            }
    },

    setLoadWaiting: function (step, keepDisabled) {
        if (step) {
            if (this.loadWaiting) {
                this.setLoadWaiting(false);
            }
            var container = $('#' + step + '-buttons-container');
            container.addClass('disabled');
            container.css('opacity', '.5');
            this._disableEnableAll(container, true);
            $('#' + step + '-please-wait').show();
        } else {
            if (this.loadWaiting) {
                var container = $('#' + this.loadWaiting + '-buttons-container');
                var isDisabled = (keepDisabled ? true : false);
                if (!isDisabled) {
                    container.removeClass('disabled');
                    container.css('opacity', '1');
                }
                this._disableEnableAll(container, isDisabled);
                $('#' + this.loadWaiting + '-please-wait').hide();
            }
        }
        this.loadWaiting = step;
    },

    gotoSection: function (section) {
        section = $('#opc-' + section);
        section.addClass('allow');
        applyCommonPlugins(section); //codehint: sm-add
        Accordion.openSection(section);
    },

    back: function () {
        if (this.loadWaiting) return;
        Accordion.openPrevSection(true, true);
    },

    setStepResponse: function (response) {
        if (response.update_section) {
            $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
        }
        if (response.allow_sections) {
            response.allow_sections.each(function (e) {
                $('#opc-' + e).addClass('allow');
            });
        }
        
        //TODO move it to a new method
        if ($("#billing-address-select").length > 0) {
            Billing.newAddress(!$('#billing-address-select').val());
        }
        if ($("#shipping-address-select").length > 0) {
            Shipping.newAddress(!$('#shipping-address-select').val());
        }

        if (response.goto_section) {
            Checkout.gotoSection(response.goto_section);
            return true;
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        return false;
    }
};





var Billing = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    newAddress: function (isNew) {
        if (isNew) {
            this.resetSelectedAddress();
            $('#billing-new-address-form').show();
        } else {
            $('#billing-new-address-form').hide();
        }
    },

    resetSelectedAddress: function () {
        var selectElement = $('#billing-address-select')
        if (selectElement) {
            selectElement.value = '';
        }
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('billing');
        
        console.log(this.saveUrl);

        $.ajax({
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: function (xhr, ajaxOptions, thrownError) {
                //Checkout.ajaxFailure
                alert(xhr.status);
                alert(thrownError);
            }
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var Shipping = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    newAddress: function (isNew) {
        if (isNew) {
            this.resetSelectedAddress();
            $('#shipping-new-address-form').show();
        } else {
            $('#shipping-new-address-form').hide();
        }
    },

    resetSelectedAddress: function () {
        var selectElement = $('#shipping-address-select')
        if (selectElement) {
            selectElement.value = '';
        }
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('shipping');
        
        $.ajax({
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var ShippingMethod = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    validate: function() {
        var methods = document.getElementsByName('shippingoption');
        if (methods.length==0) {
            alert('Your order cannot be completed at this time as there is no shipping methods available for it. Please make necessary changes in your shipping address.');
            return false;
        }

        for (var i = 0; i< methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify shipping method.');
        return false;
    },
    
    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        if (this.validate()) {
            Checkout.setLoadWaiting('shipping-method');
        
            $.ajax({
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentMethod = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    validate: function () {
        var methods = document.getElementsByName('paymentmethod');
        if (methods.length == 0) {
            alert('Your order cannot be completed at this time as there is no payment methods available for it.');
            return false;
        }
        
        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify payment method.');
        return false;
    },
    
    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        if (this.validate()) {
            Checkout.setLoadWaiting('payment-method');
            $.ajax({
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentInfo = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        Checkout.setLoadWaiting('payment-info');
        $.ajax({
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var ConfirmOrder = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (saveUrl, successUrl) {
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        Checkout.setLoadWaiting('confirm-order');
        $.ajax({
            url: this.saveUrl,
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },
    
    resetLoadWaiting: function (transport) {
        Checkout.setLoadWaiting(false, ConfirmOrder.isSuccess);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }
        
        if (response.redirect) {
            ConfirmOrder.isSuccess = true;
            location.href = response.redirect;
            return;
        }
        if (response.success) {
            ConfirmOrder.isSuccess = true;
            window.location = ConfirmOrder.successUrl;
        }

        Checkout.setStepResponse(response);
    }
};