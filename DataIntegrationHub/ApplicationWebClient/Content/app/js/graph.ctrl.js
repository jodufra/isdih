angApp.controller('graphController', ['$scope', '$http', function ($scope, $http) {
    $scope.isLoading = true;
    $scope.parent;
    $scope.element;
    $scope.channel;
    $scope.channels;
    $scope.hasResults = false;

    $scope.data = [];

    $scope.init = function (element, parent, channel, span) {
        $scope.element = element;
        $scope.parent = parent;
        if (channel != null) {
            $scope.channel = channel;
            $scope.channels = [channel];
        } else {
            $scope.channel = "T";
            $scope.channels = ["T", "P", "H"];
        }
        if (span != null) {
            $scope.span = span;
        } else {
            $scope.span = "m";
        }
        $scope.load()
    }

    $scope.load = function () {
        $scope.isLoading = true;
        $http.get(API_URL + "/records/?" + $.param({ channels: $scope.channels, span: $scope.span })).success(function (data) {
            $scope.data = data;
            $scope.updateGraph();
            $scope.isLoading = false;
        }).error(function () {
            $scope.data = [];
            $scope.updateGraph();
            $scope.isLoading = false;
            App.ShowErrorMessage("Error while communicating with server");
        });
    }

    $scope.updateGraph = function () {
        var graph = [];
        var minDate = new Date('31/12/2100');
        var maxDate = new Date('1/1/0001');
        var minValue = Number.POSITIVE_INFINITY
        var maxValue = Number.NEGATIVE_INFINITY;
        for (var i = 0; i < $scope.data.length; i++) {
            if ($scope.data[i].Channel != $scope.channel) continue;
            var date = parseServerDate($scope.data[i].DateCreated);
            minDate = minDate > date ? date : minDate;
            maxDate = maxDate < date ? date : maxDate;
            minValue = minValue > $scope.data[i].Value ? $scope.data[i].Value : minValue;
            maxValue = maxValue < $scope.data[i].Value ? $scope.data[i].Value : maxValue;
            graph.push([date, $scope.data[i].Value]);
        }
        $scope.hasResults = graph.length > 0;
        if (!$scope.hasResults) return;

        $($scope.element).width( $($scope.parent).width() );
        var plot = $.plot($($scope.element), [graph], {
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

        var tipname = $scope.element + "-tooltip";
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

}]);