$(document).ready(function () {
    function calculateTotalRecord() {
        var totaltarget = $('.table-target tbody tr').length;
        $('.total-target').text(totaltarget);
    }
    calculateTotalRecord();

    $('#dropdownhotnumberCase').on('change', function () {
        var tempText = $(this).find("option:selected").text();
        $.ajax({
            type: "GET",
            url: '/PerTwoMinutes/GetListIntercept',
            dataType: "json",
            data: { casename: tempText },
            contentType: "application/json; charset=utf-8",
            success: function (connObjReturn) {
                $('#dropdownhotnumberIntercept').find('option').remove();
                $.each(connObjReturn, function (index, item) {
                    $('#dropdownhotnumberIntercept').append("<option value='" + item.InterceptId + "'>" + item.InterceptName + "</option>");
                });
            },
            error: function () {

            }
        });
    });

    $(document).on('click', '.btn-update', function () {
        var casename = $(this).closest('tr').find('.casename').html();
        var caseid = $(this).closest('tr').find('.caseId').html();
        var isActive = $(this).closest('tr').find('.chkActive').prop('checked');
        console.log(isActive);
        $.ajax({
            type: "POST",
            url: '/Home/Update',
            dataType: "json",
            data: JSON.stringify({ target: { TargetId: caseid, TargetName: casename, Active: isActive } }),
            contentType: "application/json; charset=utf-8",
            success: function (connObjReturn) {
                if (connObjReturn == true) {
                    $('.alert-success-target').html("<strong>Well done!</strong> Successfully update target.")
                    $('.alert-success-target').addClass('show');
                    setTimeout(function () {
                        $('.alert-success-target').removeClass('show');
                    }, 3000);
                } else {
                    $('.alert-danger-target').addClass('show');
                    setTimeout(function () {
                        $('.alert-danger-target').removeClass('show');
                    }, 3000);
                }
            },
            error: function () {

            }
        });
    });

    $('.table-target tbody tr .btn-delete').each(function () {
        $(this).click(function (e) {
            var caseid = $(this).closest('tr').find('.caseId').html();
            var $this = $(this);

            $('#modal-1').modal('show');
            $('.btn-sure-delete').off('click').on('click', function () {
                $('#modal-1').modal('hide');
                $.ajax({
                    type: "GET",
                    url: '/Home/Delete',
                    dataType: "json",
                    data: { targetId: caseid },
                    contentType: "application/json; charset=utf-8",
                    success: function (connObjReturn) {
                        if (connObjReturn == true) {
                            //console.log("1");
                            $('.alert-success-target').addClass('show');
                            $this.closest('tr').remove();
                            setTimeout(function () {
                                $('.alert-success-target').removeClass('show');
                            }, 3000);
                        } else {
                            $('.alert-danger-target').addClass('show');
                            setTimeout(function () {
                                $('.alert-danger-target').removeClass('show');
                            }, 3000);
                        }
                        calculateTotalRecord();
                    },
                    error: function () {

                    }
                });
            });
            //console.log(caseid);
           
        });
    });



    $('#btnAddTarget').click(function () {
        var dropValue = $('#dropdownCase').val();
        var dropText = $('#dropdownCase :selected').text();
        console.log(dropValue);
        console.log(dropText);
        if ($(".table-target tbody td[id="+dropValue+"]").length)         // use this if you are using id to check
        {
            // it exists
            $('.target_add_fail').html('Target already existed!!!');
            $('.target_add_fail').removeClass('hide');
            setTimeout(function () {
                $('.target_add_fail').addClass('hide');
            }, 5000);
        }
        else {
            console.log("not existed");
            $.ajax({
                type: "POST",
                url: '/Home/AddTarget',
                dataType: "json",
                data: JSON.stringify({ target: { TargetId: dropValue, TargetName: dropText, Active : true } }),
                //data: { id: dropValue, name: dropText },
                contentType: "application/json; charset=utf-8",
                success: function (connObjReturn) {
                    if (connObjReturn == "Ok") {
                        $('.target_add_success').removeClass('hide');
                        setTimeout(function () {
                            $('.target_add_success').addClass('hide');
                        }, 5000);
                        $('.table-target tbody').append("<tr role='row'><td class='caseId' id='" + dropValue + "'>" + dropValue + "</td><td class='casename'>" + dropText +
                            "</td><td><input type='checkbox' class='chkActive' checked='checked' /></td>"+
                            "<td><button type='button' class='btn btn-orange btn-update btn-icon'>Update<i class='entypo-down'></i></button>" +
                            "<button type='button' class='btn btn-red btn-delete btn-icon'>Delete<i class='entypo-cancel'></i></button></td></tr>");
                        calculateTotalRecord();
                    } else {
                        $('.target_add_fail').html('Failed! Cannot add target!');
                        $('.target_add_fail').removeClass('hide');
                        setTimeout(function () {
                            $('.target_add_fail').addClass('hide');
                        }, 5000);
                    }
                },
                error: function () {
                }
            });
        }
        
    });
});