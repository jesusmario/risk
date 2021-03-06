﻿function obtenerIdCurso() {
    var folio = $('#folio').val();
    if (folio != "") {
        $.ajax({
            url: baseUrl + "Orden/ObtenerPorFolio",
            data: { folio: folio },
            type: 'GET',
            dataType: 'json',
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $("#orden-id").val(data.Id);
            }
        });
    }
    return false;
}

function cargarTabla() {
    obtenerIdCurso();
    var table = $('#table-historiales').DataTable();
    var id = $('#folio').val();
    table.destroy();
    $('#table-historiales').DataTable({
        "autoWidth": true,
        "processing": true,
        "ajax": baseUrl + "Historial/ObtenerPorOrden/" + id,
        "columns": [
            { "data": "Id", visible: false, searchable: false },
            { "data": "Fecha", className: "fecha" },
            { "data": "Descripcion" },
            { "data": "Estado" },
            { "data": "Ciudad" },
            { "data": "EstadoPaquete" }
        ]
    });

    activarRenglon();
}

function add() {
    var modalC = $("#mdContent");
    $('#mdMain').modal();
    modalC.load(baseUrl + 'Historial/Add/0', {});
}

function obtenerId() {
    var table = $('#table-historiales').DataTable();
    var id = 0;
    if (table.$('tr.info')[0] != undefined) {
        var selectedIndex = table.$('tr.info')[0]._DT_RowIndex
        var row = table.row(selectedIndex).data();
        id = row.Id;
    }
    return id;
}

function activarRenglon() {
    var singleSelect = $('.datatable-selection-single').DataTable();
    $('.datatable-selection-single tbody').on('click', 'tr', function () {
        if ($(this).hasClass('info')) {
            $(this).removeClass('info');
        }
        else {
            singleSelect.$('tr.info').removeClass('info');
            $(this).addClass('info');
        }
    });
}


function cargarDatos() {
    var id = $.trim($("#estado-id").val());
    if (id != "" && id != 0) {
        $.ajax({
            url: baseUrl + "Historial/ObtenerPorId",
            data: { id: id },
            type: 'GET',
            dataType: 'json',
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $("#descripcion").val(data.Descripcion);
                $("#municipio").val(data.Ciudad);
                $("#estado").val(data.Estado);
                $("#estadoPaquete").val(data.EstadoPaquete);
            }
        });
    }
    return false;
}



function guardar() {
    var id = $.trim($("#estado-id").val());
    var descripcion = $.trim($("#descripcion").val());
    var estado = $.trim($("#estado").val());
    var ciudad = $.trim($("#municipio").val());
    var estadoPaquete = $.trim($("#estadoPaquete").val());
    var idUsuario = 0;
    var idOrden = $('#folio').val();

    if (estado === "PENDIENTE") {
        estado = 1;
    }
    if (estado === "ENTREGADO") {
        estado = 2;
    }
    if (estado === "CANCELADO") {
        estado = 3;
    }

    //obtenerLatLng();
    
        $.ajax({
            url: baseUrl + "Historial/Guardar/",
            data: {
                id: id, descripcion: descripcion, ciudad: ciudad, estado: estado, estadoPaquete: estadoPaquete, idUsuario: idUsuario, idOrden: idOrden
            },
            cache: false,
            tradicional: true,
            success: function (data) {
                if (data == "true") {
                    var modal = $("#mdMain");
                    modal.modal("hide");
                    activarRenglon();
                }
            },
            error: function (xhr, exception) {

            }
        });
        if (id != 0) {
            swal("Listo", "Se ha actualizado el historial", "success");
        } else {
            swal("Listo", "Se ha guardado el historial", "success");
        }
        cargarTabla();
}


function edit() {
    var modalC = $("#mdContent");
    var id = obtenerId();
    if (id != 0) {
        $('#mdMain').modal();
        modalC.load(baseUrl + 'Historial/Add/' + id, function () {
            cargarDatos();
        });
    } else {
        swal("Seleccione un registro");
        return false;
    }
}

function del() {
    var id = obtenerId();
    if (id != 0) {
        swal({
            title: "¿Estas seguro?",
            text: "Una vez eliminado, no podrás recuperar este historial",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
            .then((willDelete) => {
                if (willDelete) {
                    $.ajax({
                        url: baseUrl + "Historial/Borrar/",
                        data: {
                            id: id
                        },
                        cache: false,
                        tradicional: true,
                        success: function (data) {
                            if (data == "true") {
                                cargarTabla();
                            }
                        },
                        error: function (xhr, exception) {

                        }
                    });
                    swal("Poof! Haz eliminado el historial", {
                        icon: "success",
                    });
                } else {
                    swal("Se ha cancelado la operación");
                    cargarTabla();
                }
            });
    } else {
        swal("Seleccione un registro");
        return false;
    }
    cargarTabla();
}