function MyDTBtnInit(tableName) {

    var table = $('#' + tableName).DataTable({
        //stateSave: true,
        scrollX: true,
        destroy: true,
        language:
            {
                "info": "_START_ to _END_ of _TOTAL_",
                "infoEmpty": "0",
                "paginate": {
                    "next": "NEXT <i class='fa fa-chevron-right'></i>",
                    "previous": "<i class='fa fa-chevron-left'></i> PREV"
                }
            },
        dom: 'B<rt><"row"<"col-sm-5 col-md-6"l><"col-sm-7 col-md-6"p>i>',
        //createdRow: function (nRow, aData, iDataIndex) {
        //    $(nRow).find('.dtb').attr('data-rowid', iDataIndex);
        //},
        buttons: [
            {
                extend: 'print',
                className: 'btn btn-primary btn-lg',
                text: '<i class="fa fa-print"></i>',
                titleAttr: 'Print'
            },
            {
                extend: 'copyHtml5',
                className: 'btn btn-warning btn-lg',
                text: '<i class="fas fa-copy"></i>',
                titleAttr: 'Copy'
            },
            {
                extend: 'excelHtml5',
                className: 'btn btn-success btn-lg',
                text: '<i class="fas fa-file-excel"></i>',
                titleAttr: 'Export to Excel'
            },
            {
                extend: 'csvHtml5',
                className: 'btn btn-info btn-lg',
                text: '<i class="fas fa-file-alt"></i>',
                titleAttr: 'Export to CSV'
            },
            {
                extend: 'pdfHtml5',
                className: 'btn btn-danger btn-lg',
                text: '<i class="fas fa-file-pdf"></i>',
                titleAttr: 'Export to PDF'
            },
            {
                extend: 'colvis',
                className: 'btn btn-primary btn-lg',
                text: '<i class="fa fa-columns"></i>',
                titleAttr: 'Column Visibility'
            }
        ]
    });

    MyDTBtnStyler(tableName, table, true, false);
}

function MyDTBtnInitCF(tableName, columnDefs) {

    columnDefs = columnDefs || [];

    var table = $('#' + tableName).DataTable({
        scrollX: true,
        destroy: true,
        language:
            {
                "info": "_START_ to _END_ of _TOTAL_",
                "infoEmpty": "0",
                "paginate": {
                    "next": "NEXT <i class='fa fa-chevron-right'></i>",
                    "previous": "<i class='fa fa-chevron-left'></i> PREV"
                }
            },
        dom: 'B<rt><"row"<"col-sm-5 col-md-6"l><"col-sm-7 col-md-6"p>i>',
        columnDefs: columnDefs,
        buttons: [
            {
                extend: 'print',
                className: 'btn btn-primary btn-lg',
                text: '<i class="fa fa-print"></i>',
                titleAttr: 'Print'
            },
            {
                extend: 'copyHtml5',
                className: 'btn btn-warning btn-lg',
                text: '<i class="fas fa-copy"></i>',
                titleAttr: 'Copy'
            },
            {
                extend: 'excelHtml5',
                className: 'btn btn-success btn-lg',
                text: '<i class="fas fa-file-excel"></i>',
                titleAttr: 'Export to Excel'
            },
            {
                extend: 'csvHtml5',
                className: 'btn btn-info btn-lg',
                text: '<i class="fas fa-file-alt"></i>',
                titleAttr: 'Export to CSV'
            },
            {
                extend: 'pdfHtml5',
                className: 'btn btn-danger btn-lg',
                text: '<i class="fas fa-file-pdf"></i>',
                titleAttr: 'Export to PDF'
            },
            {
                extend: 'colvis',
                className: 'btn btn-primary btn-lg',
                text: '<i class="fa fa-columns"></i>',
                titleAttr: 'Column Visibility'
            }
        ],
        initComplete: function () {
            this.api().columns('.' + tableName + 'Filter').every(function () {
                var column = this;
                var select = $('<select class="form-control"><option value=""></option></select>')
                    .appendTo($(column.footer()).empty())
                    .on('change', function () {
                        var val = $.fn.dataTable.util.escapeRegex(
                            $(this).val()
                        );

                        column
                            .search(val ? '^' + val + '$' : '', true, false)
                            .draw();

                        if (val === '') {
                            $(this).removeClass('filtered');
                        }
                        else {
                            $(this).addClass('filtered');
                        }

                        if ($('.dataTables_scrollFoot #' + tableName + 'tfoot').find(".filtered").length) {
                            $('.filterBtn').addClass("btn-warning");
                        }
                        else {
                            $('.filterBtn').removeClass("btn-warning");
                        }

                    });

                column.data().unique().sort().each(function (d, j) {
                    select.append('<option value="' + d + '">' + d + '</option>');
                });
            });
        }
    });

    MyDTBtnStyler(tableName, table, true, true);

    //table.columns.adjust();
}

