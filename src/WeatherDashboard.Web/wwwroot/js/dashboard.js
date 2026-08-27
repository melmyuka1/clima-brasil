(function () {
    "use strict";

    var root = document.querySelector(".dashboard");
    if (!root) return;

    // O histórico climático mora na API separada (WeatherDashboard.Api), consumida direto do
    // navegador. Usa a URL da API no mesmo protocolo da própria página (http/https) — misturar
    // os dois é bloqueado como "conteúdo misto" por navegadores como o Firefox.
    var apiBaseUrl = window.location.protocol === "https:" ? root.dataset.apiBaseUrlHttps : root.dataset.apiBaseUrlHttp;
    var dataUrl = apiBaseUrl + "/api/weather/data";
    var highlightsUrl = apiBaseUrl + "/api/weather/highlights";
    var citySelect = document.getElementById("city-select");
    var startInput = document.getElementById("start-date");
    var endInput = document.getElementById("end-date");
    var form = document.getElementById("filters-form");
    var currentCard = document.getElementById("current-card");
    var statsGrid = document.getElementById("today-stats");
    var emptyState = document.getElementById("empty-state");
    var chartsGrid = document.querySelector(".charts-grid");
    var highlightsStrip = document.getElementById("highlights-strip");
    var highlightsToggle = document.getElementById("highlights-toggle");
    var highlightsToggleLabel = document.getElementById("highlights-toggle-label");
    var dayStripCard = document.getElementById("day-strip-card");
    var dayStrip = document.getElementById("day-strip");
    var unitToggle = document.getElementById("unit-toggle");

    var temperatureChart = null;
    var humidityWindChart = null;
    var AUTO_REFRESH_MS = 5 * 60 * 1000; // acompanha a coleta de 15 em 15 min sem sobrecarregar o servidor
    var UNIT_STORAGE_KEY = "climaBrasil.unit";

    var unit = localStorage.getItem(UNIT_STORAGE_KEY) === "F" ? "F" : "C";
    var lastData = null;

    var numberFormatter = new Intl.NumberFormat("pt-BR", { maximumFractionDigits: 1 });
    var prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    var lastDisplayedTemp = null;
    var heroRevealed = false;
    var dayLabelFormatter = new Intl.DateTimeFormat("pt-BR", { day: "2-digit", month: "2-digit" });
    var weekdayFormatter = new Intl.DateTimeFormat("pt-BR", { weekday: "short" });
    var dateTimeFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short" });
    var timeFormatter = new Intl.DateTimeFormat("pt-BR", { timeStyle: "short" });

    function toDisplayTemp(celsius) {
        if (celsius === null || celsius === undefined) return null;
        return unit === "F" ? (celsius * 9) / 5 + 32 : celsius;
    }

    function fmt(value, suffix) {
        if (value === null || value === undefined) return "—";
        return numberFormatter.format(value) + (suffix || "");
    }

    function fmtTemp(celsius) {
        return fmt(toDisplayTemp(celsius), "°" + unit);
    }

    function todayKey() {
        var d = new Date();
        return d.getFullYear() + "-" + String(d.getMonth() + 1).padStart(2, "0") + "-" + String(d.getDate()).padStart(2, "0");
    }

    function buildQuery() {
        var params = new URLSearchParams({
            city: citySelect.value,
            start: startInput.value,
            end: endInput.value,
        });
        return dataUrl + "?" + params.toString();
    }

    function escapeHtml(str) {
        var div = document.createElement("div");
        div.textContent = str;
        return div.innerHTML;
    }

    function iconUrl(icon, size) {
        return "https://openweathermap.org/img/wn/" + (icon || "01d") + (size === "small" ? ".png" : "@2x.png");
    }

    // Conta do valor anterior até o novo — só no número da temperatura em destaque, de propósito
    // (um único momento de movimento, não um efeito espalhado pela tela).
    function animateNumber(el, from, to, durationMs) {
        if (!el) return;
        if (prefersReducedMotion) {
            el.textContent = numberFormatter.format(to);
            return;
        }
        var start = null;
        function step(ts) {
            if (start === null) start = ts;
            var progress = Math.min(1, (ts - start) / durationMs);
            var eased = 1 - Math.pow(1 - progress, 3);
            el.textContent = numberFormatter.format(from + (to - from) * eased);
            if (progress < 1) requestAnimationFrame(step);
        }
        requestAnimationFrame(step);
    }

    // Arco do dia: um ponto se move de nascer a pôr do sol conforme a hora atual — a temperatura
    // já diz "quanto", isso diz "em que momento do dia" sem precisar de mais dois textos soltos.
    function buildSunArc(sunriseIso, sunsetIso) {
        var sunrise = new Date(sunriseIso).getTime();
        var sunset = new Date(sunsetIso).getTime();
        var span = sunset - sunrise;
        var progress = span > 0 ? (Date.now() - sunrise) / span : 0;
        var isDaytime = progress >= 0 && progress <= 1;
        var clamped = Math.max(0, Math.min(1, progress));
        var theta = Math.PI * (1 - clamped);
        var cx = (100 + 85 * Math.cos(theta)).toFixed(1);
        var cy = (95 - 85 * Math.sin(theta)).toFixed(1);

        return (
            '<div class="sun-arc">' +
            '<svg viewBox="0 0 200 108" aria-hidden="true">' +
            '<path d="M15,95 A85,85 0 0 1 185,95" fill="none" stroke="var(--color-divider)" stroke-width="2"/>' +
            '<circle cx="' + cx + '" cy="' + cy + '" r="7" class="sun-arc-dot' + (isDaytime ? "" : " is-night") + '"/>' +
            "</svg>" +
            '<div class="sun-arc-labels text-muted">' +
            '<span><i class="ph ph-sun-horizon"></i> ' + timeFormatter.format(new Date(sunrise)) + "</span>" +
            '<span>' + timeFormatter.format(new Date(sunset)) + ' <i class="ph ph-moon-stars"></i></span>' +
            "</div></div>"
        );
    }

    // ---------- Cena de fundo conforme a condição climática atual ----------
    // (sol, nuvens, chuva/tempestade ou céu estrelado — ver wwwroot/js/weather-scene.js)

    function updateWeatherScene(icon) {
        if (window.WeatherScene) window.WeatherScene.render(icon);
    }

    // ---------- Toggle °C / °F (segmented control do Nocturne: .seg > .seg-opt > input[type=radio]) ----------

    var unitRadio = unitToggle.querySelector('input[value="' + unit + '"]');
    if (unitRadio) unitRadio.checked = true;

    unitToggle.addEventListener("change", function (evt) {
        if (evt.target.name !== "unit") return;
        unit = evt.target.value;
        localStorage.setItem(UNIT_STORAGE_KEY, unit);
        if (lastData) renderAll(lastData);
        renderHighlights();
    });

    // ---------- Tira de cidades em destaque (Região Metropolitana de Curitiba ⇄ Capitais) ----------

    var highlightsData = { metroCities: [], capitals: [] };
    var highlightsView = "metro"; // sempre inicia mostrando a RMC ao acessar a aplicação

    function renderHighlights() {
        var items = highlightsView === "metro" ? highlightsData.metroCities : highlightsData.capitals;
        highlightsStrip.innerHTML = items
            .map(function (item) {
                var active = item.cityId === citySelect.value;
                var temp = item.currentTempC === null || item.currentTempC === undefined ? "—" : fmtTemp(item.currentTempC);
                return (
                    '<button type="button" class="btn ' + (active ? "btn-primary" : "btn-secondary") + ' highlight-chip" data-city="' + item.cityId + '">' +
                    '<img src="' + iconUrl(item.icon, "small") + '" alt="" />' +
                    "<span>" + escapeHtml(item.cityName) + "</span>" +
                    '<span class="highlight-chip-temp">' + temp + "</span>" +
                    "</button>"
                );
            })
            .join("");
    }

    function loadHighlights() {
        fetch(highlightsUrl)
            .then(function (r) { return r.ok ? r.json() : { metroCities: [], capitals: [] }; })
            .then(function (data) {
                highlightsData = data;
                renderHighlights();
            })
            .catch(function () { /* tira de destaque é cosmética; falha aqui não deve incomodar o usuário */ });
    }

    highlightsStrip.addEventListener("click", function (evt) {
        var chip = evt.target.closest(".highlight-chip");
        if (!chip) return;
        citySelect.value = chip.dataset.city;
        loadData();
    });

    highlightsToggle.addEventListener("click", function () {
        highlightsView = highlightsView === "metro" ? "capitals" : "metro";
        highlightsToggleLabel.textContent = highlightsView === "metro" ? "Ver capitais" : "Ver região metropolitana";
        renderHighlights();
    });

    // ---------- Cartão de clima atual ----------

    function renderCurrentCard(data) {
        var today = data.today;
        if (!today || today.currentTempC === null || today.currentTempC === undefined) {
            currentCard.innerHTML =
                '<div class="current-weather-error"><i class="ph ph-cloud-slash"></i> ' +
                escapeHtml(data.cityName) + " ainda não tem leitura hoje. A próxima coleta chega em até 15 minutos.</div>";
            updateWeatherScene(null);
            lastDisplayedTemp = null;
            return;
        }

        updateWeatherScene(today.currentIcon);

        var feelsLikeHtml = today.currentFeelsLikeC !== null && today.currentFeelsLikeC !== undefined
            ? '<div class="current-weather-detail"><i class="ph ph-thermometer-simple"></i><span>' +
              '<span class="current-weather-detail-label text-muted">Sensação térmica</span>' +
              '<span class="current-weather-detail-value">' + fmtTemp(today.currentFeelsLikeC) + "</span></span></div>"
            : "";

        var arcHtml = today.sunriseUtc && today.sunsetUtc ? buildSunArc(today.sunriseUtc, today.sunsetUtc) : "";

        var secondaryHtml = feelsLikeHtml || arcHtml
            ? '<div class="current-weather-secondary">' + feelsLikeHtml + arcHtml + "</div>"
            : "";

        var displayTemp = toDisplayTemp(today.currentTempC);
        var isFirstReveal = !heroRevealed;
        heroRevealed = true;
        var animFrom = lastDisplayedTemp !== null ? lastDisplayedTemp : displayTemp - 6;

        currentCard.innerHTML =
            '<img class="current-weather-icon" src="' + iconUrl(today.currentIcon) + '" alt="' + escapeHtml(today.currentDescription || "") + '" />' +
            '<div class="current-weather-main">' +
            '<div class="current-weather-location text-muted"><i class="ph ph-map-pin"></i> ' + escapeHtml(data.cityName) + " / " + escapeHtml(data.uf) + "</div>" +
            '<div class="current-weather-temp"><span id="hero-temp-value">' + numberFormatter.format(animFrom) + '</span><span>°' + unit + "</span></div>" +
            '<div class="current-weather-desc text-muted">' + escapeHtml(today.currentDescription || "") + "</div>" +
            "</div>" +
            '<div class="current-weather-updated text-muted">' +
            (today.lastUpdatedUtc ? "Atualizado em<br/>" + dateTimeFormatter.format(new Date(today.lastUpdatedUtc)) : "") +
            "</div>" +
            secondaryHtml;

        currentCard.classList.toggle("reveal-in", isFirstReveal && !prefersReducedMotion);
        animateNumber(document.getElementById("hero-temp-value"), animFrom, displayTemp, 700);
        lastDisplayedTemp = displayTemp;
    }

    function statCard(icon, label, value) {
        return (
            '<div class="card stat-card"><i class="ph ' + icon + '"></i><div class="stat-card-body">' +
            '<span class="card-meta">' + label + "</span>" +
            '<span class="stat-card-value">' + value + "</span></div></div>"
        );
    }

    function renderStats(today) {
        statsGrid.innerHTML = [
            statCard("ph-thermometer-simple", "Máxima hoje", fmtTemp(today.tempMaxC)),
            statCard("ph-thermometer", "Mínima hoje", fmtTemp(today.tempMinC)),
            statCard("ph-chart-line-up", "Média hoje", fmtTemp(today.tempAvgC)),
            statCard("ph-drop", "Umidade média", fmt(today.humidityAvgPercent, "%")),
            statCard("ph-wind", "Vento médio", fmt(today.windAvgMs, " m/s")),
            statCard("ph-database", "Leituras hoje", today.readingsCount ?? 0),
        ].join("");
    }

    // ---------- Tira de dias ----------

    function renderDayStrip(dailySeries) {
        if (!dailySeries || dailySeries.length === 0) {
            dayStripCard.hidden = true;
            return;
        }
        dayStripCard.hidden = false;

        var today = todayKey();
        dayStrip.innerHTML = dailySeries
            .map(function (d) {
                var isToday = d.date === today;
                var label = isToday ? "Hoje" : weekdayFormatter.format(new Date(d.date));
                return (
                    '<div class="card day-card' + (isToday ? " is-today" : "") + '">' +
                    '<span class="day-card-label text-muted">' + escapeHtml(label) + "</span>" +
                    '<img src="' + iconUrl(d.representativeIcon, "small") + '" alt="' + escapeHtml(d.representativeDescription || "") + '" />' +
                    '<span class="day-card-temps">' + fmtTemp(d.tempMaxC) + ' <span class="text-muted">' + fmtTemp(d.tempMinC) + "</span></span>" +
                    "</div>"
                );
            })
            .join("");
    }

    // ---------- Gráficos ----------

    function chartPalette() {
        var styles = getComputedStyle(document.documentElement);
        return {
            text: styles.getPropertyValue("--color-text").trim() || "#e9e9ed",
            grid: styles.getPropertyValue("--color-divider").trim() || "rgba(233,233,237,0.16)",
            accent: styles.getPropertyValue("--color-accent").trim() || "#9184d9",
            accent2: styles.getPropertyValue("--color-accent-300").trim() || "#d2cefd",
            warn: styles.getPropertyValue("--color-accent-2").trim() || "#a7a1db",
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
                    { label: "Mínima (°" + unit + ")", data: dailySeries.map(function (d) { return toDisplayTemp(d.tempMinC); }), borderColor: palette.accent2, backgroundColor: "transparent", tension: 0.35 },
                    { label: "Média (°" + unit + ")", data: dailySeries.map(function (d) { return toDisplayTemp(d.tempAvgC); }), borderColor: palette.accent, backgroundColor: "transparent", tension: 0.35, borderWidth: 3 },
                    { label: "Máxima (°" + unit + ")", data: dailySeries.map(function (d) { return toDisplayTemp(d.tempMaxC); }), borderColor: palette.warn, backgroundColor: "transparent", tension: 0.35 },
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
                        backgroundColor: "rgba(145,132,217,0.45)",
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

    // ---------- Orquestração ----------

    function renderAll(data) {
        renderCurrentCard(data);
        renderStats(data.today || {});
        renderDayStrip(data.dailySeries);

        var hasSeries = data.dailySeries && data.dailySeries.length > 0;
        chartsGrid.hidden = !hasSeries;
        emptyState.hidden = hasSeries;
        if (hasSeries) renderCharts(data.dailySeries);

        highlightsStrip.querySelectorAll(".highlight-chip").forEach(function (chip) {
            var active = chip.dataset.city === data.cityId;
            chip.classList.toggle("btn-primary", active);
            chip.classList.toggle("btn-secondary", !active);
        });
    }

    function loadData() {
        fetch(buildQuery())
            .then(function (response) {
                if (!response.ok) throw new Error("A API respondeu com erro " + response.status + ".");
                return response.json();
            })
            .then(function (data) {
                if (lastData && lastData.cityId !== data.cityId) {
                    // cidade trocou: deixa a temperatura "revelar" de novo em vez de só atualizar
                    heroRevealed = false;
                    lastDisplayedTemp = null;
                }
                lastData = data;
                renderAll(data);

                var url = new URL(window.location.href);
                url.searchParams.set("city", data.cityId);
                window.history.replaceState({}, "", url);
            })
            .catch(function (err) {
                currentCard.innerHTML =
                    '<div class="current-weather-error"><i class="ph ph-plugs"></i><span>' +
                    "A API não respondeu.<br/>" +
                    '<span class="text-muted">Confirme se WeatherDashboard.Api está rodando. (' + escapeHtml(err.message) + ")</span></span></div>";
            });
    }

    form.addEventListener("submit", function (evt) {
        evt.preventDefault();
        loadData();
    });

    loadData();
    loadHighlights();
    setInterval(function () {
        loadData();
        loadHighlights();
    }, AUTO_REFRESH_MS);
})();
