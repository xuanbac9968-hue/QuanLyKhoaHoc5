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
    createTable($("#apdexTable"), {"supportsControllersDiscrimination": true, "overall": {"data": [0.7910277473530486, 500, 1500, "Total"], "isController": false}, "titles": ["Apdex", "T (Toleration threshold)", "F (Frustration threshold)", "Label"], "items": [{"data": [0.2606132075471698, 500, 1500, "02 POST /Account/Login (Admin)"], "isController": false}, {"data": [0.8896933560477002, 500, 1500, "03 GET / (Admin Dashboard)"], "isController": false}, {"data": [0.9803921568627451, 500, 1500, "05 GET /ThanhToan (Duyet thanh toan)"], "isController": false}, {"data": [0.9948899371069182, 500, 1500, "01 GET /Account/Login"], "isController": false}, {"data": [0.5927672955974843, 500, 1500, "02 POST /Account/Login (Admin)-0"], "isController": false}, {"data": [0.5581761006289309, 500, 1500, "02 POST /Account/Login (Admin)-1"], "isController": false}, {"data": [0.997870528109029, 500, 1500, "03 GET / (Admin Dashboard)-0"], "isController": false}, {"data": [0.9169505962521295, 500, 1500, "03 GET / (Admin Dashboard)-1"], "isController": false}, {"data": [0.9914748508098892, 500, 1500, "04 GET /KhoaHoc (Danh sach khoa hoc)"], "isController": false}]}, function(index, item){
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
    createTable($("#statisticsTable"), {"supportsControllersDiscrimination": true, "overall": {"data": ["Total", 10956, 0, 0.0, 474.13864549105523, 0, 2833, 131.0, 1549.0, 2251.0, 2525.4300000000003, 178.04790847336432, 3900.297260076746, 182.63134379976924], "isController": false}, "titles": ["Label", "#Samples", "FAIL", "Error %", "Average", "Min", "Max", "Median", "90th pct", "95th pct", "99th pct", "Transactions/s", "Received", "Sent"], "items": [{"data": ["02 POST /Account/Login (Admin)", 1272, 0, 0.0, 1691.287735849057, 127, 2833, 2190.5, 2515.0, 2576.35, 2748.0, 20.851091731689724, 585.4391976345813, 33.67940012130352], "isController": false}, {"data": ["03 GET / (Admin Dashboard)", 1174, 0, 0.0, 323.66269165247013, 6, 2289, 247.0, 646.5, 849.25, 1473.25, 19.505873361357104, 531.8969744816156, 38.53552911135295], "isController": false}, {"data": ["05 GET /ThanhToan (Duyet thanh toan)", 1173, 0, 0.0, 36.78346121057124, 1, 1614, 5.0, 34.0, 165.5999999999999, 618.0, 20.01672326410812, 674.7825069111449, 19.89943777623249], "isController": false}, {"data": ["01 GET /Account/Login", 1272, 0, 0.0, 17.778301886792438, 0, 1048, 1.0, 18.0, 63.34999999999991, 560.0, 21.69463774048301, 113.49431091383545, 2.733015887228817], "isController": false}, {"data": ["02 POST /Account/Login (Admin)-0", 1272, 0, 0.0, 867.8765723270453, 118, 1890, 843.0, 1561.0, 1630.3999999999996, 1788.4299999999998, 21.08823236844723, 19.378932283895356, 13.18014523027952], "isController": false}, {"data": ["02 POST /Account/Login (Admin)-1", 1272, 0, 0.0, 823.3294025157239, 5, 2296, 782.0, 1573.2000000000003, 1878.7499999999995, 2155.27, 20.892188423888047, 567.3942969417252, 20.688163146311016], "isController": false}, {"data": ["03 GET / (Admin Dashboard)-0", 1174, 0, 0.0, 26.133730834752996, 0, 1077, 1.0, 56.5, 193.25, 265.5, 20.207931699256406, 2.229976837906224, 19.91191707475558], "isController": false}, {"data": ["03 GET / (Admin Dashboard)-1", 1174, 0, 0.0, 297.45996592844955, 5, 2180, 239.5, 600.0, 795.25, 1140.25, 19.506197454557537, 529.7532726672316, 19.315707245040375], "isController": false}, {"data": ["04 GET /KhoaHoc (Danh sach khoa hoc)", 1173, 0, 0.0, 56.68456947996599, 2, 1595, 12.0, 186.0, 254.49999999999977, 519.5999999999999, 20.578947368421055, 1023.661955180921, 20.418174342105264], "isController": false}]}, function(index, item){
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
    createTable($("#top5ErrorsBySamplerTable"), {"supportsControllersDiscrimination": false, "overall": {"data": ["Total", 10956, 0, "", "", "", "", "", "", "", "", "", ""], "isController": false}, "titles": ["Sample", "#Samples", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors"], "items": [{"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}]}, function(index, item){
        return item;
    }, [[0, 0]], 0);

});
