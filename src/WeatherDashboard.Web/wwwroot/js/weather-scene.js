/*
 * Cena climática animada atrás do dashboard. Adaptado de duas referências:
 * - chuva/relâmpago: efeito de chuva CSS clássico (drop + stem + splat), aqui sem jQuery
 *   e com uma variante "tempestade" mais densa, com flash de relâmpago e fundo mais escuro.
 * - sol/nuvens: sol com raios girando + nuvens flutuantes geradas dinamicamente.
 * Estrelas (noite limpa) são construção própria, no mesmo estilo dos demais efeitos.
 */
window.WeatherScene = (function () {
    "use strict";

    var container = null;
    var currentMode = null;

    var backgrounds = {
        sunny: "radial-gradient(circle at 82% 6%, rgba(255,196,90,.22), transparent 60%)",
        "sunny-cloudy": "radial-gradient(circle at 78% 8%, rgba(255,205,140,.18), transparent 60%)",
        cloudy: "radial-gradient(circle at 70% 10%, rgba(180,190,220,.14), transparent 60%)",
        rain: "radial-gradient(circle at 65% 10%, rgba(110,150,255,.18), transparent 60%)",
        storm: "radial-gradient(circle at 60% 12%, rgba(90,80,180,.30), transparent 65%)",
        "night-clear": "radial-gradient(circle at 82% 10%, rgba(150,140,255,.20), transparent 60%)",
        "night-cloudy": "radial-gradient(circle at 74% 10%, rgba(120,130,170,.16), transparent 60%)",
    };

    function ensureContainer() {
        if (!container) container = document.getElementById("weather-scene");
        return container;
    }

    // Mapeia o código de ícone da OpenWeatherMap (ex.: "10d", "01n") para uma das cenas.
    function modeFor(icon) {
        var code = (icon || "").slice(0, 2);
        var isNight = (icon || "").slice(-1) === "n";

        if (code === "11") return "storm";
        if (code === "09" || code === "10") return "rain";
        // Neve (13) é praticamente inexistente nas cidades rastreadas (RMC + capitais
        // brasileiras); neblina (50) e nuvens (03/04) reaproveitam a cena nublada.
        if (code === "13" || code === "50" || code === "03" || code === "04") return "cloudy";
        if (code === "02") return isNight ? "night-cloudy" : "sunny-cloudy";
        return isNight ? "night-clear" : "sunny";
    }

    function clear(el) {
        el.innerHTML = "";
        el.className = "weather-scene";
    }

    function buildSun(el) {
        var rays = document.createElement("div");
        rays.className = "ws-rays";
        var sun = document.createElement("div");
        sun.className = "ws-sun";
        el.appendChild(rays);
        el.appendChild(sun);
    }

    function buildClouds(el, count, opts) {
        opts = opts || {};
        var opacityBase = opts.opacity || 0.5;
        var maxTop = opts.maxTop || 80;
        for (var i = 0; i < count; i++) {
            var cloud = document.createElement("div");
            cloud.className = "ws-cloud";
            var width = 100 + Math.random() * 160;
            cloud.style.width = width + "px";
            cloud.style.height = width * 0.35 + "px";
            cloud.style.top = Math.random() * maxTop + "%";
            var duration = 34 + Math.random() * 50;
            cloud.style.animationDuration = duration + "s";
            cloud.style.animationDelay = -Math.random() * duration + "s";
            cloud.style.opacity = opacityBase + Math.random() * 0.2;
            el.appendChild(cloud);
        }
    }

    function buildStars(el, count) {
        var frag = document.createDocumentFragment();
        for (var i = 0; i < count; i++) {
            var star = document.createElement("div");
            star.className = "ws-star";
            star.style.left = Math.random() * 100 + "%";
            star.style.top = Math.random() * 70 + "%";
            var size = (1.5 + Math.random() * 2).toFixed(1);
            star.style.width = size + "px";
            star.style.height = size + "px";
            star.style.animationDuration = (2 + Math.random() * 3).toFixed(1) + "s";
            star.style.animationDelay = (Math.random() * 3).toFixed(1) + "s";
            frag.appendChild(star);
        }
        el.appendChild(frag);
    }

    function makeDrop(offsetPercent, side, delay, duration) {
        var drop = document.createElement("div");
        drop.className = "ws-drop";
        drop.style[side] = offsetPercent + "%";
        drop.style.bottom = 100 + Math.random() * 10 + "%";
        drop.style.animationDelay = delay + "s";
        drop.style.animationDuration = duration + "s";

        var stem = document.createElement("div");
        stem.className = "ws-stem";
        stem.style.animationDelay = delay + "s";
        stem.style.animationDuration = duration + "s";

        var splat = document.createElement("div");
        splat.className = "ws-splat";
        splat.style.animationDelay = delay + "s";
        splat.style.animationDuration = duration + "s";

        drop.appendChild(stem);
        drop.appendChild(splat);
        return drop;
    }

    function buildRainLayer(count, side) {
        var layer = document.createElement("div");
        layer.className = "ws-rain-layer " + (side === "right" ? "ws-back-row" : "ws-front-row");
        var frag = document.createDocumentFragment();
        var acc = 0;
        var step = 100 / count;
        for (var i = 0; i < count; i++) {
            acc += step * (0.6 + Math.random() * 0.8);
            var delay = (Math.random() * 0.9).toFixed(2);
            var duration = (0.35 + Math.random() * 0.3).toFixed(2);
            frag.appendChild(makeDrop(acc % 100, side, delay, duration));
        }
        layer.appendChild(frag);
        return layer;
    }

    function buildRain(el, intense) {
        var count = intense ? 130 : 65;
        el.appendChild(buildRainLayer(count, "right")); // fileira de trás, mais sutil (opacidade via CSS)
        el.appendChild(buildRainLayer(count, "left")); // fileira da frente

        if (intense) {
            var flash = document.createElement("div");
            flash.className = "ws-flash";
            el.appendChild(flash);
        }
    }

    function render(icon) {
        var el = ensureContainer();
        if (!el) return;

        var mode = modeFor(icon);
        el.style.background = backgrounds[mode] || "";

        if (mode === currentMode) return; // já está com a cena certa; evita reconstruir o DOM a cada refresh
        currentMode = mode;

        clear(el);
        el.classList.add("ws-" + mode);

        switch (mode) {
            case "sunny":
                buildSun(el);
                break;
            case "sunny-cloudy":
                buildSun(el);
                buildClouds(el, 4, { opacity: 0.45, maxTop: 55 });
                break;
            case "cloudy":
                buildClouds(el, 6, { opacity: 0.5 });
                break;
            case "rain":
                buildClouds(el, 3, { opacity: 0.35, maxTop: 40 });
                buildRain(el, false);
                break;
            case "storm":
                buildClouds(el, 3, { opacity: 0.3, maxTop: 35 });
                buildRain(el, true);
                break;
            case "night-clear":
                buildStars(el, 60);
                break;
            case "night-cloudy":
                buildStars(el, 25);
                buildClouds(el, 4, { opacity: 0.25, maxTop: 50 });
                break;
        }
    }

    return { render: render };
})();
