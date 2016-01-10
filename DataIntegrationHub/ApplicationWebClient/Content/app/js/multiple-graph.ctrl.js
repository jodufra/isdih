angApp.controller('multipleGraphController', ['$scope', '$http', function ($scope, $http) {

    $scope.isLoading = true;
    $scope.hasTResults = false;
    $scope.hasPResults = false;
    $scope.hasHResults = false;
    $scope.channels = ["T", "P", "H"];
    $scope.minDate;
    $scope.maxDate;
    $scope.group = "avg"
    $scope.dataT = [];
    $scope.dataP = [];
    $scope.dataH = [];

    $scope.init = function (dateMin, dateMax, group) {
        $scope.minDate = new Date();
        $scope.minDate.setDate($scope.minDate.getDate() - 1);
        $scope.maxDate = new Date();

        if (dateMin != null) {
            $scope.minDate = dateMin;
        }
        if (dateMax != null) {
            $scope.maxDate = dateMax;
        }
        if (group != null) {
            $scope.group = group;
        }
        $scope.load()
    }

    $scope.load = function () {
        $scope.isLoading = true;
        $http.get(API_URL + "/records/?" + $.param({ minDate: parseDate($scope.minDate), maxDate: parseDate($scope.maxDate), group: $scope.group })).success(function (data) {
            $scope.updateGraph(data);
            $scope.isLoading = false;
        }).error(function () {
            $scope.updateGraph([]);
            $scope.isLoading = false;
            App.ShowErrorMessage("Error while communicating with server");
        });
    }

    $scope.updateGraph = function (data) {
        $scope.dataT = [];
        $scope.dataP = [];
        $scope.dataH = [];
        for (var i = 0; i < data.length; i++) {
            if (data[i].Channel != "T") $scope.dataT.push(data[i]);
            if (data[i].Channel != "P") $scope.dataP.push(data[i]);
            if (data[i].Channel != "H") $scope.dataH.push(data[i]);
        }

        drawGraph("T", $scope.dataT);
        $scope.hasTResults = $scope.dataT.length > 0;
        drawGraph("P", $scope.dataP);
        $scope.hasPResults = $scope.dataP.length > 0;
        drawGraph("H", $scope.dataH);
        $scope.hasHResults = $scope.dataH.length > 0;
    }

    function drawGraph(channel, data) {
        var element = "#graph-" + channel;
        var wrapper = "#graph-" + channel + "-wrapper";

        $(element).width($(wrapper).width());


        var graph = [];
        var minDate = new Date('31/12/2100');
        var maxDate = new Date('1/1/0001');
        var minValue = Number.POSITIVE_INFINITY
        var maxValue = Number.NEGATIVE_INFINITY;
        for (var i = 0; i < data.length; i++) {
            var date = new Date(data[i].Date);
            minDate = minDate > date ? date : minDate;
            maxDate = maxDate < date ? date : maxDate;
            minValue = minValue > data[i].Value ? data[i].Value : minValue;
            maxValue = maxValue < data[i].Value ? data[i].Value : maxValue;
            graph.push([date, data[i].Value]);
        }

        var plot = $.plot($(element), [graph], {
            series: {
                label: "", lines: { show: true, lineWidth: 1, fill: 0.6 },
                color: $scope.channel == "T" ? '#E89623' : $scope.channel == "P" ? '#8BC540' : '#1C499C', shadowSize: 0,
            },
            yaxis: {
                min: minValue, max: maxValue, tickColor: '#aaa', font: { lineHeight: 13, style: "normal", color: "#454545", },
                shadowSize: 0,
            },
            xaxis: {
                mode: "time", minTickSize: [1, "hour"],
                tickColor: '#aaa', show: true, font: { lineHeight: 13, style: "normal", color: "#454545", },
                shadowSize: 0, min: minDate, max: maxDate
            },
            grid: { borderWidth: 0, borderColor: '#eee', labelMargin: 5, hoverable: true, clickable: true, mouseActiveRadius: 6, },
            legend: { container: '.flc-dynamic', backgroundOpacity: 0.5, noColumns: 0, backgroundColor: $scope.channel == "T" ? '#E89623' : $scope.channel == "P" ? '#8BC540' : '#1C499C', lineWidth: 0 }
        });

        var tipname = element + "-tooltip";
        $("<div id='" + tipname + "'></div>").css({
            position: "absolute",
            display: "none",
            border: "1px solid " + $scope.channel == "T" ? '#E89623' : $scope.channel == "P" ? '#8BC540' : '#1C499C',
            padding: "2px",
            "background-color": $scope.channel == "T" ? '#E89623' : $scope.channel == "P" ? '#8BC540' : '#1C499C',
            opacity: 0.80
        }).appendTo("body");

        plot.draw();
    }

    function parseDate(date) {
        if (date == null) return "";
        var day = date.getDay() + 1;        // yields day
        var month = date.getMonth() + 1;    // yields month
        var year = date.getFullYear();  // yields year
        var hour = date.getHours();     // yields hours 
        var minute = date.getMinutes(); // yields minutes
        var second = date.getSeconds(); // yields seconds

        return (day < 10 ? "0" : "") + day + "/" + (month < 10 ? "0" : "") + month + "/" + year + " " + (hour < 10 ? "0" : "") + hour + ':' + (minute < 10 ? "0" : "") + minute + ':' + (second < 10 ? "0" : "") + second;

    }
}]);