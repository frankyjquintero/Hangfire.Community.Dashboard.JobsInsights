// Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.chart.js
(function () {
    if (!window.JobsInsights) return;

    var Util = window.JobsInsights.Util;

    var chartInstance = null;   // instancia actual de Chart.js
    var currentMode = 'exec';   // 'exec', '1d', '7d', '15d', '30d', '45d', 'throughput'

    /**
     * Renderiza el gráfico según el modo y los datos proporcionados.
     * @param {Array} allJobs - Array de objetos job (de la API de detalle)
     * @param {string} mode - Modo de visualización
     */
    function renderChart(allJobs, mode) {
        if (!allJobs || allJobs.length === 0) return;
        currentMode = mode;

        var labels = [], values = [];
        var threshold = parseFloat(document.getElementById('alert-threshold').value) || 5;
        var alertBanner = document.getElementById('alert-banner');

        // ── Throughput ─────────────────────────────────────────
        if (mode === 'throughput') {
            var now = new Date();
            var since = new Date(now.getTime() - 24 * 60 * 60 * 1000);
            var byHour = {};
            allJobs.forEach(function (j) {
                var ts = new Date(j.lastTimestamp);
                if (ts >= since && ts <= now) {
                    var hourKey = ts.getHours();
                    if (!byHour[hourKey]) byHour[hourKey] = { total: 0, success: 0 };
                    byHour[hourKey].total++;
                    if (j.lastState === 'Succeeded') byHour[hourKey].success++;
                }
            });

            var totalExecs = [], successRates = [];
            for (var h = 0; h < 24; h++) {
                labels.push(('0' + h).slice(-2) + ':00');
                var bucket = byHour[h] || { total: 0, success: 0 };
                totalExecs.push(bucket.total);
                var rate = bucket.total > 0 ? (bucket.success / bucket.total * 100) : null;
                successRates.push(rate !== null ? Math.round(rate) : null);
            }

            var datasets = [
                {
                    label: 'Total Executions',
                    type: 'bar',
                    data: totalExecs,
                    backgroundColor: 'rgba(51,122,183,0.6)',
                    borderColor: '#337ab7',
                    borderWidth: 1,
                    yAxisID: 'y-count'
                },
                {
                    label: 'Success Rate (%)',
                    type: 'line',
                    data: successRates,
                    borderColor: '#5cb85c',
                    backgroundColor: 'transparent',
                    pointBackgroundColor: '#5cb85c',
                    pointRadius: 3,
                    lineTension: 0.2,
                    spanGaps: true,
                    yAxisID: 'y-rate'
                }
            ];

            var options = getThroughputOptions();
            drawChart(labels, datasets, options);
            return;
        }

        // ── Modos de duración ─────────────────────────────────
        if (mode === 'exec') {
            var sorted = allJobs
                .filter(function (j) { return j.duration != null; })
                .sort(function (a, b) { return new Date(a.lastTimestamp) - new Date(b.lastTimestamp); })
                .slice(-100);
            sorted.forEach(function (j) {
                var d = new Date(j.lastTimestamp);
                labels.push(d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
                values.push(parseFloat(j.duration.toFixed(2)));
            });
        } else if (mode === '1d') {
            var today = new Date().toISOString().slice(0, 10);
            var byHour = {};
            allJobs.forEach(function (j) {
                if (j.duration == null) return;
                var ts = new Date(j.lastTimestamp);
                if (ts.toISOString().slice(0, 10) !== today) return;
                var hour = ts.getHours();
                if (!byHour[hour]) byHour[hour] = { total: 0, count: 0 };
                byHour[hour].total += j.duration;
                byHour[hour].count += 1;
            });
            for (var h = 0; h <= 23; h++) {
                labels.push(String(h).padStart(2, '0') + ':00');
                var avg = byHour[h] ? (byHour[h].total / byHour[h].count) : null;
                values.push(avg !== null ? parseFloat(avg.toFixed(2)) : null);
            }
        } else {
            var dayOptions = { '7d': 7, '15d': 15, '30d': 30, '45d': 45 };
            var days = dayOptions[mode] || 7;
            var cutoff = new Date();
            cutoff.setDate(cutoff.getDate() - days);
            var byDay = {};
            allJobs.forEach(function (j) {
                if (j.duration == null) return;
                var ts = new Date(j.lastTimestamp);
                if (ts < cutoff) return;
                var key = ts.toISOString().slice(0, 10);
                if (!byDay[key]) byDay[key] = { total: 0, count: 0 };
                byDay[key].total += j.duration;
                byDay[key].count += 1;
            });
            for (var i = days - 1; i >= 0; i--) {
                var d = new Date();
                d.setDate(d.getDate() - i);
                var key = d.toISOString().slice(0, 10);
                labels.push(key.slice(5));
                var avg = byDay[key] ? (byDay[key].total / byDay[key].count) : null;
                values.push(avg !== null ? parseFloat(avg.toFixed(2)) : null);
            }
        }

        // ── Estadísticas de la serie ──────────────────────────
        var filteredVals = values.filter(function (v) { return v != null && !isNaN(v); });
        var avgValue = filteredVals.length > 0
            ? filteredVals.reduce(function (a, b) { return a + b; }, 0) / filteredVals.length
            : null;
        var exceeded = avgValue !== null && avgValue > threshold;
        if (alertBanner) alertBanner.style.display = exceeded ? 'block' : 'none';

        var sortedVals = filteredVals.slice().sort(function (a, b) { return a - b; });
        var min = sortedVals[0] || 0;
        var max = sortedVals[sortedVals.length - 1] || 0;
        var p95 = sortedVals[Math.floor(sortedVals.length * 0.95)] || max;
        var statsDiv = document.getElementById('chart-stats');
        if (statsDiv) {
            statsDiv.innerHTML =
                '<span class="label label-default">Min: ' + min.toFixed(1) + 's</span> ' +
                '<span class="label label-default">Max: ' + max.toFixed(1) + 's</span> ' +
                '<span class="label label-default">P95: ' + p95.toFixed(1) + 's</span>';
        }

        var lineColor = exceeded ? '#d9534f' : '#337ab7';
        var dataset = {
            label: 'Avg Duration (s)',
            data: values,
            borderColor: lineColor,
            backgroundColor: 'rgba(51,122,183,0.08)',
            pointBackgroundColor: function (ctx) {
                var v = ctx.raw;
                if (v == null) return '#ccc';
                return v < 1 ? '#5cb85c' : v < 5 ? '#f0ad4e' : '#d9534f';
            },
            pointRadius: mode === 'exec' ? 4 : 5,
            pointHoverRadius: 7,
            borderWidth: 2,
            tension: mode === 'exec' ? 0.2 : 0.3,
            spanGaps: true
        };

        var options = getDurationOptions(mode, allJobs);
        drawChart(labels, [dataset], options);
    }

    function getThroughputOptions() {
        return {
            responsive: true,
            maintainAspectRatio: true,
            aspectRatio: 4,
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        var value = tooltipItem.yLabel;
                        if (tooltipItem.datasetIndex === 1 && value != null) {
                            return value + '%';
                        }
                        return value + ' jobs';
                    }
                }
            },
            scales: {
                xAxes: [{ gridLines: { display: false } }],
                yAxes: [
                    {
                        id: 'y-count',
                        position: 'left',
                        ticks: { beginAtZero: true },
                        scaleLabel: { display: true, labelString: 'Executions' },
                        gridLines: { color: 'rgba(0,0,0,0.05)' }
                    },
                    {
                        id: 'y-rate',
                        position: 'right',
                        ticks: {
                            beginAtZero: true,
                            suggestedMin: 0,
                            suggestedMax: 100,
                            callback: function (v) { return v + '%'; }
                        },
                        scaleLabel: { display: true, labelString: 'Success Rate (%)' },
                        gridLines: { drawOnChartArea: false }
                    }
                ]
            }
        };
    }

    function getDurationOptions(mode, allJobs) {
        return {
            responsive: true,
            maintainAspectRatio: true,
            aspectRatio: 4,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            return ctx.raw != null ? ctx.raw + 's' : 'No data';
                        },
                        title: function (items) {
                            if (mode !== 'exec') return items[0].label;
                            var job = allJobs
                                .filter(function (j) { return j.duration != null; })
                                .sort(function (a, b) { return new Date(a.lastTimestamp) - new Date(b.lastTimestamp); })
                                .slice(-100)[items[0].dataIndex];
                            return job ? new Date(job.lastTimestamp).toLocaleString() : items[0].label;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: false,
                    suggestedMin: 0,
                    ticks: {
                        callback: function (v) { return v + 's'; },
                        maxTicksLimit: 6
                    },
                    grid: { color: 'rgba(0,0,0,0.05)' }
                },
                x: {
                    grid: { display: false },
                    ticks: {
                        maxTicksLimit: mode === '1d' ? 24 : 10,
                        autoSkip: mode !== '1d',
                        maxRotation: 0
                    }
                }
            }
        };
    }

    /**
     * Dibuja el gráfico en el canvas 'duration-chart'.
     * @param {Array} labels
     * @param {Array} datasets
     * @param {object} options
     */
    function drawChart(labels, datasets, options) {
        var container = document.getElementById('duration-chart-panel');
        if (!container) return;

        var oldCanvas = document.getElementById('duration-chart');
        if (oldCanvas) oldCanvas.remove();

        var newCanvas = document.createElement('canvas');
        newCanvas.id = 'duration-chart';
        container.appendChild(newCanvas);

        var ctx = newCanvas.getContext('2d');
        if (!ctx) return;

        if (chartInstance) {
            chartInstance.destroy();
            chartInstance = null;
        }

        try {
            chartInstance = new Chart(ctx, {
                type: 'line',
                data: { labels: labels, datasets: datasets },
                options: options
            });
        } catch (e) {
            console.error('[JobsInsights.Chart] Error al crear gráfico:', e);
        }
    }

    /** Cambia el modo y redibuja */
    function setMode(mode) {
        currentMode = mode;
        // El redibujado lo dispara Detail cuando tenga los datos
    }

    window.JobsInsights.Chart = {
        render: renderChart,
        setMode: setMode,
        getMode: function () { return currentMode; }
    };
})();