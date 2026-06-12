/* Cart page ajax: qty inc/dec, remove. Server recomputes price (GetCartQuery). */
(function () {
    'use strict';

    function token() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function fmt(n) {
        return (n || 0).toLocaleString('en-US') + ' تومان';
    }

    async function post(url, data) {
        const body = new URLSearchParams(data);
        const res = await fetch(url, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token()
            },
            body: body.toString()
        });
        if (!res.ok) throw new Error('request failed');
        return res.json();
    }

    function updateBadges(count) {
        document.querySelectorAll('[data-cart-count]').forEach(b => {
            b.textContent = count;
            b.hidden = count <= 0;
        });
    }

    const page = document.querySelector('[data-cart-page]');
    if (!page) return;

    function lineData(line) {
        return {
            productId: line.getAttribute('data-pid'),
            variantId: line.getAttribute('data-vid') || ''
        };
    }

    async function setQty(line, qty) {
        const d = lineData(line);
        try {
            const r = await post('/cart/update', { productId: d.productId, variantId: d.variantId, quantity: qty });
            if (qty <= 0) {
                line.remove();
                if (!page.querySelector('[data-line]')) { location.reload(); return; }
            } else {
                line.querySelector('[data-qty-val]').textContent = qty;
            }
            refreshSummary(r);
        } catch (e) { location.reload(); }
    }

    async function removeLine(line) {
        const d = lineData(line);
        try {
            const r = await post('/cart/remove', { productId: d.productId, variantId: d.variantId });
            line.remove();
            if (!page.querySelector('[data-line]')) { location.reload(); return; }
            refreshSummary(r);
        } catch (e) { location.reload(); }
    }

    function refreshSummary(r) {
        const q = document.querySelector('[data-sum-qty]');
        const p = document.querySelector('[data-sum-pay]');
        if (q) q.textContent = r.count;
        if (p) p.textContent = fmt(r.payable);
        updateBadges(r.count);
    }

    page.addEventListener('click', function (e) {
        const line = e.target.closest('[data-line]');
        if (!line) return;
        const valEl = line.querySelector('[data-qty-val]');
        const cur = parseInt(valEl.textContent, 10) || 1;

        if (e.target.closest('[data-qty-inc]')) setQty(line, cur + 1);
        else if (e.target.closest('[data-qty-dec]')) setQty(line, cur - 1);
        else if (e.target.closest('[data-remove]')) removeLine(line);
    });
})();
