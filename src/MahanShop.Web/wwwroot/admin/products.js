// محصولات — روشن/خاموش‌کردن سریع محصول با سوییچ (AJAX) + بازخورد toast.
(function () {
    "use strict";

    function token() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : "";
    }

    // toast بازخورد (همان استایل صفحهٔ موجودی)
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

    function post(handler, params, onDone, onFail) {
        var body = new URLSearchParams();
        body.append("__RequestVerificationToken", token());
        Object.keys(params).forEach(function (k) { body.append(k, params[k]); });

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
            .catch(function () { if (onFail) onFail(); toast("خطای ارتباط با سرور.", false); });
    }

    // سوییچ فعال/غیرفعال
    document.addEventListener("change", function (e) {
        var cb = e.target;
        if (!cb.classList || !cb.classList.contains("prod-toggle")) return;

        var id = cb.getAttribute("data-product");
        var desired = cb.checked; // وضعیتی که کاربر می‌خواهد
        cb.disabled = true;

        post("Toggle", { id: id }, function (data) {
            cb.disabled = false;
            if (data.ok) {
                cb.checked = data.isActive;
                var label = cb.parentElement.querySelector(".prod-switch__text");
                if (label) label.textContent = data.isActive ? "فعال" : "غیرفعال";
                toast(data.isActive ? "محصول فعال شد." : "محصول غیرفعال شد.", true);
            } else {
                cb.checked = !desired; // برگرداندن به حالت قبل
                toast(data.error || "خطا در تغییر وضعیت.", false);
            }
        }, function () {
            cb.disabled = false;
            cb.checked = !desired;
        });
    });
})();
