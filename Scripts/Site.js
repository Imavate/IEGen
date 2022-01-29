
//(function ($) {
//    var $span = $('<span class="fa" style="display:none"></span>').appendTo('body');
//    if ($span.css('fontFamily') !== 'FontAwesome') {
//        // Fallback Link
//        $('head').append('<link rel="stylesheet" href="../Content/font-awesome-4.7.0/css/font-awesome.min.css">');
//    }
//    $span.remove();
//})(jQuery);

(function ($) {
    $(document).ready(function () {
        $('ul.dropdown-menu [data-toggle=dropdown]').on('click', function (event) {
            event.preventDefault();
            event.stopPropagation();
            $(this).parent().siblings().removeClass('open');
            $(this).parent().toggleClass('open');
        });

        $('.datetimepicker').datetimepicker({
            format: 'm/d/Y H:i'
        });

        $('.datepicker').datetimepicker({
            timepicker: false,
            format: 'm/d/Y'
        });

        $('.timepicker').datetimepicker({
            datepicker: false,
            format: 'H:i'
        });

        $('.datetimepicker').attr("placeholder", "mm/dd/yyyy HH:mi");
        $('.datepicker').attr("placeholder", "mm/dd/yyyy");
        $('.timepicker').attr("placeholder", "HH:mi");
    });
})(jQuery);

function showAlert() {
    $(".alert").show(300);
}

function RefreshPage() {
    window.location.reload(true);
}

function InitializeDatePicker(element) {
    $('#' + element).datetimepicker({
        timepicker: false,
        format: 'm/d/Y'
    });
    $('#' + element).attr("placeholder", "mm/dd/yyyy");
}

function InitializeTimePicker(element) {
    $('#' + element).datetimepicker({
        format: 'm/d/Y H:i'
    });
    $('#' + element).attr("placeholder", "mm/dd/yyyy HH:mi");
}

function InitializePastTimePicker(element) {
    $('#' + element).datetimepicker({
        format: 'm/d/Y H:i',
        maxDate: '0'
    });
    $('#' + element).attr("placeholder", "mm/dd/yyyy HH:mi");
}

function AddSpinner(ID, Message) {
    $("#" + ID).prop("disabled", true).addClass("nohover")
               .html("<i class=\"fa fa-spinner fa-pulse fa-lg\"></i> <span>" + Message + "</span>");
}

function AddCog(ID, Message) {
    $("#" + ID).prop("disabled", true).addClass("nohover")
               .html("<i class=\"fa fa-cog fa-spin fa-lg\"></i> &nbsp; <span>" + Message + "</span>");
}

function AddClassCog(ClassName, Message) {
    $("." + ClassName).prop("disabled", true).addClass("nohover")
               .html("<i class=\"fa fa-cog fa-spin fa-lg\"></i> &nbsp; <span>" + Message + "</span>");
}

function RestoreElement(ID, Text) {
    $("#" + ID).prop("disabled", false).removeClass("nohover").html(Text).show();
}

function RestoreClass(ClassName, Text) {
    $("." + ClassName).prop("disabled", false).removeClass("nohover").html(Text).show();
}

function HideElement(ID) {
    $("#" + ID).hide();
}

function HideClass(ClassName) {
    $("." + ClassName).hide();
}

function RestoreElementShowAlert(ButtonId, Text, UpdateTargetId, xhr) {
    $("#" + ButtonId).prop("disabled", false).removeClass("nohover").html(Text);
    $("#" + UpdateTargetId).html(xhr.responseText);
    $(".alert").show(300);
    $("#" + UpdateTargetId).attr("tabindex", -1).focus();
}

function HideElementShowAlert(ButtonId, UpdateTargetId, xhr) {
    $("#" + ButtonId).hide();
    $("#" + UpdateTargetId).html(xhr.responseText);
    $(".alert").show(300);
    $("#" + UpdateTargetId).attr("tabindex", -1).focus();
}

function RestoreClassShowAlert(ClassName, Text, UpdateTargetId, xhr) {
    $("." + ClassName).prop("disabled", false).removeClass("nohover").html(Text);
    $("#" + UpdateTargetId).html(xhr.responseText);
    $(".alert").show(300);
    $("#" + UpdateTargetId).attr("tabindex", -1).focus();
}

function HideClassShowAlert(ClassName, UpdateTargetId, xhr) {
    $("." + ClassName).hide();
    $("#" + UpdateTargetId).html(xhr.responseText);
    $(".alert").show(300);
    $("#" + UpdateTargetId).attr("tabindex", -1).focus();
}

function ShowAlertMsg(UpdateTargetId, xhr) {
    $("#" + UpdateTargetId).html(xhr.responseText);
    $(".alert").show(300);
    $("#" + UpdateTargetId).attr("tabindex", -1).focus();
}

function UpdateElement(UpdateTargetId, xhr, ExtraFunction) {
    $("#" + UpdateTargetId).html(xhr.responseText);

    if (ExtraFunction !== undefined) ExtraFunction(xhr.responseText);
}
