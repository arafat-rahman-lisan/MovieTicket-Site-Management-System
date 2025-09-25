// Schedule Builder page script (Language required + safe create gating)
(function () {
    const dateInput = document.getElementById('dateInput');
    const formDate = document.getElementById('formDate');        // hidden Date inside <form>
    const languageInput = document.getElementById('languageInput'); // visible Language (outside form)
    const movieSelect = document.getElementById('movieSelect');
    const moviesContainer = document.getElementById('moviesContainer');
    const hiddenFields = document.getElementById('hiddenFields'); // inside <form>
    const previewBtn = document.getElementById('previewBtn');
    const createBtn = document.getElementById('createBtn') || document.querySelector('#createBtn');
    const scheduleForm = document.getElementById('scheduleForm');

    let scheduleData = null;

    // ---------- Enable/disable Create button ----------
    function hasAnySelection() {
        return !!moviesContainer.querySelector('input[type="checkbox"]:checked');
    }
    function canSubmit() {
        const hasDate = !!formDate.value;
        const langOk = !!(languageInput.value && languageInput.value.trim().length > 0);
        return hasDate && langOk && hasAnySelection();
    }
    function updateCreateButton() {
        if (createBtn) createBtn.disabled = !canSubmit();
    }

    // ---------- Data ----------
    function fetchData() {
        const d = dateInput.value;
        if (!d) {
            scheduleData = null;
            rebuildUI();
            return;
        }
        formDate.value = d;

        fetch(`/AdminSchedule/Data?date=${encodeURIComponent(d)}`)
            .then(r => r.json())
            .then(json => { scheduleData = json; rebuildUI(); })
            .catch(() => {
                moviesContainer.innerHTML = '<div class="text-danger">Failed to load schedule data.</div>';
                scheduleData = null;
            })
            .finally(updateCreateButton);
    }

    function getSelectedMovieIds() {
        return Array.from(movieSelect.selectedOptions).map(o => Number(o.value));
    }

    // ---------- UI Build ----------
    function rebuildUI() {
        moviesContainer.innerHTML = '';
        const mids = getSelectedMovieIds();

        if (!scheduleData || mids.length === 0) {
            moviesContainer.innerHTML = '<div class="text-muted">Pick a date and one or more movies to begin.</div>';
            rebuildHiddenFields();
            updateCreateButton();
            return;
        }

        mids.forEach(mid => {
            const movieName = (movieSelect.querySelector(`option[value="${mid}"]`) || {}).textContent || `Movie #${mid}`;
            const section = document.createElement('div');
            section.className = 'movie-section';

            const header = document.createElement('div');
            header.className = 'd-flex align-items-center justify-content-between mb-2';
            header.innerHTML = `<strong>${movieName}</strong><span class="text-muted small">Select theatre → hall → time</span>`;
            section.appendChild(header);

            scheduleData.theatres.forEach(th => {
                const thWrap = document.createElement('div');
                thWrap.className = 'mb-2';
                thWrap.innerHTML = `<div class="fw-semibold mb-1">${th.theatreName}</div>`;

                th.halls.forEach(h => {
                    const hWrap = document.createElement('div');
                    hWrap.className = 'mb-2 ms-2';
                    hWrap.innerHTML = `<div class="text-muted mb-1">${h.hallName}</div>`;

                    const slotsWrap = document.createElement('div');
                    slotsWrap.className = 'ms-3 d-flex flex-wrap gap-2';

                    h.hallSlots.forEach(s => {
                        const id = `m${mid}_hs${s.hallSlotId}`;

                        const cb = document.createElement('input');
                        cb.type = 'checkbox';
                        cb.id = id;
                        cb.dataset.movieId = String(mid);
                        cb.dataset.hallSlotId = String(s.hallSlotId);
                        cb.dataset.occupied = s.isOccupied ? '1' : '0';
                        cb.style.display = 'none';
                        cb.disabled = s.isOccupied;

                        const label = document.createElement('label');
                        label.className = 'slot-pill' + (s.isOccupied ? ' disabled' : '');
                        label.setAttribute('for', id);
                        label.textContent = `${s.start}–${s.end}`;
                        label.tabIndex = s.isOccupied ? -1 : 0;
                        label.setAttribute('role', 'button');
                        label.setAttribute('aria-pressed', 'false');

                        slotsWrap.appendChild(cb);
                        slotsWrap.appendChild(label);
                    });

                    hWrap.appendChild(slotsWrap);
                    thWrap.appendChild(hWrap);
                });

                section.appendChild(thWrap);
            });

            moviesContainer.appendChild(section);
        });

        rebuildHiddenFields();
        updateCreateButton();
    }

    // ---------- Hidden fields + constraints ----------
    function rebuildHiddenFields() {
        hiddenFields.innerHTML = '';

        // Mirror Language into the form (visible input may be outside <form>)
        const langHidden = document.createElement('input');
        langHidden.type = 'hidden';
        langHidden.name = 'Language';
        langHidden.value = (languageInput.value || '').trim();
        hiddenFields.appendChild(langHidden);

        // Persist selected MovieIds for the POST body
        const mids = getSelectedMovieIds();
        mids.forEach(mid => {
            const inp = document.createElement('input');
            inp.type = 'hidden';
            inp.name = 'MovieIds';
            inp.value = String(mid);
            hiddenFields.appendChild(inp);
        });

        // Persist Selections[i].MovieId + Selections[i].HallSlotId
        const selected = moviesContainer.querySelectorAll('input[type="checkbox"]:checked');
        let i = 0;
        selected.forEach(cb => {
            const m = document.createElement('input');
            m.type = 'hidden';
            m.name = `Selections[${i}].MovieId`;
            m.value = cb.dataset.movieId;

            const hs = document.createElement('input');
            hs.type = 'hidden';
            hs.name = `Selections[${i}].HallSlotId`;
            hs.value = cb.dataset.hallSlotId;

            hiddenFields.appendChild(m);
            hiddenFields.appendChild(hs);
            i++;
        });

        // Enforce "one movie per hallslot" within the page
        const selectedSlotIds = new Set(Array.from(selected).map(cb => cb.dataset.hallSlotId));
        const all = moviesContainer.querySelectorAll('input[type="checkbox"]');
        all.forEach(cb => {
            const label = moviesContainer.querySelector(`label[for="${cb.id}"]`);
            const originallyOccupied = cb.dataset.occupied === '1';

            if (!cb.checked && selectedSlotIds.has(cb.dataset.hallSlotId)) {
                cb.disabled = true;
                if (label) label.classList.add('disabled');
            } else {
                cb.disabled = originallyOccupied;
                if (label && !originallyOccupied) label.classList.remove('disabled');
            }

            if (label) {
                label.classList.toggle('active', cb.checked);
                label.setAttribute('aria-pressed', cb.checked ? 'true' : 'false');
            }
        });
    }

    // ---------- Delegated interactions ----------
    document.addEventListener('click', (e) => {
        const label = e.target.closest('label.slot-pill');
        if (!label || !moviesContainer.contains(label)) return;

        const id = label.getAttribute('for');
        const cb = document.getElementById(id);
        if (!cb || cb.disabled) return;

        cb.checked = !cb.checked;
        label.classList.toggle('active', cb.checked);
        label.setAttribute('aria-pressed', cb.checked ? 'true' : 'false');
        rebuildHiddenFields();
        updateCreateButton();
        e.preventDefault();
    });

    document.addEventListener('keydown', (e) => {
        if (e.key !== 'Enter' && e.key !== ' ') return;
        const label = e.target.closest('label.slot-pill');
        if (!label || !moviesContainer.contains(label)) return;
        e.preventDefault();
        label.click();
    }, true);

    // ---------- Page hooks ----------
    previewBtn && previewBtn.addEventListener('click', () => {
        const total = moviesContainer.querySelectorAll('input[type="checkbox"]:checked').length;
        alert(`Selected ${total} show slot(s).`);
    });

    // Keep date + language + movie selection in sync
    dateInput.addEventListener('change', () => { fetchData(); updateCreateButton(); });
    languageInput.addEventListener('input', () => { rebuildHiddenFields(); updateCreateButton(); });
    movieSelect.addEventListener('change', () => { rebuildUI(); updateCreateButton(); });

    // Ensure hidden fields are ready on submit and block if incomplete
    if (scheduleForm) {
        scheduleForm.addEventListener('submit', (e) => {
            rebuildHiddenFields();
            if (!canSubmit()) {
                e.preventDefault();
                alert('Please enter Language, pick a date, and select at least one show slot.');
                return;
            }
            updateCreateButton();
        });
    }

    // initial
    fetchData();
    rebuildHiddenFields();
    updateCreateButton();
})();
