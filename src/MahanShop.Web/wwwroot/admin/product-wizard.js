// ویزارد افزودن محصول (F5b): هماهنگ با تب‌بندیِ فرم.
//  • جابه‌جایی بین حالت «ساده» و «چندبرندی» (تیک «چندمدلی» در تب «مدل‌ها»).
//  • انتخاب برند → نمایش/انتخاب گوشی‌ها (مدل‌ها)، و محاسبهٔ تعداد گزینه‌هایی که تولید می‌شود.
//  • فیلدهای فرمِ غیرفعال disable می‌شوند تا با فرمِ فعال تداخل/ارسالِ ناخواسته نکنند.
(function () {
    "use strict";

    // ریشه = همان جعبهٔ فرم محصول (Create یا Edit). هر دو شناسهٔ ممکن را می‌پذیریم.
    var root = document.getElementById("product-wizard")
            || document.getElementById("product-create-form");
    if (!root) return;

    var panes = root.querySelectorAll(".wizard-pane");

    // فعال/غیرفعال‌کردن فیلدهای یک فرم بدون پنهان‌کردن آن (برای جلوگیری از ارسال فرمِ غیرفعال).
    function setPaneDisabled(pane, disabled) {
        pane.querySelectorAll("input, select, textarea, button").forEach(function (el) {
            el.disabled = disabled;
        });
    }

    function showMode(mode) {
        panes.forEach(function (pane) {
            var isMatch = pane.getAttribute("data-mode") === mode;
            pane.classList.toggle("d-none", !isMatch);
            setPaneDisabled(pane, !isMatch);
        });
        // هم‌گام‌سازیِ همهٔ گروه‌های رادیوییِ نوعِ محصول (در هر دو فرم) با حالتِ فعلی.
        root.querySelectorAll('input[name="wizardModeToggle"]').forEach(function (r) {
            r.checked = (r.value === mode);
        });
        // اعلام به لایهٔ تب‌بندی که حالت عوض شد (تا تب فعال را در فرمِ درست نشان دهد).
        root.dispatchEvent(new CustomEvent("wizard:modechange", { detail: { mode: mode } }));
    }

    // به همهٔ رادیوها گوش می‌دهیم (نه فقط toggleِ فرمِ ساده).
    root.querySelectorAll('input[name="wizardModeToggle"]').forEach(function (r) {
        r.addEventListener("change", function (e) {
            if (e.target.checked) showMode(e.target.value);
        });
    });

    // حالت اولیه (با احترام به انتخابِ از پیش‌تیک‌خورده، مثلاً پس از خطای اعتبارسنجی).
    var checked = root.querySelector('input[name="wizardModeToggle"]:checked');
    showMode(checked ? checked.value : "simple");

    // ---- منطق چندبرندی ----
    var multiForm = root.querySelector('#form-multi');
    if (multiForm) {
        var summaryEl = document.getElementById("wizard-variant-summary");

        function updateSummary() {
            if (!summaryEl) return;
            var models = multiForm.querySelectorAll('.wizard-model-check:checked').length;
            var colors = multiForm.querySelectorAll('input[name="ColorValueIds"]:checked').length;
            if (models === 0) {
                summaryEl.textContent = "با انتخاب گوشی‌ها، تعداد گزینه‌هایی که ساخته می‌شود اینجا نمایش داده می‌شود.";
                return;
            }
            var total = colors > 0 ? models * colors : models;
            var msg = models + " گوشی";
            if (colors > 0) msg += " × " + colors + " رنگ";
            msg += " = " + total + " گزینه ساخته خواهد شد.";
            summaryEl.textContent = msg;
        }

        // باز/بستن لیست مدل‌های هر برند + هماهنگ‌سازی تیک برند با مدل‌ها.
        root.querySelectorAll(".wizard-brand").forEach(function (brandBox) {
            var brandCheck = brandBox.querySelector(".wizard-brand-check");
            var modelsBox = brandBox.querySelector(".wizard-models");
            var modelChecks = brandBox.querySelectorAll(".wizard-model-check");

            if (brandCheck && modelsBox) {
                brandCheck.addEventListener("change", function () {
                    modelsBox.classList.toggle("d-none", !brandCheck.checked);
                    modelChecks.forEach(function (mc) { mc.checked = brandCheck.checked; });
                    updateSummary();
                });
            }

            modelChecks.forEach(function (mc) {
                mc.addEventListener("change", function () {
                    var anyChecked = Array.prototype.some.call(modelChecks, function (x) { return x.checked; });
                    if (brandCheck) brandCheck.checked = anyChecked;
                    if (modelsBox && anyChecked) modelsBox.classList.remove("d-none");
                    updateSummary();
                });
            });
        });

        multiForm.querySelectorAll('input[name="ColorValueIds"]').forEach(function (cc) {
            cc.addEventListener("change", updateSummary);
        });

        updateSummary();
    }
})();
