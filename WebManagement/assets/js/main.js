$(document).ready(function () {
    function calculateTotalRecord() {
        var totaltarget = $('.table-target tbody tr').length;
        $('.total-target').text(totaltarget);
    }
    calculateTotalRecord();

    $('#dropdownhotnumberCase').on('change', function () {
        //alert(this.value);
        var tempText = $(this).find("option:selected").text();
        //console.log(tempText);
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

    $('.table-target tbody tr .btn-delete').each(function () {
        $(this).click(function (e) {
            var caseid = $(this).closest('tr').find('.caseId').html();
            var $this = $(this);

            $('#modal-1').modal('show');
            $('.btn-sure-delete').click(function () {
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
                            }, 5000);
                            
                        } else {
                            $('.alert-danger-target').addClass('show');
                            setTimeout(function () {
                                $('.alert-danger-target').removeClass('show');
                            }, 5000);
                            //console.log("2");
                        }
                        calculateTotalRecord();

                        //recountConnect();
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
                data: JSON.stringify({ target: { TargetId: dropValue, TargetName: dropText } }),
                //data: { id: dropValue, name: dropText },
                contentType: "application/json; charset=utf-8",
                success: function (connObjReturn) {
                    if (connObjReturn == "Ok") {
                        $('.target_add_success').removeClass('hide');
                        setTimeout(function () {
                            $('.target_add_success').addClass('hide');
                        }, 5000);
                        $('.table-target tbody').append("<tr role='row'><td class='caseId' id='"+dropValue+"'>" + dropValue + "</td><td>" + dropText + "</td><td><a href='javascript:;' class='btn btn-danger btn-sm btn-icon btn-delete icon-left'><i class='entypo-cancel'></i>Delete</a></td></tr>");
                        calculateTotalRecord();
                    } else {
                        $('.target_add_fail').html('Failed! Cannot add target!');
                        $('.target_add_fail').removeClass('hide');
                        setTimeout(function () {
                            $('.target_add_fail').addClass('hide');
                        }, 5000);
                    }
                    

                    //recountConnect();
                },
                error: function () {
                }
            });
        }
        
    });
});