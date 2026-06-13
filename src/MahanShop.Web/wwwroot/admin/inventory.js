// مدیریت موجودی — ویرایش سریع inline، تغییر +/-، انتخاب چندتایی، شمارندهٔ انتخاب.
(function () {
    "use strict";

    function token() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : "";
    }

    function toast(msg, ok) {
        var t = document.createElement("div");
        t.className = "inv-toast " + (ok ? "is-ok" : "is-err");
        t.textContent = msg;
        document.body.appendChild(t);
        // انیمیشن ورود
        requestAnimationFrame(function () { t.classList.add("is-show"); });
        setTimeout(function () {
            t.classList.remove("is-show");
            setTimeout(function () { t.remove(); }, 300);
        }, 2600);
    }

    function applyTone(input, stock) {
        var dot = input.parentElement.querySelector(".inv-dot");
        if (!dot) return;
        dot.classList.remove("is-out", "is-low", "is-in");
        if (stock === 0) { dot.classList.add("is-out"); dot.title = "ناموجود"; }
        else if (stock <= 5) { dot.classList.add("is-low"); dot.title = "فقط " + stock + " عدد"; }
        else { dot.classList.add("is-in"); dot.title = "موجود"; }
    }

    function post(handler, params, onDone) {
        var body = new URLSearchParams();
        body.append("__RequestVerificationToken", token());
        Object.keys(params).forEach(function (k) { body.append(k, params[k]); });

        var url = window.location.pathname + "?handler=" + handler;
        fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded", "X-Requested-With": "XMLHttpRequest" },
            body: body.toString()
        })
            .then(function (r) { return r.json(); })
            .then(function (data) { onDone(data); })
            .catch(function () { toast("خطای ارتباط با سرور.", false); });
    }

    // ویرایش مستقیم موجودی (Enter یا blur)
    function commitStock(input) {
        var id = input.getAttribute("data-variant");
        var val = parseInt(input.value, 10);
        if (isNaN(val) || val < 0) { toast("عدد نامعتبر.", false); return; }
        post("SetStock", { variantId: id, stock: val }, function (data) {
            if (data.ok) { input.value = data.stock; applyTone(input, data.stock); toast("موجودی ذخیره شد.", true); }
            else toast(data.error || "خطا.", false);
        });
    }

    document.addEventListener("keydown", function (e) {
        if (e.key === "Enter" && e.target.classList.contains("inv-stock")) {
            e.preventDefault();
            commitStock(e.target);
            e.target.blur();
        }
    });

    document.addEventListener("blur", function (e) {
        if (e.target.classList && e.target.classList.contains("inv-stock")) {
            var orig = e.target.defaultValue;
            if (String(e.target.value) !== String(orig)) commitStock(e.target);
        }
    }, true);

    // دکمه‌های +/-
    document.addEventListener("click", function (e) {
        var btn = e.target.closest(".inv-adj");
        if (!btn) return;
        var id = btn.getAttribute("data-variant");
        var delta = parseInt(btn.getAttribute("data-delta"), 10) || 0;
        post("AdjustStock", { variantId: id, delta: delta }, function (data) {
            if (data.ok) {
                var input = document.querySelector('.inv-stock[data-variant="' + id + '"]');
                if (input) { input.value = data.stock; input.defaultValue = data.stock; applyTone(input, data.stock); }
                // پرش کوچک برای بازخورد
                btn.classList.add("inv-bump");
                setTimeout(function () { btn.classList.remove("inv-bump"); }, 200);
            } else toast(data.error || "خطا.", false);
        });
    });

    // انتخاب همه + شمارندهٔ انتخاب
    function updateCount() {
        var n = document.querySelectorAll(".inv-row-check:checked").length;
        var c = document.getElementById("invSelCount");
        if (c) c.textContent = n;
    }
    document.addEventListener("change", function (e) {
        if (e.target.classList.contains("inv-check-all")) {
            var checked = e.target.checked;
            document.querySelectorAll(".inv-row-check").forEach(function (cb) { cb.checked = checked; });
            updateCount();
        } else if (e.target.classList.contains("inv-row-check")) {
            updateCount();
        }
    });

    // جلوگیری از ارسال فرم دسته‌ای بدون انتخاب
    var bulkForm = document.getElementById("invBulkForm");
    if (bulkForm) {
        bulkForm.addEventListener("submit", function (e) {
            // فقط دکمهٔ «اعمال روی انتخاب‌شده‌ها» این فرم را submit می‌کند
            var n = document.querySelectorAll(".inv-row-check:checked").length;
            if (n === 0) { e.preventDefault(); toast("ابتدا حداقل یک ردیف را انتخاب کنید.", false); }
        });
    }

    document.addEventListener("DOMContentLoaded", updateCount);
})();
