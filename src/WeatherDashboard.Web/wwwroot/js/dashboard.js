(function () {
    "use strict";

    var root = document.querySelector(".dashboard");
    if (!root) return;

    var dataUrl = root.dataset.dataUrl;
    var citySelect = document.getElementById("city-select");
    var startInput = document.getElementById("start-date");
    var endInput = document.getElementById("end-date");
    var form = document.getElementById("filters-form");
    var currentCard = document.getElementById("current-card");
    var statsGrid = document.getElementById("today-stats");
    var emptyState = document.getElementById("empty-state");
    var chartsGrid = document.querySelector(".charts-grid");

    var temperatureChart = null;
    var humidityWindChart = null;
    var AUTO_REFRESH_MS = 5 * 60 * 1000; // acompanha a coleta de 15 em 15 min sem sobrecarregar o servidor

    var numberFormatter = new Intl.NumberFormat("pt-BR", { maximumFractionDigits: 1 });
    var dayLabelFormatter = new Intl.DateTimeFormat("pt-BR", { day: "2-digit", month: "2-digit" });
    var dateTimeFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short" });

    function fmt(value, suffix) {
        if (value === null || value === undefined) return "—";
        return numberFormatter.format(value) + (suffix || "");
    }

    function buildQuery() {
        var params = new URLSearchParams({
            city: citySelect.value,
            start: startInput.value,
            end: endInput.value,
        });
        return dataUrl + "?" + params.toString();
    }

    function renderCurrentCard(data) {
        var today = data.today;
        if (!today || today.currentTempC === null || today.currentTempC === undefined) {
            currentCard.innerHTML =
                '<div class="current-weather-error"><i class="ph ph-cloud-slash"></i> ' +
                "Sem leitura registrada hoje para " + escapeHtml(data.cityName) + " ainda.</div>";
            return;
        }

        var iconUrl = "https://openweathermap.org/img/wn/" + (today.currentIcon || "01d") + "@2x.png";
        currentCard.innerHTML =
            '<img class="current-weather-icon" src="' + iconUrl + '" alt="' + escapeHtml(today.currentDescription || "") + '" />' +
            '<div class="current-weather-main">' +
            '<div class="current-weather-location"><i class="ph ph-map-pin"></i> ' + escapeHtml(data.cityName) + " / " + escapeHtml(data.uf) + "</div>" +
            '<div class="current-weather-temp">' + fmt(today.currentTempC) + "<span>°C</span></div>" +
            '<div class="current-weather-desc">' + escapeHtml(today.currentDescription || "") + "</div>" +
            "</div>" +
            '<div class="current-weather-updated">' +
            (today.lastUpdatedUtc ? "Atualizado em<br/>" + dateTimeFormatter.format(new Date(today.lastUpdatedUtc)) : "") +
            "</div>";
    }

    function statCard(icon, label, value) {
        return (
            '<div class="stat-card"><i class="ph ' + icon + '"></i><div class="stat-card-body">' +
            '<span class="stat-card-label">' + label + "</span>" +
            '<span class="stat-card-value">' + value + "</span></div></div>"
        );
    }

    function renderStats(today) {
        statsGrid.innerHTML = [
            statCard("ph-thermometer-simple", "Máxima hoje", fmt(today.tempMaxC, "°C")),
            statCard("ph-thermometer", "Mínima hoje", fmt(today.tempMinC, "°C")),
            statCard("ph-chart-line-up", "Média hoje", fmt(today.tempAvgC, "°C")),
            statCard("ph-drop", "Umidade média", fmt(today.humidityAvgPercent, "%")),
            statCard("ph-wind", "Vento médio", fmt(today.windAvgMs, " m/s")),
            statCard("ph-database", "Leituras hoje", today.readingsCount ?? 0),
        ].join("");
    }

    function chartPalette() {
        var styles = getComputedStyle(document.documentElement);
        return {
            text: styles.getPropertyValue("--color-text").trim() || "#f2f3fb",
            grid: "rgba(255,255,255,0.08)",
            accent: styles.getPropertyValue("--color-accent").trim() || "#6c8bff",
            accent2: styles.getPropertyValue("--color-accent-200").trim() || "#a9bcff",
            warn: styles.getPropertyValue("--color-warn").trim() || "#ff8a65",
        };
    }

    function renderCharts(dailySeries) {
        var palette = chartPalette();
        var labels = dailySeries.map(function (d) { return dayLabelFormatter.format(new Date(d.date)); });

        var tempCtx = document.getElementById("temperature-chart").getContext("2d");
        var humidityCtx = document.getElementById("humidity-wind-chart").getContext("2d");

        if (temperatureChart) temperatureChart.destroy();
        if (humidityWindChart) humidityWindChart.destroy();

        var commonScales = {
            x: { ticks: { color: palette.text }, grid: { color: "transparent" } },
        };

        temperatureChart = new Chart(tempCtx, {
            type: "line",
            data: {
                labels: labels,
                datasets: [
                    { label: "Mínima (°C)", data: dailySeries.map(function (d) { return d.tempMinC; }), borderColor: palette.accent2, backgroundColor: "transparent", tension: 0.35 },
                    { label: "Média (°C)", data: dailySeries.map(function (d) { return d.tempAvgC; }), borderColor: palette.accent, backgroundColor: "transparent", tension: 0.35, borderWidth: 3 },
                    { label: "Máxima (°C)", data: dailySeries.map(function (d) { return d.tempMaxC; }), borderColor: palette.warn, backgroundColor: "transparent", tension: 0.35 },
                ],
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { labels: { color: palette.text } } },
                scales: {
                    x: commonScales.x,
                    y: { ticks: { color: palette.text }, grid: { color: palette.grid } },
                },
            },
        });

        humidityWindChart = new Chart(humidityCtx, {
            data: {
                labels: labels,
                datasets: [
                    {
                        type: "bar",
                        label: "Umidade média (%)",
                        data: dailySeries.map(function (d) { return d.humidityAvgPercent; }),
                        backgroundColor: "rgba(108,139,255,0.45)",
                        yAxisID: "y",
                    },
                    {
                        type: "line",
                        label: "Vento médio (m/s)",
                        data: dailySeries.map(function (d) { return d.windAvgMs; }),
                        borderColor: palette.warn,
                        backgroundColor: "transparent",
                        yAxisID: "y1",
                        tension: 0.35,
                    },
                ],
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { labels: { color: palette.text } } },
                scales: {
                    x: commonScales.x,
                    y: { position: "left", ticks: { color: palette.text }, grid: { color: palette.grid }, title: { display: true, text: "%", color: palette.text } },
                    y1: { position: "right", ticks: { color: palette.text }, grid: { display: false }, title: { display: true, text: "m/s", color: palette.text } },
                },
            },
        });
    }

    function escapeHtml(str) {
        var div = document.createElement("div");
        div.textContent = str;
        return div.innerHTML;
    }

    function loadData() {
        fetch(buildQuery())
            .then(function (response) {
                if (!response.ok) throw new Error("Falha ao carregar dados (" + response.status + ")");
                return response.json();
            })
            .then(function (data) {
                renderCurrentCard(data);
                renderStats(data.today || {});

                var hasSeries = data.dailySeries && data.dailySeries.length > 0;
                chartsGrid.hidden = !hasSeries;
                emptyState.hidden = hasSeries;
                if (hasSeries) renderCharts(data.dailySeries);

                var url = new URL(window.location.href);
                url.searchParams.set("city", data.cityId);
                window.history.replaceState({}, "", url);
            })
            .catch(function (err) {
                currentCard.innerHTML = '<div class="current-weather-error"><i class="ph ph-warning"></i> ' + escapeHtml(err.message) + "</div>";
            });
    }

    form.addEventListener("submit", function (evt) {
        evt.preventDefault();
        loadData();
    });

    loadData();
    setInterval(loadData, AUTO_REFRESH_MS);
})();
