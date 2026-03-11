document.addEventListener("DOMContentLoaded", () => {
  const btn = document.getElementById("styleBtn");
  const card = document.getElementById("mainCard");
  const titleEl = document.getElementById("cardTitle");
  const textEl = document.getElementById("mainText");

  if (btn && card && titleEl && textEl) {
    const originalTitle = titleEl.textContent;
    const originalText = textEl.innerHTML;

    btn.addEventListener("click", () => {
      const isActive = card.classList.toggle("active-style");

      if (isActive) {
        titleEl.textContent = "Cristiano Ronaldo’nun İstatistikleri";
        textEl.textContent =
          "Bu sayfa Cristiano Ronaldo’nun son 5 sezondaki gol ve asist istatistiklerini içerir.";
        btn.textContent = "Geri Al";
      } else {
        titleEl.textContent = originalTitle;
        textEl.innerHTML = originalText;
        btn.textContent = "Değiştir";
      }
    });
  }
});

document.addEventListener("DOMContentLoaded", () => {
  const btn = document.getElementById("sortFinalBtn");
  const table = document.getElementById("studentTable");

  if (btn && table) {
    const tbody = table.querySelector("tbody");

    btn.addEventListener("click", () => {
      const rows = Array.from(tbody.querySelectorAll("tr"));

      rows.sort((a, b) => {
        const finalA = Number(a.children[3].textContent.trim());
        const finalB = Number(b.children[3].textContent.trim());
        return finalB - finalA;
      });

      rows.forEach((row) => {
        tbody.appendChild(row);
      });
    });
  }
});

document.addEventListener("DOMContentLoaded", () => {
  const img = document.getElementById("mainImage");
  const btn = document.getElementById("changeBtn");

  if (img && btn) {
    const firstImage = "/images/image_1.jpg";
    const secondImage = "/images/image_2.jpg";

    btn.addEventListener("click", () => {
      if (img.src.includes("image_1.jpg")) {
        img.src = secondImage;
      } else {
        img.src = firstImage;
      }
    });
  }
});

