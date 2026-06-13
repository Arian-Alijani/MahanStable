// ویزارد افزودن محصول: جابه‌جایی بین حالت «ساده» و «چندبرندی»،
// انتخاب برند → نمایش/انتخاب گوشی‌ها، و محاسبهٔ تعداد گزینه‌هایی که تولید می‌شود.
(function () {
    "use strict";

    var root = document.getElementById("product-wizard");
    if (!root) return;

    var panes = root.querySelectorAll(".wizard-pane");
    var toggle = root.querySelector("#wizard-mode-toggle");

    function showMode(mode) {
        panes.forEach(function (pane) {
            var isMatch = pane.getAttribute("data-mode") === mode;
            pane.classList.toggle("d-none", !isMatch);
            // غیرفعال کردن فیلدهای فرم پنهان تا با فرم فعال تداخل/ارسال ناخواسته نکنند
            pane.querySelectorAll("input, select, textarea, button").forEach(function (el) {
                el.disabled = !isMatch;
            });
        });
    }

    if (toggle) {
        toggle.addEventListener("change", function (e) {
            if (e.target && e.target.name === "wizardModeToggle") {
                showMode(e.target.value);
            }
        });
    }

    // حالت اولیه
    var checked = root.querySelector('input[name="wizardModeToggle"]:checked');
    showMode(checked ? checked.value : "simple");

    // ---- منطق چندبرندی ----
    var multiForm = root.querySelector('#form-multi');
    if (multiForm) {
        // باز/بستن لیست مدل‌های هر برند + هماهنگ‌سازی تیک برند با مدل‌ها
        root.querySelectorAll(".wizard-brand").forEach(function (brandBox) {
            var brandCheck = brandBox.querySelector(".wizard-brand-check");
            var modelsBox = brandBox.querySelector(".wizard-models");
            var modelChecks = brandBox.querySelectorAll(".wizard-model-check");

            if (brandCheck && modelsBox) {
                brandCheck.addEventListener("change", function () {
                    modelsBox.classList.toggle("d-none", !brandCheck.checked);
                    // انتخاب/لغو همهٔ مدل‌های این برند
                    modelChecks.forEach(function (mc) {
                        mc.checked = brandCheck.checked;
                    });
                    updateSummary();
                });
            }

            modelChecks.forEach(function (mc) {
                mc.addEventListener("change", function () {
                    // اگر هیچ مدلی تیک نیست، تیک برند هم برداشته شود؛ اگر حداقل یکی هست، برند تیک بخورد
                    var anyChecked = Array.prototype.some.call(modelChecks, function (x) { return x.checked; });
                    if (brandCheck) brandCheck.checked = anyChecked;
                    if (modelsBox && anyChecked) modelsBox.classList.remove("d-none");
                    updateSummary();
                });
            });
        });

        // رنگ‌ها روی تعداد گزینه‌ها تأثیر دارند
        multiForm.querySelectorAll('input[name="ColorValueIds"]').forEach(function (cc) {
            cc.addEventListener("change", updateSummary);
        });

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

        updateSummary();
    }
})();
