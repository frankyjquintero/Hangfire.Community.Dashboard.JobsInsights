// Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.init.js
(function () {
    if (!window.JobsInsights || !window.JobsInsightsConfig) return;

    var Summary = window.JobsInsights.Summary;
    var Modal = window.JobsInsights.DetailModal;
    var Chart = window.JobsInsights.Chart;
    var config = window.JobsInsightsConfig;

    var refreshSeconds = 15;
    var refreshTimer = null;

    function init() {
        // Protección: solo ejecutar si existe el contenedor principal
        if (!document.getElementById('summary-view')) return;

        // Versión en topbar
        var versionEl = document.getElementById('version-display');
        if (versionEl) versionEl.textContent = config.version || '';

        // ── Eventos de la TopBar ───────────────────────────────
        document.getElementById('refresh-btn').addEventListener('click', function () {
            Summary.loadSummary();
        });

        document.getElementById('summary-apply').addEventListener('click', Summary.loadSummary);

        // ── Auto-refresh configurable ──────────────────────────
        startRefreshTimer();
        document.querySelectorAll('.refresh-interval-opt').forEach(function (el) {
            el.addEventListener('click', function (e) {
                e.preventDefault();
                refreshSeconds = parseInt(this.getAttribute('data-seconds'));
                var label = refreshSeconds > 0 ? refreshSeconds + 's' : 'Paused';
                document.getElementById('refresh-interval-label').textContent = label;
                startRefreshTimer();
            });
        });

        // ── Export CSV ─────────────────────────────────────────
        document.getElementById('export-csv').addEventListener('click', exportCsv);

        // ── Cleanup ────────────────────────────────────────────
        document.getElementById('cleanup-btn').addEventListener('click', function () {
            var days = parseInt(document.getElementById('retention-days').value);
            if (!confirm('Delete all job results older than ' + days + ' days?')) return;
            var msgEl = document.getElementById('cleanup-msg');
            msgEl.textContent = 'Cleaning...';
            window.JobsInsights.Util.fetchJson(config.adminApiBase + '/cleanup?days=' + encodeURIComponent(days), { method: 'POST' })
                .then(function (data) {
                    msgEl.innerHTML = '<span class="label label-success">Removed ' + data.removed + ' jobs.</span>';
                    Summary.loadSummary();
                })
                .catch(function (err) {
                    msgEl.innerHTML = '<span class="label label-danger">Error: ' + window.JobsInsights.Util.escapeHtml(err.message) + '</span>';
                });
        });

        // ── Gráfico: cambio de modo (delegado global porque el gráfico está en el modal) ──
        document.addEventListener('click', function (e) {
            var modeBtn = e.target.closest('[data-mode]');
            if (!modeBtn) return;
            var mode = modeBtn.getAttribute('data-mode');
            document.querySelectorAll('[data-mode]').forEach(function (b) { b.classList.remove('active'); });
            modeBtn.classList.add('active');
            if (Chart) Chart.setMode(mode);
            // Si el modal está abierto, redibujar con los datos actuales
            if (Modal && Modal.getCurrentData && typeof Modal.getCurrentData === 'function') {
                var data = Modal.getCurrentData();
                if (data && data.length) Chart.render(data, mode);
            }
        });

        // ── Threshold en el gráfico (también delegado) ────────
        document.addEventListener('input', function (e) {
            if (e.target.id === 'alert-threshold') {
                if (Modal && Modal.getCurrentData && typeof Modal.getCurrentData === 'function') {
                    var data = Modal.getCurrentData();
                    if (data && data.length) Chart.render(data, Chart.getMode());
                }
            }
        });

        // ── Carga inicial ─────────────────────────────────────
        Summary.loadSummary();
    }

    function startRefreshTimer() {
        clearInterval(refreshTimer);
        if (refreshSeconds > 0) {
            refreshTimer = setInterval(function () {
                Summary.loadSummary();
            }, refreshSeconds * 1000);
        }
    }

    function exportCsv() {
        var rows = [['Job Type', 'Success', 'Failed', 'Avg Duration', 'Last Execution']];
        document.querySelectorAll('#summary-body tr').forEach(function (tr) {
            var cells = tr.querySelectorAll('td');
            if (cells.length >= 6) {
                rows.push([
                    '"' + (cells[0].textContent || '').replace(/"/g, '""') + '"',
                    (cells[2].textContent || '').trim(),
                    (cells[3].textContent || '').trim(),
                    (cells[5].textContent || '').trim(),
                    (cells[4].textContent || '').trim()
                ]);
            }
        });
        var csv = rows.map(function (r) { return r.join(','); }).join('\n');
        var blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        var a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = 'job-insights-' + new Date().toISOString().slice(0, 10) + '.csv';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }

    // Arrancar
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();