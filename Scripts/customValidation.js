window.customValidation = window.customValidation ||
    {
        relatedControlValidationCalled: function (event) {
            if (!customValidation.activeValidator) {
                customValidation.formValidator = $(event.data.source).closest('form').data('validator');
            }
            customValidation.formValidator.element($(event.data.target));
        },
        relatedControlCollection: [],
        formValidator: undefined,
        addDependentControlValidationHandler: function (element, dependentPropertyName) {
            var id = $(element).attr('id');
            if ($.inArray(id, customValidation.relatedControlCollection) < 0) {
                customValidation.relatedControlCollection.push(id);
                $(element).on(
                    'blur',
                    { source: $(element), target: $('#' + dependentPropertyName) },
                    customValidation.relatedControlValidationCalled);
            }
        }
    };

    $.validator.unobtrusive.adapters.add('customrange', ['minpropertyname', 'maxpropertyname', 'allowequality'],
        function (options) {
            options.rules['customrange'] = options.params;
            if (options.message) {
                options.messages['customrange'] = options.message;
            }
        }
    );

    $.validator.unobtrusive.adapters.add('comparedates', ['otherpropertyname', 'allowequality'],
        function (options) {
            options.rules['comparedates'] = options.params;
            if (options.message) {
                options.messages['comparedates'] = options.message;
            }
        }
    );

    $.validator.unobtrusive.adapters.add('comparenumbers', ['otherpropertyname', 'allowequality'],
        function (options) {
            options.rules['comparenumbers'] = options.params;
            if (options.message) {
                options.messages['comparenumbers'] = options.message;
            }
        }
    );

    $.validator.unobtrusive.adapters.add('minvalue', ['otherpropertyname', 'allowequality'],
        function (options) {
            options.rules['minvalue'] = options.params;
            if (options.message) {
                options.messages['minvalue'] = options.message;
            }
        }
    );

    $.validator.unobtrusive.adapters.add('maxvalue', ['otherpropertyname', 'allowequality'],
        function (options) {
            options.rules['maxvalue'] = options.params;
            if (options.message) {
                options.messages['maxvalue'] = options.message;
            }
        }
    );


$.validator.addMethod('comparedates', function (value, element, params) {
    var otherFieldValue = $('input[name="' + params.otherpropertyname + '"]').val();
    if (otherFieldValue && value) {
        var currentValue = Date.parse(value);
        var otherValue = Date.parse(otherFieldValue);
        if ($(element).attr('name').toLowerCase().indexOf('max') >= 0) {
            if (params.allowequality) {
                if (currentValue < otherValue) {
                    return false;
                }
            } else {
                if (currentValue <= otherValue) {
                    return false;
                }
            }
        } else {
            if (params.allowequality) {
                if (currentValue > otherValue) {
                    return false;
                }
            } else {
                if (currentValue >= otherValue) {
                    return false;
                }
            }
        }
    }
    customValidation.addDependentControlValidationHandler(element, params.otherpropertyname);
    return true;
}, '');



$.validator.addMethod('comparenumbers', function (value, element, params) {
    var otherFieldValue = $('input[name="' + params.otherpropertyname + '"]').val();
    if (otherFieldValue && value) {
        var currentValue = parseFloat(value);
        var otherValue = parseFloat(otherFieldValue);
        if ($(element).attr('name').toLowerCase().indexOf('max') >= 0) {
            if (params.allowequality) {
                if (currentValue < otherValue) {
                    return false;
                }
            } else {
                if (currentValue <= otherValue) {
                    return false;
                }
            }
        } else {
            if (params.allowequality) {
                if (currentValue > otherValue) {
                    return false;
                };
            } else {
                if (currentValue >= otherValue) {
                    return false;
                };
            }
        }
    }
    customValidation.addDependentControlValidationHandler(element, params.otherpropertyname);
    return true;
}, '');



$.validator.addMethod('minvalue', function (value, element, params) {
    var otherFieldValue = $('input[name="' + params.otherpropertyname + '"]').val();
    if (otherFieldValue && value) {
        var currentValue = parseFloat(value);
        var otherValue = parseFloat(otherFieldValue);
        if (params.allowequality) {
            if (currentValue < otherValue) {
                return false;
            }
        } else {
            if (currentValue <= otherValue) {
                return false;
            }
        }
    }
    customValidation.addDependentControlValidationHandler(element, params.otherpropertyname);
    return true;
}, '');



$.validator.addMethod('maxvalue', function (value, element, params) {
    var otherFieldValue = $('input[name="' + params.otherpropertyname + '"]').val();
    if (otherFieldValue && value) {
        var currentValue = parseFloat(value);
        var otherValue = parseFloat(otherFieldValue);
        if (params.allowequality) {
            if (currentValue > otherValue) {
                return false;
            }
        } else {
            if (currentValue >= otherValue) {
                return false;
            }
        }
    }
    customValidation.addDependentControlValidationHandler(element, params.otherpropertyname);
    return true;
}, '');



$.validator.addMethod('customrange', function (value, element, params) {
    var minFieldValue = $('input[name="' + params.minpropertyname + '"]').val();
    var maxFieldValue = $('input[name="' + params.maxpropertyname + '"]').val();
    if (minFieldValue && maxFieldValue && value) {
        var currentValue = parseFloat(value);
        var minValue = parseFloat(minFieldValue);
        var maxValue = parseFloat(maxFieldValue);
        if (params.allowequality) {
            if (currentValue < minValue || currentValue > maxValue) {
                return false;
            }
        } else {
            if (currentValue <= minValue || currentValue >= maxValue) {
                return false;
            }
        }
    }
    customValidation.addDependentControlValidationHandler(element, params.minpropertyname);
    return true;
}, '');