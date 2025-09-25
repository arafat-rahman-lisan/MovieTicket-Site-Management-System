(function () {
    const root = document.getElementById("seatmap");
    if (!root) return;

    const saveBtn = document.getElementById("saveLayoutBtn");
    const resetBtn = document.getElementById("resetChangesBtn");
    const disabledInput = document.getElementById("disabledSeatIds");
    const pendingCountEl = document.getElementById("pendingCount");

    // ASP.NET Antiforgery token (used for Toggle)
    const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    const antiToken = tokenEl ? tokenEl.value : null;

    // Track initial and current disabled states
    const initial = new Map(); // id -> bool
    const current = new Map(); // id -> bool

    // Initialize from DOM
    root.querySelectorAll(".seat-tile").forEach(btn => {
        const id = Number(btn.getAttribute("data-seat-id"));
        const isDisabled = btn.classList.contains("is-disabled");
        initial.set(id, isDisabled);
        current.set(id, isDisabled);
    });

    function computePending() {
        let pend = 0;
        for (const [id, cur] of current.entries()) {
            if (initial.get(id) !== cur) pend++;
        }
        pendingCountEl.textContent = String(pend);
        saveBtn.disabled = pend === 0;
        resetBtn.disabled = pend === 0;
    }

    function renderBtn(btn, disabled) {
        btn.classList.toggle("is-enabled", !disabled);
        btn.classList.toggle("is-disabled", disabled);
    }

    // Toggle locally and also instant-save via /SeatMap/Toggle (optional, keeps both in sync)
    async function toggleSeat(btn) {
        const id = Number(btn.getAttribute("data-seat-id"));
        const nowDisabled = !current.get(id);
        current.set(id, nowDisabled);
        renderBtn(btn, nowDisabled);
        computePending();

        // Instant persist (optional) — still keep batch Save available
        if (antiToken) {
            try {
                await fetch(`/SeatMap/Toggle/${id}`, {
                    method: "POST",
                    headers: { "X-Requested-With": "XMLHttpRequest", "RequestVerificationToken": antiToken }
                });
            } catch { /* ignore */ }
        }
    }

    // Click to toggle
    root.addEventListener("click", (e) => {
        const btn = e.target.closest(".seat-tile");
        if (!btn) return;
        toggleSeat(btn);
    });

    // Reset to initial
    resetBtn?.addEventListener("click", () => {
        root.querySelectorAll(".seat-tile").forEach(btn => {
            const id = Number(btn.getAttribute("data-seat-id"));
            const wantDisabled = initial.get(id) === true;
            current.set(id, wantDisabled);
            renderBtn(btn, wantDisabled);
        });
        computePending();
    });

    // On submit, serialize disabled ids
    saveBtn?.form?.addEventListener("submit", () => {
        const disabledIds = [];
        for (const [id, cur] of current.entries()) {
            if (cur) disabledIds.push(id);
        }
        disabledInput.value = disabledIds.join(",");
    });

    // Initial compute
    computePending();
})();
