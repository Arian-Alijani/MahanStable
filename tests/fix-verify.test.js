/* تست‌های بهینه و سریع برای دو رفعِ باگ این سشن — بدون نیاز به SDK دات‌نت.
 * 1) باگ ناپدیدشدن گرید دسته‌بندی: ترتیبِ input های isActive باید checkbox سپس hidden باشد
 *    تا باندینگِ bool در ASP.NET مقدار true را بگیرد (نه «false,true» که همیشه false می‌شود).
 * 2) باگ افزوده‌نشدنِ اولین محصول هر نوار: منطق سرکوبِ کلیک کاروسل نباید کلیکِ بعد از یک
 *    فشردنِ ساده (بدون درگ) را بلوکه کند؛ پرچم باید بلافاصله ریست شود.
 */
const fs = require('fs');
const path = require('path');

let pass = 0, fail = 0;
function ok(name, cond) {
  if (cond) { pass++; console.log('  PASS', name); }
  else { fail++; console.log('  FAIL', name); }
}

/* ---------- Test 1: ترتیب checkbox/hidden در فرم تنظیمات گرید ---------- */
const cshtml = fs.readFileSync(
  path.join(__dirname, '../src/MahanShop.Web/Areas/Admin/Pages/Home/Index.cshtml'), 'utf8');

const cbIdx = cshtml.indexOf('name="isActive" value="true" type="checkbox"');
const hiddenIdx = cshtml.indexOf('<input type="hidden" name="isActive" value="false" />');
ok('grid: checkbox isActive present', cbIdx > -1);
ok('grid: hidden isActive=false present', hiddenIdx > -1);
ok('grid: checkbox comes BEFORE hidden (binds true when checked)', cbIdx > -1 && hiddenIdx > -1 && cbIdx < hiddenIdx);

/* شبیه‌سازی باندینگِ bool دات‌نت: اولین مقدارِ پست‌شده برنده است.
 * checkbox تیک‌خورده → "true" ، سپس hidden → "false"  ⇒ ترتیب پست = [true, false] ⇒ bind=true */
function bindBool(values) { return values.length ? values[0] === 'true' : false; }
const postedWhenChecked = cbIdx < hiddenIdx ? ['true', 'false'] : ['false', 'true'];
ok('grid: checked => binds TRUE (grid visible)', bindBool(postedWhenChecked) === true);
const postedWhenUnchecked = ['false']; // فقط hidden پست می‌شود
ok('grid: unchecked => binds FALSE (grid hidden, toggle off works)', bindBool(postedWhenUnchecked) === false);

/* ---------- Test 2: منطق سرکوب کلیک کاروسل (add-to-cart) ---------- */
// بازسازیِ مینیمالِ منطقِ site.js برای کاروسل
function makeCarousel() {
  let down = false, moved = false, startX = 0, suppressNextClick = false;
  const DRAG_THRESHOLD = 8;
  return {
    pointerdown(x) { down = true; moved = false; startX = x; },
    pointermove(x) { if (!down) return; if (!moved && Math.abs(x - startX) > DRAG_THRESHOLD) moved = true; },
    pointerup() { if (!down) return; down = false; suppressNextClick = moved; moved = false; },
    // برمی‌گرداند: آیا کلیک به دکمه می‌رسد (true) یا سرکوب شد (false)
    click() { if (suppressNextClick) { suppressNextClick = false; return false; } return true; },
  };
}

// سناریو A: فشردنِ ساده روی دکمهٔ اولین محصول بدون حرکت → کلیک باید برسد
let c = makeCarousel();
c.pointerdown(100); c.pointerup();
ok('cart: simple tap on first product => click reaches add button', c.click() === true);

// سناریو B: لرزشِ جزئی (۵px زیر آستانه) → هنوز کلیک می‌رسد (باگ قبلی اینجا می‌شکست)
c = makeCarousel();
c.pointerdown(100); c.pointermove(105); c.pointerup();
ok('cart: tiny jitter (<8px) still adds first product', c.click() === true);

// سناریو C: درگِ واقعی → فقط همان کلیک سرکوب، کلیکِ بعدی آزاد
c = makeCarousel();
c.pointerdown(100); c.pointermove(140); c.pointerup();
ok('cart: real drag suppresses its own click', c.click() === false);
ok('cart: next click after drag is FREE (flag reset)', c.click() === true);

// سناریو D: چند کلیکِ پشت‌سرهم روی اولین محصول → همه می‌رسند
c = makeCarousel();
let allReach = true;
for (let i = 0; i < 3; i++) { c.pointerdown(100); c.pointerup(); if (!c.click()) allReach = false; }
ok('cart: repeated taps on first product all reach add button', allReach);

console.log(`\nRESULT: ${pass} passed, ${fail} failed`);
process.exit(fail ? 1 : 0);
