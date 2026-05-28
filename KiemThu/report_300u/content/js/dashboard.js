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
    createTable($("#apdexTable"), {"supportsControllersDiscrimination": true, "overall": {"data": [0.5283880171184023, 500, 1500, "Total"], "isController": false}, "titles": ["Apdex", "T (Toleration threshold)", "F (Frustration threshold)", "Label"], "items": [{"data": [0.17110036275695284, 500, 1500, "02 POST /Account/Login (Admin)"], "isController": false}, {"data": [0.22517845554834523, 500, 1500, "03 GET / (Admin Dashboard)"], "isController": false}, {"data": [0.6308139534883721, 500, 1500, "05 GET /ThanhToan (Duyet thanh toan)"], "isController": false}, {"data": [0.9755729794933655, 500, 1500, "01 GET /Account/Login"], "isController": false}, {"data": [0.6819830713422007, 500, 1500, "02 POST /Account/Login (Admin)-0"], "isController": false}, {"data": [0.281136638452237, 500, 1500, "02 POST /Account/Login (Admin)-1"], "isController": false}, {"data": [0.9500324464633355, 500, 1500, "03 GET / (Admin Dashboard)-0"], "isController": false}, {"data": [0.24140168721609345, 500, 1500, "03 GET / (Admin Dashboard)-1"], "isController": false}, {"data": [0.6163454675231977, 500, 1500, "04 GET /KhoaHoc (Danh sach khoa hoc)"], "isController": false}]}, function(index, item){
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
    createTable($("#statisticsTable"), {"supportsControllersDiscrimination": true, "overall": {"data": ["Total", 14020, 0, 0.0, 1232.8586305278175, 0, 6757, 808.0, 3153.8999999999996, 3645.899999999998, 4590.949999999995, 231.6359911443016, 4984.156424640856, 237.89466510260053], "isController": false}, "titles": ["Label", "#Samples", "FAIL", "Error %", "Average", "Min", "Max", "Median", "90th pct", "95th pct", "99th pct", "Transactions/s", "Received", "Sent"], "items": [{"data": ["02 POST /Account/Login (Admin)", 1654, 0, 0.0, 2478.0024183796877, 127, 6439, 2586.0, 4122.0, 4551.5, 5046.450000000004, 27.568964080340027, 774.0578967518127, 44.530338465705476], "isController": false}, {"data": ["03 GET / (Admin Dashboard)", 1541, 0, 0.0, 2181.4302401038326, 6, 6757, 2383.0, 3802.5999999999995, 3938.8999999999996, 4944.0, 25.96505417108966, 708.0294996282583, 51.296195886830446], "isController": false}, {"data": ["05 GET /ThanhToan (Duyet thanh toan)", 1376, 0, 0.0, 748.2296511627898, 2, 2577, 567.0, 1680.3, 1765.4499999999996, 2035.3000000000002, 23.78277477228339, 801.7396339250221, 23.64342257635204], "isController": false}, {"data": ["01 GET /Account/Login", 1658, 0, 0.0, 117.30398069963817, 0, 908, 11.0, 344.10000000000014, 474.0999999999999, 748.6400000000003, 28.086937371889345, 146.9352768566516, 3.538295821263404], "isController": false}, {"data": ["02 POST /Account/Login (Admin)-0", 1654, 0, 0.0, 637.5169286577991, 120, 2275, 585.0, 1207.5, 1505.25, 1854.3500000000001, 28.14121650361548, 25.860238994045087, 17.588260314759676], "isController": false}, {"data": ["02 POST /Account/Login (Admin)-1", 1654, 0, 0.0, 1840.3984280532056, 6, 5584, 1883.5, 3312.0, 3645.0, 4302.700000000001, 27.625601282736504, 750.2616910868186, 27.35582002020978], "isController": false}, {"data": ["03 GET / (Admin Dashboard)-0", 1541, 0, 0.0, 155.3277092796887, 0, 777, 25.0, 501.9999999999998, 635.8999999999999, 748.1599999999999, 26.63693562884602, 2.939427466855079, 26.246746142095347], "isController": false}, {"data": ["03 GET / (Admin Dashboard)-1", 1541, 0, 0.0, 2026.0486696950047, 5, 6028, 2238.0, 3340.0, 3751.7999999999997, 4827.079999999998, 25.965491676214867, 705.1760971831401, 25.71192242156433], "isController": false}, {"data": ["04 GET /KhoaHoc (Danh sach khoa hoc)", 1401, 0, 0.0, 814.0264097073511, 2, 3137, 642.0, 1765.8, 2108.8999999999996, 2414.8, 24.142684818197484, 1200.9335318208255, 23.954070093055314], "isController": false}]}, function(index, item){
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
    createTable($("#top5ErrorsBySamplerTable"), {"supportsControllersDiscrimination": false, "overall": {"data": ["Total", 14020, 0, "", "", "", "", "", "", "", "", "", ""], "isController": false}, "titles": ["Sample", "#Samples", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors"], "items": [{"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}]}, function(index, item){
        return item;
    }, [[0, 0]], 0);

});
