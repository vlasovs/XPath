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
            "dataType": 'json',
            
            "dataSrc": function (json) {
                data = new Array(json.data.length);
                for (var i = 0, ien = json.data.length; i < ien; i++) {                                        
                    
                    var p = $(json.data[i].code);                    
                    p.find("img").attr("width", "100");
                    p.find("img").attr("height", "100");
                    var hp = p.html();
                    if (hp != undefined) {
                        json.data[i].code = hp;
                    } else {
                        json.data[i].code = "null";
                    }               

                    data[i] = new Array(11);    
                    data[i][0] = json.data[i].id;
                    data[i][1] = json.data[i].count;
                    data[i][2] = '<a href=' + json.data[i].url+'>'+json.data[i].url +'</a>';
                    data[i][3] = json.data[i].download;
                    data[i][4] = json.data[i].analyzis;
                    data[i][5] = '<div style="width:300px;height:150px;overflow:auto;padding:5px;">' + json.data[i].code + '</div>';
                    data[i][6] = json.data[i].xpath;
                    data[i][7] = json.data[i].page;
                    data[i][8] = json.data[i].regex;
                    data[i][9] = json.data[i].date;
                    data[i][10] = json.data[i].tag;

                }
                return data;
            }
            
        },
        /*
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
        */

    })
} );
