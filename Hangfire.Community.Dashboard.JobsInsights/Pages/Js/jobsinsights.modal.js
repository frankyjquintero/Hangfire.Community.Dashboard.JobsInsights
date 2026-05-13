// Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.modal.js
(function () {
    if (!window.JobsInsights) return;

    var Util = window.JobsInsights.Util;
    var ChartMod = window.JobsInsights.Chart;
    var config = window.JobsInsightsConfig;

    // Estado interno del modal
    var allDetailJobs = [];
    var filteredJobs = [];
    var currentJobType = null;
    var currentPage = 1;
    var pageSize = 20;
    var currentSort = { field: 'lastTimestamp', direction: 'desc' };

    // Referencias a elementos del DOM (se cachean al abrir)
    var overlay, container, jobtypeLabel, searchInput, stateFilter, applyBtn, clearBtn;
    var tbody, paginationDiv, tableHead, chartPanel, alertBanner, alertThreshold;

    // ── Inicialización de referencias (se llama al abrir por primera vez) ──
    function cacheElements() {
        overlay = document.getElementById('detail-modal-overlay');
        container = document.getElementById('detail-modal');
        jobtypeLabel = document.getElementById('detail-modal-jobtype');
        searchInput = document.getElementById('detail-modal-search');
        stateFilter = document.getElementById('detail-modal-state-filter');
        applyBtn = document.getElementById('detail-modal-apply');
        clearBtn = document.getElementById('detail-modal-clear');
        tbody = document.getElementById('detail-modal-body');
        paginationDiv = document.getElementById('detail-modal-pagination');
        tableHead = document.querySelector('#detail-modal-table thead');
        chartPanel = document.getElementById('duration-chart-panel');
        alertBanner = document.getElementById('alert-banner');
        alertThreshold = document.getElementById('alert-threshold');
    }

    // ── Eventos (se bindean una sola vez) ──
    var eventsBound = false;
    function bindEvents() {
        if (eventsBound) return;
        eventsBound = true;

        // Cierre del modal
        document.getElementById('detail-modal-close').addEventListener('click', close);
        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) close();
        });

        // Filtros
        applyBtn.addEventListener('click', function () {
            if (currentJobType) loadDetail(currentJobType);
        });
        clearBtn.addEventListener('click', function () {
            searchInput.value = '';
            stateFilter.value = '';
            if (currentJobType) loadDetail(currentJobType);
        });

        // Ordenación de columnas (delegado desde el thead)
        tableHead.addEventListener('click', function (e) {
            var th = e.target.closest('th.sortable');
            if (!th) return;
            var field = th.getAttribute('data-sort');
            if (currentSort.field === field) {
                currentSort.direction = currentSort.direction === 'asc' ? 'desc' : 'asc';
            } else {
                currentSort.field = field;
                currentSort.direction = 'asc';
            }
            sortAndFilter();
            renderDetailPage();
            updateSortIndicators();
        });

        // Paginación (delegado desde el div de paginación)
        paginationDiv.addEventListener('click', function (e) {
            var btn = e.target.closest('button');
            if (!btn) return;
            if (btn.id === 'prev-page') {
                if (currentPage > 1) { currentPage--; renderDetailPage(); }
            } else if (btn.id === 'next-page') {
                var totalPages = Math.ceil(filteredJobs.length / pageSize);
                if (currentPage < totalPages) { currentPage++; renderDetailPage(); }
            }
        });

        // Cambio de modo del gráfico (delegado desde el contenedor del gráfico)
        if (chartPanel) {
            chartPanel.addEventListener('click', function (e) {
                var modeBtn = e.target.closest('[data-mode]');
                if (!modeBtn) return;
                var mode = modeBtn.getAttribute('data-mode');
                document.querySelectorAll('[data-mode]').forEach(function (b) { b.classList.remove('active'); });
                modeBtn.classList.add('active');
                if (ChartMod) ChartMod.setMode(mode);
                if (allDetailJobs.length) ChartMod.render(allDetailJobs, mode);
            });

            // Exportar PNG
            var exportBtn = document.getElementById('export-chart');
            if (exportBtn) {
                exportBtn.addEventListener('click', function () {
                    var canvas = document.getElementById('duration-chart');
                    if (!canvas) return;
                    var link = document.createElement('a');
                    link.download = 'execution-duration-chart.png';
                    link.href = canvas.toDataURL('image/png');
                    link.click();
                });
            }

            // Threshold
            if (alertThreshold) {
                alertThreshold.addEventListener('input', function () {
                    if (allDetailJobs.length) ChartMod.render(allDetailJobs, ChartMod.getMode());
                });
            }
        }
    }

    // ── API pública ─────────────────────────────────────────────
    function open(jobType) {
        if (!overlay) cacheElements();
        bindEvents();

        currentJobType = jobType;
        currentPage = 1;
        currentSort = { field: 'lastTimestamp', direction: 'desc' };
        searchInput.value = '';
        stateFilter.value = '';
        jobtypeLabel.textContent = jobType;

        overlay.style.display = 'block';
        container.style.display = 'flex';
        loadDetail(jobType);
    }

    function close() {
        overlay.style.display = 'none';
        container.style.display = 'none';
    }

    // ── Carga de datos ─────────────────────────────────────────
    function loadDetail(jobType) {
        var state = stateFilter.value;
        var url = config.detailApiUrl + '?jobType=' + encodeURIComponent(jobType);
        if (state) url += '&state=' + encodeURIComponent(state);

        tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Loading...</td></tr>';
        paginationDiv.innerHTML = '';

        Util.fetchJson(url)
            .then(function (data) {
                allDetailJobs = data.jobs || [];
                sortAndFilter();
                renderDetailPage();
                if (ChartMod && typeof ChartMod.render === 'function') {
                    ChartMod.render(allDetailJobs, ChartMod.getMode());
                }
            })
            .catch(function (err) {
                tbody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error: ' + Util.escapeHtml(err.message) + '</td></tr>';
            });
    }

    // ── Filtrado y ordenación local ────────────────────────────
    function sortAndFilter() {
        var search = (searchInput.value || '').toLowerCase();
        filteredJobs = allDetailJobs.filter(function (j) {
            if (!search) return true;
            return (j.jobId && j.jobId.toLowerCase().indexOf(search) !== -1) ||
                (j.errorMessage && j.errorMessage.toLowerCase().indexOf(search) !== -1) ||
                (j.queue && j.queue.toLowerCase().indexOf(search) !== -1);
        });

        var field = currentSort.field;
        var dir = currentSort.direction;
        filteredJobs.sort(function (a, b) {
            var valA = a[field] != null ? a[field] : '';
            var valB = b[field] != null ? b[field] : '';
            if (field === 'lastTimestamp') {
                valA = new Date(a.lastTimestamp).getTime();
                valB = new Date(b.lastTimestamp).getTime();
            } else if (field === 'duration') {
                valA = parseFloat(a.duration) || 0;
                valB = parseFloat(b.duration) || 0;
            } else {
                valA = valA.toString().toLowerCase();
                valB = valB.toString().toLowerCase();
            }
            if (valA < valB) return dir === 'asc' ? -1 : 1;
            if (valA > valB) return dir === 'asc' ? 1 : -1;
            return 0;
        });
    }

    // ── Paginación ─────────────────────────────────────────────
    function renderDetailPage() {
        var start = (currentPage - 1) * pageSize;
        var pageJobs = filteredJobs.slice(start, start + pageSize);
        renderDetail(pageJobs, filteredJobs.length);
    }

    function renderDetail(jobs, total) {
        if (!jobs.length) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">No jobs found.</td></tr>';
            paginationDiv.innerHTML = '';
            return;
        }

        var html = '';
        jobs.forEach(function (j) {
            var shortId = j.jobId ? j.jobId.substring(0, 8) : '';
            var stateClass = j.lastState ? j.lastState.toLowerCase() : '';
            var badgeClass = stateClass === 'succeeded' ? 'label label-success' :
                stateClass === 'failed' ? 'label label-danger' : 'label label-default';
            var ts = j.lastTimestamp ? Util.relativeTime(new Date(j.lastTimestamp)) : '';
            var errorHtml = '';
            if (j.errorMessage) {
                if (j.errorMessage.length > 80) {
                    errorHtml = '<span class="error-short">' + Util.escapeHtml(j.errorMessage.substring(0, 80)) + '…</span>'
                        + '<span class="error-full" style="display:none;">' + Util.escapeHtml(j.errorMessage) + '</span> '
                        + '<a href="javascript:void(0)" class="error-toggle small">more</a>';
                } else {
                    errorHtml = Util.escapeHtml(j.errorMessage);
                }
            }

            var durHtml = 'N/A';
            if (j.duration != null) {
                var d = j.duration;
                var dc = d < 1 ? 'dur-green' : d < 5 ? 'dur-yellow' : 'dur-red';
                durHtml = '<span class="' + dc + '">' + d.toFixed(1) + 's</span>';
            }

            html += '<tr class="detail-row ' + stateClass + ' fade-in">';
            html += '<td><a href="' + jobDetailUrl(j.jobId) + '" target="_blank">#' + shortId + '</a></td>';
            html += '<td><a href="' + queueUrl(j.queue) + '" target="_blank" class="text-muted">' + Util.escapeHtml(j.queue) + '</a></td>';
            html += '<td><span class="' + badgeClass + '">' + j.lastState + '</span></td>';
            html += '<td>' + ts + '</td>';
            html += '<td>' + durHtml + '</td>';
            html += '<td>' + errorHtml + '</td>';
            html += '<td><a href="' + jobDetailUrl(j.jobId) + '" target="_blank" class="btn btn-xs btn-default">Detail</a></td>';
            html += '</tr>';
        });
        tbody.innerHTML = html;

        // Manejo de "more/less" para errores largos
        tbody.querySelectorAll('.error-toggle').forEach(function (toggle) {
            toggle.addEventListener('click', function () {
                var cell = this.parentNode;
                var short = cell.querySelector('.error-short');
                var full = cell.querySelector('.error-full');
                if (short.style.display !== 'none') {
                    short.style.display = 'none';
                    full.style.display = 'inline';
                    this.textContent = 'less';
                } else {
                    short.style.display = 'inline';
                    full.style.display = 'none';
                    this.textContent = 'more';
                }
            });
        });

        // Paginación
        var totalPages = Math.ceil(total / pageSize);
        var startCount = (currentPage - 1) * pageSize + 1;
        var endCount = Math.min(currentPage * pageSize, total);
        var pagHtml = '<div class="clearfix">';
        pagHtml += '<span class="pull-left">Showing ' + startCount + ' – ' + endCount + ' of ' + total + ' jobs</span>';
        pagHtml += '<div class="pull-right">';
        if (currentPage > 1) pagHtml += '<button class="btn btn-default btn-xs" id="prev-page">Previous</button> ';
        if (currentPage < totalPages) pagHtml += '<button class="btn btn-default btn-xs" id="next-page">Next</button>';
        pagHtml += '</div></div>';
        paginationDiv.innerHTML = pagHtml;
    }

    function updateSortIndicators() {
        tableHead.querySelectorAll('th.sortable').forEach(function (th) {
            th.classList.remove('asc', 'desc');
            if (th.getAttribute('data-sort') === currentSort.field) {
                th.classList.add(currentSort.direction);
            }
        });
    }

    // ── Helpers de URL ─────────────────────────────────────────
    function jobDetailUrl(jobId) {
        return (config.jobDetailBase || '/jobs/details') + '/' + jobId;
    }
    function queueUrl(queue) {
        return (config.queueBase || '/jobs/enqueued') + '/' + encodeURIComponent(queue);
    }

    // ── Exportación pública ────────────────────────────────────
    window.JobsInsights.DetailModal = {
        open: open,
        close: close
    };
})();