function MyDTBtnInitSS(tableName, url, columns, data, search) {
    if (search === undefined) search = "";

    var table = $('#' + tableName).DataTable({
        serverSide: true,
        processing: true,
        scrollX: true,
        ajax: { "url": url, "data": data, "type": "POST" },
        columns: columns,
        search: { "search": search },
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        language:
            {
                "info": "_START_ to _END_ of _TOTAL_",
                "infoEmpty": "0",
                "paginate": {
                    "next": "NEXT <i class='fa fa-chevron-right'></i>",
                    "previous": "<i class='fa fa-chevron-left'></i> PREV"
                }
            },
        dom: 'B<rt><"row"<"col-sm-5 col-md-6"l><"col-sm-7 col-md-6"p>i>',
        buttons: [
            {
                extend: 'print',
                className: 'btn btn-primary btn-lg',
                text: '<i class="fa fa-print"></i>',
                titleAttr: 'Print'
            },
            {
                extend: 'copyHtml5',
                className: 'btn btn-warning btn-lg',
                text: '<i class="fas fa-copy"></i>',
                titleAttr: 'Copy'
            },
            {
                extend: 'excelHtml5',
                className: 'btn btn-success btn-lg',
                text: '<i class="fas fa-file-excel"></i>',
                titleAttr: 'Export to Excel'
            },
            {
                extend: 'csvHtml5',
                className: 'btn btn-info btn-lg',
                text: '<i class="fas fa-file-alt"></i>',
                titleAttr: 'Export to CSV'
            },
            {
                extend: 'pdfHtml5',
                className: 'btn btn-danger btn-lg',
                text: '<i class="fas fa-file-pdf"></i>',
                titleAttr: 'Export to PDF'
            },
            {
                extend: 'colvis',
                className: 'btn btn-primary btn-lg',
                text: '<i class="fa fa-columns"></i>',
                titleAttr: 'Column Visibility'
            }
        ]
    });

    MyDTBtnStyler(tableName, table, false, false);

    return table;
}

