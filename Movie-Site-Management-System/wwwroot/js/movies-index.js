// ===== Movies Index — Client Filtering by Status + Genre + Theatre =====
(function () {
    // Only run this script on the Movies Index page (it has the #cards container)
    const cardsHost = document.getElementById('cards');
    if (!cardsHost) return;

    // Read dataset
    const rawJson = document.getElementById('moviesData')?.textContent || '[]';
    let MOVIES = [];
    try { MOVIES = JSON.parse(rawJson); } catch { MOVIES = []; }

    // Normalize IDs to strings; keep extra fields
    MOVIES = MOVIES.map(m => ({
        ...m,
        id: String(m.id ?? ''),
        status: String(m.status ?? ''), // "NowShowing" | "ComingSoon" | "Archived"
        genre: String(m.genre ?? ''),   // single-genre string
        shows: (m.shows || []).map(s => ({
            theatreId: s.theatreId,
            date: s.date // "yyyy-MM-dd" (already >= today from server)
        }))
    }));

    // Map cards by id (as string)
    const cardEls = Array.from(cardsHost.querySelectorAll('.card') || []);
    const mapCardById = new Map(cardEls.map(el => [String(el.dataset.id || ''), el]));

    function setActive(el) {
        ['nav_now', 'nav_soon', 'nav_showtime', 'nav_top', 'nav_genre'].forEach(id =>
            document.getElementById(id)?.classList.remove('active')
        );
        el?.classList.add('active');
    }

    function showOnly(ids) {
        const set = new Set(ids.map(String));
        mapCardById.forEach((el, id) => {
            el.style.display = set.has(id) ? '' : 'none';
        });
    }

    // ----- Helpers for date -----
    const todayStr = new Date().toISOString().slice(0, 10);

    // ----- Subsets by status / theatre -----
    function idsNowShowing() {
        return MOVIES.filter(m => m.status === 'NowShowing').map(m => m.id);
    }

    function idsComingSoon() {
        return MOVIES.filter(m => m.status === 'ComingSoon').map(m => m.id);
    }

    function idsTopRated() {
        // Typical expectation: exclude Archived from "Top Rated"
        return MOVIES
            .filter(m => m.status !== 'Archived')
            .slice()
            .sort((a, b) => (b.imdb ?? 0) - (a.imdb ?? 0))
            .slice(0, 20)
            .map(m => m.id);
    }

    // All movies that have any *upcoming* show at this theatre (>= today)
    function idsByTheatre(theatreId, { todayOnly = false } = {}) {
        if (!theatreId) return [];
        return MOVIES
            .filter(m => (m.shows || []).some(s =>
                s.theatreId === theatreId && (!todayOnly || s.date === todayStr)
            ))
            .map(m => m.id);
    }

    // ===== Genre filter (UI + state) =====
    const pill = document.getElementById('nav_genre');
    const panel = document.getElementById('genre_panel');
    const listBox = document.getElementById('genre_list');
    const btnClear = document.getElementById('genre_clear');
    const btnApply = document.getElementById('genre_apply');
    const badge = document.getElementById('genre-selected-count');

    // Build unique list of genres (each movie has one genre string)
    const uniqueGenres = Array.from(new Set(MOVIES.map(m => m.genre).filter(Boolean))).sort();
    if (listBox) {
        listBox.innerHTML = uniqueGenres.map(g => `
            <label class="gd_item">
                <input type="checkbox" value="${g}">
                <span>${g}</span>
            </label>
        `).join('');
    }

    const selectedGenres = new Set(); // Set<string>

    function updateBadge() {
        const n = selectedGenres.size;
        if (!badge) return;
        badge.textContent = String(n);
        badge.classList.toggle('hidden', n === 0);
    }

    function openPanel(show) { if (panel) panel.style.display = show ? 'flex' : 'none'; }
    let panelOpen = false;

    // Toggle on pill click (doesn't take active state)
    pill?.addEventListener('click', (e) => {
        e.preventDefault();
        panelOpen = !panelOpen; openPanel(panelOpen);
    });

    // Close on outside/Escape
    document.addEventListener('click', (e) => {
        if (!panelOpen || !panel) return;
        if (!panel.contains(e.target) && e.target !== pill) { panelOpen = false; openPanel(false); }
    });
    document.addEventListener('keydown', (e) => { if (e.key === 'Escape' && panelOpen) { panelOpen = false; openPanel(false); } });

    // Clear/Apply behavior
    btnClear?.addEventListener('click', () => {
        selectedGenres.clear();
        listBox?.querySelectorAll('input[type="checkbox"]').forEach(cb => cb.checked = false);
        updateBadge();
        applyCombinedFilter(); // re-apply with no genre selection
    });

    btnApply?.addEventListener('click', () => {
        selectedGenres.clear();
        listBox?.querySelectorAll('input[type="checkbox"]:checked').forEach(cb => selectedGenres.add(cb.value));
        updateBadge();
        panelOpen = false; openPanel(false);
        applyCombinedFilter(); // apply with current selection
    });

    // Combine current base ids with genre selection (intersection)
    function applyCombinedFilter(baseIds) {
        let ids = baseIds || idsNowShowing(); // default to Now Showing if none provided
        if (selectedGenres.size > 0) {
            ids = ids.filter(id => {
                const m = MOVIES.find(x => x.id === id);
                return m && selectedGenres.has(m.genre);
            });
        }
        showOnly(ids);
    }

    // ===== Wire up tabs =====
    const elNow = document.getElementById('nav_now');
    const elSoon = document.getElementById('nav_soon');
    const elTime = document.getElementById('nav_showtime'); // TOP-NAV link should navigate (no JS handler!)
    const elTop = document.getElementById('nav_top');

    // Keep only the tabs that must filter on THIS page.
    elNow?.addEventListener('click', e => { e.preventDefault(); setActive(elNow); applyCombinedFilter(idsNowShowing()); });
    elSoon?.addEventListener('click', e => { e.preventDefault(); setActive(elSoon); applyCombinedFilter(idsComingSoon()); });
    // IMPORTANT: Do NOT attach a click handler to elTime; let it navigate to /Movies/ShowTickets
    elTop?.addEventListener('click', e => { e.preventDefault(); setActive(elTop); applyCombinedFilter(idsTopRated()); });

    // ===== Public hook for Theatre selection from the modal (still useful on index) =====
    // Called after the user picks a theatre; shows ALL movies that have any upcoming show at that theatre (>= today).
    window.applyFilterForTheatre = function (theatreId) {
        // If your top nav "Show Time" is an <a href="/Movies/ShowTickets">, this function isn't used there.
        // We keep it here for the index page theatre modal experience.
        const ids = idsByTheatre(Number(theatreId), { todayOnly: false });
        setActive(document.getElementById('nav_now')); // fall back to "Now Showing" styling
        applyCombinedFilter(ids);
    };

    // Default view on index
    setActive(elNow);
    applyCombinedFilter(idsNowShowing());
    updateBadge();
})();
