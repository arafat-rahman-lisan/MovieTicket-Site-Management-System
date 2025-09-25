(() => {
    const grid = document.querySelector(".seat-grid");
    if (!grid) return;

    const selectedIds = new Set();
    const selectedMeta = new Map();
    const listEl = document.getElementById("selected-list");
    const totalEl = document.getElementById("total");
    const proceedBtn = document.getElementById("proceedBtn");
    const seatsInput = document.getElementById("selectedSeatsInput");
    const cartEl = document.getElementById("cart"); // for glassy active aura

    function refreshSummary() {
        if (selectedIds.size === 0) {
            listEl.textContent = "None";
            totalEl.textContent = "৳0.00";
            proceedBtn.disabled = true;
            seatsInput.value = "";
            if (cartEl) cartEl.classList.remove("has-items");
            return;
        }
        const items = [];
        let sum = 0;
        for (const [id, meta] of selectedMeta.entries()) {
            items.push(meta.label);
            sum += meta.price;
        }
        listEl.textContent = items.join(", ");
        totalEl.textContent = "৳" + sum.toFixed(2);
        seatsInput.value = Array.from(selectedIds).join(",");
        proceedBtn.disabled = false;
        if (cartEl) cartEl.classList.add("has-items");
    }

    grid.addEventListener("click", (e) => {
        const el = e.target.closest(".seat.available");
        if (!el) return;

        const id = el.getAttribute("data-id");
        const price = parseFloat(el.getAttribute("data-price"));
        const label = el.getAttribute("data-label");

        if (selectedIds.has(id)) {
            selectedIds.delete(id);
            selectedMeta.delete(id);
            el.classList.remove("selected");
        } else {
            selectedIds.add(id);
            selectedMeta.set(id, { label, price });
            el.classList.add("selected");
        }
        refreshSummary();
    });

    refreshSummary();
})();
