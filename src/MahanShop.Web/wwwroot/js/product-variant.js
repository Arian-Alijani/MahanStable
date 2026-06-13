/* product-variant.js — انتخابگر تنوع صفحهٔ محصول.
   دو جریان:
     device  → برند (لوگو) ← انتخاب → لیست مدل (با موجودی) ← [رنگ popup] → سبد انتخاب → افزودن همه
     simple  → swatch رنگ + دراپ‌داون سایر ویژگی‌ها → افزودن تکی
   داده فقط ساختار/موجودی است؛ قیمت نهایی همیشه سمت سرور دوباره محاسبه می‌شود. */
(function () {
    'use strict';

    var root = document.querySelector('[data-pv-root]');
    if (!root) return;

    var modelEl = document.querySelector('[type="application/json"][data-pd-model]');
    if (!modelEl) return;

    var MODEL;
    try { MODEL = JSON.parse(modelEl.textContent); } catch (e) { return; }
    if (!MODEL || !MODEL.variants || !MODEL.variants.length) return;

    var KIND = { OTHER: 0, BRAND: 1, MODEL: 2, COLOR: 3 };

    // ---------- helpers ----------
    function el(tag, cls, html) {
        var n = document.createElement(tag);
        if (cls) n.className = cls;
        if (html != null) n.innerHTML = html;
        return n;
    }
    function fmt(n) { return (n || 0).toLocaleString('en-US'); }
    function token() {
        var i = document.querySelector('input[name="__RequestVerificationToken"]');
        return i ? i.value : '';
    }
    function valueById(id) {
        for (var a = 0; a < MODEL.attributes.length; a++) {
            var vs = MODEL.attributes[a].values;
            for (var v = 0; v < vs.length; v++) if (vs[v].id === id) return vs[v];
        }
        return null;
    }
    function attrByKind(k) { return MODEL.attributes.filter(function (a) { return a.kind === k; }); }

    function stockClass(s) { return s <= 0 ? 'is-out' : (s <= 5 ? 'is-low' : 'is-in'); }
    function stockLabel(s) { return s <= 0 ? 'ناموجود' : (s <= 5 ? ('فقط ' + s + ' عدد') : 'موجود'); }

    function bumpCartCount(count) {
        document.querySelectorAll('[data-cart-count]').forEach(function (b) {
            b.textContent = count; b.hidden = count <= 0;
        });
    }

    function toast(msg) {
        var t = el('div', 'pv-toast', msg);
        document.body.appendChild(t);
        requestAnimationFrame(function () { t.classList.add('is-show'); });
        setTimeout(function () { t.classList.remove('is-show'); setTimeout(function () { t.remove(); }, 300); }, 2600);
    }

    // ============================================================
    //  SIMPLE FLOW
    // ============================================================
    function buildSimple() {
        var fields = el('div', 'pv-simple');
        var selected = {}; // attrId -> valueId

        MODEL.attributes.forEach(function (attr) {
            var field = el('div', 'pv-field');
            field.appendChild(el('span', 'pv-field__label', attr.name));

            if (attr.isColor || attr.kind === KIND.COLOR) {
                var wrap = el('div', 'pv-swatches');
                attr.values.forEach(function (v) {
                    var b = el('button', 'pv-swatch', '<span class="pv-swatch__dot" style="background:' + (v.color || '#888') + '"></span><span class="pv-swatch__txt">' + v.value + '</span>');
                    b.type = 'button';
                    b.setAttribute('aria-label', v.value);
                    b.addEventListener('click', function () {
                        wrap.querySelectorAll('.pv-swatch').forEach(function (x) { x.classList.remove('is-active'); });
                        b.classList.add('is-active');
                        selected[attr.id] = v.id;
                        refresh();
                    });
                    wrap.appendChild(b);
                });
                field.appendChild(wrap);
            } else {
                var sel = el('select', 'pv-select');
                sel.appendChild(new Option('انتخاب کنید', ''));
                attr.values.forEach(function (v) { sel.appendChild(new Option(v.value, v.id)); });
                sel.addEventListener('change', function () {
                    selected[attr.id] = sel.value ? parseInt(sel.value, 10) : null;
                    refresh();
                });
                field.appendChild(sel);
            }
            fields.appendChild(field);
        });

        var stockBar = el('div', 'pv-stockbar');
        fields.appendChild(stockBar);
        root.appendChild(fields);

        var cta = document.querySelector('[data-pd-cta]');
        var priceEl = document.querySelector('[data-pd-price]');
        var oldEl = document.querySelector('[data-pd-old]');

        function matchVariant() {
            var ids = Object.keys(selected).map(function (k) { return selected[k]; }).filter(function (x) { return x; });
            // همه ویژگی‌ها باید انتخاب شده باشند
            if (ids.length < MODEL.attributes.length) return null;
            return MODEL.variants.find(function (v) {
                return ids.every(function (id) { return v.values.indexOf(id) !== -1; });
            }) || null;
        }

        function refresh() {
            var vr = matchVariant();
            if (!vr) {
                stockBar.className = 'pv-stockbar';
                stockBar.textContent = '';
                if (cta) { cta.setAttribute('disabled', 'disabled'); cta.classList.add('is-disabled'); cta.removeAttribute('data-selected-variant'); }
                return;
            }
            stockBar.className = 'pv-stockbar ' + stockClass(vr.stock);
            stockBar.innerHTML = '<span class="pv-dot"></span>' + (vr.stock > 0 ? ('موجودی: ' + vr.stock + ' عدد') : 'ناموجود');
            if (priceEl) priceEl.textContent = fmt(vr.final);
            if (oldEl) { if (vr.hasDiscount) { oldEl.textContent = fmt(vr.price); oldEl.hidden = false; } else { oldEl.hidden = true; } }
            if (cta) {
                var label = cta.querySelector('[data-pd-cta-label]');
                if (vr.stock > 0) {
                    cta.removeAttribute('disabled'); cta.classList.remove('is-disabled');
                    cta.setAttribute('data-selected-variant', vr.id);
                    if (label) label.textContent = 'افزودن به سبد خرید';
                } else {
                    cta.setAttribute('disabled', 'disabled'); cta.classList.add('is-disabled');
                    cta.removeAttribute('data-selected-variant');
                    if (label) label.textContent = 'ناموجود';
                }
            }
        }

        // پیش‌انتخاب اگر تک‌مقداری
        MODEL.attributes.forEach(function (attr) {
            if (attr.values.length === 1) selected[attr.id] = attr.values[0].id;
        });
        refresh();
    }

    // ============================================================
    //  DEVICE FLOW  (برند → مدل [→ رنگ] → سبد انتخاب)
    // ============================================================
    function buildDevice() {
        var brandAttr = attrByKind(KIND.BRAND)[0];
        var modelAttr = attrByKind(KIND.MODEL)[0];
        var colorAttr = attrByKind(KIND.COLOR)[0]; // اختیاری
        if (!brandAttr || !modelAttr) { buildSimple(); return; }

        // ایندکس: برای هر مدل، رنگ‌های موجود + variant مربوطه
        // variantها = ترکیب (brandVal, modelVal[, colorVal])
        function variantOf(modelValId, colorValId) {
            return MODEL.variants.find(function (v) {
                var okModel = v.values.indexOf(modelValId) !== -1;
                var okColor = colorAttr ? (colorValId ? v.values.indexOf(colorValId) !== -1 : true) : true;
                return okModel && okColor;
            }) || null;
        }
        // مدل‌های هر برند
        function modelsOfBrand(brandValId) {
            return modelAttr.values.filter(function (mv) {
                return MODEL.variants.some(function (v) {
                    return v.values.indexOf(brandValId) !== -1 && v.values.indexOf(mv.id) !== -1;
                });
            });
        }
        function colorsOfModel(brandValId, modelValId) {
            if (!colorAttr) return [];
            return colorAttr.values.filter(function (cv) {
                return MODEL.variants.some(function (v) {
                    return v.values.indexOf(brandValId) !== -1 && v.values.indexOf(modelValId) !== -1 && v.values.indexOf(cv.id) !== -1;
                });
            });
        }
        // موجودی کل یک مدل (مجموع رنگ‌ها یا variant مستقیم)
        function modelStock(brandValId, modelValId) {
            var sum = 0;
            MODEL.variants.forEach(function (v) {
                if (v.values.indexOf(brandValId) !== -1 && v.values.indexOf(modelValId) !== -1) sum += v.stock;
            });
            return sum;
        }
        // برندهای دارای variant
        function brandHasStock(brandValId) {
            return MODEL.variants.some(function (v) { return v.values.indexOf(brandValId) !== -1; });
        }

        var state = { brand: null };

        // ---- search box ----
        var search = el('div', 'pv-search');
        var input = el('input', 'pv-search__input');
        input.type = 'search';
        input.placeholder = 'نام مدل را تایپ کنید (مثلاً A31)';
        input.setAttribute('aria-label', 'جستجوی سریع مدل');
        var results = el('div', 'pv-search__results');
        results.hidden = true;
        search.appendChild(input);
        search.appendChild(results);
        root.appendChild(search);

        // ---- brand row ----
        var brandLabel = el('span', 'pv-step-label', 'انتخاب گوشی');
        root.appendChild(brandLabel);
        var brandRow = el('div', 'pv-brands');
        brandAttr.values.forEach(function (bv) {
            var has = brandHasStock(bv.id);
            var b = el('button', 'pv-brand' + (has ? '' : ' is-empty'),
                (bv.logo ? '<img class="pv-brand__logo" src="' + bv.logo + '" alt="' + bv.value + '">' : '') +
                '<span class="pv-brand__name">' + bv.value + '</span>');
            b.type = 'button';
            if (!has) b.disabled = true;
            b.addEventListener('click', function () { selectBrand(bv.id, b); });
            brandRow.appendChild(b);
        });
        root.appendChild(brandRow);

        // ---- model list ----
        var modelWrap = el('div', 'pv-models');
        modelWrap.hidden = true;
        root.appendChild(modelWrap);

        function selectBrand(brandValId, btn) {
            state.brand = brandValId;
            brandRow.querySelectorAll('.pv-brand').forEach(function (x) { x.classList.remove('is-active'); });
            if (btn) btn.classList.add('is-active');
            renderModels(brandValId);
        }

        function renderModels(brandValId, filterText) {
            modelWrap.hidden = false;
            modelWrap.innerHTML = '';
            var list = modelsOfBrand(brandValId);
            if (filterText) {
                var q = filterText.trim().toLowerCase();
                list = list.filter(function (m) { return m.value.toLowerCase().indexOf(q) !== -1; });
            }
            // ترتیب: موجود → محدود → ناموجود
            list.sort(function (a, b) {
                return rank(modelStock(brandValId, b.id)) - rank(modelStock(brandValId, a.id));
            });
            function rank(s) { return s > 5 ? 2 : (s > 0 ? 1 : 0); }

            if (!list.length) {
                modelWrap.appendChild(el('p', 'pv-models__empty', 'مدلی یافت نشد.'));
                return;
            }
            list.forEach(function (mv) {
                var stock = modelStock(brandValId, mv.id);
                var row = el('div', 'pv-model ' + stockClass(stock));
                row.appendChild(el('span', 'pv-model__name', mv.value));
                var st = el('span', 'pv-model__stock');
                st.innerHTML = '<span class="pv-dot"></span>' + stockLabel(stock);
                row.appendChild(st);

                var chosen = trayQty(brandValId, mv.id);
                if (stock <= 0) {
                    var dis = el('button', 'pv-model__add is-disabled', 'ناموجود');
                    dis.type = 'button'; dis.disabled = true;
                    row.appendChild(dis);
                } else {
                    var add = el('button', 'pv-model__add', chosen > 0 ? ('✓ ' + chosen + ' عدد') : '+ افزودن');
                    add.type = 'button';
                    add.addEventListener('click', function () { onAddModel(brandValId, mv, add); });
                    row.appendChild(add);
                }
                modelWrap.appendChild(row);
            });
        }

        // popup رنگ کنار دکمه
        function onAddModel(brandValId, mv, btn) {
            var colors = colorsOfModel(brandValId, mv.id);
            if (colorAttr && colors.length > 1) {
                openColorPopup(brandValId, mv, colors, btn);
            } else {
                var colorVal = colors.length === 1 ? colors[0] : null;
                addToTray(brandValId, mv.id, colorVal ? colorVal.id : null);
            }
        }

        function openColorPopup(brandValId, mv, colors, anchor) {
            closePopup();
            var pop = el('div', 'pv-colorpop');
            pop.setAttribute('data-pv-pop', '');
            colors.forEach(function (cv) {
                var vr = variantOf(mv.id, cv.id);
                var out = !vr || vr.stock <= 0;
                var c = el('button', 'pv-colorpop__c' + (out ? ' is-out' : ''),
                    '<span class="pv-colorpop__dot" style="background:' + (cv.color || '#888') + '"></span><span>' + cv.value + '</span>');
                c.type = 'button';
                if (out) c.disabled = true;
                c.addEventListener('click', function () { addToTray(brandValId, mv.id, cv.id); closePopup(); });
                pop.appendChild(c);
            });
            anchor.parentNode.appendChild(pop);
            setTimeout(function () { document.addEventListener('click', outside, true); }, 0);
            function outside(e) { if (!pop.contains(e.target) && e.target !== anchor) closePopup(); }
            pop._outside = outside;
        }
        function closePopup() {
            var p = document.querySelector('[data-pv-pop]');
            if (p) { if (p._outside) document.removeEventListener('click', p._outside, true); p.remove(); }
        }

        // ---- search live ----
        input.addEventListener('input', function () {
            var q = input.value.trim();
            if (!q) { results.hidden = true; results.innerHTML = ''; return; }
            var ql = q.toLowerCase();
            var hits = [];
            brandAttr.values.forEach(function (bv) {
                modelsOfBrand(bv.id).forEach(function (mv) {
                    if (mv.value.toLowerCase().indexOf(ql) !== -1) hits.push({ brand: bv, model: mv });
                });
            });
            results.innerHTML = '';
            if (!hits.length) { results.hidden = false; results.appendChild(el('div', 'pv-search__none', 'یافت نشد')); return; }
            hits.slice(0, 8).forEach(function (h) {
                var stock = modelStock(h.brand.id, h.model.id);
                var r = el('button', 'pv-search__item ' + stockClass(stock), '<b>' + h.model.value + '</b><span>' + h.brand.value + '</span><i class="pv-dot"></i>');
                r.type = 'button';
                r.addEventListener('click', function () {
                    // فعال‌سازی خودکار برند + رفتن به مدل
                    var bbtn = Array.prototype.find.call(brandRow.querySelectorAll('.pv-brand'), function (x) {
                        return x.querySelector('.pv-brand__name') && x.querySelector('.pv-brand__name').textContent === h.brand.value;
                    });
                    selectBrand(h.brand.id, bbtn);
                    results.hidden = true; input.value = '';
                    if (stock > 0) onAddModel(h.brand.id, h.model, r);
                });
                results.appendChild(r);
            });
            results.hidden = false;
        });

        // ============ TRAY ============
        var tray = document.querySelector('[data-pv-tray]');
        var trayList = tray.querySelector('[data-pv-tray-list]');
        var trayEmpty = tray.querySelector('[data-pv-tray-empty]');
        var trayCount = tray.querySelector('[data-pv-tray-count]');
        var traySum = tray.querySelector('[data-pv-tray-sum]');
        var traySubmit = tray.querySelector('[data-pv-tray-submit]');
        var trayToggle = tray.querySelector('[data-pv-tray-toggle]');

        var TRAY = []; // {brandId, modelId, colorId, variantId, qty}

        function findVariantExact(modelId, colorId) {
            return MODEL.variants.find(function (v) {
                var ok = v.values.indexOf(modelId) !== -1;
                if (colorId) ok = ok && v.values.indexOf(colorId) !== -1;
                return ok;
            }) || null;
        }
        function trayQty(brandId, modelId) {
            return TRAY.filter(function (t) { return t.brandId === brandId && t.modelId === modelId; })
                .reduce(function (s, t) { return s + t.qty; }, 0);
        }

        function addToTray(brandId, modelId, colorId) {
            var vr = findVariantExact(modelId, colorId);
            if (!vr || vr.stock <= 0) { toast('این گزینه ناموجود است'); return; }
            var line = TRAY.find(function (t) { return t.variantId === vr.id; });
            if (line) {
                if (line.qty + 1 > vr.stock) { toast('بیشتر از موجودی نمی‌توان افزود'); return; }
                line.qty++;
            } else {
                TRAY.push({ brandId: brandId, modelId: modelId, colorId: colorId, variantId: vr.id, qty: 1 });
            }
            renderTray();
            if (state.brand != null) renderModels(state.brand, input.value.trim() || null);
        }

        function renderTray() {
            var totalQty = TRAY.reduce(function (s, t) { return s + t.qty; }, 0);
            trayCount.textContent = totalQty;
            tray.hidden = false;
            if (!TRAY.length) {
                trayEmpty.hidden = false; trayList.hidden = true;
                traySubmit.disabled = true; traySum.textContent = '0';
                tray.classList.remove('has-items');
                return;
            }
            trayEmpty.hidden = true; trayList.hidden = false;
            tray.classList.add('has-items');
            trayList.innerHTML = '';
            var sum = 0;
            TRAY.forEach(function (t) {
                var vr = MODEL.variants.find(function (v) { return v.id === t.variantId; });
                sum += (vr ? vr.final : 0) * t.qty;
                var bName = (valueById(t.brandId) || {}).value || '';
                var mName = (valueById(t.modelId) || {}).value || '';
                var cName = t.colorId ? (' • ' + ((valueById(t.colorId) || {}).value || '')) : '';
                var li = el('li', 'pv-tray__item');
                li.innerHTML =
                    '<span class="pv-tray__lbl">' + bName + ' ' + mName + cName + '</span>' +
                    '<span class="pv-tray__qty">' +
                    '<button type="button" class="pv-qbtn" data-dec aria-label="کم">−</button>' +
                    '<b>' + t.qty + '</b>' +
                    '<button type="button" class="pv-qbtn" data-inc aria-label="زیاد">+</button>' +
                    '</span>' +
                    '<button type="button" class="pv-tray__del" data-del aria-label="حذف">×</button>';
                li.querySelector('[data-inc]').addEventListener('click', function () {
                    if (t.qty + 1 > (vr ? vr.stock : t.qty)) { toast('بیشتر از موجودی نمی‌توان افزود'); return; }
                    t.qty++; renderTray(); if (state.brand != null) renderModels(state.brand, input.value.trim() || null);
                });
                li.querySelector('[data-dec]').addEventListener('click', function () {
                    t.qty--; if (t.qty <= 0) TRAY.splice(TRAY.indexOf(t), 1);
                    renderTray(); if (state.brand != null) renderModels(state.brand, input.value.trim() || null);
                });
                li.querySelector('[data-del]').addEventListener('click', function () {
                    TRAY.splice(TRAY.indexOf(t), 1); renderTray();
                    if (state.brand != null) renderModels(state.brand, input.value.trim() || null);
                });
                trayList.appendChild(li);
            });
            traySum.textContent = fmt(sum);
            traySubmit.disabled = false;
        }

        trayToggle.addEventListener('click', function () {
            var collapsed = tray.classList.toggle('is-collapsed');
            trayToggle.setAttribute('aria-expanded', String(!collapsed));
        });

        traySubmit.addEventListener('click', function () {
            if (!TRAY.length) return;
            traySubmit.disabled = true; traySubmit.classList.add('is-loading');
            var payload = TRAY.map(function (t) { return { productId: MODEL.productId, variantId: t.variantId, quantity: t.qty }; });
            fetch('/cart/add-batch', {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token()
                },
                body: JSON.stringify(payload)
            }).then(function (r) {
                if (!r.ok) throw new Error('failed');
                return r.json();
            }).then(function (j) {
                bumpCartCount(j.count);
                TRAY.length = 0; renderTray();
                if (state.brand != null) renderModels(state.brand, input.value.trim() || null);
                toast('به سبد خرید اضافه شد');
            }).catch(function () {
                toast('خطا در افزودن به سبد');
            }).finally(function () {
                traySubmit.classList.remove('is-loading');
            });
        });

        renderTray();
    }

    // ---------- boot ----------
    if (MODEL.layout === 'device') buildDevice();
    else buildSimple();

    // برای سازگاری با site.js (دکمهٔ تکی)
    window.PdVariant = window.PdVariant || {};
    window.PdVariant.flashRequired = function () {
        var bar = document.querySelector('.pv-stockbar');
        if (bar) { bar.classList.add('pv-flash'); setTimeout(function () { bar.classList.remove('pv-flash'); }, 600); }
    };
})();
