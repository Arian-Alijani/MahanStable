// نمایش/مخفی‌کردن انتخابگر دسته در فرم نوار محصول، وقتی منبع = «بر اساس دسته‌بندی»
(function () {
    'use strict';
    var sel = document.querySelector('[data-source-select]');
    var field = document.querySelector('[data-category-field]');
    if (!sel || !field) return;

    var BY_CATEGORY = '4'; // HomeProductSource.ByCategory
    function sync() { field.style.display = (sel.value === BY_CATEGORY) ? '' : 'none'; }
    sel.addEventListener('change', sync);
    sync();
})();
