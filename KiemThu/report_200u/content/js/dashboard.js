/*
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
var showControllersOnly = false;
var seriesFilter = "";
var filtersOnlySampleSeries = true;

/*
 * Add header in statistics table to group metrics by category
 * format
 *
 */
function summaryTableHeader(header) {
    var newRow = header.insertRow(-1);
    newRow.className = "tablesorter-no-sort";
    var cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 1;
    cell.innerHTML = "Requests";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 3;
    cell.innerHTML = "Executions";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 7;
    cell.innerHTML = "Response Times (ms)";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 1;
    cell.innerHTML = "Throughput";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 2;
    cell.innerHTML = "Network (KB/sec)";
    newRow.appendChild(cell);
}

/*
 * Populates the table identified by id parameter with the specified data and
 * format
 *
 */
function createTable(table, info, formatter, defaultSorts, seriesIndex, headerCreator) {
    var tableRef = table[0];

    // Create header and populate it with data.titles array
    var header = tableRef.createTHead();

    // Call callback is available
    if(headerCreator) {
        headerCreator(header);
    }

    var newRow = header.insertRow(-1);
    for (var index = 0; index < info.titles.length; index++) {
        var cell = document.createElement('th');
        cell.innerHTML = info.titles[index];
        newRow.appendChild(cell);
    }

    var tBody;

    // Create overall body if defined
    if(info.overall){
        tBody = document.createElement('tbody');
        tBody.className = "tablesorter-no-sort";
        tableRef.appendChild(tBody);
        var newRow = tBody.insertRow(-1);
        var data = info.overall.data;
        for(var index=0;index < data.length; index++){
            var cell = newRow.insertCell(-1);
            cell.innerHTML = formatter ? formatter(index, data[index]): data[index];
        }
    }

    // Create regular body
    tBody = document.createElement('tbody');
    tableRef.appendChild(tBody);

    var regexp;
    if(seriesFilter) {
        regexp = new RegExp(seriesFilter, 'i');
    }
    // Populate body with data.items array
    for(var index=0; index < info.items.length; index++){
        var item = info.items[index];
        if((!regexp || filtersOnlySampleSeries && !info.supportsControllersDiscrimination || regexp.test(item.data[seriesIndex]))
                &&
                (!showControllersOnly || !info.supportsControllersDiscrimination || item.isController)){
            if(item.data.length > 0) {
                var newRow = tBody.insertRow(-1);
                for(var col=0; col < item.data.length; col++){
                    var cell = newRow.insertCell(-1);
                    cell.innerHTML = formatter ? formatter(col, item.data[col]) : item.data[col];
                }
            }
        }
    }

    // Add support of columns sort
    table.tablesorter({sortList : defaultSorts});
}

