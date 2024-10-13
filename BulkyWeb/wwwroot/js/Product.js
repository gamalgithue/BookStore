var dataTable;
$(document).ready(function (){
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tbldata').DataTable({
        "ajax": { url: '/admin/product/getall'},
        "columns": [
            { data: 'title'},
            { data: 'isbn'},
            { data: 'price' },
            { data: 'author' }




        ]
    });
}