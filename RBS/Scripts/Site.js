function clearText()
{
    $("#SearchTerm").val("");
    $("#SearchTerm").closest('form').submit();
}

function searchByType(type)
{
    $("#SearchType").val(type);
    $("#form").submit();
}

function filterByStatus(object)
{
    $("#Status").val(object.value);
    $("#form").submit();
}

function assignValue() {
    var type = $("#btnType").val();
    $("#SearchType").val(type);
}

function showprogressbar() {
    var form = $("#form");
    form.validate();

    if (form.valid()) { // here you check if validation returned true or false 
        form.submit();
        $('input[type="submit"]').removeClass("btn-primary");
        $('input[type="submit"]').attr('disabled', '');
        $('input[type="file"]').attr('disabled', '');
        $('select').attr('disabled', '');

        $('#progress_bar').removeClass("hidden");
        $('#cancel_link').addClass("hidden");
    }
}


function showOptions()
{
    $("#advanced_search").removeClass("hidden");
    $("#hide_option").removeClass("hidden");
    $("#show_option").addClass("hidden");
}

function hideOptions()
{
    $("#advanced_search").addClass("hidden");
    $("#hide_option").addClass("hidden");
    $("#show_option").removeClass("hidden");
}

//$(function () {
//    var $window = $(window);
//    var sectionTop = $('.top').outerHeight() + 20;
//    var $createDestroy = $('#switch-create-destroy');

//    // initialize all the inputs
//    $("[name='showIncomplete']").not("[type='hidden']").bootstrapSwitch();
//});