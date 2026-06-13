// مدیریت موجودی F7 — expand/collapse واریانت‌ها + ویرایش inline قیمت/تخفیف/موجودی
(function () {
    "use strict";

    // ─── ابزارها ─────────────────────────────────────────────────────────────
    function token() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : "";
    }

    function toast(msg, ok) {
        var t = document.createElement("div");
        t.className = "inv-toast " + (ok ? "is-ok" : "is-err");
        t.textContent = msg;
        document.body.appendChild(t);
        requestAnimationFrame(function () { t.classList.add("is-show"); });
        setTimeout(function () {
            t.classList.remove("is-show");
            setTimeout(function () { t.remove(); }, 300);
        }, 2600);
    }

    function post(handler, params, onDone) {
        var body = new URLSearchParams();
        body.append("__RequestVerificationToken", token());
        Object.keys(params).forEach(function (k) {
            if (params[k] !== undefined && params[k] !== null) body.append(k, params[k]);
        });
        var url = window.location.pathname + "?handler=" + handler;
        fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "X-Requested-With": "XMLHttpRequest"
            },
            body: body.toString()
        })
            .then(function (r) { return r.json(); })
            .then(function (data) { onDone(data); })
            .catch(function () { toast("خطای ارتباط با سرور.", false); });
    }

    function applyStockDot(input, stock) {
        var dot = input.parentElement.querySelector(".inv-dot");
        if (!dot) return;
        dot.classList.remove("is-out", "is-low", "is-in");
        if (stock === 0) { dot.classList.add("is-out"); dot.title = "ناموجود"; }
        else if (stock <= 5) { dot.classList.add("is-low"); dot.title = "فقط " + stock + " عدد"; }
        else { dot.classList.add("is-in"); dot.title = "موجود"; }
    }

    // ─── Expand/Collapse واریانت‌ها ─────────────────────────────────────────
    document.addEventListener("click", function (e) {
        var btn = e.target.closest(".inv7-expand-btn");
        if (!btn) return;
        var targetId = btn.getAttribute("data-target");
        var row = document.getElementById(targetId);
        if (!row) return;
        var expanded = btn.getAttribute("aria-expanded") === "true";
        if (expanded) {
            row.classList.add("d-none");
            btn.setAttribute("aria-expanded", "false");
            btn.classList.remove("is-open");
        } else {
            row.classList.remove("d-none");
            btn.setAttribute("aria-expanded", "true");
            btn.classList.add("is-open");
        }
    });

    // ─── ویرایش موجودی محصول ساده (Enter/blur) ──────────────────────────────
    function commitSimpleStock(input) {
        var pid = input.getAttribute("data-product");
        var val = parseInt(input.value, 10);
        if (isNaN(val) || val < 0) { toast("عدد نامعتبر.", false); return; }
        post("SetSimpleStock", { productId: pid, stock: val }, function (data) {
            if (data.ok) {
                input.defaultValue = input.value;
                applyStockDot(input, val);
                toast("موجودی ذخیره شد.", true);
            } else toast(data.error || "خطا.", false);
        });
    }

    // ─── ویرایش قیمت/تخفیف محصول ساده (Enter/blur) ─────────────────────────
    function commitSimplePrice(row) {
        var pid = row.getAttribute("data-product");
        var priceInput = row.querySelector('.inv7-price[data-field="price"]');
        var discInput  = row.querySelector('.inv7-price[data-field="discount"]');
        if (!priceInput || !discInput) return;
        var price    = parseInt(priceInput.value, 10) || 0;
        var discount = parseInt(discInput.value, 10)  || 0;
        post("SetSimplePrice", {
            productId: pid,
            price: price,
            discountPrice: discount > 0 ? discount : 0
        }, function (data) {
            if (data.ok) {
                priceInput.defaultValue  = priceInput.value;
                discInput.defaultValue   = discInput.value;
                toast("قیمت ذخیره شد.", true);
            } else toast(data.error || "خطا.", false);
        });
    }

    // ─── ویرایش قیمت/تخفیف/موجودی واریانت (Enter/blur) ─────────────────────
    function commitVariantFields(variantRow) {
        var vid = variantRow.getAttribute("data-variant");
        var priceInput    = variantRow.querySelector('.inv7-vt-price[data-field="price"]');
        var discInput     = variantRow.querySelector('.inv7-vt-price[data-field="discount"]');
        var stockInput    = variantRow.querySelector(".inv7-vt-stock");
        if (!priceInput || !stockInput) return;
        var price    = parseInt(priceInput.value, 10)  || 0;
        var discount = discInput ? (parseInt(discInput.value, 10) || 0) : 0;
        var stock    = parseInt(stockInput.value, 10)  || 0;
        post("SetVariant", {
            variantId: vid,
            price: price,
            discountPrice: discount > 0 ? discount : 0,
            stock: stock
        }, function (data) {
            if (data.ok) {
                priceInput.defaultValue = priceInput.value;
                if (discInput) discInput.defaultValue = discInput.value;
                stockInput.defaultValue = stockInput.value;
                applyStockDot(stockInput, stock);
                toast("گزینه ذخیره شد.", true);
            } else toast(data.error || "خطا.", false);
        });
    }

    // ─── ویرایش مستقیم موجودی واریانت (Enter/blur) ──────────────────────────
    function commitVariantStock(input) {
        var vid = input.getAttribute("data-variant");
        var val = parseInt(input.value, 10);
        if (isNaN(val) || val < 0) { toast("عدد نامعتبر.", false); return; }
        post("SetVariantStock", { variantId: vid, stock: val }, function (data) {
            if (data.ok) {
                input.defaultValue = data.stock.toString();
                input.value = data.stock;
                applyStockDot(input, data.stock);
                toast("موجودی ذخیره شد.", true);
            } else toast(data.error || "خطا.", false);
        });
    }

    // ─── کلید Enter ─────────────────────────────────────────────────────────
    document.addEventListener("keydown", function (e) {
        if (e.key !== "Enter") return;

        // موجودی ساده
        if (e.target.classList.contains("inv7-simple-stock")) {
            e.preventDefault();
            commitSimpleStock(e.target);
            e.target.blur();
            return;
        }

        // موجودی واریانت (فقط موجودی — سریع)
        if (e.target.classList.contains("inv7-vt-stock")) {
            e.preventDefault();
            var vr = e.target.closest("tr[data-variant]");
            if (vr) commitVariantFields(vr);
            e.target.blur();
            return;
        }

        // قیمت محصول ساده
        if (e.target.classList.contains("inv7-price")) {
            e.preventDefault();
            var sr = e.target.closest("tr[data-product]");
            if (sr) commitSimplePrice(sr);
            e.target.blur();
            return;
        }

        // قیمت واریانت
        if (e.target.classList.contains("inv7-vt-price")) {
            e.preventDefault();
            var vr2 = e.target.closest("tr[data-variant]");
            if (vr2) commitVariantFields(vr2);
            e.target.blur();
        }
    });

    // ─── blur (ذخیره خودکار وقتی فوکوس رها می‌شود) ──────────────────────────
    document.addEventListener("blur", function (e) {
        var t = e.target;
        if (!t.classList) return;
        var changed = String(t.value) !== String(t.defaultValue);
        if (!changed) return;

        if (t.classList.contains("inv7-simple-stock")) {
            commitSimpleStock(t);
        } else if (t.classList.contains("inv7-price")) {
            var sr = t.closest("tr[data-product]");
            if (sr) commitSimplePrice(sr);
        } else if (t.classList.contains("inv7-vt-stock") || t.classList.contains("inv7-vt-price")) {
            var vr = t.closest("tr[data-variant]");
            if (vr) commitVariantFields(vr);
        }
    }, true);

    // ─── دکمه‌های +/- واریانت ─────────────────────────────────────────────
    document.addEventListener("click", function (e) {
        var btn = e.target.closest(".inv7-adj");
        if (!btn) return;
        var vid   = btn.getAttribute("data-variant");
        var delta = parseInt(btn.getAttribute("data-delta"), 10) || 0;
        post("AdjustVariantStock", { variantId: vid, delta: delta }, function (data) {
            if (data.ok) {
                var input = document.querySelector('.inv7-vt-stock[data-variant="' + vid + '"]');
                if (input) {
                    input.value = data.stock;
                    input.defaultValue = data.stock;
                    applyStockDot(input, data.stock);
                }
                btn.classList.add("inv-bump");
                setTimeout(function () { btn.classList.remove("inv-bump"); }, 200);
                toast("موجودی: " + data.stock, true);
            } else toast(data.error || "خطا.", false);
        });
    });
})();