document.addEventListener("DOMContentLoaded", () => {
  const game = document.getElementById("underwaterGame");
  const playfield = document.getElementById("playfield");
  const rod = document.getElementById("rodCursor");

  const failOverlay = document.getElementById("failOverlay");
  const retryBtn = document.getElementById("retryBtn");

  const timeEl = document.getElementById("time");
  const bestEl = document.getElementById("best");
  const failStatsEl = document.getElementById("failStats");

  if (
    !game ||
    !playfield ||
    !rod ||
    !failOverlay ||
    !retryBtn ||
    !timeEl ||
    !bestEl ||
    !failStatsEl
  ) {
    return;
  }

  const fishSrc = "/images/fish.png";

  let isFailed = false;
  let isRunning = false;

  let startTime = 0;
  let bestMs = 0;

  let rafId = null;
  let spawnIntervalId = null;

  const fishes = [];

  function formatMs(ms) {
    ms = Math.max(0, ms);
    const totalSec = ms / 1000;
    const m = Math.floor(totalSec / 60);
    const s = totalSec % 60;
    const mm = String(m).padStart(2, "0");
    const ss = String(Math.floor(s)).padStart(2, "0");
    const t = Math.floor((s - Math.floor(s)) * 10);
    return `${mm}:${ss}.${t}`;
  }

  function setRodPosition(e) {
    const r = game.getBoundingClientRect();
    const x = e.clientX - r.left;
    const y = e.clientY - r.top;
    rod.style.left = `${x}px`;
    rod.style.top = `${y}px`;
  }

  function getHookRect() {
    const r = rod.getBoundingClientRect();
    const tipX = r.left + r.width * 0.78;
    const tipY = r.top + r.height * 0.78;
    const size = 12;
    return {
      left: tipX - size / 2,
      top: tipY - size / 2,
      right: tipX + size / 2,
      bottom: tipY + size / 2
    };
  }

  function rectsOverlap(a, b) {
    return !(a.right < b.left || a.left > b.right || a.bottom < b.top || a.top > b.bottom);
  }

  const hint = document.createElement("div");
  hint.className = "startHint";
  hint.innerHTML = `Double click to <b>START</b>`;
  game.appendChild(hint);

  function showHint(v) {
    hint.style.display = v ? "block" : "none";
  }

  function createFish() {
    const el = document.createElement("img");
    el.className = "fish";
    el.src = fishSrc;
    el.alt = "";
    playfield.appendChild(el);

    const r = playfield.getBoundingClientRect();

    const x = Math.random() * (r.width - 120) + 40;
    const y = Math.random() * (r.height - 220) + 120;

    const speed = 1.0 + Math.random() * 2.8;
    const angle = Math.random() * Math.PI * 2;
    let vx = Math.cos(angle) * speed;
    let vy = Math.sin(angle) * speed;

    if (Math.abs(vx) < 0.6) vx = 0.6 * Math.sign(vx || 1);
    if (Math.abs(vy) < 0.4) vy = 0.4 * Math.sign(vy || 1);

    const f = {
      el,
      x,
      y,
      vx,
      vy,
      phase: Math.random() * Math.PI * 2,
      wobbleAmp: 6 + Math.random() * 10,
      wobbleSpeed: 0.03 + Math.random() * 0.05,
      w: 50,
      h: 30
    };

    fishes.push(f);

    el.addEventListener(
      "load",
      () => {
        const br = el.getBoundingClientRect();
        f.w = br.width || f.w;
        f.h = br.height || f.h;
      },
      { once: true }
    );

    updateFishDOM(f);
    return f;
  }

  function clearFishes() {
    for (const f of fishes) f.el.remove();
    fishes.length = 0;
  }

  function updateFishDOM(f, wobbleY = 0) {
    const flip = f.vx < 0 ? -1 : 1;
    f.el.style.left = f.x + "px";
    f.el.style.top = f.y + wobbleY + "px";
    f.el.style.transform = `scaleX(${flip})`;
  }

  function stepFishes() {
    const r = playfield.getBoundingClientRect();

    for (const f of fishes) {
      f.phase += f.wobbleSpeed;
      f.x += f.vx;
      f.y += f.vy;

      const wobbleY = Math.sin(f.phase) * f.wobbleAmp;

      const minX = 8;
      const maxX = r.width - Math.max(30, f.w) - 8;
      const minY = 80;
      const maxY = r.height - Math.max(20, f.h) - 12;

      if (f.x <= minX) {
        f.x = minX;
        f.vx *= -1;
      } else if (f.x >= maxX) {
        f.x = maxX;
        f.vx *= -1;
      }

      if (f.y <= minY) {
        f.y = minY;
        f.vy *= -1;
      } else if (f.y >= maxY) {
        f.y = maxY;
        f.vy *= -1;
      }

      updateFishDOM(f, wobbleY);
    }
  }

  function tick() {
    if (!isRunning || isFailed) return;

    const now = performance.now();
    const elapsed = now - startTime;

    timeEl.textContent = formatMs(elapsed);

    stepFishes();

    const hook = getHookRect();
    for (const f of fishes) {
      const fr = f.el.getBoundingClientRect();
      const fishRect = {
        left: fr.left,
        top: fr.top,
        right: fr.right,
        bottom: fr.bottom
      };

      if (rectsOverlap(hook, fishRect)) {
        fail(elapsed);
        return;
      }
    }

    rafId = requestAnimationFrame(tick);
  }

  function startGame() {
    if (isRunning) return;

    isFailed = false;
    isRunning = true;

    showHint(false);
    failOverlay.classList.remove("isOpen");
    failOverlay.setAttribute("aria-hidden", "true");

    clearFishes();
    timeEl.textContent = "00:00.0";

    startTime = performance.now();

    if (spawnIntervalId) {
      clearInterval(spawnIntervalId);
      spawnIntervalId = null;
    }

    createFish();

    spawnIntervalId = setInterval(() => {
      if (!isRunning || isFailed) return;
      createFish();
    }, 5000);

    rafId = requestAnimationFrame(tick);
  }

  function stopTimers() {
    if (rafId) cancelAnimationFrame(rafId);
    rafId = null;

    if (spawnIntervalId) clearInterval(spawnIntervalId);
    spawnIntervalId = null;
  }

  function fail(elapsedMs) {
    isFailed = true;
    isRunning = false;
    stopTimers();

    if (elapsedMs > bestMs) {
      bestMs = elapsedMs;
      bestEl.textContent = formatMs(bestMs);
    }

    failStatsEl.textContent = `You lasted: ${formatMs(elapsedMs)}`;

    failOverlay.classList.add("isOpen");
    failOverlay.setAttribute("aria-hidden", "false");

    showHint(true);
  }

  function resetToIdle() {
    isFailed = false;
    isRunning = false;
    stopTimers();
    clearFishes();

    timeEl.textContent = "00:00.0";
    failOverlay.classList.remove("isOpen");
    failOverlay.setAttribute("aria-hidden", "true");

    showHint(true);
  }

  playfield.addEventListener("mousemove", (e) => {
    if (isFailed) return;
    setRodPosition(e);
  });

  playfield.addEventListener("mouseleave", () => {
    rod.style.opacity = "0";
  });

  playfield.addEventListener("mouseenter", () => {
    rod.style.opacity = "1";
  });

  playfield.addEventListener("dblclick", () => {
    if (!isRunning) startGame();
  });

  retryBtn.addEventListener("click", resetToIdle);

  resetToIdle();
});