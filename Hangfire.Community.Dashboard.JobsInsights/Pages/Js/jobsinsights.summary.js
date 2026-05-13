// Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.summary.js
(function () {
    if (!window.JobsInsights) return;

    var Util = window.JobsInsights.Util;
    var config = window.JobsInsightsConfig;

    var tooltipDiv = document.getElementById('dot-tooltip');

    /**
     * Carga y renderiza el resumen.
     */
    function loadSummary() {
        var state = document.getElementById('summary-state-filter').value;
        var jobTypeFilter = (document.getElementById('summary-jobtype-filter').value || '').toLowerCase();
        var url = config.summaryApiUrl + (state ? '?state=' + encodeURIComponent(state) : '');

        var tbody = document.getElementById('summary-body');
        if (!tbody) return;
        tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Loading...</td></tr>';

        Util.fetchJson(url)
            .then(function (data) {
                var summaries = data.summaries || [];
                if (jobTypeFilter) {
                    summaries = summaries.filter(function (s) {
                        return s.jobType.toLowerCase().indexOf(jobTypeFilter) !== -1;
                    });
                }
                renderSummary(summaries);
            })
            .catch(function (err) {
                tbody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error: ' + Util.escapeHtml(err.message) + '</td></tr>';
            });
    }

    function renderSummary(summaries) {
        var tbody = document.getElementById('summary-body');
        if (!tbody) return;

        if (!summaries.length) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">No job types found.</td></tr>';
            updateStats(0, 0, 0);
            return;
        }

        var totalSuccess = 0, totalFailed = 0;
        var html = '';

        summaries.forEach(function (s) {
            // Dots
            var dots = '';
            if (s.recentExecutions) {
                s.recentExecutions.forEach(function (exec) {
                    var cls = exec.state ? exec.state.toLowerCase() : '';
                    dots += '<span class="state-dot ' + cls + '"'
                        + ' data-state="' + exec.state + '"'
                        + ' data-time="' + new Date(exec.timestamp).toLocaleString() + '"'
                        + ' data-jobid="' + (exec.jobId || '') + '"'
                        + ' data-duration="' + (exec.duration != null ? exec.duration.toFixed(1) + 's' : '') + '"'
                        + '></span>';
                });
            }

            // Determinar último estado
            var lastState = '';
            if (s.recentExecutions && s.recentExecutions.length) {
                lastState = s.recentExecutions[s.recentExecutions.length - 1].state || '';
            }
            var rowClass = lastState === 'Failed' ? ' last-failed' : '';

            // Fallos consecutivos
            var consecutive = 0;
            if (s.recentExecutions) {
                for (var i = s.recentExecutions.length - 1; i >= 0; i--) {
                    if (s.recentExecutions[i].state === 'Failed') consecutive++;
                    else break;
                }
            }
            var failBadge = '';
            if (consecutive >= 2) {
                failBadge = ' <span class="label label-danger consecutive-badge" title="' + consecutive + ' consecutive failures">🔥 ' + consecutive + '</span>';
            }

            // Avg Duration con color
            var avgDuration = 'N/A';
            if (s.avgDuration != null) {
                var secs = s.avgDuration;
                var durClass = secs < 1 ? 'dur-green' : secs < 5 ? 'dur-yellow' : 'dur-red';
                avgDuration = '<span class="' + durClass + '">' + secs.toFixed(1) + 's</span>';
            }

            var lastExec = s.lastExecution ? Util.relativeTime(new Date(s.lastExecution)) : 'N/A';

            html += '<tr class="' + rowClass + '">';
            html += '<td>' + Util.escapeHtml(s.jobType) + '</td>';
            html += '<td>' + dots + '</td>';
            html += '<td><span class="badge">' + s.successCount + '</span></td>';
            html += '<td><span class="badge">' + s.failedCount + '</span>' + failBadge + '</td>';
            html += '<td>' + lastExec + '</td>';
            html += '<td>' + avgDuration + '</td>';
            html += '<td><button class="btn btn-xs btn-default view-history" data-jobtype="' + Util.escapeHtml(s.jobType) + '">View Full History</button></td>';
            html += '</tr>';

            totalSuccess += s.successCount;
            totalFailed += s.failedCount;
        });

        tbody.innerHTML = html;
        updateStats(summaries.length, totalSuccess, totalFailed);

        // Tooltips en dots
        attachDotEvents();
        // Botones "View Full History"
        attachViewHistoryButtons();
    }

    function updateStats(types, success, failed) {
        var total = success + failed;
        document.getElementById('stat-types').textContent = types;
        document.getElementById('stat-total').textContent = total;
        document.getElementById('stat-rate').textContent = total ? (success / total * 100).toFixed(1) + '%' : '-';
        document.getElementById('stat-updated').textContent = new Date().toLocaleTimeString();
    }

    function attachDotEvents() {
        if (!tooltipDiv) return;
        document.querySelectorAll('#summary-body .state-dot').forEach(function (dot) {
            dot.addEventListener('mouseenter', function (e) {
                var el = e.target;
                var state = el.getAttribute('data-state');
                var time = el.getAttribute('data-time');
                var jobId = el.getAttribute('data-jobid');
                var duration = el.getAttribute('data-duration');
                var ttHtml = '<strong>' + state + '</strong><br>' + time;
                if (duration) ttHtml += '<br>Duration: ' + duration;
                if (jobId) ttHtml += '<br>Job: <a href="' + jobDetailUrl(jobId) + '" target="_blank">#' + jobId.substring(0, 8) + '</a>';
                tooltipDiv.innerHTML = ttHtml;
                tooltipDiv.style.display = 'block';
                var rect = el.getBoundingClientRect();
                tooltipDiv.style.left = (rect.left + window.scrollX + rect.width / 2 - tooltipDiv.offsetWidth / 2) + 'px';
                tooltipDiv.style.top = (rect.top + window.scrollY - tooltipDiv.offsetHeight - 8) + 'px';
            });
            dot.addEventListener('mouseleave', function () {
                tooltipDiv.style.display = 'none';
            });
            dot.addEventListener('click', function () {
                var jobId = this.getAttribute('data-jobid');
                if (jobId) window.open(jobDetailUrl(jobId), '_blank');
            });
        });
    }

    function attachViewHistoryButtons() {
        document.querySelectorAll('.view-history').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var jobType = this.getAttribute('data-jobtype');
                if (jobType && window.JobsInsights.DetailModal) {
                    window.JobsInsights.DetailModal.open(jobType);
                }
            });
        });
    }

    function jobDetailUrl(jobId) {
        return (config.jobDetailBase || '/jobs/details') + '/' + jobId;
    }

    window.JobsInsights.Summary = {
        loadSummary: loadSummary
    };
})();