// STAR CINEPLEX — ShowTickets micro-interactions (parallax + ripple)
// Path: /wwwroot/js/showtickets-glass.js

(function () {
    // Respect reduced-motion users
    var prefersReduced = false;
    try {
        prefersReduced = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    } catch (e) { }

    // ---------- Parallax tilt for cards (ES5) ----------
    function initParallaxTilt() {
        if (prefersReduced) return;

        var tiltEls = document.querySelectorAll('.movie-card, .schedule-col');
        for (var i = 0; i < tiltEls.length; i++) {
            (function (el) {
                var frame = null;

                function onMove(e) {
                    if (frame) return;
                    frame = window.requestAnimationFrame(function () {
                        frame = null;
                        var r = el.getBoundingClientRect();
                        var cx = (e.touches && e.touches[0]) ? e.touches[0].clientX : e.clientX;
                        var cy = (e.touches && e.touches[0]) ? e.touches[0].clientY : e.clientY;
                        var x = cx - r.left;
                        var y = cy - r.top;
                        var rx = ((y / r.height) - 0.5) * -6; // tilt X
                        var ry = ((x / r.width) - 0.5) * 6;   // tilt Y
                        el.style.transform = 'perspective(900px) rotateX(' + rx + 'deg) rotateY(' + ry + 'deg) translateZ(0)';
                    });
                }

                function onLeave() {
                    if (frame) { window.cancelAnimationFrame(frame); frame = null; }
                    el.style.transform = '';
                }

                el.addEventListener('mousemove', onMove, false);
                el.addEventListener('mouseleave', onLeave, false);
                el.addEventListener('touchmove', onMove, false);
                el.addEventListener('touchend', onLeave, false);
                el.addEventListener('touchcancel', onLeave, false);
            })(tiltEls[i]);
        }
    }

    // ---------- Ripple position on hover/click for time buttons (ES5) ----------
    function initRipple() {
        var btns = document.querySelectorAll('.time-btn');
        for (var i = 0; i < btns.length; i++) {
            (function (btn) {
                function setVars(e) {
                    var r = btn.getBoundingClientRect();
                    var cx = (e.touches && e.touches[0]) ? e.touches[0].clientX : e.clientX;
                    var cy = (e.touches && e.touches[0]) ? e.touches[0].clientY : e.clientY;
                    btn.style.setProperty('--x', (cx - r.left) + 'px');
                    btn.style.setProperty('--y', (cy - r.top) + 'px');
                }
                btn.addEventListener('mousemove', setVars, false);
                btn.addEventListener('click', setVars, false);
                btn.addEventListener('touchstart', setVars, false);
                btn.addEventListener('touchmove', setVars, false);
            })(btns[i]);
        }
    }

    // Init after DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            initParallaxTilt();
            initRipple();
        });
    } else {
        initParallaxTilt();
        initRipple();
    }
})();
