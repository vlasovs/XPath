$(document).ready(function () {
      
    /*
    var content = "<table>"
    
    content += '<thead>'

    content += "<th>id</th>"
    content += "<th>count</th>"
    content += "<th>url</th>"
    content += "<th>download</th>"
    content += "<th>analyzis</th>"
    content += "<th>code</th>"
    content += "<th>xpath</th>"
    content += "<th>page</th>"
    content += "<th>regex</th>"
    content += "<th>date</th>"
    content += "<th>tag</th>"

    content += '</thead>'

    content += '<tbody>'    
    content += '<tr>'
    
    content += "<td>id</td>"
    content += "<td>count</td>"
    content += "<td>url</td>"
    content += "<td>download</td>"
    content += "<td>analyzis</td>"
    content += "<td>code</td>"
    content += "<td>xpath</td>"
    content += "<td>page</td>"
    content += "<td>regex</td>"
    content += "<td>date</td>"    
    content += "<td>tag</td>"

    content += '</tr>'
    content += '</tbody>'
    content += '</table>'

    $('body').append(content)
    var t = $('table')
    t.id = 'myTable'
    t.addClass('display')
    */
    $('#table_id').DataTable({
        "processing": true,
        "serverSide": true,
        "ajax": {
            "url": "/Data/DataRequest1",           
            "dataType": 'json'
        },
        "columns": [
            { "data": "id" },
            { "data": "count" },
            { "data": "url" },
            { "data": "download" },
            { "data": "analyzis" },
            { "data": "code" },
            { "data": "xpath" },
            { "data": "page" },
            { "data": "regex" },
            { "data": "date" },
            { "data": "tag" }
        ]

    })
} );
