// مدیریت گرید دسته‌بندی صفحهٔ اصلی — انتخاب سبک + مرتب‌سازی درگ‌ودراپ (وانیلا، بدون وابستگی)
(function () {
    'use strict';

    /* ---------- انتخاب سبک نمایش ---------- */
    var styleInput = document.getElementById('catStyleInput');
    var cards = document.querySelectorAll('.cat-style-card');
    cards.forEach(function (card) {
        card.addEventListener('click', function () {
            cards.forEach(function (c) {
                c.classList.remove('is-selected');
                c.setAttribute('aria-pressed', 'false');
            });
            card.classList.add('is-selected');
            card.setAttribute('aria-pressed', 'true');
            if (styleInput) styleInput.value = card.getAttribute('data-style');
        });
    });

    /* ---------- مرتب‌سازی درگ‌ودراپ ---------- */
    var list = document.getElementById('catSortable');
    var orderedInput = document.getElementById('catOrderedIds');
    var saveBtn = document.getElementById('catSaveOrderBtn');
    if (!list) return;

    var dragging = null;
    var dirty = false;

    function markDirty() {
        dirty = true;
        if (saveBtn) saveBtn.disabled = false;
        syncOrder();
    }

    function syncOrder() {
        if (!orderedInput) return;
        var ids = Array.prototype.map.call(
            list.querySelectorAll('.cat-sortable__item'),
            function (li) { return li.getAttribute('data-id'); }
        );
        orderedInput.value = ids.join(',');
    }

    function getAfterElement(y) {
        var items = Array.prototype.slice.call(
            list.querySelectorAll('.cat-sortable__item:not(.is-dragging)')
        );
        var closest = { offset: Number.NEGATIVE_INFINITY, el: null };
        items.forEach(function (child) {
            var box = child.getBoundingClientRect();
            var offset = y - box.top - box.height / 2;
            if (offset < 0 && offset > closest.offset) {
                closest = { offset: offset, el: child };
            }
        });
        return closest.el;
    }

    list.querySelectorAll('.cat-sortable__item').forEach(function (item) {
        item.addEventListener('dragstart', function () {
            dragging = item;
            requestAnimationFrame(function () { item.classList.add('is-dragging'); });
        });
        item.addEventListener('dragend', function () {
            item.classList.remove('is-dragging');
            dragging = null;
            markDirty();
        });
    });

    list.addEventListener('dragover', function (e) {
        e.preventDefault();
        if (!dragging) return;
        var after = getAfterElement(e.clientY);
        if (after == null) list.appendChild(dragging);
        else list.insertBefore(dragging, after);
    });

    // سینک اولیه تا اگر کاربر بدون درگ هم ذخیره زد، ترتیب فعلی برود
    syncOrder();
})();