function MyDTBtnStyler(tableName, table, autoSearch, hasFilter) {

    var lhgrp = $('<div class="input-group"></div>').css({ 'margin-top': '2px' });
    var lhshow = $('<span class="input-group-addon">Show</span>');
    var lhentries = $('<span class="input-group-addon">entries</span>');

    var lhselect = $('#' + tableName + '_length > label > select').detach().addClass('extend');

    lhshow.appendTo(lhgrp);
    lhselect.appendTo(lhgrp);
    lhentries.appendTo(lhgrp);

    //var idiv = $('#' + tableName + '_info').css("padding-top", "0px");
    //$('<span class="input-group-addon"></span>').html(idiv).appendTo(lhgrp);

    var lhdiv = $('#' + tableName + '_length').html(lhgrp).css({ "display": "inline-block" });

    $('<div class="clearfix visible-xs" style="margin-top:2px"></div>').appendTo(lhdiv);
    var idiv = $('<div class="filterInfo"></div>').html($('#' + tableName + '_info').css({ 'padding-top': '0px' })).appendTo(lhdiv);
    $('<div class="clearfix visible-xs" style="margin-top:5px"></div>').appendTo(lhdiv);
    $('<span class="hidden-xs hidden-sm" style="padding-right:5px"></span>').appendTo(lhdiv);


    if (hasFilter) {
        $(".dataTables_scrollFoot").hide();
        var tFBtn = $('<button type="button" class="btn btn-default hidden-xs filterBtn" style="margin-top:2px" title="Show/Hide Filters"><i class="fa fa-filter"></i></button>');
        tFBtn.appendTo(lhdiv.parent());

        tFBtn.click(function () {
            $('.dataTables_scrollFoot').toggle();

            $('.filterBtn').toggleClass("btn-default btn-info");

            table.columns.adjust();
        });
    }

    var tBtn = $('<button type="button" id="' + tableName + 'TB" class="btn btn-default hidden-xs" style="margin-top:2px">More Actions</button>');
    tBtn.appendTo(lhdiv.parent());

    var btns = $('#' + tableName + '_wrapper').find('.dt-buttons').hide();

    tBtn.click(function () {
        var btn = $(this);
        btns.css({
            position: 'absolute',
            top: btn.offset().top + btn.outerHeight() + 5,
            left: btn.offset().left
        }).toggle();
    });

    var tBtn1 = $('<button type="button" id="' + tableName + 'TB1" class="btn btn-default">More Actions</button>');
    var pgDiv = $('#' + tableName + '_paginate');
    var tBSpan = $('<span class="visible-xs" style="text-align:center; margin-top:2px;"></span>');

    if (hasFilter) {
        $(".dataTables_scrollFoot").hide();
        var tFBtn1 = $('<button type="button" class="btn btn-default filterBtn"><i class="fa fa-filter"></i></button>');
        tFBtn1.appendTo(tBSpan);

        tFBtn1.click(function () {
            $('.dataTables_scrollFoot').toggle();

            $('.filterBtn').toggleClass("btn-default btn-info");

            table.columns.adjust();
        });
    }

    tBtn1.appendTo(tBSpan);
    tBSpan.appendTo(pgDiv.parent());

    tBtn1.click(function () {
        var btn = $(this);
        btns.css({
            position: 'absolute',
            top: btn.offset().top + btn.outerHeight(),
            left: btn.offset().left + (btn.outerWidth() / 2) - (btns.outerWidth() / 2)
        }).toggle();
    });


    if (autoSearch) {
        $('#' + tableName + 'SearchBox').on('input', function () {
            table.search(this.value).draw();
        });
    }

    $('#' + tableName + 'SearchBtn').click(function () {
        table.search($('#' + tableName + 'SearchBox').val()).draw();
    });

    if (table.columns()[0].length > 2) {
        $('#' + tableName + ' tbody').on('mouseenter touchstart', 'td', function () {
            var colIdx = table.cell(this).index().column;

            $(table.cells().nodes()).removeClass('active');
            $(table.column(colIdx).nodes()).addClass('active');
        });
    }

    $('#' + tableName + ' tbody').on('click', 'tr', function () {
        $(this).toggleClass('info');
    });
}

/**
 * Externally trigger the display of DataTables' "processing" indicator. 
 *
 *  @name processing()
 *  @summary Show / hide the processing indicator via the API
 *  @author [Allan Jardine](http://datatables.net)
 *  @requires DataTables 1.10+
 *  @param {boolean} show `true` to show the processing indicator, `false` to
 *    hide it.
 *
 * @returns {DataTables.Api} Unmodified API instance
 *
 *  @example
 *    // Show a processing indicator for two seconds on initialisation
 *    var table = $('#example').DataTable( {
 *      processing: true
 *    } );
 *    
 *    table.processing( true );
 *    
 *    setTimeout( function () {
 *      table.processing( false );
 *    }, 2000 );
 */
jQuery.fn.dataTable.Api.register('processing()', function (show) {
    return this.iterator('table', function (ctx) {
        ctx.oApi._fnProcessingDisplay(ctx, show);
    });
});