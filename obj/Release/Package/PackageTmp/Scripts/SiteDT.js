function MyDTInit(tableName) {
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
        dom: '<rt><"row"<"col-sm-5 col-md-6"l><"col-sm-7 col-md-6"p>i>'
    });

    MyDTStyler(tableName, table, true);

}

function MyDTInitSS(tableName, url, columns) {
    var table = $('#' + tableName).DataTable({
        serverSide: true,
        processing: true,
        scrollX: true,
        ajax: { "url": url },
        columns: columns,
        lengthMenu: [[5, 10, 25, 50, 100], [5, 10, 25, 50, 100]],
        language:
            {
                "info": "_START_ to _END_ of _TOTAL_",
                "infoEmpty": "0",
                "paginate": {
                    "next": "NEXT <i class='fa fa-chevron-right'></i>",
                    "previous": "<i class='fa fa-chevron-left'></i> PREV"
                }
            },
        dom: '<rt><"row"<"col-sm-5 col-md-6"l><"col-sm-7 col-md-6"p>i>'
    });

    MyDTStyler(tableName, table, false);
}

function MyDTStyler(tableName, table, autoSearch) {
    var lhgrp = $('<div class="input-group"></div>').css({ 'margin-top': '2px' });
    var lhshow = $('<span class="input-group-addon">Show</span>');
    var lhentries = $('<span class="input-group-addon">entries</span>');

    var lhselect = $('#' + tableName + '_length > label > select').detach().addClass('extend');

    lhshow.appendTo(lhgrp);
    lhselect.appendTo(lhgrp);
    lhentries.appendTo(lhgrp);

    var lhdiv = $('#' + tableName + '_length').html(lhgrp).css({ "display": "inline-block" });

    $('<div class="clearfix visible-xs" style="margin-top:2px"></div>').appendTo(lhdiv);
    var idiv = $('<div class="filterInfo"></div>').html($('#' + tableName + '_info').css({ 'padding-top': '0px' })).appendTo(lhdiv);
    $('<div class="clearfix visible-xs" style="margin-top:5px"></div>').appendTo(lhdiv);
    $('<span class="hidden-xs hidden-sm" style="padding-right:5px"></span>').appendTo(lhdiv);

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

function MyDTInitPlain(tableName) {
    var table = $('#' + tableName).DataTable({
        scrollX: true,
        destroy: true,
        ordering: false,
        dom: 't'
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