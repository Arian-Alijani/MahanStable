// MahanShop — site.js  (vanilla, domestic-only، بدون وابستگی خارجی)
(function () {
    'use strict';

    /* ---------- Mobile drawer ---------- */
    const drawer = document.querySelector('[data-drawer]');
    function openDrawer() { if (drawer) { drawer.hidden = false; document.body.style.overflow = 'hidden'; } }
    function closeDrawer() { if (drawer) { drawer.hidden = true; document.body.style.overflow = ''; } }
    document.querySelectorAll('[data-drawer-open]').forEach(b => b.addEventListener('click', openDrawer));
    document.querySelectorAll('[data-drawer-close]').forEach(b => b.addEventListener('click', closeDrawer));
    document.addEventListener('keydown', e => { if (e.key === 'Escape') closeDrawer(); });

    /* ---------- Cart popover ---------- */
    const cartBtn = document.querySelector('[data-cart-toggle]');
    const cartPop = document.querySelector('[data-cart-pop]');
    if (cartBtn && cartPop) {
        cartBtn.addEventListener('click', e => { e.stopPropagation(); cartPop.hidden = !cartPop.hidden; });
        document.addEventListener('click', e => {
            if (!cartPop.hidden && !cartPop.contains(e.target) && !cartBtn.contains(e.target)) cartPop.hidden = true;
        });
    }

    /* ---------- Hero slider (crossfade خودکار — بدون دکمهٔ جابجایی، هر ۱۵ ثانیه) ---------- */
    document.querySelectorAll('[data-hero]').forEach(hero => {
        const slides = Array.from(hero.querySelectorAll('[data-hero-slide]'));
        if (slides.length < 2) return;
        let idx = 0, timer = null;

        function go(i) {
            idx = (i + slides.length) % slides.length;
            slides.forEach((s, k) => {
                const on = k === idx;
                s.classList.toggle('is-active', on);
                s.setAttribute('aria-hidden', on ? 'false' : 'true');
            });
        }
        function start() { stop(); timer = setInterval(() => go(idx + 1), 15000); }
        function stop() { if (timer) clearInterval(timer); }

        // توقف هنگام hover، ادامه با خروج موس (UX استاندارد اسلایدر خودکار)
        hero.addEventListener('mouseenter', stop);
        hero.addEventListener('mouseleave', start);
        start();
    });

    /* ---------- Drag-to-scroll carousels (pointer) ---------- */
    document.querySelectorAll('[data-carousel]').forEach(el => {
        let down = false, startX = 0, startScroll = 0, moved = false, pid = null;

        // جلوگیری از درگ native تصاویر/لینک‌ها (که باعث بهم‌ریختگی می‌شد)
        el.querySelectorAll('img, a').forEach(n => { n.setAttribute('draggable', 'false'); });
        el.addEventListener('dragstart', e => e.preventDefault());

        el.addEventListener('pointerdown', e => {
            if (e.pointerType === 'mouse' && e.button !== 0) return;
            down = true; moved = false; startX = e.clientX; startScroll = el.scrollLeft; pid = e.pointerId;
            el.classList.add('dragging');
            try { el.setPointerCapture(pid); } catch (_) {}
        });
        el.addEventListener('pointermove', e => {
            if (!down) return;
            const dx = e.clientX - startX;
            if (Math.abs(dx) > 4) moved = true;
            if (moved) {
                el.scrollLeft = startScroll - dx;
                e.preventDefault();
            }
        });
        function up() {
            if (!down) return;
            down = false; el.classList.remove('dragging');
            if (pid != null) { try { el.releasePointerCapture(pid); } catch (_) {} pid = null; }
        }
        el.addEventListener('pointerup', up);
        el.addEventListener('pointercancel', up);
        // جلوگیری از کلیک ناخواسته بعد از درگ (روی هر فرزندی)
        el.addEventListener('click', e => { if (moved) { e.preventDefault(); e.stopPropagation(); moved = false; } }, true);
    });

    /* ---------- Add to cart (P6): ajax → /cart/add. قیمت سمت سرور. ---------- */
    function antiForgeryToken() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    async function addToCart(productId, variantId, btn) {
        const data = new URLSearchParams();
        data.append('productId', productId);
        if (variantId != null && variantId !== '') data.append('variantId', variantId);
        data.append('quantity', '1');

        if (btn) btn.classList.add('is-loading');
        try {
            const res = await fetch('/cart/add', {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': antiForgeryToken()
                },
                body: data.toString()
            });
            if (!res.ok) throw new Error('failed');
            const r = await res.json();
            document.querySelectorAll('[data-cart-count]').forEach(b => {
                b.textContent = r.count;
                b.hidden = r.count <= 0;
            });
            if (btn) {
                btn.classList.add('is-added');
                setTimeout(() => btn.classList.remove('is-added'), 1200);
            }
        } catch (e) {
            // fallback: هدایت به صفحه محصول/سبد
        } finally {
            if (btn) btn.classList.remove('is-loading');
        }
    }

    document.querySelectorAll('[data-add]').forEach(btn => {
        btn.addEventListener('click', () => {
            const productId = btn.getAttribute('data-add');
            // variant انتخاب‌شده (محصول variant‌دار): از data-selected-variant روی دکمه
            const variantId = btn.getAttribute('data-selected-variant') || '';
            if (btn.hasAttribute('data-has-variants') && variantId === '') {
                window.PdVariant && window.PdVariant.flashRequired && window.PdVariant.flashRequired();
                return;
            }
            addToCart(productId, variantId, btn);
        });
    });

    /* ---------- Catalog (Phase 4A): drawer + auto-submit + dual price slider ---------- */
    const catalogForm = document.querySelector('[data-js-enhance="catalog"]');
    if (catalogForm) {
        const sidebar = document.getElementById('filter-sidebar');
        const overlay = document.getElementById('filter-overlay');
        const openBtn = document.getElementById('filter-toggle');
        const closeBtn = document.getElementById('filter-close');

        function openFilters() {
            if (!sidebar || !overlay) return;
            sidebar.classList.add('is-open');
            overlay.hidden = false;
            requestAnimationFrame(() => overlay.classList.add('is-visible'));
            if (openBtn) openBtn.setAttribute('aria-expanded', 'true');
            document.body.style.overflow = 'hidden';
        }
        function closeFilters() {
            if (!sidebar || !overlay) return;
            sidebar.classList.remove('is-open');
            overlay.classList.remove('is-visible');
            setTimeout(() => { overlay.hidden = true; }, 250);
            if (openBtn) openBtn.setAttribute('aria-expanded', 'false');
            document.body.style.overflow = '';
        }
        if (openBtn) openBtn.addEventListener('click', openFilters);
        if (closeBtn) closeBtn.addEventListener('click', closeFilters);
        if (overlay) overlay.addEventListener('click', closeFilters);
        document.addEventListener('keydown', e => {
            if (e.key === 'Escape' && sidebar && sidebar.classList.contains('is-open')) closeFilters();
        });

        const isDesktop = () => window.matchMedia('(min-width: 992px)').matches;
        const submitForm = () => catalogForm.requestSubmit
            ? catalogForm.requestSubmit() : catalogForm.submit();

        // auto-submit روی دسکتاپ؛ موبایل با دکمه «اعمال»
        catalogForm.querySelectorAll('input[type="radio"], input[type="checkbox"], #sort-by')
            .forEach(el => el.addEventListener('change', () => { if (isDesktop()) submitForm(); }));

        // ----- dual price slider -----
        const wrap = catalogForm.querySelector('.price-filter');
        if (wrap) {
            const minR = document.getElementById('price-min-range');
            const maxR = document.getElementById('price-max-range');
            const minH = document.getElementById('PriceMin');
            const maxH = document.getElementById('PriceMax');
            const minL = document.getElementById('price-min-label');
            const maxL = document.getElementById('price-max-label');
            const fill = document.getElementById('price-range-fill');
            const floor = parseInt(wrap.dataset.floor, 10) || 0;
            const ceil = parseInt(wrap.dataset.ceil, 10) || 0;
            const span = (ceil - floor) || 1;
            const fmt = v => (v || 0).toLocaleString('fa-IR');

            function sync(submit) {
                let lo = parseInt(minR.value, 10);
                let hi = parseInt(maxR.value, 10);
                if (lo > hi) { if (this === maxR) lo = hi; else hi = lo; minR.value = lo; maxR.value = hi; }
                if (minL) minL.textContent = fmt(lo);
                if (maxL) maxL.textContent = fmt(hi);
                if (fill) {
                    // اسلایدر LTR: چپ=floor، راست=ceil
                    fill.style.left = ((lo - floor) / span * 100) + '%';
                    fill.style.right = ((ceil - hi) / span * 100) + '%';
                }
                // فقط وقتی از پیش‌فرض جدا شد به hidden مقدار بده
                minH.value = lo > floor ? lo : '';
                maxH.value = hi < ceil ? hi : '';
                if (submit && isDesktop()) submitForm();
            }
            let t = null;
            const debounced = function () {
                sync.call(this, false);
                clearTimeout(t);
                const self = this;
                t = setTimeout(() => { if (isDesktop()) submitForm(); }, 600);
            };
            if (minR && maxR) {
                minR.addEventListener('input', function () { sync.call(this, false); });
                maxR.addEventListener('input', function () { sync.call(this, false); });
                minR.addEventListener('change', debounced);
                maxR.addEventListener('change', debounced);
                sync(false);
            }
        }
    }

    /* ---------- Product detail (Phase 4B): gallery + tabs ---------- */
    const pd = document.querySelector('[data-pd]');
    if (pd) {
        const mainImg = document.getElementById('pd-main-image');
        const thumbs = Array.prototype.slice.call(pd.querySelectorAll('[data-pd-thumb]'));

        function showThumb(i) {
            if (!thumbs.length) return;
            i = (i + thumbs.length) % thumbs.length;
            const t = thumbs[i];
            const src = t.getAttribute('data-full-src');
            thumbs.forEach(x => x.classList.remove('is-active'));
            t.classList.add('is-active');
            if (mainImg && src) {
                mainImg.classList.add('is-fading');
                const swap = () => {
                    mainImg.src = src;
                    mainImg.classList.remove('is-fading');
                    mainImg.removeEventListener('transitionend', swap);
                };
                mainImg.addEventListener('transitionend', swap);
                setTimeout(swap, 360); // fallback اگر transitionend نیامد
            }
        }
        function activeIndex() {
            const a = pd.querySelector('[data-pd-thumb].is-active');
            return Math.max(0, thumbs.indexOf(a));
        }
        thumbs.forEach((t, i) => t.addEventListener('click', () => showThumb(i)));
        const prev = pd.querySelector('[data-pd-prev]');
        const next = pd.querySelector('[data-pd-next]');
        if (prev) prev.addEventListener('click', () => showThumb(activeIndex() - 1));
        if (next) next.addEventListener('click', () => showThumb(activeIndex() + 1));

        // Tabs
        const tabs = Array.prototype.slice.call(pd.querySelectorAll('[data-pd-tab]'));
        const panels = Array.prototype.slice.call(pd.querySelectorAll('[data-pd-panel]'));
        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                const key = tab.getAttribute('data-pd-tab');
                tabs.forEach(x => {
                    const on = x === tab;
                    x.classList.toggle('is-active', on);
                    x.setAttribute('aria-selected', on ? 'true' : 'false');
                });
                panels.forEach(p => {
                    const on = p.getAttribute('data-pd-panel') === key;
                    p.classList.toggle('is-active', on);
                    p.hidden = !on;
                });
            });
        });
    }
})();