$(document).ready(function() {

    // Customize table sorter default options
    $.extend( $.tablesorter.defaults, {
        theme: 'blue',
        cssInfoBlock: "tablesorter-no-sort",
        widthFixed: true,
        widgets: ['zebra']
    });

    var data = {"OkPercent": 100.0, "KoPercent": 0.0};
    var dataset = [
        {
            "label" : "FAIL",
            "data" : data.KoPercent,
            "color" : "#FF6347"
        },
        {
            "label" : "PASS",
            "data" : data.OkPercent,
            "color" : "#9ACD32"
        }];
    $.plot($("#flot-requests-summary"), dataset, {
        series : {
            pie : {
                show : true,
                radius : 1,
                label : {
                    show : true,
                    radius : 3 / 4,
                    formatter : function(label, series) {
                        return '<div style="font-size:8pt;text-align:center;padding:2px;color:white;">'
                            + label
                            + '<br/>'
                            + Math.round10(series.percent, -2)
                            + '%</div>';
                    },
                    background : {
                        opacity : 0.5,
                        color : '#000'
                    }
                }
            }
        },
        legend : {
            show : true
        }
    });

    // Creates APDEX table
    createTable($("#apdexTable"), {"supportsControllersDiscrimination": true, "overall": {"data": [0.6157231749886174, 500, 1500, "Total"], "isController": false}, "titles": ["Apdex", "T (Toleration threshold)", "F (Frustration threshold)", "Label"], "items": [{"data": [0.2374429223744292, 500, 1500, "02 POST /Account/Login (Admin)"], "isController": false}, {"data": [0.35136986301369866, 500, 1500, "03 GET / (Admin Dashboard)"], "isController": false}, {"data": [0.759939984996249, 500, 1500, "05 GET /ThanhToan (Duyet thanh toan)"], "isController": false}, {"data": [0.9928245270711024, 500, 1500, "01 GET /Account/Login"], "isController": false}, {"data": [0.6754729288975865, 500, 1500, "02 POST /Account/Login (Admin)-0"], "isController": false}, {"data": [0.3721461187214612, 500, 1500, "02 POST /Account/Login (Admin)-1"], "isController": false}, {"data": [0.9558219178082191, 500, 1500, "03 GET / (Admin Dashboard)-0"], "isController": false}, {"data": [0.3938356164383562, 500, 1500, "03 GET / (Admin Dashboard)-1"], "isController": false}, {"data": [0.8443360840210052, 500, 1500, "04 GET /KhoaHoc (Danh sach khoa hoc)"], "isController": false}]}, function(index, item){
        switch(index){
            case 0:
                item = item.toFixed(3);
                break;
            case 1:
            case 2:
                item = formatDuration(item);
                break;
        }
        return item;
    }, [[0, 0]], 3);

    // Create statistics table
    createTable($("#statisticsTable"), {"supportsControllersDiscrimination": true, "overall": {"data": ["Total", 13178, 0, 0.0, 907.8269843678835, 0, 5036, 533.5, 2372.0, 2974.0, 3917.4199999999983, 215.6545076668794, 4663.5056514597345, 221.93720837526797], "isController": false}, "titles": ["Label", "#Samples", "FAIL", "Error %", "Average", "Min", "Max", "Median", "90th pct", "95th pct", "99th pct", "Transactions/s", "Received", "Sent"], "items": [{"data": ["02 POST /Account/Login (Admin)", 1533, 0, 0.0, 2107.663405088059, 132, 5036, 2122.0, 3719.8, 3943.0, 4574.64, 25.31791907514451, 710.8549720014452, 40.89437319364162], "isController": false}, {"data": ["03 GET / (Admin Dashboard)", 1460, 0, 0.0, 1416.276712328766, 6, 4256, 1294.0, 2532.8, 2685.9, 3355.78, 24.420432877262233, 665.9099094060482, 48.244663779981934], "isController": false}, {"data": ["05 GET /ThanhToan (Duyet thanh toan)", 1333, 0, 0.0, 557.4681170292564, 2, 2489, 364.0, 2052.0, 2143.8999999999996, 2226.260000000001, 24.292899839626767, 818.936428187418, 24.150558629628954], "isController": false}, {"data": ["01 GET /Account/Login", 1533, 0, 0.0, 59.13633398564907, 0, 1668, 2.0, 166.0, 358.0, 544.3000000000004, 26.511024643320365, 138.69097559987028, 3.339767752918288], "isController": false}, {"data": ["02 POST /Account/Login (Admin)-0", 1533, 0, 0.0, 703.9073711676446, 123, 2405, 475.0, 1735.6000000000001, 1904.0, 2097.260000000001, 25.912778904665316, 23.812426708291078, 16.195486815415823], "isController": false}, {"data": ["02 POST /Account/Login (Admin)-1", 1533, 0, 0.0, 1403.705805609913, 6, 4571, 1391.0, 2659.6000000000004, 3056.6, 3603.840000000002, 25.39971833319526, 689.8107098107034, 25.151674208847652], "isController": false}, {"data": ["03 GET / (Admin Dashboard)-0", 1460, 0, 0.0, 114.75821917808226, 0, 1024, 3.0, 427.40000000000055, 814.8500000000001, 930.0, 25.035152097122673, 2.7626681513426385, 24.66842623632498], "isController": false}, {"data": ["03 GET / (Admin Dashboard)-1", 1460, 0, 0.0, 1301.4609589041086, 5, 4255, 1210.5, 2414.0, 2607.8, 3213.120000000001, 24.421249832731164, 663.2372635236853, 24.1827610648334], "isController": false}, {"data": ["04 GET /KhoaHoc (Danh sach khoa hoc)", 1333, 0, 0.0, 399.18979744936223, 2, 2174, 207.0, 1141.2000000000003, 1596.3, 2084.620000000001, 25.294597620448204, 1258.2333193288773, 25.096983576538456], "isController": false}]}, function(index, item){
        switch(index){
            // Errors pct
            case 3:
                item = item.toFixed(2) + '%';
                break;
            // Mean
            case 4:
            // Mean
            case 7:
            // Median
            case 8:
            // Percentile 1
            case 9:
            // Percentile 2
            case 10:
            // Percentile 3
            case 11:
            // Throughput
            case 12:
            // Kbytes/s
            case 13:
            // Sent Kbytes/s
                item = item.toFixed(2);
                break;
        }
        return item;
    }, [[0, 0]], 0, summaryTableHeader);

    // Create error table
    createTable($("#errorsTable"), {"supportsControllersDiscrimination": false, "titles": ["Type of error", "Number of errors", "% in errors", "% in all samples"], "items": []}, function(index, item){
        switch(index){
            case 2:
            case 3:
                item = item.toFixed(2) + '%';
                break;
        }
        return item;
    }, [[1, 1]]);

        // Create top5 errors by sampler
    createTable($("#top5ErrorsBySamplerTable"), {"supportsControllersDiscrimination": false, "overall": {"data": ["Total", 13178, 0, "", "", "", "", "", "", "", "", "", ""], "isController": false}, "titles": ["Sample", "#Samples", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors"], "items": [{"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}]}, function(index, item){
        return item;
    }, [[0, 0]], 0);

});
