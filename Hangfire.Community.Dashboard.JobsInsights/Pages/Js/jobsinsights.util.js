// Hangfire.Community.Dashboard.JobsInsights.Pages.Js.jobsinsights.util.js
(function () {
    /**
     * Sanitiza una cadena para prevenir inyección HTML.
     * @param {string} text
     * @returns {string}
     */
    function escapeHtml(text) {
        if (!text) return '';
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(text));
        return div.innerHTML;
    }

    /**
     * Realiza una petición fetch y retorna el JSON parseado.
     * Lanza un error si la respuesta no es OK.
     * @param {string} url
     * @param {object} options
     * @returns {Promise<any>}
     */
    function fetchJson(url, options) {
        return fetch(url, options).then(function (r) {
            if (!r.ok) throw new Error('HTTP ' + r.status);
            return r.json();
        });
    }

    /**
     * Devuelve una representación relativa del tiempo.
     * @param {Date} date
     * @returns {string}
     */
    function relativeTime(date) {
        var diff = new Date() - date;
        var seconds = Math.floor(diff / 1000);
        if (seconds < 60) return 'just now';
        var minutes = Math.floor(seconds / 60);
        if (minutes < 60) return minutes + 'm ago';
        var hours = Math.floor(minutes / 60);
        if (hours < 24) return hours + 'h ago';
        return date.toLocaleDateString();
    }

    /**
     * Formatea un timestamp (en segundos) a string legible.
     * @param {number} ts
     * @returns {string}
     */
    function formatTimestamp(ts) {
        if (!ts) return '—';
        return new Date(ts * 1000).toLocaleString();
    }

    // Exponer el módulo
    window.JobsInsights = window.JobsInsights || {};
    window.JobsInsights.Util = {
        escapeHtml: escapeHtml,
        fetchJson: fetchJson,
        relativeTime: relativeTime,
        formatTimestamp: formatTimestamp
    };
})();